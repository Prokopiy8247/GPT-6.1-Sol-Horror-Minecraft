using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MCR.Horror
{
    public sealed class HorrorRuntime
    {
        public static HorrorRuntime Current => GameManager.Instance?.horror;
        public readonly GameManager gm;
        public HorrorState state;
        readonly HorrorState previewKnowledge=new HorrorState();
        readonly HashSet<string> previewSuppressed=new HashSet<string>();
        readonly List<WardData> previewWards=new List<WardData>();
        readonly List<GameObject> temporary=new List<GameObject>();
        public List<WardData> Wards=>Player.IsCreative?previewWards:state.wards;
        public HorrorState Knowledge => Player.IsCreative ? previewKnowledge : state;
        public void Learn(string rule,bool confirmed=false) => Knowledge.Observe(rule,confirmed);
        public bool directorPaused, showcase;
        public int hushTicks;
        public long lastAction, lastContext=-1000;
        public string recentAction;
        public Vector3 actionPosition;
        public int protectionWarning;
        public bool refugeValid;
        public float cameraKick;
        public readonly List<string> diagnostics=new List<string>();
        readonly Dictionary<string,GameObject> props=new Dictionary<string,GameObject>();
        readonly Dictionary<string,List<AABB>> propShapes=new Dictionary<string,List<AABB>>();
        readonly Dictionary<string,long> familyTimes=new Dictionary<string,long>();
        int lastDimension=-1;
        GameMode lastMode;
        bool playerWasDead;
        int provisionCursor;
        public HorrorRuntime(GameManager gm)
        {
            this.gm=gm;state=HorrorSave.Read(gm.session.save.dir);
            if(!state.activated) {state.activated=true;state.graceUntil=state.clock+1800;}
            lastMode=gm.player.gameMode;
        }
        public World World=>gm.ActiveWorld;
        public Player Player=>gm.player;
        public void Save()
        {
            foreach(var e in World.entities) if(e is HorrorMob m && m.kind=="unseam" && !m.preview && !m.removed && !m.dead)
            {state.boss.active=true;state.boss.position=m.position;state.boss.dimension=(int)World.dim;state.boss.health=m.health;state.boss.final=m.final;}
            HorrorSave.Write(gm.session.save.dir,state);
        }
        public void Dispose()
        {
            foreach(var p in props.Values) if(p!=null) UnityEngine.Object.Destroy(p);
            props.Clear();propShapes.Clear();HorrorAudio.Stop();
            foreach(var effect in temporary)if(effect!=null)UnityEngine.Object.Destroy(effect);
            temporary.Clear();refugeValid=false;protectionWarning=0;cameraKick=0;
        }
        public void SetEnabled(bool enabled)
        {
            state.enabled=enabled;
            if(!enabled) {Dismiss();Dispose();}
            else {state.recoveryUntil=state.clock+900;state.graceUntil=Math.Max(state.graceUntil,state.clock+400);}
            Save();
        }
        public void Dismiss(bool previewsOnly=false)
        {
            foreach(var w in gm.session.worlds) if(w!=null)
            {
                foreach(var e in w.entities) if(e is HorrorMob m && (!previewsOnly || m.preview)) m.Remove();
                foreach(var e in w.entitiesToAdd) if(e is HorrorMob m && (!previewsOnly || m.preview)) m.Remove();
            }
            if(previewsOnly) {showcase=false;directorPaused=false;previewSuppressed.Clear();previewWards.Clear();Dispose();}
        }
        public void Tick()
        {
            var p=Player;var w=World;if(p==null || w==null) return;
            if(lastDimension!=(int)w.dim)
            {
                Save();Dismiss();Dispose();lastDimension=(int)w.dim;state.Recover(600);lastAction=0;
            }
            if(p.gameMode!=lastMode) {Dismiss(true);lastMode=p.gameMode;state.Recover(600);}
            if(p.dead) {if(!playerWasDead){state.Recover(1800);Dismiss();}playerWasDead=true;return;}
            if(playerWasDead){state.Recover(1800);playerWasDead=false;}
            if(!state.enabled) return;
            state.clock++;state.Expire();state.tension=Mathf.Max(0,state.tension-0.023f);
            cameraKick=Mathf.Max(0,cameraKick-.035f);
            if(hushTicks>0) hushTicks--;
            if(protectionWarning>0) protectionWarning--;
            if(state.clock%10==0) TickWard();
            if(state.clock%20==0)
            {
                UpdateProps();
                if(w.dim==DimensionId.Overworld && state.sites.Count<5) ProvisionSites();
                foreach(var site in state.sites)
                    if(site.dimension==(int)w.dim && (site.position-p.position).sqrMagnitude<100 && !site.visited)
                    {if(!p.IsCreative)site.visited=true;else if(previewKnowledge.observed.Contains(site.kind))continue;Learn(site.kind);gm.hud.Chat("Resonant site discovered: "+SiteName(site.kind)+". Use a tuning fork here.");}
                RestoreBoss();
            }
            if(p.onGround && (p.position-p.prevPosition).sqrMagnitude>0.003f && state.clock%8==0)
                Noise(p.position,p.sneaking||hushTicks>0?2:p.sprinting?22:10,"footsteps");
            if(state.clock%200==0 && !w.CanSeeSky(Int3.Floor(p.EyePosition)) && (p.position-p.prevPosition).sqrMagnitude>.003f)Action("cave",p.position);
            if(showcase || directorPaused || p.IsCreative || p.IsSpectator || w.dim!=DimensionId.Overworld) return;
            if(state.clock>=state.nextAmbient && !state.defeated && w.session.difficulty!=Difficulty.Peaceful)
            {
                state.nextAmbient=state.clock+900+Decision(900);
                if(state.tension<35 && recentAction!=null && state.clock-lastAction<160 && !refugeValid)
                    ContextEvent(recentAction,actionPosition);
            }
            if(state.clock<state.nextCombat) return;
            state.nextCombat=state.clock+500+Decision(600);
            var active=Active();
            if(!state.CanAdmit(w.session.difficulty==Difficulty.Peaceful,refugeValid||protectionWarning>0,active.Count)) {Log("reject combat: grace/recovery/safety/tension/cap");return;}
            bool source=false;
            foreach(var s in state.sites) if(!s.suppressed && s.visited && (s.position-p.position).sqrMagnitude<1024) source=true;
            if(state.attention<10 || NearbyBaseHostiles() || p.health<10) return;
            bool stalkEligible=state.stage>=1 && state.attention>=28;
            if(!source && !stalkEligible)return;
            int index=stalkEligible && (!source || Decision(3)==0)?0:1+Decision(6);
            if(index==2 && World.CanSeeSky(Int3.Floor(p.EyePosition)))index=1;
            if(index==4 && World.GetBrightness(p.position)>.65f)index=3;
            if(index==6 && state.stage<1)index=1;
            if(state.creatureCooldowns[index]>state.clock) return;
            string kind=HorrorRegistry.Creatures[index];
            foreach(var m in active) if(!HorrorState.Compatible(kind,m.kind)) {Log("reject "+kind+": incompatible "+m.kind);return;}
            if(!TrySpawn(kind,false,out var spawn)) return;
            state.creatureCooldowns[index]=state.clock+2400;state.tension+=22;
            state.Log("admit "+kind+" at "+spawn.position);Log("admit "+kind);
        }
        int Decision(int max)
        {
            uint value=(uint)(gm.session.seed ^ (++state.decisions*1103515245));value^=value>>16;value*=2246822519u;value^=value>>13;
            return (int)(value%(uint)max);
        }
        public List<HorrorMob> Active()
        {
            var result=new List<HorrorMob>();
            foreach(var e in World.entities) if(e is HorrorMob m && !m.dead && !m.removed) result.Add(m);
            foreach(var e in World.entitiesToAdd) if(e is HorrorMob m && !m.dead && !m.removed) result.Add(m);
            return result;
        }
        public bool NearbyBaseHostiles()
        {
            foreach(var e in World.entities) if(e is Mob m && !(e is HorrorMob) && m.def.hostile && !m.dead && !m.removed && (m.position-Player.position).sqrMagnitude<100) return true;
            return false;
        }
        public bool AttackAllowed(HorrorMob actor)
        {
            if(!state.enabled || Player.dead || Player.world!=actor.world || Player.IsSpectator) return false;
            if(actor.preview) return Player.IsCreative;
            if(Player.IsCreative || World.session.difficulty==Difficulty.Peaceful || refugeValid || protectionWarning>0 || state.clock<state.recoveryUntil) return false;
            if(!HasEscape(World,Player.position,Player.width,Player.height))return false;
            foreach(var other in Active())
                if(other!=actor && (other.position-actor.position).sqrMagnitude<400 && (!HorrorState.Compatible(other.kind,actor.kind) || other.attackTicks>0)) return false;
            return !NearbyBaseHostiles();
        }
        public void Log(string text)
        {
            diagnostics.Add(state.clock+": "+text);while(diagnostics.Count>32) diagnostics.RemoveAt(0);
        }
        public void Action(string action,Vector3 pos)
        {
            if(!state.enabled) return;
            recentAction=action;actionPosition=pos;lastAction=state.clock;
            Noise(pos,action=="mining"?26:action=="door"?18:12,action);
            if(action=="mining" && state.clock-lastContext>=1200 && state.clock>200 && state.tension<30) ContextEvent(action,pos);
        }
        void ContextEvent(string action,Vector3 pos)
        {
            if(state.clock-lastContext<900) return;
            string family=action=="mining"?"answer":action=="door"?"threshold":action=="cave"?"below":"trace";
            if(familyTimes.TryGetValue(family,out long time) && state.clock-time<1800) return;
            Vector3 origin=pos+new Vector3(5,1,4);
            if(!World.IsLoaded(Int3.Floor(origin))) return;
            lastContext=state.clock;familyTimes[family]=state.clock;state.Log("context "+family+" from "+action);
            HorrorAudio.Play(family=="below"?"warning":"answer",origin,state.intensity*0.4f);
            Learn(family=="answer"?"mining_answer":family=="threshold"?"threshold_trace":family=="below"?"ceiling_trace":"building_trace");
            gm.hud.Chat(action=="mining"?"A second knock answers from beyond the stone. Press J to investigate.":action=="cave"?"Thin ivory flecks cling to the ceiling. Something moves above.":"A faint blue trace remains beyond the threshold. Press J.");
            FlashMark(origin,3);
        }
        public void Noise(Vector3 pos,float radius,string source)
        {
            if(!state.enabled) return;
            foreach(var m in Active()) if(!m.dead && (m.position-pos).sqrMagnitude<radius*radius && m.kind!="stillwright") m.Hear(pos,source);
        }
        public static bool Sight(World w,Vector3 origin,Vector3 target)
        {
            if(!w.HasLineOfSight(origin,target))return false;
            var h=Current;if(h==null || h.World!=w)return true;
            var delta=target-origin;float length=delta.magnitude;if(length<.01f)return true;
            foreach(var shapes in h.propShapes.Values)foreach(var box in shapes)if(box.Raycast(origin,delta/length,length-.02f,out _,out _))return false;
            return true;
        }
        public static bool HasEscape(World w,Vector3 feet,float width,float height)
        {
            foreach(var direction in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right})
            {
                Vector3 previous=feet;bool clear=true;
                for(int step=1;step<=6;step++)
                {
                    bool next=false;
                    foreach(int rise in new[]{0,1,-1})
                    {
                        Vector3 point=feet+direction*(step*.6f);point.y=Mathf.Floor(previous.y+.1f)+rise;
                        if(CanStand(w,point,width,height) && Sight(w,previous+Vector3.up*1.2f,point+Vector3.up*1.2f)){previous=point;next=true;break;}
                    }
                    if(!next){clear=false;break;}
                }
                if(clear)return true;
            }
            return false;
        }
        public void CollectPropCollisions(World world,AABB area,List<AABB> result)
        {
            if(!state.enabled || world!=World)return;
            foreach(var shapes in propShapes.Values)foreach(var box in shapes)if(box.Intersects(area))result.Add(box);
        }
        public static bool CanStand(World w,Vector3 p,float width,float height,bool ground=true)
        {
            if(w==null) return false;
            int minX=Mathf.FloorToInt(p.x-width/2+0.02f),maxX=Mathf.FloorToInt(p.x+width/2-0.02f);
            int minZ=Mathf.FloorToInt(p.z-width/2+0.02f),maxZ=Mathf.FloorToInt(p.z+width/2-0.02f);
            bool supported=false;
            for(int x=minX;x<=maxX;x++) for(int z=minZ;z<=maxZ;z++)
            {
                if(!w.IsLoaded(x,z)) return false;
                if(ground){var floor=w.GetBlock(x,Mathf.FloorToInt(p.y-0.05f),z);if(floor.isLiquid)return false;if(floor.solid)supported=true;}
                for(int y=Mathf.FloorToInt(p.y+0.02f);y<Mathf.CeilToInt(p.y+height);y++)
                {var b=w.GetBlock(x,y,z);if(b.solid || b.isLiquid || b.id=="fire" || b.id=="cactus") return false;}
            }
            return !ground || supported;
        }
        public bool TrySpawn(string kind,bool preview,out HorrorMob mob)
        {
            mob=null;var d=MobRegistry.Get("horror:"+kind);
            if(kind=="unseam" && !preview) foreach(var a in Active()) if(a.kind==kind && !a.preview) return false;
            for(int i=0;i<32;i++)
            {
                float angle=Decision(628)/100f;float distance=preview?12:22+Decision(18);
                int x=Mathf.FloorToInt(Player.position.x+Mathf.Sin(angle)*distance),z=Mathf.FloorToInt(Player.position.z+Mathf.Cos(angle)*distance);
                if(!World.IsLoaded(x,z)) continue;
                for(int y=Mathf.FloorToInt(Player.position.y)+6;y>=Player.position.y-8;y--)
                {
                    Vector3 p=new Vector3(x+0.5f,y,z+0.5f);
                    if(!CanStand(World,p,d.width,d.height) || IsProtected(p)) continue;
                    if(!preview && Vector3.Dot(Player.LookDir,(p-Player.EyePosition).normalized)>0.55f && Sight(World,Player.EyePosition,p+Vector3.up)) continue;
                    mob=MobRegistry.Spawn(World,d.id,p,SpawnReason.Summon) as HorrorMob;
                    mob.preview=preview;mob.home=p;
                    if(kind=="unseam" && !preview) {state.boss.active=true;state.boss.position=p;state.boss.health=mob.health;state.boss.dimension=(int)World.dim;}
                    return true;
                }
            }
            Log("reject "+kind+": no safe loaded spawn");return false;
        }
        void RestoreBoss()
        {
            if(!state.boss.active || state.defeated || !state.enabled || state.boss.dimension!=(int)World.dim || state.clock<state.recoveryUntil || Player.IsCreative) return;
            foreach(var m in Active()) if(m.kind=="unseam" && !m.preview) return;
            var b=state.boss;
            if((b.position-Player.position).sqrMagnitude<100 || !CanStand(World,b.position,3.2f,5.6f)) return;
            var m2=MobRegistry.Spawn(World,"horror:unseam",b.position,SpawnReason.Summon) as HorrorMob;
            m2.final=b.final;m2.health=b.health;m2.home=b.position;m2.state=HuntState.Search;
            Log("restored one campaign identity");
        }
        public static string SiteName(string kind)
        {
            switch(kind){case "survey_cairn":return "Survey Cairn";case "listening_well":return "Listening Well";case "shutter_chapel":return "Shutter Chapel";case "root_archive":return "Root Archive";default:return "Open Bell";}
        }
        void ProvisionSites()
        {
            string[] kinds={"survey_cairn","listening_well","shutter_chapel","root_archive","open_bell"};
            if(provisionCursor++%2!=0) return;
            int siteIndex=0;while(siteIndex<kinds.Length && state.sites.Exists(s=>s.kind==kinds[siteIndex]))siteIndex++;
            if(siteIndex>=kinds.Length)return;string kind=kinds[siteIndex];
            for(int i=0;i<28;i++)
            {
                float a=Decision(628)/100f;float r=22+state.sites.Count*15+Decision(80);
                int x=Mathf.FloorToInt(Player.position.x+Mathf.Sin(a)*r),z=Mathf.FloorToInt(Player.position.z+Mathf.Cos(a)*r);
                if(!World.IsLoaded(x,z)) continue;
                var c=World.ChunkAtBlock(x,z);if(c==null || c.mods.Count>0 || c.blockEntities.Count>0) continue;
                bool overlap=false;foreach(var s in state.sites) if((s.position-new Vector3(x,s.position.y,z)).sqrMagnitude<400) overlap=true;
                if(overlap) continue;
                int surface=World.TopSurfaceY(x,z);
                for(int y=Mathf.Min(World.maxY-8,surface+1);y>=surface-1;y--)
                {
                    Vector3 p=new Vector3(x+0.5f,y,z+0.5f);float size=kind=="open_bell"?5:kind=="survey_cairn"?1.8f:3;
                    if(!World.CanSeeSky(Int3.Floor(p+Vector3.up)) || !SiteFootprint(World,p,size,4))continue;
                    state.sites.Add(new HorrorSite{id="quiet-site-"+kind,kind=kind,position=p,dimension=0});
                    Log("overlay provision "+kind+" without block edits");return;
                }
            }
        }
        public HorrorSite NearestSite(bool next=false)
        {
            HorrorSite best=null;float dist=float.MaxValue;
            foreach(var s in state.sites)
            {
                if(s.dimension!=(int)World.dim || next && s.kind!="open_bell" && s.suppressed) continue;
                if(next && state.stage<3 && s.kind=="open_bell") continue;
                float d=(s.position-Player.position).sqrMagnitude;if(d<dist){dist=d;best=s;}
            }
            return best;
        }
        public static bool SiteFootprint(World w,Vector3 p,float size,float tall)
        {
            if(!CanStand(w,p,size,tall,false) || !CanStand(w,p,.5f,tall))return false;
            for(int x=Mathf.FloorToInt(p.x-size/2);x<=Mathf.FloorToInt(p.x+size/2);x++)for(int z=Mathf.FloorToInt(p.z-size/2);z<=Mathf.FloorToInt(p.z+size/2);z++)
            {
                bool floor=false;for(int y=Mathf.FloorToInt(p.y)-1;y>=p.y-4;y--){var b=w.GetBlock(x,y,z);if(b.isLiquid)return false;if(b.solid){floor=true;break;}}
                if(!floor)return false;
            }
            return true;
        }
        public bool RelocateBlockedNext()
        {
            if(Player.IsCreative)return false;
            var site=NearestSite(true);if(site==null || !World.IsLoaded(Int3.Floor(site.position)))return false;
            float size=site.kind=="open_bell"?5:site.kind=="survey_cairn"?1.8f:3;
            if(SiteFootprint(World,site.position,size,4) && World.CanSeeSky(Int3.Floor(site.position+Vector3.up))){gm.hud.Chat("The next source is still accessible. No relocation needed.");return false;}
            if(site.suppressed)return false;
            state.sites.Remove(site);if(props.TryGetValue(site.id,out var old) && old!=null)UnityEngine.Object.Destroy(old);props.Remove(site.id);propShapes.Remove(site.id);
            gm.hud.Chat("Blocked source withdrawn without touching your blocks. Explore open ground; the journal will locate its replacement.");Save();return true;
        }
        public bool InteractSite()
        {
            var s=NearestSite();if(s==null || (s.position-Player.position).sqrMagnitude>36 || !World.HasLineOfSight(Player.EyePosition,s.position+Vector3.up)) return false;
            Learn(s.kind);
            if(!Player.IsCreative)s.visited=true;
            string rule=s.kind=="listening_well"?"sound":s.kind=="shutter_chapel"?"reveal":s.kind=="root_archive"?"bind":"refuge";
            Learn(rule,true);
            gm.hud.Chat(HorrorJournal.RuleText(rule));
            if(Player.IsCreative){previewSuppressed.Add(s.id);gm.hud.Chat("Preview source settled; Survival milestones unchanged.");return true;}
            if(s.kind=="survey_cairn" && !s.suppliesClaimed)
            {
                s.suppliesClaimed=true;
                string[] ids={"iron_ingot","copper_ingot","stick","string","charcoal","flint","bone_meal","glass","cooked_beef"};int[] amounts={6,3,2,4,2,1,1,1,8};
                for(int i=0;i<ids.Length;i++)Player.inventory.AddOrDrop(new ItemStack(ids[i],amounts[i]));
                gm.hud.Chat("Surveyor's sealed cache recovered. Its rations and materials can prepare the first expedition. Every component also has a normal recipe/source.");
                Learn("cache");
            }
            if(s.kind=="open_bell") {gm.hud.Chat("Use a Bell Key here once the three sources are settled. Lure, reveal, bind, strike.");return true;}
            if(!s.suppressed)
            {
                s.suppressed=true;state.RaiseAttention(14,"settled "+s.kind);state.Recover(900);
                foreach(var actor in Active())if(!actor.final && (actor.home-s.position).sqrMagnitude<1024)actor.Remove();
                if(!Player.IsCreative) Player.inventory.AddOrDrop(new ItemStack("horror:resonant_splinter",2));
                int done=0;foreach(var site in state.sites) if(site.suppressed && site.kind!="survey_cairn") done++;
                state.stage=done;
                gm.hud.Chat("The source settles. Local threats cease; its record remains available.");
                HorrorAudio.Play("settle",s.position,0.55f);
            }
            else gm.hud.Chat("Settled source: the clue can be read again. Lost components have recipes.");
            Save();return true;
        }
        public bool ReadCairn()
        {
            var s=NearestSite();return s!=null && s.kind=="survey_cairn" && InteractSite();
        }
        public bool SummonFinal()
        {
            var s=NearestSite();if(s==null || s.kind!="open_bell" || (s.position-Player.position).sqrMagnitude>64) {gm.hud.Chat("The key answers only at the Open Bell.");return false;}
            bool preview=Player.IsCreative;
            if(!preview && (state.defeated || state.stage<3)) {gm.hud.Chat(state.defeated?"The real Unseam is dead. Free play continues.":"Settle the Well, Chapel and Archive first.");return false;}
            foreach(var m in Active()) if(m.kind=="unseam" && !m.removed)
            {if(!m.final && !m.preview && !preview){m.Remove();state.boss.active=false;}else {gm.hud.Chat("The Unseam is already present.");return false;}}
            float checkpointHealth=!preview && state.boss.active && state.boss.final?state.boss.health:120;
            Vector3 spawn=s.position+new Vector3(0,0,10);
            if(!CanStand(World,spawn,3.2f,5.6f)) {if(!TrySpawn("unseam",preview,out var alt)) {gm.hud.Chat("Clear an open approach around the Bell and try again.");return false;}alt.final=true;alt.home=s.position;spawn=alt.position;}
            else {var boss=MobRegistry.Spawn(World,"horror:unseam",spawn,SpawnReason.Summon) as HorrorMob;boss.preview=preview;boss.final=true;boss.home=s.position;}
            if(!preview) {foreach(var actor in Active())if(actor.kind=="unseam" && actor.final && !actor.preview)actor.health=checkpointHealth;state.boss.active=true;state.boss.final=true;state.boss.position=spawn;state.boss.dimension=(int)World.dim;state.boss.health=checkpointHealth;state.recoveryUntil=state.clock+160;state.RaiseAttention(20,"opened the Bell");Save();}
            gm.hud.SetTitle("THE UNSEAM","Lure its strike. Reveal the shutters. Bind, then attack.");return true;
        }
        public void Victory(HorrorMob boss)
        {
            if(boss.preview || Player.IsCreative || state.defeated || !boss.final) {gm.hud.Chat("Preview death: campaign and rewards unchanged.");return;}
            state.defeated=true;state.boss.active=false;state.attention=0;state.Recover(3600);
            foreach(var s in state.sites) s.suppressed=true;
            state.rewardGranted=true;
            ClaimReward();foreach(var actor in Active())if(actor!=boss)actor.Remove();HorrorAudio.Stop();Save();
            gm.hud.SetTitle("THE QUIET RETURNS","The Unseam is dead. This world is yours again.");
        }
        public void ClaimReward()
        {
            if(!state.rewardGranted || state.rewardClaimed || Player.IsCreative) return;
            if(Player.inventory.Add(new ItemStack("horror:quiet_heart"))) state.rewardClaimed=true;
            else gm.hud.Chat("Reward reserved. Make an empty inventory slot and use the field journal to claim it.");
        }
        void TickWard()
        {
            bool valid=false;
            foreach(var ward in Wards)
            {
                if(ward.dimension!=(int)World.dim || !World.IsLoaded(Int3.Floor(ward.position))) continue;
                if(ward.charge>0) ward.charge=Math.Max(0,ward.charge-10);
                bool now=ward.charge>0 && RoomValid(World,ward.position);
                if(ward.wasValid && !now && (ward.position-Player.position).sqrMagnitude<36)
                {protectionWarning=100;ward.invalidSince=state.clock;gm.hud.Chat("Ward boundary opened / charge lost: 5 seconds to restore it or leave.");}
                ward.wasValid=now;
                if(now && (ward.position-Player.position).sqrMagnitude<36) valid=true;
                if(now && ward.charge==300) gm.hud.Chat("Ward charge low: 15 seconds remain. Recharge with coal and the tuning fork.");
            }
            refugeValid=valid;
            if(valid) {state.tension=Mathf.Max(0,state.tension-0.6f);if(state.clock%100==0)gm.hud.ShowActionBar("Ward refuge secure | J: Field Journal");}
        }
        public bool IsProtected(Vector3 p)
        {
            foreach(var ward in Wards) if(ward.dimension==(int)World.dim && ward.charge>0 && (p-ward.position).sqrMagnitude<36 && RoomValid(World,ward.position)) return true;
            return false;
        }
        public static bool RoomValid(World w,Vector3 pos)
        {
            if(!w.IsLoaded(Int3.Floor(pos))) return false;
            // Flood connected air cells, using the same collision shapes as creature attacks.
            // A corner opening, floor hole or open door therefore breaks the circuit.
            var start=Int3.Floor(pos);var seen=new HashSet<Int3>();var todo=new Queue<Int3>();
            var offsets=new[]{new Int3(1,0,0),new Int3(-1,0,0),new Int3(0,1,0),new Int3(0,-1,0),new Int3(0,0,1),new Int3(0,0,-1)};
            seen.Add(start);todo.Enqueue(start);
            while(todo.Count>0)
            {
                var cell=todo.Dequeue();
                if(!w.IsLoaded(cell) || w.GetBlock(cell).isLiquid || seen.Count>384)return false;
                if(Mathf.Abs(cell.x-start.x)>5 || Mathf.Abs(cell.z-start.z)>5 || cell.y<start.y || cell.y>start.y+3)return false;
                foreach(var offset in offsets)
                {
                    var next=cell+offset;if(seen.Contains(next) || !w.HasLineOfSight(cell.Center,next.Center))continue;
                    seen.Add(next);todo.Enqueue(next);
                }
            }
            return seen.Count>=2;
        }
        public bool PlaceWard(Vector3 pos)
        {
            if(!CanStand(World,pos,0.5f,0.8f) || Wards.Count>=16) {gm.hud.Chat("Ward needs a dry, clear floor; limit 16 placed wards.");return false;}
            foreach(var ward in Wards) if((ward.position-pos).sqrMagnitude<1 && ward.dimension==(int)World.dim) return false;
            Wards.Add(new WardData{id="ward-"+state.clock+"-"+Wards.Count,position=pos,dimension=(int)World.dim});
            Learn("refuge",true);gm.hud.Chat("Ward placed. A roof and closed solid boundaries within 5 blocks are required. Coal recharges it.");Save();return true;
        }
        public bool RechargeWard()
        {
            foreach(var ward in Wards)
                if(ward.dimension==(int)World.dim && (ward.position-Player.position).sqrMagnitude<16)
                {
                    if(Player.sneaking){Wards.Remove(ward);Player.inventory.AddOrDrop(new ItemStack("horror:ward_lantern"));gm.hud.Chat("Ward recovered. Place it again to establish a new circuit.");Save();return true;}
                    if(!Player.IsCreative && !ConsumeInventory("coal",1) && !ConsumeInventory("charcoal",1)) {gm.hud.Chat("Recharge needs coal or charcoal in your inventory.");return true;}
                    ward.charge=12000;gm.hud.Chat("Ward recharged for ten minutes of active play.");Save();return true;
                }
            return false;
        }
        public bool ConsumeInventory(string id,int count)
        {
            for(int i=0;i<Player.inventory.main.Length;i++) {var s=Player.inventory.main[i];if(s!=null && s.item.id==id && s.count>=count){s.count-=count;Player.inventory.Cleanup();return true;}}
            return false;
        }
        void UpdateProps()
        {
            var expected=new HashSet<string>();
            foreach(var s in state.sites) if(s.dimension==(int)World.dim && (s.position-Player.position).sqrMagnitude<16384 && World.IsLoaded(Int3.Floor(s.position)))
            {expected.Add(s.id);MakeProp(s.id,s.kind,s.position,s.suppressed || previewSuppressed.Contains(s.id));}
            foreach(var w in Wards) if(w.dimension==(int)World.dim && (w.position-Player.position).sqrMagnitude<16384 && World.IsLoaded(Int3.Floor(w.position)))
            {expected.Add(w.id);MakeProp(w.id,"ward_lantern",w.position,w.wasValid);}
            var remove=new List<string>();foreach(var pair in props) if(!expected.Contains(pair.Key)){if(pair.Value!=null)UnityEngine.Object.Destroy(pair.Value);remove.Add(pair.Key);}
            foreach(var id in remove){props.Remove(id);propShapes.Remove(id);}
        }
        void MakeProp(string id,string model,Vector3 position,bool calm)
        {
            if(!props.TryGetValue(id,out var obj) || obj==null)
            {
                obj=HorrorVisual.Create(model);if(obj==null)return;obj.transform.position=position;props[id]=obj;
                if(model=="open_bell" || model=="root_archive" || model=="shutter_chapel" || model=="listening_well")foreach(var baseOffset in new[]{new Vector3(-1.7f,0,0),new Vector3(1.7f,0,0),new Vector3(0,0,-2.1f),new Vector3(0,0,2.1f)})
                {
                    var offset=baseOffset*(model=="open_bell"?1:.55f);
                    Vector3 top=position+offset;int floor=Mathf.FloorToInt(position.y)-1;
                    while(floor>position.y-5 && !World.GetBlock(Mathf.FloorToInt(top.x),floor,Mathf.FloorToInt(top.z)).solid)floor--;
                    float length=position.y-floor-1;if(length<=.05f)continue;
                    var support=HorrorVisual.Create("site_support");if(support==null)continue;support.transform.SetParent(obj.transform);support.transform.position=new Vector3(top.x,floor+1,top.z);support.transform.localScale=new Vector3(1,length,1);
                }
                if(model!="ward_lantern")
                {
                    var shapes=new List<AABB>();
                    foreach(var renderer in obj.GetComponentsInChildren<Renderer>())
                    {
                        string name=renderer.name;
                        if(!(name.Contains("wall") || name.Contains("floor") || name.Contains("shelf") || name.Contains("gate") || name.Contains("lintel") || name.Contains("beam") || name.Contains("dais") || name.Contains("well_course") || name.Contains("cairn_layer") || name.Contains("support_")))continue;
                        var bounds=renderer.bounds;shapes.Add(new AABB(bounds.min,bounds.max));
                    }
                    propShapes[id]=shapes;
                }
            }
            HorrorVisual.SetGlow(obj,calm?new Color(0.12f,0.65f,0.62f):new Color(0.7f,0.24f,0.08f));
        }
        public void FlashMark(Vector3 pos,int seconds)
        {
            var go=HorrorVisual.Create("resonant_splinter");if(go==null)return;go.transform.position=pos;go.transform.localScale=Vector3.one*0.5f;temporary.RemoveAll(e=>e==null);temporary.Add(go);UnityEngine.Object.Destroy(go,seconds);
        }
    }
}
