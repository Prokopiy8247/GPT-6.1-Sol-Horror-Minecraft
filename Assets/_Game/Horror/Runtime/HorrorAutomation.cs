using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MCR.Horror
{
    // Reachable only through the existing explicit command-line AutoTest, on disposable saves.
    // No production journal/debug victory button calls this adapter.
    public sealed class HorrorAutomation:MonoBehaviour
    {
        [Serializable] public class Results {public string task,save;public bool done,success;public List<string> checks=new List<string>();public int attacks,uses;public float startHealth,endHealth;}
        Results result;GameManager gm;HorrorRuntime H=>gm.horror;Player P=>gm.player;World W=>gm.ActiveWorld;
        string output;bool failed;
        class Pilot:Mob {}
        Pilot pilot;
        public static void Run(GameManager gm,string args)
        {
            if(AutoTest.Instance==null || gm.session==null || gm.session.worldName!="AutoTest")throw new InvalidOperationException("Horror automation requires an explicit disposable AutoTest save");
            var words=args.Split(new[]{' '},2);string task=words[0];
            if(task=="journal"){gm.hud.Push(new HorrorJournal());return;}
            if(task=="search"){CreativeScreen.LastTab=0;CreativeScreen.LastSearch="horror:";gm.hud.Push(new CreativeScreen());return;}
            var adapter=gm.gameObject.AddComponent<HorrorAutomation>();adapter.gm=gm;adapter.output=words.Length>1?words[1]:"Tools/_out/horror-runtime.json";
            adapter.result=new Results{task=task,save=gm.session.save.dir,startHealth=gm.player.health};
            if(task=="suite")adapter.StartCoroutine(adapter.Suite());else if(task=="route")adapter.StartCoroutine(adapter.Route());else if(task=="checkpoint")adapter.StartCoroutine(adapter.Checkpoint());else if(task=="eggs")adapter.StartCoroutine(adapter.SurvivalEggs());else throw new ArgumentException("Unknown horror test "+task);
        }
        void Check(bool ok,string message){result.checks.Add((ok?"PASS ":"FAIL ")+message);Debug.Log("[HorrorTest] "+result.checks[result.checks.Count-1]);if(!ok)failed=true;}
        void Finish(){result.done=true;result.success=!failed;result.endHealth=P.health;Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,JsonUtility.ToJson(result,true));Debug.Log("[HorrorTest] FINISHED "+result.task+" success="+result.success);Destroy(this);}
        IEnumerator Ticks(int count){long until=gm.TicksRun+count;while(gm.TicksRun<until)yield return null;}
        void Aim(Vector3 point){Vector3 d=point-P.EyePosition;P.yaw=Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg;P.prevYaw=P.yaw;P.pitch=-Mathf.Atan2(d.y,new Vector2(d.x,d.z).magnitude)*Mathf.Rad2Deg;}
        ItemStack Select(string id)
        {
            for(int i=0;i<P.inventory.main.Length;i++)if(P.inventory.main[i]!=null && P.inventory.main[i].item.id==id && P.inventory.main[i].count>0)
            {var swap=P.inventory.main[0];P.inventory.main[0]=P.inventory.main[i];P.inventory.main[i]=swap;P.inventory.selected=0;return P.MainHand;}
            return null;
        }
        bool Use(string id,Vector3 point)
        {
            var stack=Select(id);if(stack==null || P.OnCooldown(stack.item))return false;Aim(point);var used=stack.item.Use(W,P,stack);P.inventory.Cleanup();if(used==UseResult.Success){result.uses++;return true;}return false;
        }
        bool Craft(string id)
        {
            foreach(var recipe in MCR.Recipes.Crafting.Where(r=>r.result!=null && r.result.item.id==id))
            {
                var needed=new Dictionary<Item,int>();var ingredients=recipe.AllIngredients();bool possible=true;
                foreach(var ingredient in ingredients)
                {
                    Item choice=null;foreach(var item in ingredient.items)
                    {int count=P.inventory.main.Where(s=>s!=null && s.item==item).Sum(s=>s.count);if(count>(needed.TryGetValue(item,out int n)?n:0)){choice=item;break;}}
                    if(choice==null){possible=false;break;}needed[choice]=(needed.TryGetValue(choice,out int used)?used:0)+1;
                }
                if(!possible)continue;
                var grid=new CraftingGrid(3,3);
                if(recipe is ShapedRecipe shaped)
                {for(int y=0;y<shaped.h;y++)for(int x=0;x<shaped.w;x++){var ing=shaped.pattern[y*shaped.w+x];if(ing!=null)grid.items[y*3+x]=new ItemStack(needed.Keys.First(i=>ing.items.Contains(i)),1);}}
                else {for(int i=0;i<ingredients.Count;i++)grid.items[i]=new ItemStack(needed.Keys.First(it=>ingredients[i].items.Contains(it)),1);}
                if(!recipe.Matches(grid))continue;
                foreach(var pair in needed)for(int i=0;i<pair.Value;i++)if(!H.ConsumeInventory(pair.Key.id,1))throw new InvalidOperationException("Recipe inventory mismatch");
                P.inventory.AddOrDrop(recipe.Assemble(grid));result.checks.Add("CRAFT "+id+" from acquired ingredients");return true;
            }
            Check(false,"obtainable recipe/inventory for "+id);return false;
        }
        IEnumerator WalkTo(Vector3 goal,int budget=2000,float near=4)
        {
            if(pilot==null){pilot=new Pilot();pilot.Setup(new MobDef{id="test-pilot",speed=.22f,width=.6f,height=1.8f,followRange=36});pilot.world=W;}
            for(int tick=0;tick<budget && (P.position-goal).sqrMagnitude>near*near && !P.dead;tick++)
            {
                pilot.position=P.position;pilot.age++;
                if(pilot.nav.IsDone || tick%40==0)pilot.nav.MoveTo(goal,1);
                pilot.nav.Tick();Vector3 target=pilot.moveTarget??goal;Aim(new Vector3(target.x,P.EyePosition.y,target.z));
                gm.scriptWalkTicks=2;if(target.y>P.position.y+.4f || P.horizontalCollision)gm.scriptJumpTicks=2;
                yield return Ticks(1);
            }
            gm.scriptWalkTicks=0;gm.scriptJumpTicks=0;
            if((P.position-goal).sqrMagnitude>near*near)Check(false,"normal walking route to "+goal+" from "+P.position);
        }
        IEnumerator Suite()
        {
            Check(P.IsCreative,"preview suite in Creative");H.directorPaused=true;H.showcase=true;
            Check(HorrorRegistry.Tools.All(id=>Items.Get("horror:"+id)!=null && !Items.Get("horror:"+id).hiddenInCreative),"12 Creative items searchable");
            Check(HorrorRegistry.Creatures.All(id=>Items.Get("horror:"+id+"_spawn_egg") is HorrorEgg),"7 actual Creative eggs");
            Vector3 center=new Vector3(Mathf.Floor(P.position.x)+.5f,Mathf.Floor(P.position.y)+12,Mathf.Floor(P.position.z)+.5f);
            var baseCell=Int3.Floor(center);
            for(int x=-20;x<=20;x++)for(int z=-20;z<=20;z++){W.SetBlock(baseCell.Offset(x,-1,z),Blocks.Get("stone"));for(int y=0;y<8;y++)W.SetBlock(baseCell.Offset(x,y,z),Blocks.Air);}
            P.Teleport(center+new Vector3(0,0,-8));P.abilities.flying=false;Aim(center+Vector3.up*3);yield return Ticks(10);
            bool victory=H.state.defeated;int stage=H.state.stage;
            foreach(string kind in HorrorRegistry.Creatures)
            {
                var egg=Items.Get("horror:"+kind+"_spawn_egg");var stack=new ItemStack(egg);var ctx=new UseOnContext{world=W,player=P,stack=stack,pos=baseCell.Offset(0,-1,0),face=Dir.Up};
                Check(egg.UseOn(ref ctx)==UseResult.Success,"real egg placement "+kind);yield return Ticks(10);
                var mob=H.Active().Find(m=>m.kind==kind);Check(mob!=null && mob.go!=null && mob.preview,"imported live preview "+kind);
                if(kind=="unseam")
                {
                    ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(output),"unseam-day.png"));
                    var originalTime=gm.session.dayTime;gm.session.dayTime=18000;yield return Ticks(20);Aim(mob.position+Vector3.up*3);ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(output),"unseam-night.png"));gm.session.dayTime=originalTime;
                    mob.Hear(center+new Vector3(0,0,-3),"lure");yield return Ticks(50);Check(mob.lureTicks>0,"lure changes real investigation");
                    Check(mob.Reveal(),"misdirected strike opens shutters");Check(mob.Bind(),"exposed boss can bind");
                    Check(mob.Hurt(DamageSource.PlayerAttack(P),1),"bound preview accepts ordinary damage");
                }
                if(kind=="rattleblind"){mob.Hear(center+new Vector3(5,0,0),"lure");var target=mob.evidencePosition;yield return Ticks(20);Check(mob.evidencePosition==target && mob.lureTicks>0,"Rattleblind follows false sound rather than sight");}
                if(kind=="stillwright"){Aim(mob.EyePosition);var frozen=mob.position;yield return Ticks(20);Check((mob.position-frozen).sqrMagnitude<.1f,"Stillwright stops under actual gaze");Aim(P.EyePosition+Vector3.back*10);yield return Ticks(30);Check((mob.position-frozen).sqrMagnitude>.01f,"Stillwright resumes when gaze leaves");}
                if(kind=="wickdrinker")Check(mob.state==HuntState.Retreat && !mob.hasEvidence,"Wick Drinker retreats from daylight");
                if(kind=="hearthmimic"){Check(mob.Reveal(),"dust exposes mimic");Check(mob.bindTicks>0,"mimic reveal gives safe opening");}
                if(kind=="lintel")
                {for(int x=-10;x<=10;x++)for(int z=-10;z<=10;z++)W.SetBlock(baseCell.Offset(x,7,z),Blocks.Get("stone"));float before=mob.position.y,peak=before;mob.Hear(center+new Vector3(8,0,0),"lure");for(int t=0;t<30;t++){yield return Ticks(1);peak=Mathf.Max(peak,mob.position.y);}Check(peak>before+.3f,"Lintel climbs toward real ceiling: "+before+" -> "+peak+" end "+mob.position+" flying="+mob.def.flying);for(int x=-10;x<=10;x++)for(int z=-10;z<=10;z++)W.SetBlock(baseCell.Offset(x,7,z),Blocks.Air);}
                if(kind=="briarchoir"){P.Teleport(center+new Vector3(0,0,-17));yield return Ticks(20);Check(!mob.hasEvidence && mob.attackTicks==0,"Briar Choir respects territorial exit");P.Teleport(center+new Vector3(0,0,-8));}
                if(mob!=null && mob.go!=null){Aim(mob.position+Vector3.up*(mob.height*.55f));yield return Ticks(2);ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(output),kind+".png"));yield return Ticks(2);}
                H.Dismiss(true);H.showcase=true;H.directorPaused=true;
            }
            Check(H.state.defeated==victory && H.state.stage==stage && !H.state.rewardGranted,"Creative preview never earns campaign victory");
            for(int x=-3;x<=3;x++)for(int z=-3;z<=3;z++)for(int y=0;y<=3;y++)if(Math.Abs(x)==3 || Math.Abs(z)==3 || y==3)W.SetBlock(baseCell.Offset(x,y,z),Blocks.Get("stone"));
            Check(HorrorRuntime.RoomValid(W,center),"closed built refuge flood fill");
            Check(!HorrorRuntime.HasEscape(W,center,.6f,1.8f),"cornered player has no validated escape; attack admission defers");
            W.SetBlock(baseCell.Offset(3,0,0),Blocks.Air);W.SetBlock(baseCell.Offset(3,1,0),Blocks.Air);Check(HorrorRuntime.HasEscape(W,center,.6f,1.8f),"reachable dry opening restores an escape");W.SetBlock(baseCell.Offset(3,0,0),Blocks.Get("stone"));W.SetBlock(baseCell.Offset(3,1,0),Blocks.Get("stone"));
            W.SetBlock(baseCell.Offset(3,0,2),Blocks.Air);Check(!HorrorRuntime.RoomValid(W,center),"off-axis wall gap invalidates refuge");W.SetBlock(baseCell.Offset(3,0,2),Blocks.Get("stone"));
            Check(H.PlaceWard(center),"ward valid placement");P.Teleport(center);yield return Ticks(20);Check(H.refugeValid,"actual ward protects player");
            W.SetBlock(baseCell.Offset(0,3,0),Blocks.Air);yield return Ticks(20);Check(!H.refugeValid && H.protectionWarning>0,"broken roof grants warning before danger");
            Check(H.state.wards.Count==0,"Creative ward preview isolated from saved defenses");
            H.Dismiss(true);H.SetEnabled(false);Check(H.Active().Count==0,"disable removes actors");H.SetEnabled(true);gm.SaveAll();Finish();
        }
        IEnumerator SurvivalEggs()
        {
            P.SetGameMode(GameMode.Survival);H.directorPaused=true;H.showcase=true;
            yield return Ticks(3);
            Vector3 center=new Vector3(Mathf.Floor(P.position.x)+.5f,Mathf.Floor(P.position.y)+12,Mathf.Floor(P.position.z)+.5f);
            var baseCell=Int3.Floor(center);
            for(int x=-24;x<=24;x++)for(int z=-24;z<=24;z++)
            {
                W.SetBlock(baseCell.Offset(x,-1,z),Blocks.Get("stone"));
                for(int y=0;y<9;y++)W.SetBlock(baseCell.Offset(x,y,z),Blocks.Air);
            }
            P.Teleport(center+new Vector3(0,0,-8));P.abilities.flying=false;
            H.state.recoveryUntil=H.state.clock;H.state.tension=0;H.refugeValid=false;H.protectionWarning=0;
            foreach(string kind in HorrorRegistry.Creatures)
            {
                H.Dismiss(false);H.directorPaused=true;H.showcase=true;
                var egg=Items.Get("horror:"+kind+"_spawn_egg") as HorrorEgg;
                var stack=new ItemStack(egg,2);
                var ctx=new UseOnContext{world=W,player=P,stack=stack,pos=baseCell.Offset(0,-1,0),face=Dir.Up};
                Check(egg.UseOn(ref ctx)==UseResult.Success,"Survival egg places "+kind);
                yield return Ticks(3);
                var mob=H.Active().Find(m=>m.kind==kind);
                Check(mob!=null && !mob.preview && mob.ShouldSave && !mob.campaignBoss,"Survival "+kind+" is a real saved actor");
                Check(stack.count==1,"Survival "+kind+" egg is consumed once");
                if(kind=="unseam")Check(mob.final,"egg Unseam uses its combat phases without becoming the campaign boss");
            }
            H.Dismiss(false);H.showcase=false;H.directorPaused=false;Finish();
        }
        IEnumerator Checkpoint()
        {
            Vector3 center=new Vector3(Mathf.Floor(P.position.x)+.5f,Mathf.Floor(P.position.y)+12,Mathf.Floor(P.position.z)+.5f);var cell=Int3.Floor(center);
            for(int x=-20;x<=20;x++)for(int z=-20;z<=20;z++){W.SetBlock(cell.Offset(x,-1,z),Blocks.Get("stone"));for(int y=0;y<8;y++)W.SetBlock(cell.Offset(x,y,z),Blocks.Air);}
            for(int x=-9;x<=9;x++)for(int y=0;y<7;y++)W.SetBlock(cell.Offset(x,y,-5),Blocks.Get("stone"));
            P.Teleport(center+new Vector3(0,0,-12));var boss=MobRegistry.Spawn(W,"horror:unseam",center,SpawnReason.Summon) as HorrorMob;
            H.state.boss.active=true;boss.Hear(center+new Vector3(8,0,2),"lure");yield return Ticks(10);gm.SaveAll();
            var loaded=HorrorSave.Read(gm.session.save.dir);Check(loaded.boss.active && loaded.boss.health==boss.health && !loaded.boss.final,"live campaign actor checkpoint saved");Check(loaded.memory.Count>0,"actual heard evidence persisted");Check(!HorrorRuntime.Sight(W,P.EyePosition,boss.EyePosition),"real cover blocks flagship sight");Finish();
        }
        IEnumerator Route()
        {
            Check(!P.IsCreative && P.inventory.main.All(s=>s==null || s.IsEmpty),"fresh Survival; empty inventory; no setup grants");
            // Walk the normal world, obtain the ordinary cairn cache, craft through existing recipes,
            // settle sources using the normal item methods, and defeat with normal Player.Attack.
            for(int i=0;i<400 && H.state.sites.Count<5;i++)yield return Ticks(1);
            Check(H.state.sites.Count==5,"five non-destructive sources provisioned");if(failed){Finish();yield break;}
            var cairn=H.state.sites.Find(s=>s.kind=="survey_cairn");yield return WalkTo(cairn.position);Check(H.ReadCairn(),"read reachable cairn and obtain real finite cache");
            if(failed){Finish();yield break;}
            Check(Craft("horror:tuning_fork") && Craft("iron_sword") && Craft("horror:clatter_lure") && Craft("horror:reveal_dust") && Craft("horror:binding_spool") && Craft("horror:binding_spool"),"prepare equipment with ordinary ingredients");
            foreach(string kind in new[]{"listening_well","shutter_chapel","root_archive"})
            {var site=H.state.sites.Find(s=>s.kind==kind);yield return WalkTo(site.position);Check(Use("horror:tuning_fork",site.position+Vector3.up),"settle "+kind+" through normal fork use");yield return Ticks(21);if(failed || P.dead){Finish();yield break;}}
            Check(H.state.stage==3 && H.state.confirmed.Contains("sound") && H.state.confirmed.Contains("reveal") && H.state.confirmed.Contains("bind"),"investigation taught all final counterplay");
            Check(Craft("horror:shutter_lens") && Craft("horror:bell_key"),"prepare renewable Bell access");var bell=H.state.sites.Find(s=>s.kind=="open_bell");yield return WalkTo(bell.position);Check(Use("horror:bell_key",bell.position+Vector3.up),"summon actual final body");yield return Ticks(180);
            if(failed){Finish();yield break;}
            var boss=H.Active().Find(m=>m.kind=="unseam" && m.final && !m.preview);Check(boss!=null,"one real final boss");if(boss==null){Finish();yield break;}
            for(int tick=0;tick<3600 && !boss.dead && !P.dead;tick++)
            {
                float distance=(boss.position-P.position).magnitude;
                if(tick%100==0)Debug.Log("[HorrorRoute] battle "+tick+" player="+P.health+" boss="+boss.health+" distance="+distance+" action="+boss.attackKind+" windup="+boss.attackTicks+" bind="+boss.bindTicks);
                bool dodge=false;
                if(boss.attackTicks>8 && boss.bindTicks<=0)
                {
                    Vector3 from=P.position-boss.attackPoint;from.y=0;
                    if(boss.attackKind==1)
                    {
                        Vector3 line=boss.attackPoint-boss.position;line.y=0;line.Normalize();Vector3 rel=P.position-boss.position;rel.y=0;float along=Vector3.Dot(rel,line);
                        if(along>-.5f && along<8 && (rel-line*along).sqrMagnitude<4){var side=new Vector3(-line.z,0,line.x);Aim(P.EyePosition+side*5);dodge=true;}
                    }
                    else if(from.sqrMagnitude<(boss.attackKind==2?25:9)){if(from.sqrMagnitude<.1f)from=Vector3.right;Aim(P.EyePosition+from.normalized*5);dodge=true;}
                    if(dodge){gm.scriptWalkTicks=2;if(P.horizontalCollision)gm.scriptJumpTicks=2;}
                }
                if(dodge){yield return Ticks(1);continue;}
                if(boss.bindTicks>0 && boss.revealTicks>0)
                {
                    if(distance>4.4f){Aim(boss.EyePosition);gm.scriptWalkTicks=2;if(P.horizontalCollision)gm.scriptJumpTicks=2;}
                    else if(P.AttackStrength(.5f)>.95f && HorrorRuntime.Sight(W,P.EyePosition,boss.position+Vector3.up*1.4f))
                    {Select("iron_sword");Aim(boss.position+Vector3.up*1.4f);if(P.RaycastEntity(P.EntityReach)==boss){gm.interaction.Tick(false,true,false,false,false);result.attacks++;}else gm.scriptWalkTicks=2;}
                }
                else
                {
                    gm.scriptWalkTicks=0;
                    if(distance>9){Aim(boss.EyePosition);gm.scriptWalkTicks=2;}
                    else if(boss.revealTicks>0){if(Use("horror:reveal_dust",boss.EyePosition))yield return Ticks(21);Use("horror:binding_spool",boss.EyePosition);}
                    else if(boss.lureTicks<=0)Use("horror:clatter_lure",boss.position+new Vector3(3.5f,.05f,0));
                }
                if(P.health<17 && P.hunger.food<20 && (boss.bindTicks>75 || distance>9 && boss.attackTicks==0)){var food=Select("cooked_beef");if(food!=null){food.item.Use(W,P,food);P.StartUsing(food);yield return Ticks(34);P.StopUsing();}}
                yield return Ticks(1);
            }
            gm.scriptWalkTicks=0;Check(!P.dead && boss.dead && H.state.defeated,"actual Survival boss death with learned counterplay and ordinary sword; no health/kill/victory command");
            Check(H.state.rewardClaimed && P.inventory.main.Any(s=>s!=null && s.item.id=="horror:quiet_heart"),"one tangible victory reward");gm.SaveAll();var loaded=HorrorSave.Read(gm.session.save.dir);Check(loaded.defeated && !loaded.boss.active && loaded.rewardClaimed,"victory survives save/load");yield return Ticks(60);Check(H.Active().Count==0 && H.state.sites.All(s=>s.suppressed),"world recovery and free play continue");Finish();
        }
    }
}
