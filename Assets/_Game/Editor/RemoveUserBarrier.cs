using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class RemoveUserBarrier {
    public static void Run(){
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/House_Refined.unity");
        var barrier=GameObject.Find("User model - central barrier");
        if(barrier)Object.DestroyImmediate(barrier);
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("USER_BARRIER_REMOVED");
    }
}
