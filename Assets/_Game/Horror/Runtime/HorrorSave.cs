using System;
using System.IO;
using UnityEngine;

namespace MCR.Horror
{
    public static class HorrorSave
    {
        public static HorrorState Read(string dir)
        {
            string path=Path.Combine(dir,"horror-v1.json");
            foreach(var p in new[]{path,path+".bak"})
            {
                if(!File.Exists(p)) continue;
                try
                {
                    var s=JsonUtility.FromJson<HorrorState>(File.ReadAllText(p));
                    if(s==null || s.version!=1 || s.memory==null || s.sites==null || s.boss==null) throw new InvalidDataException("Invalid horror sidecar");
                    if(s.wards==null || s.observed==null || s.confirmed==null || s.creatureCooldowns==null || s.creatureCooldowns.Count!=7 || s.clock<0)throw new InvalidDataException("Invalid horror collections/clock");
                    s.memory.RemoveAll(e=>e==null);while(s.memory.Count>12)s.memory.RemoveAt(0);
                    s.attention=Mathf.Clamp(s.attention,0,100);s.tension=Mathf.Clamp(s.tension,0,100);s.intensity=Mathf.Clamp(s.intensity,.05f,1);
                    s.Expire();
                    // No offline time; always a safe warning window after joining a world.
                    s.recoveryUntil=Math.Max(s.recoveryUntil,s.clock+400);
                    return s;
                }
                catch(Exception e) { Debug.LogWarning("[Horror] Sidecar recovery: "+e.Message); }
            }
            return new HorrorState();
        }
        public static void Write(string dir,HorrorState state)
        {
            Directory.CreateDirectory(dir);
            string path=Path.Combine(dir,"horror-v1.json"), temp=path+".tmp";
            File.WriteAllText(temp,JsonUtility.ToJson(state,true));
            if(File.Exists(path)) File.Replace(temp,path,path+".bak");
            else File.Move(temp,path);
        }
    }
}
