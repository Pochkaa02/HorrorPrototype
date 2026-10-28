using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class SimplifyHouse {
    [MenuItem("Horror/Simplify house to walls and roof")]
    public static void Run() {
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/House.unity");
        var root=GameObject.Find("HOUSE - reference blockout");
        if(!root)throw new System.Exception("House missing");
        var plaster=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/Warm plaster.mat");
        plaster.color=new Color(.65f,.65f,.63f);
        var roofMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/Brown roof.mat");
        roofMaterial.color=new Color(.22f,.23f,.24f);
        // Remove exterior dressing and window inserts, preserving the wall volumes and floor.
        foreach(var tr in root.GetComponentsInChildren<Transform>(true)) {
            if(!tr || tr==root.transform)continue;
            string n=tr.name;
            if(n.EndsWith(" glass") || n.EndsWith(" frame") || n.EndsWith(" rail") || n.EndsWith(" jamb") || n.Contains("brick base") || n.Contains("facade siding") || n.StartsWith("Garage door panel") || n.StartsWith("Front door") || n.StartsWith("Porch ") || n=="Driveway" || n=="Footpath" || n.StartsWith("Stair tread")) Object.DestroyImmediate(tr.gameObject);
        }
        // Fill the former window apertures with plain wall blocks. Doorways stay open.
        AddWall("West rear infill",620,220,620,550,120,1.7f,plaster,root.transform);
        AddWall("West bathroom infill",620,550,620,685,65,.7f,plaster,root.transform);
        AddWall("Rear left infill",620,220,870,220,125,1.8f,plaster,root.transform);
        AddWall("Rear service infill",870,220,960,220,45,.65f,plaster,root.transform);
        AddWall("East rear infill",1290,220,1290,400,90,1.7f,plaster,root.transform);
        AddWall("East living infill",1290,400,1290,590,95,1.7f,plaster,root.transform);
        AddWall("East garage infill",1290,590,1290,835,115,1.3f,plaster,root.transform);
        AddWall("Front left infill",620,880,870,880,125,2.8f,plaster,root.transform);
        AddWall("Front right infill",960,785,1105,785,72.5f,2f,plaster,root.transform);
        foreach(var r in root.GetComponentsInChildren<Renderer>(true)) {
            if(r.transform.parent && r.transform.parent.name.StartsWith("Roof")) continue;
            if(r.name=="Ground") continue;
            r.sharedMaterial=plaster;
        }
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        // Review only: the saved scene retains its player and roof.
        var camera=Object.FindFirstObjectByType<Camera>();camera.transform.SetParent(null);camera.transform.position=new Vector3(23,8,-16);camera.transform.LookAt(new Vector3(8,1.5f,6));camera.fieldOfView=52;
        var rt=new RenderTexture(1200,900,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
        var tex=new Texture2D(1200,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1200,900),0,0);tex.Apply();File.WriteAllBytes("../output/house/House-simple.png",tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);
        Debug.Log("SIMPLE_HOUSE_SAVED");
    }
    static void AddWall(string name,float x1,float y1,float x2,float y2,float offset,float width,Material material,Transform parent) {
        var existing=parent.Find(name);if(existing)Object.DestroyImmediate(existing.gameObject);
        Vector3 a=new Vector3((x1-620)*.025f,0,(880-y1)*.025f),b=new Vector3((x2-620)*.025f,0,(880-y2)*.025f);
        Vector3 center=a+(b-a).normalized*offset*.025f;center.y=1.475f;
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent);g.transform.position=center;g.transform.localScale=Mathf.Abs(x2-x1)>Mathf.Abs(y2-y1)?new Vector3(width,1.35f,.16f):new Vector3(.16f,1.35f,width);g.GetComponent<Renderer>().sharedMaterial=material;g.isStatic=true;
    }
}
