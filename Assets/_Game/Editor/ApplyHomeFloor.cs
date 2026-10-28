using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
public static class ApplyHomeFloor {
    [MenuItem("Horror/Apply dark home laminate")]
    public static void Run(){
        const string scene="Assets/_Game/Scenes/House_Refined.unity";
        const string texturePath="Assets/_Game/Textures/LaminateFloor02/laminate_floor_02_diff_1k.jpg";
        EditorSceneManager.OpenScene(scene);
        AssetDatabase.ImportAsset(texturePath,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);
        importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;importer.maxTextureSize=512;importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Repeat;importer.filterMode=FilterMode.Bilinear;importer.anisoLevel=1;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);if(!texture)throw new System.Exception("Texture missing");
        string materialPath="Assets/_Game/Materials/Dark home laminate.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(!material){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,materialPath);}
        material.mainTexture=texture;material.color=new Color(.58f,.51f,.46f);material.SetFloat("_Metallic",0);material.SetFloat("_Glossiness",.12f);material.SetFloat("_SpecularHighlights",0);material.SetFloat("_GlossyReflections",0);
        var floor=GameObject.Find("Floor - continuous slab");if(!floor)throw new System.Exception("Floor missing");
        var filter=floor.GetComponent<MeshFilter>();var mesh=Object.Instantiate(filter.sharedMesh);mesh.name="Floor with continuous laminate UVs";
        var vertices=mesh.vertices;var uv=new Vector2[vertices.Length];
        for(int i=0;i<uv.Length;i++)uv[i]=new Vector2(vertices[i].z/1.7f,vertices[i].x/1.7f);
        mesh.uv=uv;
        var top=new List<int>();var sides=new List<int>();var indices=mesh.triangles;
        for(int i=0;i<indices.Length;i+=3){var a=vertices[indices[i]];var b=vertices[indices[i+1]];var c=vertices[indices[i+2]];var list=Vector3.Cross(b-a,c-a).normalized.y>.9f?top:sides;list.Add(indices[i]);list.Add(indices[i+1]);list.Add(indices[i+2]);}
        mesh.subMeshCount=2;mesh.SetTriangles(top,0);mesh.SetTriangles(sides,1);
        string meshPath="Assets/_Game/Models/Floor-laminate-UV.asset";
        var saved=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(saved){EditorUtility.CopySerialized(mesh,saved);Object.DestroyImmediate(mesh);mesh=saved;}else AssetDatabase.CreateAsset(mesh,meshPath);
        filter.sharedMesh=mesh;floor.GetComponent<MeshCollider>().sharedMesh=mesh;
        floor.GetComponent<MeshRenderer>().sharedMaterials=new[]{material,AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/Blockout floor.mat")};
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        var camera=Object.FindFirstObjectByType<HouseWalk>().view;camera.transform.SetParent(null);camera.transform.position=new Vector3(7.375f,1.66f,3.5f);camera.transform.rotation=Quaternion.Euler(28,0,0);camera.fieldOfView=70;
        var rt=new RenderTexture(1280,800,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var preview=new Texture2D(1280,800,TextureFormat.RGB24,false);preview.ReadPixels(new Rect(0,0,1280,800),0,0);preview.Apply();File.WriteAllBytes("../output/house/Dark-laminate-interior.png",preview.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(preview);
        Debug.Log("DARK_HOME_FLOOR_APPLIED: 512 texture, 1.7m repeat, continuous UV, preserved floor geometry and collider.");
    }
}
