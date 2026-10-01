using System.Collections.Generic;
using UnityEngine;

namespace MCR.Horror
{
    /// <summary>Real voxel-colliding entities; movement/navigation authority remains the base entity controller.</summary>
    public sealed class HorrorMob : Mob
    {
        public readonly string kind;
        public bool preview, final;
        public HuntState state=HuntState.Observe;
        public Vector3 home, evidencePosition, attackPoint;
        public bool hasEvidence;
        public int evidenceTicks, searchTicks, attackTicks, revealTicks, bindTicks, lureTicks, lostTicks;
        public int phase, attackKind;
        public bool sawPlayer, attackHit;
        public float posture;
        HorrorVisual horrorVisual;
        int replan;
        public HorrorMob(string kind) {this.kind=kind;}
        public override bool ShouldSave=>false; // One campaign boss checkpoint/memory lives in the sidecar.
        public override bool CanDespawn=>false;
        public override string AmbientSound=>null;
        public override string HurtSound=>null;
        public override string DeathSound=>null;
        public override void OnInitialSpawn(SpawnReason reason)
        {
            home=position;preview=reason==SpawnReason.SpawnEgg || reason==SpawnReason.Command;
            if(preview) final=kind=="unseam";
            persistent=true;
        }
        public override void CreateVisual()
        {
            horrorVisual=new HorrorVisual(this);go=horrorVisual.root;
        }
        public override void Render(float partial) {horrorVisual?.Update(partial);}
        public void Hear(Vector3 pos,string source)
        {
            if(kind=="stillwright" || kind=="briarchoir" || dead || removed) return;
            var h=HorrorRuntime.Current;if(h==null) return;
            evidencePosition=pos;hasEvidence=true;evidenceTicks=source=="lure"?400:160;lostTicks=0;
            if(source=="lure") {lureTicks=300;state=HuntState.Investigate;h.Learn("sound",true);}
            else if(state!=HuntState.Pursue && attackTicks<=0) state=HuntState.Investigate;
            if(kind=="unseam" && !preview) h.state.Remember(source=="lure"?EvidenceSource.Lure:EvidenceSource.Sound,pos,(int)world.dim,source=="lure"?1:0.75f);
        }
        protected override void OnHurtBy(LivingEntity attacker)
        {
            // Provocation is detectable evidence, not permission to follow hidden live positions forever.
            if(attacker!=null && HorrorRuntime.Sight(world,EyePosition,attacker.EyePosition)) Hear(attacker.position,"impact");
        }
        protected override void AiStep()
        {
            var h=HorrorRuntime.Current;
            if(h==null || !h.state.enabled || preview && !h.Player.IsCreative) {Remove();return;}
            var p=h.Player;
            if(dead) return;
            if(!preview && !final && age>2400){if(kind=="unseam")h.state.boss.active=false;h.state.Recover(900);Remove();return;}
            if(kind=="briarchoir" && (p.position-home).sqrMagnitude>196){hasEvidence=false;evidenceTicks=0;attackTicks=0;state=HuntState.Retreat;nav.Stop();base.AiStep();return;}
            if(p.world!=world || p.dead) {nav.Stop();base.AiStep();return;}
            if(revealTicks>0) revealTicks--;if(bindTicks>0) bindTicks--;if(lureTicks>0) lureTicks--;
            if(evidenceTicks>0) evidenceTicks--;if(evidenceTicks==0) hasEvidence=false;
            phase=kind=="unseam" && final ? health>80?0:health>40?1:2 : 0;
            if(bindTicks>0) {state=HuntState.Stagger;nav.Stop();moveForward=0;base.AiStep();return;}
            if(attackTicks>0) {TickAttack(h,p);base.AiStep();return;}
            bool watched=HorrorRuntime.Sight(world,p.EyePosition,position+Vector3.up*height*0.65f) && Vector3.Dot(p.LookDir,(position+Vector3.up*height*0.65f-p.EyePosition).normalized)>0.92f;
            bool canSee=kind!="rattleblind" && (p.position-position).sqrMagnitude<36*36 && HorrorRuntime.Sight(world,EyePosition,p.EyePosition);
            if(kind=="briarchoir" && (p.position-home).sqrMagnitude>196) canSee=false;
            sawPlayer=canSee;
            if(kind=="stillwright" && watched)
            {
                nav.Stop();velocity.x=velocity.z=0;state=HuntState.Observe;
                h.Learn("gaze",true);base.AiStep();return;
            }
            if(kind=="wickdrinker" && world.GetBrightness(p.position)>0.65f || h.IsProtected(position))
            {
                state=HuntState.Retreat;nav.Stop();if(age%20==0)nav.MoveTo(home,0.7f);
                hasEvidence=false;evidenceTicks=0;h.Learn("light",true);base.AiStep();return;
            }
            if(!preview && (p.IsCreative || p.IsSpectator || world.session.difficulty==Difficulty.Peaceful)) {nav.Stop();base.AiStep();return;}
            if(age%4==0 && canSee && lureTicks<=0)
            {
                evidencePosition=p.position;hasEvidence=true;evidenceTicks=120;lostTicks=0;
                state=kind=="unseam" && !final && age<160?HuntState.Stalk:HuntState.Pursue;
                if(kind=="unseam" && !preview)
                {
                    h.state.Remember(EvidenceSource.Sight,p.position,(int)world.dim);
                    if(h.refugeValid) h.state.Remember(EvidenceSource.ObservedShelter,p.position,(int)world.dim,0.7f);
                    if(age%80==0) h.state.RaiseAttention(2,"actual flagship detection");
                }
            }
            else if(!canSee) lostTicks++;
            if(kind=="unseam" && !preview && !hasEvidence)
            {
                var e=h.state.BestEvidence((int)world.dim);
                if(e!=null) {evidencePosition=e.position;hasEvidence=true;evidenceTicks=80;state=HuntState.Search;}
            }
            if(hasEvidence)
            {
                float dist=(evidencePosition-position).magnitude;
                lookTarget=evidencePosition+Vector3.up;
                if(dist>1.8f && replan--<=0) {if(!nav.MoveTo(evidencePosition,state==HuntState.Pursue?1.25f:0.6f))moveTarget=evidencePosition;replan=16;}
                if(kind=="briarchoir")
                {
                    nav.Stop();
                    if((p.position-home).sqrMagnitude>196) {hasEvidence=false;state=HuntState.Retreat;}
                }
                if(kind=="hearthmimic" && age%100==0 && dist>5) Sounds.Play("block.chest.open",position,0.45f);
                if(kind=="hearthmimic" && revealTicks<=0 && dist>4.5f){nav.Stop();state=HuntState.Observe;}
                if(kind=="lintel" && state!=HuntState.Pursue && (p.position-position).sqrMagnitude<144)
                {
                    // Hover only below an actual ceiling; outside caves it folds above its rooted legs.
                    if(world.ClipCollision(position+Vector3.up*height,position+Vector3.up*(height+5),out var roof))
                        moveTarget=new Vector3(position.x,roof.point.y-height-0.1f,position.z);
                }
                float reach=kind=="unseam"?6:kind=="briarchoir"?13:kind=="lintel"?4:3;
                if(dist<reach && attackCooldown<=0 && h.AttackAllowed(this) && (canSee || kind=="rattleblind" || lureTicks>0))
                    BeginAttack(evidencePosition);
                if(dist<1.6f && !canSee && lureTicks<=0)
                {
                    state=HuntState.Search;nav.Stop();
                    if(++searchTicks>100) {hasEvidence=false;evidenceTicks=0;searchTicks=0;state=HuntState.Retreat;
                        if(kind=="unseam" && !preview) {h.state.Remember(EvidenceSource.FailedSearch,position,(int)world.dim,0.2f);h.state.memory.RemoveAll(e=>e.source!=EvidenceSource.FailedSearch);h.state.Recover();h.state.boss.active=false;}
                        h.Log(kind+" unsuccessful search; disengage");}
                }
            }
            else
            {
                nav.Stop();state=HuntState.Observe;
                if(age>1200 && !final && !preview) {if(kind=="unseam"){h.state.boss.active=false;h.state.Recover();}Remove();return;}
            }
            posture=Mathf.MoveTowards(posture,state==HuntState.Pursue?1:0,0.06f);
            if(kind=="unseam" && !preview && h.state.boss.active && age%20==0){h.state.boss.position=position;h.state.boss.health=health;h.state.boss.final=final;h.state.boss.dimension=(int)world.dim;}
            // Conservative collider includes the unfolding head and swinging arms in every state.
            base.AiStep();
        }
        protected override void CustomTravel()
        {
            if(kind=="lintel" && attackTicks>0)moveTarget=attackPoint+Vector3.up*.2f;
            if(kind=="lintel" && attackTicks<=0 && hasEvidence)
            {
                Vector3 point=evidencePosition;
                if(world.ClipCollision(point+Vector3.up*height,point+Vector3.up*(height+6),out var roof))point.y=roof.point.y-height-.12f;
                moveTarget=point;
            }
            base.CustomTravel();
        }
        void BeginAttack(Vector3 point)
        {
            nav.Stop();attackPoint=point;attackHit=false;
            attackKind=kind=="briarchoir"?2:kind=="lintel"?3:kind=="unseam"?phase:0;
            attackTicks=attackKind==2?44:32;state=HuntState.Windup;attackCooldown=70;
            HorrorAudio.Play(kind=="unseam"?"bell":"warning",position,0.6f);
            if(HorrorRuntime.Current.state.shake && !preview)HorrorRuntime.Current.cameraKick=.6f;
            HorrorRuntime.Current.FlashMark(attackPoint,3);
        }
        void TickAttack(HorrorRuntime h,Player p)
        {
            nav.Stop();moveForward=0;attackTicks--;
            int hitTime=attackKind==2?12:8;
            state=attackTicks>hitTime?HuntState.Windup:HuntState.Strike;
            if(attackTicks==hitTime && !attackHit)
            {
                attackHit=true;
                bool inVolume;
                Vector3 delta=p.position-attackPoint;delta.y=0;
                if(attackKind==1)
                {
                    Vector3 direction=attackPoint-position;direction.y=0;direction.Normalize();Vector3 offset=p.position-position;offset.y=0;
                    float along=Vector3.Dot(offset,direction);inVolume=along>0 && along<7 && (offset-direction*along).sqrMagnitude<1.5f;
                }
                else inVolume=delta.sqrMagnitude<(attackKind==2?12.25f:3.24f) && Mathf.Abs(p.position.y-attackPoint.y)<2.5f;
                bool obstruction=!HorrorRuntime.Sight(world,position+Vector3.up*Mathf.Min(height*0.55f,2.5f),p.EyePosition);
                if(inVolume && !obstruction && h.AttackAllowed(this))
                {
                    if(!preview) p.Hurt(DamageSource.MobAttack(this),kind=="unseam"?6:3);
                    else h.Log("preview hit volume "+kind+" action "+attackKind+"; Creative health preserved");
                }
                HorrorAudio.Play("strike",attackPoint,0.55f);
                if(lureTicks>0 && kind=="unseam") {revealTicks=140;h.Learn("sound",true);h.gm.hud.ShowActionBar("The shutters loosen. Reveal, then bind.");}
            }
            if(attackTicks==0) {state=revealTicks>0?HuntState.Stagger:HuntState.Search;h.state.tension=Mathf.Min(100,h.state.tension+5);}
        }
        public bool Reveal()
        {
            if(kind=="unseam" && revealTicks<=0) return false;
            revealTicks=240;
            if(kind=="hearthmimic") {state=HuntState.Stagger;bindTicks=60;}
            HorrorRuntime.Current?.Learn("reveal",true);
            HorrorAudio.Play("reveal",position,0.5f);return true;
        }
        public bool Bind()
        {
            if(revealTicks<=0)return false;
            if(kind=="unseam" && (!final || revealTicks<=0)) return false;
            bindTicks=kind=="unseam"?180:100;state=HuntState.Stagger;nav.Stop();attackTicks=0;
            HorrorRuntime.Current?.Learn("bind",true);return true;
        }
        public override bool Hurt(DamageSource src,float amount)
        {
            var h=HorrorRuntime.Current;
            if(kind=="unseam" && (!final || bindTicks<=0 || revealTicks<=0))
            {
                if(src.attacker is Player) {h?.Learn("shutters");h?.gm.hud.ShowActionBar(final?"Shutters sealed: lure, reveal, then bind.":"Only an echo. The true body waits at the Open Bell.");}
                return false;
            }
            bool result=base.Hurt(src,amount);
            if(result) {HorrorAudio.Play("hurt",position,0.35f);if(kind=="unseam" && !preview){h.state.boss.health=health;h.state.boss.position=position;}}
            return result;
        }
        protected override void OnDeath(DamageSource src)
        {
            state=HuntState.Dead;nav.Stop();HorrorAudio.Play("settle",position,0.6f);
            var h=HorrorRuntime.Current;
            if(kind=="unseam") {h?.Victory(this);return;}
            if(!preview && src.attacker is Player p && !p.IsCreative)
            {p.inventory.AddOrDrop(new ItemStack("horror:resonant_splinter",1));XpOrb.Spawn(world,position,5);h?.state.Recover(900);}
        }
    }
}
