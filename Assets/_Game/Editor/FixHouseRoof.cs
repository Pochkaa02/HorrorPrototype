using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
public static class FixHouseRoof {
    static Transform parent;
    static Material roof,wall,edge;
    [MenuItem("Horror/Rebuild plain roof")]
    public static void Run(){
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/House.unity");
        var house=GameObject.Find("HOUSE - reference blockout");
        parent=house.transform.Find("Roof - R to toggle in Play");
        for(int i=parent.childCount-1;i>=0;i--){var child=parent.GetChild(i);if(!child.name.Contains("ceiling"))Object.DestroyImmediate(child.gameObject);}
        roof=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/Brown roof.mat");roof.color=new Color(.19f,.20f,.21f);roof.SetFloat("_Glossiness",0);
        wall=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/Warm plaster.mat");
        string path="Assets/_Game/Materials/Roof edge.mat";edge=AssetDatabase.LoadAssetAtPath<Material>(path);if(!edge){edge=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(edge,path);}edge.color=new Color(.44f,.45f,.44f);edge.SetFloat("_Glossiness",0);
        Gable("Left roof",-.25f,6.5f,-.25f,16.75f,2.95f,1.85f,true);
        Gable("Main roof",6.05f,17,-.25f,16.75f,2.95f,2.35f,false);
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        var camera=Object.FindFirstObjectByType<Camera>();camera.transform.SetParent(null);camera.transform.position=new Vector3(24,9,-18);camera.transform.LookAt(new Vector3(8,1.5f,6));camera.fieldOfView=52;
        var rt=new RenderTexture(1200,900,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var texture=new Texture2D(1200,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1200,900),0,0);texture.Apply();File.WriteAllBytes("../output/house/House-roof.png",texture.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(texture);
        Debug.Log("ROOF_FIXED");
    }
    static void Gable(string name,float x1,float x2,float z1,float z2,float y,float rise,bool alongZ){
        float mx=(x1+x2)/2,mz=(z1+z2)/2;
        Vector3[] p=alongZ?new[]{new Vector3(x1,y,z1),new Vector3(x2,y,z1),new Vector3(mx,y+rise,z1),new Vector3(x1,y,z2),new Vector3(x2,y,z2),new Vector3(mx,y+rise,z2)}:new[]{new Vector3(x1,y,z1),new Vector3(x1,y,z2),new Vector3(x1,y+rise,mz),new Vector3(x2,y,z1),new Vector3(x2,y,z2),new Vector3(x2,y+rise,mz)};
        // Separate vertices per triangle give planar roof faces instead of smoothed bulges.
        int[] order={0,2,1,3,4,5,0,3,5,0,5,2,2,5,4,2,4,1,0,1,4,0,4,3};
        var v=new List<Vector3>();var caps=new List<int>();var slopes=new List<int>();
        for(int i=0;i<order.Length;i+=3){int start=v.Count;if(alongZ){v.Add(p[order[i]]);v.Add(p[order[i+1]]);v.Add(p[order[i+2]]);}else{v.Add(p[order[i+2]]);v.Add(p[order[i+1]]);v.Add(p[order[i]]);}var indices=i<6?caps:slopes;indices.Add(start);indices.Add(start+1);indices.Add(start+2);}
        var mesh=new Mesh();mesh.SetVertices(v);mesh.subMeshCount=2;mesh.SetTriangles(slopes,0);mesh.SetTriangles(caps,1);mesh.RecalculateNormals();mesh.RecalculateBounds();
        string path="Assets/_Game/Models/"+name.Replace(" ","-")+".asset";AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(mesh,path);
        var obj=new GameObject(name);obj.transform.SetParent(parent);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterials=new[]{roof,wall};
        Beam(name+" front fascia",p[0],p[1]);Beam(name+" rear fascia",p[3],p[4]);Beam(name+" edge",p[0],p[3]);Beam(name+" edge",p[1],p[4]);
        Beam(name+" rake",p[0],p[2]);Beam(name+" rake",p[1],p[2]);Beam(name+" rake",p[3],p[5]);Beam(name+" rake",p[4],p[5]);
    }
    static void Beam(string name,Vector3 a,Vector3 b){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent);g.transform.position=(a+b)/2;g.transform.rotation=Quaternion.FromToRotation(Vector3.forward,b-a);g.transform.localScale=new Vector3(.09f,.13f,Vector3.Distance(a,b)+.06f);g.GetComponent<Renderer>().sharedMaterial=edge;g.isStatic=true;}
}
