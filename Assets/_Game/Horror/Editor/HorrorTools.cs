using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MCR.Horror.EditorTools
{
    public static class HorrorTools
    {
        const string Root="Assets/_Game/Horror/Resources/Horror";
        [MenuItem("Tools/The Quiet Below/Import Horror Blender Assets")]
        public static void Prepare()
        {
            AssetDatabase.Refresh();Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Prefabs");
            var palette=new Dictionary<string,Color>{{"ash",new Color(.14f,.19f,.20f)},{"ivory",new Color(.72f,.69f,.52f)},{"copper",new Color(.38f,.19f,.09f)},{"dark",new Color(.035f,.043f,.052f)},{"blue",new Color(.1f,.7f,.72f)},{"red",new Color(.61f,.2f,.12f)},{"linen",new Color(.38f,.42f,.32f)}};
            var materials=new Dictionary<string,Material>();
            foreach(var pair in palette)
            {
                string path=Root+"/Materials/HORROR_"+pair.Key+".mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null){material=new Material(Shader.Find("MCR/HorrorSurface"));AssetDatabase.CreateAsset(material,path);}
                material.SetColor("_BaseColor",pair.Value);material.SetFloat("_Glow",pair.Key=="blue"?.45f:0);EditorUtility.SetDirty(material);materials["HORROR_"+pair.Key]=material;
            }
            var lines=new List<string>();
            foreach(string path in Directory.GetFiles(Root+"/Models","*.fbx"))
            {
                var importer=AssetImporter.GetAtPath(path) as UnityEditor.ModelImporter;
                if(importer==null)continue;
                importer.isReadable=true;importer.globalScale=1;importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.meshCompression=ModelImporterMeshCompression.Off;
                importer.SaveAndReimport();
                var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);var go=UnityEngine.Object.Instantiate(source);
                foreach(var renderer in go.GetComponentsInChildren<Renderer>())
                {
                    var mats=renderer.sharedMaterials;
                    for(int i=0;i<mats.Length;i++)if(mats[i]!=null && materials.TryGetValue(mats[i].name,out var correct))mats[i]=correct;
                    renderer.sharedMaterials=mats;
                    renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                string id=Path.GetFileNameWithoutExtension(path);
                go.name=id;
                PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+id+".prefab");
                Bounds bounds=new Bounds(go.transform.position,Vector3.zero);foreach(var r in go.GetComponentsInChildren<Renderer>())bounds.Encapsulate(r.bounds);
                lines.Add(id+" size="+bounds.size+" renderers="+go.GetComponentsInChildren<Renderer>().Length);
                UnityEngine.Object.DestroyImmediate(go);
            }
            AssetDatabase.SaveAssets();Directory.CreateDirectory("Tools/_out");File.WriteAllLines("Tools/_out/horror-assets.txt",lines);
            Debug.Log("[HorrorAssets] Prepared "+lines.Count+" authored prefabs\n"+string.Join("\n",lines));
        }
        [MenuItem("Tools/The Quiet Below/Build Horror Windows")]
        public static void Build()
        {BuildTo("Builds/HorrorWindows",BuildOptions.Development);}
        [MenuItem("Tools/The Quiet Below/Build Release Windows")]
        public static void BuildRelease()
        {BuildTo("Builds/HorrorRelease",BuildOptions.None);}
        static void BuildTo(string folder,BuildOptions options)
        {
            Prepare();Directory.CreateDirectory(folder);
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/_Game/Scenes/Main.unity"},locationPathName=folder+"/MinecraftHorror.exe",target=BuildTarget.StandaloneWindows64,options=options});
            Debug.Log("[HorrorBuild] "+result.summary.result+" errors="+result.summary.totalErrors+" bytes="+result.summary.totalSize);
            File.WriteAllText("Tools/_out/horror-build.txt",result.summary.result+" errors="+result.summary.totalErrors+" time="+result.summary.totalTime);
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Horror build failed");
        }
    }
}
