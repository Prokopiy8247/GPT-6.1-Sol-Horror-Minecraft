using System.Collections.Generic;
using UnityEngine;

namespace MCR.Horror
{
    public sealed class HorrorVisual
    {
        public readonly GameObject root;
        readonly HorrorMob mob;
        readonly Dictionary<string,Transform> parts=new Dictionary<string,Transform>();
        readonly Dictionary<Transform,Vector3> positions=new Dictionary<Transform,Vector3>();
        readonly Dictionary<Transform,Quaternion> rotations=new Dictionary<Transform,Quaternion>();
        public HorrorVisual(HorrorMob mob)
        {
            this.mob=mob;root=Create(mob.kind);
            if(root==null) {Debug.LogError("[Horror] Missing Blender prefab "+mob.kind);return;}
            root.name="Horror_"+mob.kind+"_"+mob.id;
            foreach(var t in root.GetComponentsInChildren<Transform>())
            {string name=t.name;int dot=name.LastIndexOf('.');if(dot>0 && int.TryParse(name.Substring(dot+1),out _))name=name.Substring(0,dot);parts[name]=t;positions[t]=t.localPosition;rotations[t]=t.localRotation;}
        }
        public static GameObject Create(string id)
        {
            var source=Resources.Load<GameObject>("Horror/Prefabs/"+id);
            if(source==null)return null;
            // Preserve the FBX axis conversion and centimetre scale on the imported child.
            var wrapper=new GameObject("HorrorModel_"+id);var imported=Object.Instantiate(source);imported.transform.SetParent(wrapper.transform,false);return wrapper;
        }
        public static void SetGlow(GameObject obj,Color color)
        {
            foreach(var r in obj.GetComponentsInChildren<Renderer>())
            {
                if(r.sharedMaterial==null || !r.sharedMaterial.name.Contains("blue")) continue;
                var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",color);r.SetPropertyBlock(block);
            }
        }
        void Rotate(string name,Vector3 degrees)
        {
            if(parts.TryGetValue(name,out var t))
            {
                Quaternion basis=Quaternion.Inverse(root.transform.rotation)*t.parent.rotation;
                t.localRotation=rotations[t]*Quaternion.Inverse(basis)*Quaternion.Euler(degrees)*basis;
            }
        }
        public void Update(float partial)
        {
            if(root==null) return;
            root.transform.position=Vector3.Lerp(mob.prevPosition,mob.position,partial);
            root.transform.rotation=Quaternion.Euler(0,Mathf.LerpAngle(mob.prevYaw,mob.yaw,partial),0);
            foreach(var pair in rotations) {if(pair.Key!=root.transform) {pair.Key.localRotation=pair.Value;pair.Key.localPosition=positions[pair.Key];}}
            float clock=(mob.age+partial)*0.05f;
            float moved=new Vector2(mob.position.x-mob.prevPosition.x,mob.position.z-mob.prevPosition.z).magnitude;
            float gait=clock*(mob.state==HuntState.Pursue?4.2f:2.1f),amplitude=Mathf.Clamp01(moved*14);
            int legs=mob.kind=="unseam"?3:mob.kind=="rattleblind"?4:2;
            for(int i=0;i<legs;i++)
            {
                float stride=Mathf.Sin(gait+i*Mathf.PI*2/legs)*amplitude;
                Rotate("leg_"+i,new Vector3(stride*17,0,stride*4));
                Rotate("leg_"+i+"_foot",new Vector3(-stride*10,0,0));
            }
            float wind=mob.state==HuntState.Windup?Mathf.Clamp01(mob.attackTicks/32f):mob.state==HuntState.Strike?-0.8f:0;
            Rotate("body",new Vector3(Mathf.Sin(clock*1.1f)*2+mob.posture*10,0,Mathf.Sin(clock*0.7f)*1.5f));
            Rotate("head",new Vector3(Mathf.Sin(clock*0.6f)*3,Mathf.Sin(clock*0.45f)*12+(mob.state==HuntState.Search?20:0),-mob.posture*8));
            for(int i=0;i<4;i++) Rotate("arm_"+i,new Vector3(Mathf.Sin(gait+i*2)*6*amplitude-wind*(i==0?85:22),Mathf.Sin(clock+i)*4,wind*(i==0?24:-12)));
            if(mob.kind=="unseam")
            {
                float opening=mob.revealTicks>0?38:0;
                Rotate("shutter_-1",new Vector3(0,-opening,0));Rotate("shutter_1",new Vector3(0,opening,0));
                if(parts.TryGetValue("body",out var b)) b.localPosition=positions[b]+b.parent.InverseTransformVector(Vector3.down*((1-mob.posture)*0.45f));
            }
            if(mob.kind=="briarchoir")Rotate("head",new Vector3(0,clock*18,0));
            if(mob.kind=="hearthmimic" && mob.revealTicks>0)Rotate("head",new Vector3(-55,clock*35,0));
            if(mob.dead)
            {
                root.transform.rotation*=Quaternion.Euler(0,0,Mathf.Clamp01((mob.deathTime+partial)/20f)*75);
                root.transform.localScale=Vector3.one*Mathf.Max(0.1f,1-(mob.deathTime+partial)/25f);
            }
        }
    }
    public static class HorrorAudio
    {
        static readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
        static AudioSource[] sources;
        static GameObject root;
        static int index;
        public static void Play(string motif,Vector3 position,float volume=0.5f)
        {
            if(Sounds.muted) return;
            if(root==null)
            {
                root=new GameObject("HorrorSpatialAudio");Object.DontDestroyOnLoad(root);sources=new AudioSource[8];
                for(int i=0;i<8;i++){var go=new GameObject("horror_sound_"+i);go.transform.SetParent(root.transform);sources[i]=go.AddComponent<AudioSource>();sources[i].spatialBlend=1;sources[i].dopplerLevel=0;sources[i].minDistance=2;sources[i].maxDistance=40;sources[i].rolloffMode=AudioRolloffMode.Linear;}
            }
            if(!clips.TryGetValue(motif,out var clip))
            {
                const int sr=22050;float duration=motif=="bell"?1.6f:motif=="settle"?2.2f:0.8f;
                float[] data=new float[(int)(sr*duration)];
                float freq=motif=="bell"?93:motif=="answer"?61:motif=="warning"?143:motif=="settle"?220:motif=="reveal"?370:75;
                for(int i=0;i<data.Length;i++)
                {
                    float t=i/(float)sr;float env=Mathf.Min(1,t*25)*Mathf.Exp(-t*(motif=="settle"?1.8f:3.2f));
                    float s=Mathf.Sin(t*freq*Mathf.PI*2)+0.35f*Mathf.Sin(t*freq*2.73f*Mathf.PI*2)+0.18f*Mathf.Sin(t*(freq*4.12f+Mathf.Sin(t*4)*12)*Mathf.PI*2);
                    data[i]=Mathf.Clamp(s*env*0.17f,-0.35f,0.35f);
                }
                clip=AudioClip.Create("Horror_"+motif,data.Length,1,sr,false);clip.SetData(data,0);clips[motif]=clip;
            }
            var source=sources[index++%sources.Length];source.transform.position=position;source.clip=clip;source.volume=Mathf.Clamp01(volume)*(HorrorRuntime.Current?.state.intensity??.65f)*Sounds.masterVolume*Sounds.hostileVolume;source.Play();
        }
        public static void Stop(){if(sources!=null)foreach(var s in sources)if(s!=null)s.Stop();}
    }
}
