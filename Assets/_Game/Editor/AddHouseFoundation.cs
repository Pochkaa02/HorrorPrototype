using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class AddHouseFoundation {
    [MenuItem("Horror/Add foundation")]
    public static void Run() {
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/House.unity");
        var house=GameObject.Find("HOUSE - reference blockout");
        if(!house)throw new System.Exception("House missing");
        var old=house.transform.Find("Concrete foundation");if(old)Object.DestroyImmediate(old.gameObject);
        var root=new GameObject("Concrete foundation");root.transform.SetParent(house.transform);
        string path="Assets/_Game/Materials/Foundation concrete.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!material){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,path);}
        material.color=new Color(.34f,.35f,.34f);material.SetFloat("_Glossiness",.05f);
        // Follow the union of the floor rectangles, with a 10 cm perimeter projection.
        Slab("Left wing",-.1f,6.25f,-.1f,16.6f,root.transform,material);
        Slab("Entry and central wing",6.25f,8.5f,-.1f,16.6f,root.transform,material);
        Slab("Right rooms",8.5f,12.125f,2.275f,16.6f,root.transform,material);
        Slab("Garage wing",12.125f,16.85f,1.025f,16.6f,root.transform,material);
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        Debug.Log("FOUNDATION_SAVED");
    }
    static void Slab(string name,float x1,float x2,float z1,float z2,Transform parent,Material material){
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent);
        // Floor top stays at 0; the slab extends from -0.65 to -0.12, below the existing floor.
        g.transform.position=new Vector3((x1+x2)/2,-.385f,(z1+z2)/2);
        g.transform.localScale=new Vector3(x2-x1,.53f,z2-z1);g.GetComponent<Renderer>().sharedMaterial=material;g.isStatic=true;
    }
}
