using System.IO;
using System.Linq;
using MCR.Horror;
using NUnit.Framework;
using UnityEngine;

namespace MCR.Tests
{
    public class HorrorTests
    {
        [OneTimeSetUp] public void Init(){Biome.Init();Blocks.Init();Items.Init();Tags.Init();Recipes.Init();}
        [Test] public void BoundedEvidenceCannotTrackExpiredOrDifferentDimension()
        {
            var s=new HorrorState();for(int i=0;i<30;i++){s.clock=i;s.Remember(EvidenceSource.Sound,new Vector3(i,60,0),0);}
            Assert.AreEqual(12,s.memory.Count);Assert.IsNull(s.BestEvidence(1));
            var e=s.BestEvidence(0);Assert.AreEqual(29,e.position.x);s.clock=2000;Assert.IsNull(s.BestEvidence(0));Assert.AreEqual(0,s.memory.Count);
            s.Remember(EvidenceSource.FailedSearch,Vector3.zero,0);Assert.IsNull(s.BestEvidence(0));
        }
        [Test] public void AttentionDoesNotOverrideRecoveryGraceVictoryOrPeaceful()
        {
            var s=new HorrorState{activated=true,clock=3000,attention=100};Assert.IsTrue(s.CanAdmit(false,false,0));
            s.Recover();Assert.IsFalse(s.CanAdmit(false,false,0));s.clock+=1000;s.tension=0;Assert.IsTrue(s.CanAdmit(false,false,0));
            Assert.IsFalse(s.CanAdmit(true,false,0));Assert.IsFalse(s.CanAdmit(false,true,0));Assert.IsFalse(s.CanAdmit(false,false,2));
            s.defeated=true;Assert.IsFalse(s.CanAdmit(false,false,0));
        }
        [Test] public void ThreatCompatibilityIsSymmetricAndBossExclusive()
        {
            foreach(var a in HorrorRegistry.Creatures)foreach(var b in HorrorRegistry.Creatures)
                Assert.AreEqual(HorrorState.Compatible(a,b),HorrorState.Compatible(b,a),a+" / "+b);
            foreach(var kind in HorrorRegistry.Creatures)Assert.IsFalse(HorrorState.Compatible("unseam",kind));
            Assert.IsFalse(HorrorState.Compatible("stillwright","lintel"));Assert.IsTrue(HorrorState.Compatible("stillwright","wickdrinker"));
        }
        [Test] public void RealRecipesCoverEveryRenewableMandatoryTool()
        {
            foreach(var id in HorrorRegistry.Tools.Where(id=>id!="quiet_heart"))
            {var recipe=Recipes.Crafting.FirstOrDefault(r=>r.result?.item.id=="horror:"+id);Assert.IsNotNull(recipe,id);Assert.IsTrue(recipe.AllIngredients().All(i=>i.items.Count>0),id);}
            foreach(var kind in HorrorRegistry.Creatures)Assert.IsInstanceOf<HorrorEgg>(Items.Get("horror:"+kind+"_spawn_egg"));
        }
        [Test] public void AuthoredPrefabRetainsItsAxisConversionAndWorldScale()
        {
            var root=HorrorVisual.Create("unseam");Assert.IsNotNull(root);
            try{Assert.AreEqual(Quaternion.identity,root.transform.rotation);var renderers=root.GetComponentsInChildren<Renderer>();Assert.Greater(renderers.Length,50);Bounds bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);Assert.Greater(bounds.size.y,5);Assert.Less(bounds.size.y,5.6f);Assert.Less(bounds.size.x,3.3f);}
            finally{Object.DestroyImmediate(root);}
        }
        [Test] public void AtomicSidecarRecoversWithoutResettingBossDeathOrReward()
        {
            string dir=Path.Combine(Path.GetTempPath(),"HorrorTest-"+System.Guid.NewGuid());Directory.CreateDirectory(dir);
            try
            {
                var s=new HorrorState{clock=1234,defeated=true,rewardGranted=true,rewardClaimed=true,stage=3};s.boss.health=0;
                s.Observe("bind",true);s.Remember(EvidenceSource.Lure,new Vector3(12,64,19),0);HorrorSave.Write(dir,s);HorrorSave.Write(dir,s);
                var r=HorrorSave.Read(dir);Assert.IsTrue(r.defeated && r.rewardClaimed);Assert.AreEqual(3,r.stage);Assert.AreEqual(1234,r.clock);Assert.IsTrue(r.confirmed.Contains("bind"));
                File.WriteAllText(Path.Combine(dir,"horror-v1.json"),"broken");r=HorrorSave.Read(dir);Assert.IsTrue(r.defeated && r.rewardClaimed);Assert.Greater(r.recoveryUntil,r.clock);
            }
            finally {Directory.Delete(dir,true);}
        }
    }
}
