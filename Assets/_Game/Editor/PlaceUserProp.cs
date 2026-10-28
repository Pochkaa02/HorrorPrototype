using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
public static class PlaceUserProp {
    public static void Run(){
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/House_Refined.unity");
        const string path="Assets/_Game/Models/UserProp/UserProp.fbx";
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importCameras=false;importer.importLights=false;importer.SaveAndReimport();
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!prefab)throw new System.Exception("FBX not imported");
        var old=GameObject.Find("User model - central barrier");if(old)Object.DestroyImmediate(old);
        var holder=new GameObject("User model - central barrier");
        var model=(GameObject)PrefabUtility.InstantiatePrefab(prefab);model.transform.SetParent(holder.transform,false);
        var renderers=holder.GetComponentsInChildren<Renderer>();if(renderers.Length==0)throw new System.Exception("Model has no renderers");
        Bounds bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
        float scale=1.05f/bounds.size.y;holder.transform.localScale=Vector3.one*scale;
        bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
        holder.transform.position=new Vector3(7.375f-bounds.center.x,.005f-bounds.min.y,8.25f-bounds.center.z);
        string materialPath="Assets/_Game/Materials/User barrier.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);if(!material){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,materialPath);}
        material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Models/UserProp/prop_albedo.png");material.color=Color.white;material.SetFloat("_Glossiness",.08f);material.SetFloat("_Metallic",0);
        foreach(var r in renderers){var materials=new Material[r.sharedMaterials.Length];for(int i=0;i<materials.Length;i++)materials[i]=material;r.sharedMaterials=materials;}
        foreach(var filter in holder.GetComponentsInChildren<MeshFilter>()){var collider=filter.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=filter.sharedMesh;}
        Physics.SyncTransforms();
        foreach(var collider in holder.GetComponentsInChildren<Collider>()){
            var b=collider.bounds;foreach(var hit in Physics.OverlapBox(b.center,b.extents*.98f,Quaternion.identity)){
                if(hit.name=="Walls - continuous joined shell")throw new System.Exception("Model overlaps house wall");
            }
        }
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        var c=Object.FindFirstObjectByType<HouseWalk>().view;c.transform.SetParent(null);c.transform.position=new Vector3(6.95f,1.65f,5.35f);c.transform.LookAt(new Vector3(7.375f,.5f,8.25f));c.fieldOfView=75;
        var rt=new RenderTexture(1280,900,24);c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(1280,900,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,900),0,0);t.Apply();File.WriteAllBytes("../output/house/Barrier-in-house.png",t.EncodeToPNG());c.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(t);
        Debug.Log("USER_PROP_PLACED: central hall, on floor, textured, 1.05m tall, no wall overlap.");
    }
}
