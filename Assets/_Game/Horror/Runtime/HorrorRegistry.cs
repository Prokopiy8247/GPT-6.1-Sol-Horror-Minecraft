using System;
using System.Collections.Generic;
using UnityEngine;

namespace MCR.Horror
{
    public static class HorrorRegistry
    {
        public static readonly string[] Creatures={"unseam","rattleblind","lintel","hearthmimic","wickdrinker","stillwright","briarchoir"};
        public static readonly string[] Names={"The Unseam","Rattleblind","Lintel","Hearth Mimic","Wick Drinker","Stillwright","Briar Choir"};
        public static readonly string[] Tools={"field_journal","listening_compass","tuning_fork","ward_lantern","clatter_lure","reveal_dust","binding_spool","hush_balm","resonant_splinter","shutter_lens","bell_key","quiet_heart"};
        public static readonly string[] ToolNames={"Field Journal","Listening Compass","Tuning Fork","Ward Lantern","Clatter Lure","Reveal Dust","Binding Spool","Hush Balm","Resonant Splinter","Shutter Lens","Bell Key","Quiet Heart"};
        public static readonly string[] Descriptions={"Use to read discoveries; knowledge survives loss","Use to find the next resonant site","Use at a source to settle it; coal recharges wards","Place inside a roofed, closed room; radius 6","Throw to make a real false sound trail","Expose a mimic or open boss shutters","Bind an exposed creature for a damage window","Quiet steps for 60 seconds","Renewable source of resonant craft material","Inspect hidden seams and record revealing rules","Call the real Unseam at the Open Bell","Victory relic: heal and quiet the local hunt"};
        static bool registered, recipes;
        static readonly float[] Widths={3.2f,1.7f,1.8f,1.6f,1.2f,1.0f,1.9f};
        static readonly float[] Heights={5.6f,1.8f,2.6f,1.4f,2.3f,3.0f,2.9f};
        public static void Register()
        {
            if(registered) return; registered=true;
            for(int i=0;i<Creatures.Length;i++)
            {
                string kind=Creatures[i];
                var d=new MobDef {id="horror:"+kind,displayName=Names[i],model=kind,soundId=kind,category=MobCategory.Misc,hostile=true,
                    maxHealth=i==0?120:i==6?28:18,width=Widths[i],height=Heights[i],flying=i==2,noGravity=i==2,flySpeed=.18f,
                    speed=i==0?0.24f:0.22f,attackDamage=i==0?6:3,followRange=36,ambientInterval=400,xp=i==0?50:5,
                    eggBase=new Color32((byte)(35+i*14),(byte)(50+i*7),(byte)(56+i*5),255),eggSpots=new Color32(166,202,179,255),
                    factory=()=>new HorrorMob(kind),boss=i==0,hasEgg=true};
                MobRegistry.RegisterExtension(d);
            }
            for(int i=0;i<Tools.Length;i++)
            {
                bool consume=i>=4 && i<=7;
                var it=new HorrorItem(Tools[i]) {displayName=ToolNames[i],description=Descriptions[i],maxStack=consume?16:1,
                    tab=i==8?CreativeTab.Ingredients:CreativeTab.Tools,rarity=Rarity.Uncommon,modelName=Tools[i],iconName="horror:"+Tools[i]};
                if(i==8) it.maxStack=64;
                Items.Reg("horror:"+Tools[i],it);
            }
            foreach(string kind in Creatures) Items.Reg("horror:"+kind+"_spawn_egg",new HorrorEgg(kind) {displayName=Names[Array.IndexOf(Creatures,kind)]+" Spawn Egg",iconName="horror:"+kind+"_spawn_egg"});
            Debug.Log("[Horror] Registered 12 functional items and 7 spawn eggs after baseline IDs");
        }
        public static void Recipes()
        {
            if(recipes) return;recipes=true;
            MCR.Recipes.Shapeless("horror:field_journal",1,"book","charcoal");
            MCR.Recipes.Shapeless("horror:listening_compass",1,"compass","copper_ingot");
            MCR.Recipes.Shapeless("horror:tuning_fork",1,"iron_ingot","copper_ingot","stick");
            MCR.Recipes.Shapeless("horror:ward_lantern",1,"lantern","copper_ingot","charcoal");
            MCR.Recipes.Shapeless("horror:clatter_lure",4,"copper_ingot","flint");
            MCR.Recipes.Shapeless("horror:reveal_dust",4,"bone_meal","charcoal");
            MCR.Recipes.Shapeless("horror:binding_spool",2,"string","string","iron_ingot");
            MCR.Recipes.Shapeless("horror:hush_balm",2,"clay_ball","wheat");
            MCR.Recipes.Shapeless("horror:resonant_splinter",2,"copper_ingot","charcoal");
            MCR.Recipes.Shapeless("horror:shutter_lens",1,"glass","horror:resonant_splinter");
            MCR.Recipes.Shapeless("horror:bell_key",1,"horror:resonant_splinter","horror:shutter_lens","iron_ingot");
        }
    }
    public sealed class HorrorEgg : SpawnEggItem
    {
        public readonly string kind;
        public HorrorEgg(string kind):base("horror:"+kind) {this.kind=kind;}
        public override UseResult UseOn(ref UseOnContext ctx)
        {
            if(!ctx.player.IsCreative) {GameManager.Instance?.hud.Chat("Horror spawn eggs are Creative previews.");return UseResult.Fail;}
            var p=ctx.Adjacent.Center-Vector3.up*0.5f;
            var d=MobRegistry.Get(mobId);
            if(!HorrorRuntime.CanStand(ctx.world,p,d.width,d.height))
            {GameManager.Instance?.hud.Chat("No clear loaded space. The Unseam needs a wide, tall opening.");return UseResult.Fail;}
            var mob=MobRegistry.Spawn(ctx.world,mobId,p,SpawnReason.SpawnEgg) as HorrorMob;
            if(mob==null) return UseResult.Fail;
            mob.preview=true;mob.final=kind=="unseam";
            return UseResult.Success;
        }
        public override UseResult Use(World w,Player p,ItemStack s)=>UseResult.Pass;
    }
}
