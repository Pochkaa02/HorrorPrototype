using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class CleanHouseBase {
    public static void Run(){
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/House.unity");
        var house=GameObject.Find("HOUSE - reference blockout");
        // The earlier frame/jamb trim was removed, but the garage portal still had wall returns.
        // Align foundation faces with the outer wall faces rather than making a projecting ledge.
        var foundation=house.transform.Find("Concrete foundation");
        foreach(Transform t in foundation){
            float x1=0,x2=0,z1=0,z2=16.58f;
            switch(t.name){
                case "Left wing":x1=-.08f;x2=6.25f;z1=-.08f;break;
                case "Entry and central wing":x1=6.25f;x2=8.58f;z1=-.08f;break;
                case "Right rooms":x1=8.58f;x2=12.125f;z1=2.295f;break;
                default:x1=12.125f;x2=16.83f;z1=1.045f;break;
            }
            t.position=new Vector3((x1+x2)/2,-.425f,(z1+z2)/2);
            t.localScale=new Vector3(x2-x1,.65f,z2-z1);
        }
        // Remove narrow facade ends left by window-wall construction and replace each
        // exterior run by a single continuous wall, except the entrance and garage opening.
        var shell=house.transform.Find("Walls and windows");
        string[] prefixes={"West rear window","West bathroom window","West front","Rear left window","Rear service window","Rear stairs","Rear right","East rear window","East living window","East garage window","Front left triple window","Front right window"};
        for(int i=shell.childCount-1;i>=0;i--){var t=shell.GetChild(i);foreach(var p in prefixes)if(t.name==p || t.name.StartsWith(p+" ")){Object.DestroyImmediate(t.gameObject);break;}}
        for(int i=house.transform.childCount-1;i>=0;i--){var t=house.transform.GetChild(i);if(t.name.EndsWith(" infill"))Object.DestroyImmediate(t.gameObject);}
        var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/Warm plaster.mat");
        Wall("Clean west wall",620,220,620,880,shell,mat);
        Wall("Clean rear wall",620,220,1290,220,shell,mat);
        Wall("Clean east wall",1290,220,1290,835,shell,mat);
        Wall("Clean front left",620,880,870,880,shell,mat);
        Wall("Clean front right",960,785,1105,785,shell,mat);
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("HOUSE_BASE_CLEANED");
    }
    static void Wall(string name,float x1,float y1,float x2,float y2,Transform parent,Material mat){
        var existing=parent.Find(name);if(existing)Object.DestroyImmediate(existing.gameObject);
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent);
        g.transform.position=new Vector3(((x1+x2)/2-620)*.025f,1.4f,(880-(y1+y2)/2)*.025f);
        g.transform.localScale=new Vector3(Mathf.Abs(x2-x1)*.025f+.16f,2.8f,Mathf.Abs(y2-y1)*.025f+.16f);
        g.GetComponent<Renderer>().sharedMaterial=mat;g.isStatic=true;
    }
}
