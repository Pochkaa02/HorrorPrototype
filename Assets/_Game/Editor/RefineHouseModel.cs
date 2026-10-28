using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public static class RefineHouseModel {
    struct Volume { public Vector3 min,max; public Volume(Vector3 a,Vector3 b){min=a;max=b;} }
    static string folder="Assets/_Game/Models/";
    static Transform root;
    [MenuItem("Horror/Refine entire house")]
    public static void Run(){
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/House.unity");
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/_Game/Scenes/House_Refined.unity");
        root=GameObject.Find("HOUSE - reference blockout").transform;
        var shell=root.Find("Walls and windows");
        var volumes=new List<Volume>();
        foreach(var box in shell.GetComponentsInChildren<BoxCollider>()){
            Bounds b=box.bounds;volumes.Add(new Volume(b.min,b.max));
        }
        if(volumes.Count==0)throw new System.Exception("Missing wall volumes");
        // Complete the shared corner square at every perpendicular junction.
        // This removes the half-thickness step left by wall centerline endpoints.
        var original=volumes.ToArray();
        for(int i=0;i<volumes.Count;i++){
            Volume a=volumes[i];bool ax=(a.max.x-a.min.x)<.2f;
            foreach(var b in original){
                bool bx=(b.max.x-b.min.x)<.2f;if(ax==bx || a.min.y>=b.max.y || a.max.y<=b.min.y)continue;
                int along=ax?2:0,across=ax?0:2;
                if(a.max[across]<b.min[across]-.001f || a.min[across]>b.max[across]+.001f)continue;
                if(a.min[along]>=b.min[along]-.001f && a.min[along]<=b.max[along]+.001f)a.min[along]=Mathf.Min(a.min[along],b.min[along]);
                if(a.max[along]>=b.min[along]-.001f && a.max[along]<=b.max[along]+.001f)a.max[along]=Mathf.Max(a.max[along],b.max[along]);
            }
            volumes[i]=a;
        }
        var wall=Material("Refined plaster",new Color(.72f,.70f,.65f));
        Mesh mesh=Union(volumes,"Unified-walls");
        Object.DestroyImmediate(shell.gameObject);
        var walls=Create("Walls - continuous joined shell",mesh,wall,root,true);
        // Replace every overlapping floor plate with one watertight footprint.
        string[] replaceNames={"Left rear room","Left wet room","Left front room","Hall and entry","Entry recess","Main hall","Rear service room","Stairwell","Rear right passage","Rear right landing","Right rear room","Center storage","Center bathroom","Right front room","Garage","Concrete foundation"};
        foreach(string replaceName in replaceNames){var previous=root.Find(replaceName);if(previous)Object.DestroyImmediate(previous.gameObject);}
        var footprints=new[]{new Vector4(-.08f,8.58f,-.08f,16.58f),new Vector4(8.58f,12.045f,2.295f,16.58f),new Vector4(12.045f,16.83f,1.045f,16.58f)};
        var floorVolumes=new List<Volume>();var baseVolumes=new List<Volume>();
        foreach(var r in footprints){floorVolumes.Add(new Volume(new Vector3(r.x,-.16f,r.z),new Vector3(r.y,0,r.w)));baseVolumes.Add(new Volume(new Vector3(r.x,-.8f,r.z),new Vector3(r.y,-.16f,r.w)));}
        var floorMat=Material("Blockout floor",new Color(.48f,.46f,.41f));var baseMat=Material("Refined foundation",new Color(.36f,.37f,.36f));
        Create("Floor - continuous slab",Union(floorVolumes,"Unified-floor"),floorMat,root,true);
        Create("Foundation - continuous perimeter",Union(baseVolumes,"Unified-foundation"),baseMat,root,true);
        RefineRoof();
        var walk=Object.FindFirstObjectByType<HouseWalk>();var cc=walk.GetComponent<CharacterController>();cc.enabled=false;walk.transform.position=new Vector3(7.375f,.04f,3.5f);cc.enabled=true;
        Physics.SyncTransforms();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        Capture(walk.view);
        SetupHouseWalk.Configure("Assets/_Game/Scenes/House_Refined.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/_Game/Scenes/House_Refined.unity",true)};
        Debug.Log("REFINED_HOUSE_SAVED: wall union, continuous floor and foundation, clipped roof intersections.");
    }
    static Material Material(string name,Color color){string p="Assets/_Game/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,p);}m.color=color;m.SetFloat("_Glossiness",0);return m;}
    static GameObject Create(string name,Mesh mesh,Material material,Transform parent,bool collision){var g=new GameObject(name);g.transform.SetParent(parent);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=material;if(collision)g.AddComponent<MeshCollider>().sharedMesh=mesh;g.isStatic=true;return g;}
    static Mesh Save(Mesh mesh,string name){mesh.name=name;string p=folder+name+".asset";AssetDatabase.DeleteAsset(p);AssetDatabase.CreateAsset(mesh,p);return mesh;}
    static float Round(float f){return Mathf.Round(f*10000)/10000;}
    static Mesh Union(List<Volume> boxes,string name){
        var axes=new float[3][];
        for(int a=0;a<3;a++) axes[a]=boxes.SelectMany(b=>new[]{Round(b.min[a]),Round(b.max[a])}).Distinct().OrderBy(v=>v).ToArray();
        int nx=axes[0].Length-1,ny=axes[1].Length-1,nz=axes[2].Length-1;
        var filled=new bool[nx,ny,nz];
        for(int x=0;x<nx;x++)for(int y=0;y<ny;y++)for(int z=0;z<nz;z++){
            Vector3 p=new Vector3((axes[0][x]+axes[0][x+1])/2,(axes[1][y]+axes[1][y+1])/2,(axes[2][z]+axes[2][z+1])/2);
            filled[x,y,z]=boxes.Any(b=>p.x>b.min.x-.00001f && p.x<b.max.x+.00001f && p.y>b.min.y-.00001f && p.y<b.max.y+.00001f && p.z>b.min.z-.00001f && p.z<b.max.z+.00001f);
        }
        var vertices=new List<Vector3>();var triangles=new List<int>();
        // Greedily merge exposed cells on every plane into large coplanar quads.
        for(int a=0;a<3;a++){
            int u=(a+1)%3,v=(a+2)%3;int na=axes[a].Length-1,nu=axes[u].Length-1,nv=axes[v].Length-1;
            for(int plane=0;plane<=na;plane++)for(int sign=-1;sign<=1;sign+=2){
                var mask=new bool[nu,nv];
                for(int i=0;i<nu;i++)for(int j=0;j<nv;j++){
                    int[] cell=new int[3];cell[u]=i;cell[v]=j;int inner=sign>0?plane-1:plane;int outer=sign>0?plane:plane-1;
                    cell[a]=inner;bool inside=inner>=0&&inner<na&&filled[cell[0],cell[1],cell[2]];
                    cell[a]=outer;bool outside=outer>=0&&outer<na&&filled[cell[0],cell[1],cell[2]];
                    mask[i,j]=inside&&!outside;
                }
                for(int i=0;i<nu;i++)for(int j=0;j<nv;j++)if(mask[i,j]){
                    int w=1,h=1;while(i+w<nu&&mask[i+w,j])w++;
                    bool grow=true;while(j+h<nv&&grow){for(int k=0;k<w;k++)if(!mask[i+k,j+h])grow=false;if(grow)h++;}
                    for(int k=0;k<w;k++)for(int l=0;l<h;l++)mask[i+k,j+l]=false;
                    Vector3 p=Vector3.zero;p[a]=axes[a][plane];p[u]=axes[u][i];p[v]=axes[v][j];
                    Vector3 du=Vector3.zero,dv=Vector3.zero;du[u]=axes[u][i+w]-p[u];dv[v]=axes[v][j+h]-p[v];
                    int start=vertices.Count;vertices.Add(p);vertices.Add(p+du);vertices.Add(p+du+dv);vertices.Add(p+dv);
                    if(sign>0)triangles.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});else triangles.AddRange(new[]{start,start+2,start+1,start,start+3,start+2});
                }
            }
        }
        var mesh=new Mesh();mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        return Save(mesh,name);
    }
    static void RefineRoof(){
        var roof=root.Find("Roof - R to toggle in Play");
        var sources=roof.GetComponentsInChildren<MeshFilter>().Where(f=>f.name=="Left roof"||f.name=="Main roof").ToArray();
        if(sources.Length!=2)throw new System.Exception("Expected the two roof prisms");
        var planes=new List<Plane>[2];
        for(int s=0;s<2;s++){
            planes[s]=new List<Plane>();var mesh=sources[s].sharedMesh;var v=mesh.vertices;var indices=mesh.triangles;
            for(int i=0;i<indices.Length;i+=3){var p=new Plane(v[indices[i]],v[indices[i+1]],v[indices[i+2]]);if(!planes[s].Any(q=>Vector3.Dot(q.normal,p.normal)>.9999f&&Mathf.Abs(q.distance-p.distance)<.001f))planes[s].Add(p);}
        }
        var vertices=new List<Vector3>();var slopes=new List<int>();var caps=new List<int>();
        for(int s=0;s<2;s++){
            var m=sources[s].sharedMesh;var verts=m.vertices;
            for(int sub=0;sub<m.subMeshCount;sub++){
                var indices=m.GetTriangles(sub);
                for(int i=0;i<indices.Length;i+=3){
                    var remainder=new List<Vector3>{verts[indices[i]],verts[indices[i+1]],verts[indices[i+2]]};
                    foreach(var plane in planes[1-s]){
                        var outside=Clip(remainder,plane,false);Emit(outside,vertices,sub==0?slopes:caps);
                        remainder=Clip(remainder,plane,true);if(remainder.Count<3)break;
                    }
                }
            }
        }
        var roofMesh=new Mesh();roofMesh.SetVertices(vertices);roofMesh.subMeshCount=2;roofMesh.SetTriangles(slopes,0);roofMesh.SetTriangles(caps,1);roofMesh.RecalculateNormals();roofMesh.RecalculateBounds();Save(roofMesh,"Unified-roof");
        for(int i=roof.childCount-1;i>=0;i--){var t=roof.GetChild(i);if(!t.name.Contains("ceiling"))Object.DestroyImmediate(t.gameObject);}
        var g=new GameObject("Roof - clean intersecting slopes");g.transform.SetParent(roof);g.AddComponent<MeshFilter>().sharedMesh=roofMesh;g.AddComponent<MeshRenderer>().sharedMaterials=new[]{Material("Refined roof",new Color(.23f,.25f,.27f)),AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/Refined plaster.mat")};
        // Join overlapping ceiling plates as well.
        var ceilingVolumes=new List<Volume>();foreach(var c in roof.GetComponentsInChildren<BoxCollider>()){ceilingVolumes.Add(new Volume(c.bounds.min,c.bounds.max));}
        for(int i=roof.childCount-1;i>=0;i--)if(roof.GetChild(i).name.Contains("ceiling"))Object.DestroyImmediate(roof.GetChild(i).gameObject);
        if(ceilingVolumes.Count>0)Create("Continuous ceiling",Union(ceilingVolumes,"Unified-ceiling"),AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/Refined plaster.mat"),roof,true);
    }
    static List<Vector3> Clip(List<Vector3> polygon,Plane plane,bool inside){
        var output=new List<Vector3>();if(polygon.Count==0)return output;
        for(int i=0;i<polygon.Count;i++){
            Vector3 a=polygon[i],b=polygon[(i+1)%polygon.Count];float da=plane.GetDistanceToPoint(a),db=plane.GetDistanceToPoint(b);
            bool ia=inside?da<=.000001f:da>=-.000001f,ib=inside?db<=.000001f:db>=-.000001f;
            if(ia)output.Add(a);if(ia!=ib)output.Add(Vector3.LerpUnclamped(a,b,da/(da-db)));
        }return output;
    }
    static void Emit(List<Vector3> polygon,List<Vector3> vertices,List<int> triangles){
        if(polygon.Count<3)return;
        for(int i=1;i<polygon.Count-1;i++){if(Vector3.Cross(polygon[i]-polygon[0],polygon[i+1]-polygon[0]).sqrMagnitude<.00000001f)continue;int n=vertices.Count;vertices.Add(polygon[0]);vertices.Add(polygon[i]);vertices.Add(polygon[i+1]);triangles.AddRange(new[]{n,n+1,n+2});}
    }
    static void Capture(Camera camera){
        camera.transform.SetParent(null);camera.transform.position=new Vector3(25,12,-21);camera.transform.LookAt(new Vector3(8,1,7));camera.fieldOfView=45;Render(camera,"House-refined.png");
        root.Find("Roof - R to toggle in Play").gameObject.SetActive(false);camera.transform.position=new Vector3(8.375f,30,8.25f);camera.transform.rotation=Quaternion.Euler(90,0,0);camera.orthographic=true;camera.orthographicSize=10.2f;Render(camera,"House-refined-plan.png");
    }
    static void Render(Camera c,string name){var rt=new RenderTexture(1400,1100,24);c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(1400,1100,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1400,1100),0,0);t.Apply();File.WriteAllBytes("../output/house/"+name,t.EncodeToPNG());c.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(t);}
}




