using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using System.IO;

public static class BuildReferenceHouse
{
    static Transform root, shell, roof;
    static Material plaster, siding, trim, wood, tile, concrete, brick, roofMat, glass, door, grass;
    const float H = 2.8f, T = .16f;
    static float Z(float y) { return (880-y)*.025f; }
    static float X(float x) { return (x-620)*.025f; }
    static Material Mat(string name, Color c) {
        string path="Assets/_Game/Materials/"+name+".mat";
        Material m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m) {m=new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(m,path);}
        m.color=c; m.SetFloat("_Glossiness",.1f); return m;
    }
    static GameObject Box(string name, Vector3 pos, Vector3 size, Material m, Transform parent=null) {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube); g.name=name;
        g.transform.SetParent(parent ? parent : root); g.transform.position=pos; g.transform.localScale=size;
        g.GetComponent<Renderer>().sharedMaterial=m; g.isStatic=true; return g;
    }
    static void Rect(string n,float x1,float y1,float x2,float y2, Material m) {
        Box(n,new Vector3((X(x1)+X(x2))/2,-.1f,(Z(y1)+Z(y2))/2),new Vector3(X(x2)-X(x1),.2f,Z(y1)-Z(y2)),m);
    }
    // Walls follow the pixel coordinates of the supplied floor-plan reference.
    static void Wall(string n,float x1,float y1,float x2,float y2,float opening=-1,float width=1.0f,bool window=false) {
        Vector3 a=new Vector3(X(x1),0,Z(y1)), b=new Vector3(X(x2),0,Z(y2));
        Vector3 dir=(b-a).normalized; float len=Vector3.Distance(a,b);
        bool horizontal=Mathf.Abs(x2-x1)>Mathf.Abs(y2-y1);
        System.Action<float,float,float,float,string> segment=(start,end,bottom,top,suffix)=> {
            if(end-start<.005f)return;
            Vector3 p=a+dir*((start+end)/2); p.y=(bottom+top)/2;
            Box(n+suffix,p,horizontal?new Vector3(end-start,top-bottom,T):new Vector3(T,top-bottom,end-start),plaster,shell);
        };
        if(opening<0) {segment(0,len,0,H,"");return;}
        float c=opening*.025f, l=c-width/2, r=c+width/2;
        segment(0,l,0,H," A"); segment(r,len,0,H," B"); segment(l,r,window?2.15f:2.15f,H," lintel");
        if(window) {
            segment(l,r,0,.8f," sill wall");
            Vector3 p=a+dir*c; p.y=1.475f;
            Box(n+" glass",p,horizontal?new Vector3(width,1.35f,.025f):new Vector3(.025f,1.35f,width),glass,shell);
            foreach(float edge in new[]{l,r,c}) {p=a+dir*edge;p.y=1.475f; Box(n+" frame",p,horizontal?new Vector3(.055f,1.45f,.22f):new Vector3(.22f,1.45f,.055f),trim,shell);}
            foreach(float height in new[]{.78f,1.48f,2.17f}){p=a+dir*c;p.y=height;Box(n+" rail",p,horizontal?new Vector3(width+.1f,.055f,.22f):new Vector3(.22f,.055f,width+.1f),trim,shell);}
        } else {
            foreach(float edge in new[]{l,r}) {Vector3 p=a+dir*edge;p.y=1.075f;Box(n+" jamb",p,horizontal?new Vector3(.06f,2.15f,.22f):new Vector3(.22f,2.15f,.06f),trim,shell);}
        }
    }
    static void Gable(string name,float x1,float x2,float z1,float z2,float baseY,float rise,bool alongZ) {
        var g=new GameObject(name);g.transform.SetParent(roof);
        float mx=(x1+x2)/2,mz=(z1+z2)/2;
        Vector3[] v=alongZ?new[]{new Vector3(x1,baseY,z1),new Vector3(x2,baseY,z1),new Vector3(mx,baseY+rise,z1),new Vector3(x1,baseY,z2),new Vector3(x2,baseY,z2),new Vector3(mx,baseY+rise,z2)}:new[]{new Vector3(x1,baseY,z1),new Vector3(x1,baseY,z2),new Vector3(x1,baseY+rise,mz),new Vector3(x2,baseY,z1),new Vector3(x2,baseY,z2),new Vector3(x2,baseY+rise,mz)};
        var mesh=new Mesh();mesh.vertices=v;mesh.triangles=new[]{0,2,1,3,4,5,0,3,5,0,5,2,2,5,4,2,4,1,0,1,4,0,4,3};if(!alongZ) { var indices=mesh.triangles; for(int i=0;i<indices.Length;i+=3){int swap=indices[i];indices[i]=indices[i+2];indices[i+2]=swap;} mesh.triangles=indices; } mesh.RecalculateNormals();
        string path="Assets/_Game/Models/"+name+".asset";AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(mesh,path);
        g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=roofMat;
    }
    [MenuItem("Horror/Build reference house")]
    public static void Build() {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        root=new GameObject("HOUSE - reference blockout").transform;
        shell=new GameObject("Walls and windows").transform;shell.SetParent(root);
        roof=new GameObject("Roof - R to toggle in Play").transform;roof.SetParent(root);
        plaster=Mat("Warm plaster",new Color(.65f,.62f,.54f)); siding=Mat("Cream siding",new Color(.66f,.61f,.45f));
        trim=Mat("Ivory trim",new Color(.82f,.81f,.7f)); wood=Mat("Dark timber floor",new Color(.24f,.16f,.1f));
        tile=Mat("Tile",new Color(.42f,.43f,.38f)); concrete=Mat("Concrete",new Color(.32f,.33f,.31f));
        brick=Mat("Brick plinth",new Color(.29f,.16f,.12f)); roofMat=Mat("Brown roof",new Color(.23f,.19f,.14f));
        glass=Mat("Dark window glass",new Color(.12f,.21f,.23f)); door=Mat("Door walnut",new Color(.23f,.16f,.08f)); grass=Mat("Ground",new Color(.17f,.23f,.12f));
        Rect("Left rear room",620,220,870,550,wood);Rect("Left wet room",620,550,770,685,tile);
        Rect("Left front room",620,685,870,880,wood);Rect("Hall and entry",770,550,960,785,wood);
        Rect("Entry recess",870,785,960,880,wood);Rect("Main hall",870,310,960,550,wood);
        Rect("Rear service room",870,220,960,310,tile);Rect("Stairwell",960,220,1145,310,wood);
        Rect("Rear right passage",960,310,1290,400,wood);Rect("Rear right landing",1145,220,1290,310,wood);Rect("Right rear room",1105,400,1290,590,tile);
        Rect("Center storage",960,400,1105,500,wood);Rect("Center bathroom",960,500,1105,590,tile);
        Rect("Right front room",960,590,1105,785,wood);Rect("Garage",1105,590,1290,835,concrete);
        // Exterior: front is the bottom of the plan, facing -Z.
        Wall("West rear window",620,220,620,550,120,1.7f,true);
        Wall("West bathroom window",620,550,620,685,65,.7f,true);
        Wall("West front",620,685,620,880);
        Wall("Rear left window",620,220,870,220,125,1.8f,true);
        Wall("Rear service window",870,220,960,220,45,.65f,true);
        Wall("Rear stairs",960,220,1145,220);Wall("Rear right",1145,220,1290,220);
        Wall("East rear window",1290,220,1290,400,90,1.7f,true);
        Wall("East living window",1290,400,1290,590,95,1.7f,true);
        Wall("East garage window",1290,590,1290,835,115,1.3f,true);
        Wall("Front left triple window",620,880,870,880,125,2.8f,true);
        Wall("Entry door opening",870,880,960,880,45,1.05f);
        Wall("Entry east return",960,785,960,880);
        Wall("Front right window",960,785,1105,785,72.5f,2.0f,true);
        Wall("Garage west return",1105,785,1105,835);
        Wall("Garage entrance",1105,835,1290,835,92.5f,3.7f);
        // Interior partitions and open doorways.
        Wall("Left rear to hall",870,220,870,550,290,1.05f);
        Wall("Left rear south",620,550,770,550);Wall("Left rear door return",770,550,870,550,50,1.0f);
        Wall("Left bathroom door",770,550,770,685,50,.85f);
        Wall("Left front north",620,685,870,685,205,1.0f);
        Wall("Left front east",870,685,870,880);
        Wall("Service south door",870,310,960,310,45,.85f);
        Wall("Service east",960,220,960,310);
        Wall("Stairs south access",960,310,1145,310,45,1.0f);
        Wall("Stairs east",1145,220,1145,310);
        Wall("Storage west access",960,400,960,500,50,.85f);
        Wall("Storage north",960,400,1105,400);Wall("Storage south",960,500,1105,500);
        Wall("Center east",1105,400,1105,590);
        Wall("Bathroom access",960,500,960,590,45,.85f);
        Wall("Center south",960,590,1105,590);
        Wall("Right front hall door",960,590,960,785,145,.95f);
        Wall("Garage internal door",1105,590,1105,785,35,.95f);
        Wall("Garage north",1105,590,1290,590);
        Wall("Entry hall shoulder",870,785,960,785,45,1.1f);
        // Plinth and horizontal siding bands visually establish the reference facade.
        Box("Front left brick base",new Vector3(X(745),.32f,Z(880)-.09f),new Vector3(6.25f,.64f,.045f),brick);
        Box("Front right brick base",new Vector3(X(1032.5f),.32f,Z(785)-.09f),new Vector3(3.625f,.64f,.045f),brick);
        for(int i=0;i<11;i++) {
            float h=.72f+i*.19f;
            // Narrow grooves, kept outside the window spans.
            Box("Left facade siding",new Vector3(X(640),h,Z(880)-.1f),new Vector3(.85f,.018f,.025f),siding);
            Box("Left facade siding",new Vector3(X(850),h,Z(880)-.1f),new Vector3(.85f,.018f,.025f),siding);
        }
        for(int i=0;i<12;i++) Box("Garage door panel",new Vector3(X(1197.5f),.09f+i*.175f,Z(835)),new Vector3(3.65f,.16f,.07f),concrete);
        // Entry door is parked open so the prototype remains traversable.
        var entrance=Box("Front door - open",new Vector3(X(894),1.04f,Z(880)+.49f),new Vector3(.06f,2.08f,1.0f),door);
        for(int i=0;i<3;i++) Box("Porch step",new Vector3(X(915),-.38f+i*.1f,Z(880)-.9f+i*.2f),new Vector3(2.25f,.18f,1.2f-i*.3f),brick);
        for(int side=-1;side<=1;side+=2){for(int i=0;i<3;i++)Box("Porch post",new Vector3(X(915)+side*1.05f,.4f,Z(880)-i*.4f),new Vector3(.07f,.8f,.07f),trim);Box("Porch rail",new Vector3(X(915)+side*1.05f,.72f,Z(880)-.4f),new Vector3(.08f,.07f,.95f),trim);}
        // Stairs descend into a reserved lower-level volume; basement layout is not specified.
        for(int i=0;i<12;i++)Box("Stair tread "+i,new Vector3(X(990)+i*.28f,-.1f-i*.18f,Z(265)),new Vector3(.29f,.2f,1.5f),wood);
        // Ceiling slabs and intersecting gables can be hidden to inspect the plan.
        Box("Left ceiling",new Vector3(X(745),H+.05f,Z(550)),new Vector3(6.25f,.1f,16.5f),plaster,roof);
        Box("Main ceiling",new Vector3(X(1080),H+.05f,Z(550)),new Vector3(10.5f,.1f,16.5f),plaster,roof);
        Gable("Left gable",X(605),X(885),Z(895),Z(205),H+.1f,2.2f,true);
        Gable("Main cross gable",X(860),X(1305),Z(895),Z(205),H+.1f,2.0f,false);
        Box("Ground",new Vector3(8,-.5f,8),new Vector3(60,.4f,60),grass);
        Box("Driveway",new Vector3(X(1197),-.285f,-4),new Vector3(4.6f,.035f,10),concrete);
        Box("Footpath",new Vector3(X(915),-.28f,-3),new Vector3(1.5f,.035f,6),concrete);
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.45f,.47f,.51f);
        var sun=new GameObject("Afternoon sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.15f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(42,-35,0);
        foreach(var p in new[]{new Vector3(7.3f,2.35f,7),new Vector3(12,2.35f,12),new Vector3(3,2.35f,3)}){var l=new GameObject("Warm ceiling light").AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1,.8f,.57f);l.intensity=1.5f;l.range=7;l.transform.position=p;}
        var player=new GameObject("Player - WASD mouse");player.transform.position=new Vector3(X(915),.04f,Z(740));var cc=player.AddComponent<CharacterController>();cc.height=1.75f;cc.radius=.25f;cc.center=new Vector3(0,.875f,0);cc.stepOffset=.3f;
        var camera=new GameObject("Main Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.transform.SetParent(player.transform);camera.transform.localPosition=new Vector3(0,1.62f,0);camera.nearClipPlane=.05f;camera.farClipPlane=150;camera.fieldOfView=70;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.32f,.39f,.46f);camera.gameObject.AddComponent<AudioListener>();
        var walk=player.AddComponent<HouseWalk>();walk.view=camera;walk.roof=roof.gameObject;
        PlayerSettings.productName="HorrorPrototype";
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/_Game/Scenes/House.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/_Game/Scenes/House.unity",true)};
        AssetDatabase.SaveAssets();
        // Review renders are separate from the saved game camera.
        roof.gameObject.SetActive(false);camera.transform.SetParent(null);camera.transform.position=new Vector3(8,28,8);camera.transform.rotation=Quaternion.Euler(90,0,0);camera.orthographic=true;camera.orthographicSize=10.8f;
        Render(camera,"House-plan.png");
        roof.gameObject.SetActive(true);camera.orthographic=false;camera.fieldOfView=52;camera.transform.position=new Vector3(23,8,-16);camera.transform.LookAt(new Vector3(8,1.5f,6));Render(camera,"House-exterior.png");
        Debug.Log("HOUSE_BUILD_SUCCESS");
    }
    static void Render(Camera c,string name) {
        var rt=new RenderTexture(1400,1100,24);c.targetTexture=rt;c.Render();RenderTexture.active=rt;
        var tex=new Texture2D(1400,1100,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1400,1100),0,0);tex.Apply();
        Directory.CreateDirectory("../output/house");File.WriteAllBytes("../output/house/"+name,tex.EncodeToPNG());c.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);
    }
}


