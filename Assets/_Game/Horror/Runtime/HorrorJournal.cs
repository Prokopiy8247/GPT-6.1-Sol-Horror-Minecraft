using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MCR.Horror
{
    public sealed class HorrorInput:MonoBehaviour
    {
        void Update()
        {
            var g=GameManager.Instance;if(g==null || g.horror==null || g.LoadingWorld || !g.Playing || g.hud.AnyScreen)return;
            if(Keyboard.current!=null && Keyboard.current.jKey.wasPressedThisFrame)g.hud.Push(new HorrorJournal());
        }
    }
    public sealed class HorrorJournal:GuiScreen
    {
        int page,entry;
        HorrorRuntime R=>HorrorRuntime.Current;
        public override void OnOpen(){title="THE QUIET BELOW - FIELD JOURNAL";pauseGame=true;}
        public override void Input(Hud h){if(h.pressedEscape)h.Pop();}
        public static string RuleText(string rule)
        {
            switch(rule)
            {
                case "sound":return "CONFIRMED: crouching muffles steps. A clatter lure makes listeners investigate its sound. The Unseam loosens shutters after a misdirected strike.";
                case "reveal":return "CONFIRMED: reveal dust exposes the Hearth Mimic. For the Unseam, loosen shutters with a lure first, then aim dust at them within 12 blocks.";
                case "bind":return "CONFIRMED: binding thread pins an exposed target. At the Bell, reveal then bind: ordinary weapons can hurt the real Unseam for nine seconds.";
                case "refuge":return "CONFIRMED: ward refuge requires a dry floor, roof and closed solid boundaries within five blocks. Radius six; ten-minute charge; coal recharge with a tuning fork.";
                case "light":return "CONFIRMED: the Wick Drinker retreats from bright light. Torches and daylight remain useful.";
                case "gaze":return "CONFIRMED: watch the Stillwright to stop it. Break sight only when you have cover or a route out.";
                case "mining_answer":return "OBSERVED: a second knock answered mining. HYPOTHESIS: something listens from a resonant site.";
                case "shutters":return "OBSERVED: ordinary attacks did not penetrate the shutters. HYPOTHESIS: their opening depends on sound, revealing and binding.";
                case "cache":return "OBSERVED: the surveyor left recoverable supplies in the cairn. Use its iron, copper and sticks for a fork; the records at three sources teach the rest. Lost components remain craftable.";
                case "threshold_trace":return "OBSERVED: a blue trace answered a door. HYPOTHESIS: enclosed rooms interrupt resonant tracks. Test a ward in a closed room.";
                case "ceiling_trace":return "OBSERVED: ivory flecks and scraping above a cave path. HYPOTHESIS: the Lintel uses overhead space. Step out from its marked drop and use cover.";
                case "building_trace":return "OBSERVED: new stone carried a brief blue mark. HYPOTHESIS: these sources do not consume blocks. A dry enclosed shelter can still protect preparation.";
                default:return "OBSERVED: "+HorrorRuntime.SiteName(rule)+". Its source can be read repeatedly with a tuning fork.";
            }
        }
        void TextBlock(string text,float x,float y,float width,Color32 color)
        {
            int chars=Mathf.Max(12,(int)(width/6));string line="";
            foreach(string word in text.Split(' '))
            {if(line.Length+word.Length+1>chars){ui.Text(line,x,y,color);y+=11;line="";}line+=word+" ";}
            if(line.Length>0)ui.Text(line,x,y,color);
        }
        public override void Render(UiRenderer ui,Hud hud)
        {
            if(R==null){hud.Pop();return;}
            Dim(0.62f);float width=Mathf.Min(W-16,430),height=Mathf.Min(H-12,285);float x=(W-width)/2,y=(H-height)/2;
            ui.Rect(x,y,width,height,new Color32(14,27,30,250));ui.Rect(x,y,width,2,new Color32(87,161,151,255));
            ui.Text(title,x+10,y+9,Styles.Text);
            string[] tabs=P.IsCreative?new[]{"Investigation","Equipment","Settings","Showcase"}:new[]{"Investigation","Equipment","Settings"};
            for(int i=0;i<tabs.Length;i++)if(Button(x+10+i*(width-20)/tabs.Length,y+25,(width-24)/tabs.Length,18,tabs[i])){page=i;entry=0;}
            float by=y+52;
            if(page==0)
            {
                var knowledge=R.Knowledge;
                string status=R.state.defeated?"RESOLVED: the Unseam is dead.":"Settle the Well, Chapel and Archive: "+R.state.stage+" / 3";
                ui.Text(P.IsCreative?"CREATIVE PREVIEW - campaign progress is isolated":status,x+10,by,new Color32(108,208,190,255));
                var next=R.NearestSite(true);
                string hint=next==null?"Explore open terrain to find a survey site.":"Next record: "+HorrorRuntime.SiteName(next.kind)+" at "+Mathf.RoundToInt(next.position.x)+", "+Mathf.RoundToInt(next.position.y)+", "+Mathf.RoundToInt(next.position.z)+". Read the cairn here; use a tuning fork at other sites.";
                TextBlock(hint,x+10,by+18,width-20,Styles.Text);
                var entries=knowledge.observed;
                if(entries.Count==0)TextBlock("No creature rules observed yet. Sources retain records; missed knocks cannot lock your journey. Build a real shelter before disturbing sources.",x+10,by+75,width-20,Styles.TextGray);
                else
                {
                    entry=Mathf.Clamp(entry,0,entries.Count-1);ui.Text("Entry "+(entry+1)+" / "+entries.Count,x+10,by+72,Styles.TextGray);
                    TextBlock(RuleText(entries[entry]),x+10,by+88,width-20,Styles.Text);
                    if(Button(x+10,y+height-53,74,18,"Previous"))entry=(entry+entries.Count-1)%entries.Count;
                    if(Button(x+90,y+height-53,74,18,"Next"))entry=(entry+1)%entries.Count;
                }
                if(R.state.rewardGranted && !R.state.rewardClaimed && !P.IsCreative && Button(x+180,y+height-53,140,18,"Claim reserved reward"))R.ClaimReward();
                else if(next!=null && next.kind=="survey_cairn" && Button(x+180,y+height-53,Mathf.Max(90,width-190),18,"Read cairn / recover cache"))R.ReadCairn();
            }
            if(page==1)
            {
                entry=Mathf.Clamp(entry,0,HorrorRegistry.Tools.Length-1);
                ui.Text(HorrorRegistry.ToolNames[entry],x+10,by,new Color32(108,208,190,255));
                ItemIcons.Draw(ui,new ItemStack("horror:"+HorrorRegistry.Tools[entry]),x+10,by+20,16);
                TextBlock(HorrorRegistry.Descriptions[entry],x+36,by+22,width-46,Styles.Text);
                TextBlock(RecipeText(entry),x+10,by+76,width-20,Styles.TextGray);
                if(Button(x+10,y+height-53,74,18,"Previous"))entry=(entry+11)%12;
                if(Button(x+90,y+height-53,74,18,"Next"))entry=(entry+1)%12;
                TextBlock("Recipes use the existing crafting grids. Consumables are renewable. All Horror items and eggs are searchable in Creative with 'horror:'.",x+10,by+122,width-20,Styles.TextGray);
            }
            if(page==2)
            {
                if(Button(x+10,by,width-20,20,"Horror Enabled: "+(R.state.enabled?"ON":"OFF")))R.SetEnabled(!R.state.enabled);
                if(Button(x+10,by+26,width-20,20,"Scare intensity: "+Mathf.RoundToInt(R.state.intensity*100)+"%")){R.state.intensity=R.state.intensity>=0.9f?0.25f:R.state.intensity+0.2f;R.Save();}
                if(Button(x+10,by+52,width-20,20,"Camera shake: "+(R.state.shake?"ON":"OFF"))){R.state.shake=!R.state.shake;R.Save();}
                TextBlock("Reading pauses gameplay. Disabling stops threats and temporary audio, while keeping earned progression and readable items. No strobing or perception distortion is used.",x+10,by+84,width-20,Styles.TextGray);
                if(!P.IsCreative && Button(x+10,by+132,width-20,20,"Relocate next source if blocked by new construction"))R.RelocateBlockedNext();
            }
            if(page==3 && P.IsCreative)
            {
                ui.Text("Preview actors use real AI; no Survival rewards.",x+10,by,Styles.TextGray);
                if(Button(x+10,by+16,120,18,"Preparation kit"))foreach(var id in HorrorRegistry.Tools)P.inventory.AddOrDrop(new ItemStack("horror:"+id,id=="clatter_lure" || id=="reveal_dust" || id=="binding_spool"?16:1));
                if(Button(x+136,by+16,120,18,R.directorPaused?"Resume director":"Pause director"))R.directorPaused=!R.directorPaused;
                if(Button(x+262,by+16,Mathf.Max(70,width-272),18,"Stop / reset"))R.Dismiss(true);
                entry=Mathf.Clamp(entry,0,6);
                if(Button(x+10,by+43,120,18,"< Creature"))entry=(entry+6)%7;
                if(Button(x+136,by+43,120,18,"Creature >"))entry=(entry+1)%7;
                ui.Text(HorrorRegistry.Names[entry],x+10,by+68,new Color32(108,208,190,255));
                if(Button(x+10,by+83,120,18,"Preview encounter")){R.showcase=true;R.directorPaused=true;if(R.TrySpawn(HorrorRegistry.Creatures[entry],true,out _))hud.Pop();}
                if(Button(x+136,by+83,120,18,"Boss next tactic"))foreach(var m in R.Active())if(m.kind=="unseam" && m.preview)m.health=m.health>80?75:m.health>40?35:120;
                if(Button(x+262,by+83,Mathf.Max(70,width-272),18,"Nearest site")){var site=R.NearestSite();if(site!=null){Gm.BeginTravel(P,R.World,site.position+new Vector3(0,2,5),null);hud.Pop();}}
                ui.Text("Attention "+R.state.attention.ToString("0")+" | tension "+R.state.tension.ToString("0")+" | memory "+R.state.memory.Count,x+10,by+113,Styles.TextGray);
                TextBlock(R.diagnostics.Count>0?R.diagnostics[R.diagnostics.Count-1]:"Use E, search horror: to obtain all seven playable eggs.",x+10,by+127,width-20,Styles.TextGray);
            }
            if(Button(x+10,y+height-26,width-20,18,"Back to Game"))hud.Pop();
        }
        public static string RecipeText(int index)
        {
            string[] text={"book + charcoal","compass + copper ingot","iron ingot + copper ingot + stick","lantern + copper ingot + charcoal","copper ingot + flint = 4","bone meal + charcoal = 4","string + string + iron ingot = 2","clay ball + wheat = 2","copper ingot + charcoal = 2","glass + resonant splinter","resonant splinter + shutter lens + iron ingot","Earned by the real boss death; reserved if inventory is full."};
            return "Recipe: "+text[index];
        }
    }
}
