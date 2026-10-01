using UnityEngine;

namespace MCR.Horror
{
    public sealed class HorrorItem:Item
    {
        public readonly string kind;
        public HorrorItem(string kind){this.kind=kind;heldModel=HeldModel.Custom;}
        public override UseResult UseOn(ref UseOnContext ctx)
        {
            if(kind=="ward_lantern")
            {
                var h=HorrorRuntime.Current;if(h==null || !h.state.enabled)return UseResult.Fail;
                Vector3 pos=ctx.Adjacent.Center-Vector3.up*0.5f;
                if(!h.PlaceWard(pos)) return UseResult.Fail;
                if(!ctx.player.IsCreative)ctx.stack.count--;return UseResult.Success;
            }
            return Use(ctx.world,ctx.player,ctx.stack);
        }
        public override UseResult Use(World w,Player p,ItemStack stack)
        {
            var h=HorrorRuntime.Current;if(h==null)return UseResult.Fail;
            if(kind=="field_journal"){h.ClaimReward();GameManager.Instance.hud.Push(new HorrorJournal());return UseResult.Success;}
            if(!h.state.enabled){GameManager.Instance.hud.Chat("Horror is disabled. Enable it in the pause menu / journal.");return UseResult.Fail;}
            bool success=false;
            switch(kind)
            {
                case "listening_compass":
                    var site=h.NearestSite(true);GameManager.Instance.hud.Chat(site==null?"No clear site yet. Explore some open, loaded terrain; the instrument will find one.":HorrorRuntime.SiteName(site.kind)+": "+Mathf.RoundToInt(site.position.x)+", "+Mathf.RoundToInt(site.position.y)+", "+Mathf.RoundToInt(site.position.z));success=true;break;
                case "tuning_fork":success=h.InteractSite() || h.RechargeWard();if(!success)GameManager.Instance.hud.Chat("Use near a resonant site, or a ward with coal in your inventory.");break;
                case "clatter_lure":
                    Vector3 target=p.EyePosition+p.LookDir*15;
                    if(w.RaycastBlocks(p.EyePosition,p.LookDir,15,false,out var hit))target=hit.point-p.LookDir*0.2f;
                    if(!w.IsLoaded(Int3.Floor(target)))return UseResult.Fail;
                    h.Noise(target,36,"lure");h.FlashMark(target,5);HorrorAudio.Play("answer",target,0.6f);success=true;break;
                case "reveal_dust":
                    foreach(var m in h.Active()) if((m.position-p.position).sqrMagnitude<144 && Vector3.Dot(p.LookDir,(m.EyePosition-p.EyePosition).normalized)>0.55f && HorrorRuntime.Sight(w,p.EyePosition,m.EyePosition))success=m.Reveal() || success;
                    if(!success)GameManager.Instance.hud.ShowActionBar("No exposed shutters in sight. First misdirect its strike with a lure.");break;
                case "binding_spool":
                    foreach(var m in h.Active())if((m.position-p.position).sqrMagnitude<100 && HorrorRuntime.Sight(w,p.EyePosition,m.EyePosition))success=m.Bind() || success;
                    if(success)GameManager.Instance.hud.ShowActionBar("BOUND: strike with an ordinary weapon while the shutters stay open.");break;
                case "hush_balm":h.hushTicks=1200;h.Learn("sound",true);success=true;break;
                case "shutter_lens":h.Learn("reveal",true);GameManager.Instance.hud.Chat("Lens experiment: loose pale shutters transmit dust. A false sound can loosen the Unseam's shutters.");success=true;break;
                case "bell_key":success=h.SummonFinal();break;
                case "quiet_heart":if(!p.IsCreative && !h.state.defeated){GameManager.Instance.hud.Chat("This relic requires a real victory.");return UseResult.Fail;}p.Heal(4);h.state.Recover(600);success=true;break;
            }
            if(!success)return UseResult.Fail;
            p.SetCooldown(this,kind=="quiet_heart"?600:kind=="listening_compass"?40:20);
            if(!p.IsCreative && (kind=="clatter_lure" || kind=="reveal_dust" || kind=="binding_spool" || kind=="hush_balm"))stack.count--;
            return UseResult.Success;
        }
    }
    public static class HorrorIcons
    {
        public static Color32[] Pixels(string id)
        {
            var pixels=new Color32[256];var ivory=new Color32(212,208,172,255);var dark=new Color32(35,51,54,255);var copper=new Color32(161,102,52,255);var blue=new Color32(89,202,194,255);
            bool egg=id.EndsWith("_spawn_egg");int kind=System.Array.IndexOf(HorrorRegistry.Tools,id.Replace("horror:",""));
            for(int y=1;y<15;y++)for(int x=1;x<15;x++)
            {
                bool fill=egg?((x-7.5f)*(x-7.5f)/23+(y-8f)*(y-8f)/39<1):kind==0?x>=3 && x<=12 && y>=2 && y<=13:kind==1 || kind==9?(x-8)*(x-8)+(y-8)*(y-8)<38:kind==2 || kind==10?((x==5 || x==10)&&y<8 || x>=5 && x<=10 && y==8 || x==7 && y>=8):kind==3?(x>=4 && x<=11 && y>=4 && y<=12 || x>=6 && x<=9 && y==2):kind==6?x>=4 && x<=11 && y>=3 && y<=12:((x-8)*(x-8)+(y-8)*(y-8)<25 || x>=6 && x<=9 && y<5);
                if(fill)pixels[y*16+x]=egg?((x*7+y*11)%5==0?ivory:dark):((x+y)%7==0?blue:x==4 || y==3?copper:ivory);
            }
            // Unique icon accent marks distinguish functional tools and each species.
            uint hash=(uint)Hash.StringHash(id);
            for(int i=0;i<4;i++){int x=5+i*2;pixels[(9+(int)(hash>>(i*2)&1))*16+x]=blue;}
            return pixels;
        }
    }
}
