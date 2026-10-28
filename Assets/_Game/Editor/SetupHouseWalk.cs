using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class SetupHouseWalk {
    [MenuItem("Horror/Prepare house walkthrough")]
    public static void Setup() { Configure("Assets/_Game/Scenes/House.unity"); }
    public static void Configure(string scenePath) {
        EditorSceneManager.OpenScene(scenePath);
        var walk=Object.FindFirstObjectByType<HouseWalk>();
        if(!walk) throw new System.Exception("Missing player controller");
        var cc=walk.GetComponent<CharacterController>();cc.enabled=false;
        walk.transform.position=new Vector3(7.375f,.04f,3.5f);walk.transform.rotation=Quaternion.identity;
        cc.height=1.75f;cc.radius=.23f;cc.center=new Vector3(0,.875f,0);cc.skinWidth=.025f;cc.stepOffset=.32f;cc.minMoveDistance=0;cc.enabled=true;
        walk.view.transform.localPosition=new Vector3(0,1.62f,0);walk.view.transform.localRotation=Quaternion.identity;
        Physics.SyncTransforms();
        if(Physics.CheckCapsule(new Vector3(7.375f,.3f,3.5f),new Vector3(7.375f,1.5f,3.5f),.2f,~0,QueryTriggerInteraction.Ignore)) {
            // The player's own controller is excluded from the clearance query below.
            cc.enabled=false;Physics.SyncTransforms();
            if(Physics.CheckCapsule(new Vector3(7.375f,.3f,3.5f),new Vector3(7.375f,1.5f,3.5f),.2f))throw new System.Exception("Spawn blocked");
            cc.enabled=true;
        }
        // Traverse real door openings with the actual CharacterController, at human height.
        Vector3 spawn=walk.transform.position;
        string[] names={"Entry hall shoulder","Left rear door return","Left bathroom door","Left front north","Service south door","Stairs south access","Storage west access","Bathroom access","Right front hall door","Garage internal door","Entry door opening"};
        Vector3[] centers={new Vector3(7.375f,0,2.375f),new Vector3(5,0,8.25f),new Vector3(3.75f,0,7),new Vector3(5.125f,0,4.875f),new Vector3(7.375f,0,14.25f),new Vector3(9.625f,0,14.25f),new Vector3(8.5f,0,10.75f),new Vector3(8.5f,0,8.375f),new Vector3(8.5f,0,3.625f),new Vector3(12.125f,0,6.375f),new Vector3(7.375f,0,0)};
        bool[] alongX={false,false,true,false,false,false,true,true,true,true,false};
        for(int i=0;i<centers.Length;i++) {
            Vector3 direction=alongX[i]?Vector3.right:Vector3.forward;
            cc.enabled=false;walk.transform.position=centers[i]-direction*.6f+Vector3.up*.04f;cc.enabled=true;Physics.SyncTransforms();
            for(int step=0;step<60;step++) cc.Move(direction*.02f);
            if(Vector3.Dot(walk.transform.position-centers[i],direction)<.35f)throw new System.Exception("Door blocked: "+names[i]);
        }
        cc.enabled=false;walk.transform.position=spawn;cc.enabled=true;
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        File.WriteAllText("../house-walk-validation.txt","PASS: indoor spawn clear; 11 doorways crossed using CharacterController; scene saved.");
        Debug.Log("HOUSE_WALK_VALIDATED");
    }
}

