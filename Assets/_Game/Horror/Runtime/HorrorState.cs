using System;
using System.Collections.Generic;
using UnityEngine;

namespace MCR.Horror
{
    public enum HuntState { Observe, Investigate, Stalk, Pursue, Search, Windup, Strike, Stagger, Retreat, Dead }
    public enum EvidenceSource { Sight, Sound, Lure, ObservedShelter, FailedSearch }
    [Serializable] public class Evidence
    {
        public EvidenceSource source;
        public Vector3 position;
        public int dimension;
        public long tick, expiry;
        public float confidence;
    }
    [Serializable] public class HorrorSite
    {
        public string id, kind;
        public Vector3 position;
        public int dimension;
        public bool visited, suppressed, suppliesClaimed;
    }
    [Serializable] public class WardData
    {
        public string id;
        public Vector3 position;
        public int dimension, charge = 12000;
        public bool wasValid;
        public long invalidSince;
    }
    [Serializable] public class BossCheckpoint
    {
        public bool active, final;
        public Vector3 position;
        public int dimension;
        public float health = 120;
    }
    [Serializable] public class HorrorState
    {
        public int version = 1;
        public bool enabled = true, activated, defeated, rewardGranted, rewardClaimed;
        public long clock, graceUntil = 1800, recoveryUntil, nextCombat = 2400, nextAmbient = 900;
        public float attention, tension, intensity = 0.65f;
        public bool shake;
        public int stage, decisions;
        public string identity = "the-one-unseam", lastTrigger = "activation";
        public List<string> observed = new List<string>(), confirmed = new List<string>(), history = new List<string>();
        public List<Evidence> memory = new List<Evidence>();
        public List<HorrorSite> sites = new List<HorrorSite>();
        public List<WardData> wards = new List<WardData>();
        public List<long> creatureCooldowns = new List<long> { 0,0,0,0,0,0,0 };
        public BossCheckpoint boss = new BossCheckpoint();
        public void Remember(EvidenceSource source, Vector3 pos, int dimension, float confidence = 1)
        {
            Expire();
            memory.Add(new Evidence { source=source, position=pos, dimension=dimension, tick=clock, expiry=clock+1800, confidence=confidence });
            while (memory.Count > 12) memory.RemoveAt(0);
        }
        public void Expire() => memory.RemoveAll(e => e == null || e.expiry <= clock);
        public Evidence BestEvidence(int dim)
        {
            Expire(); Evidence best=null; float score=-1;
            foreach(var e in memory)
            {
                if(e.dimension!=dim || e.source==EvidenceSource.FailedSearch) continue;
                float s=e.confidence*Mathf.Clamp01((e.expiry-clock)/1800f);
                if(s>score) {best=e;score=s;}
            }
            return best;
        }
        public void RaiseAttention(float amount, string reason)
        {
            attention=Mathf.Clamp(attention+amount,0,100); lastTrigger=reason;
        }
        public void Recover(int ticks=900)
        {
            tension=Mathf.Max(tension,65); recoveryUntil=Math.Max(recoveryUntil,clock+ticks);
        }
        public bool CanAdmit(bool peaceful, bool protectedPlayer, int activeCount)
            => enabled && activated && !defeated && !peaceful && !protectedPlayer && clock>=graceUntil && clock>=recoveryUntil && tension<45 && activeCount<2;
        public void Log(string message)
        {
            history.Add(clock+": "+message); while(history.Count>16) history.RemoveAt(0);
        }
        public static bool Compatible(string a, string b)
        {
            if(a=="unseam" || b=="unseam") return false;
            if(a=="stillwright" || b=="stillwright") return a=="wickdrinker" || b=="wickdrinker";
            return a!=b && !(a=="lintel" && b=="briarchoir" || b=="lintel" && a=="briarchoir");
        }
        public void Observe(string rule, bool confirm=false)
        {
            if(!observed.Contains(rule)) observed.Add(rule);
            if(confirm && !confirmed.Contains(rule)) confirmed.Add(rule);
        }
    }
}
