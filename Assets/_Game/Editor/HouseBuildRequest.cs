using UnityEditor;
using System.IO;
[InitializeOnLoad]
public static class HouseBuildRequest {
    static HouseBuildRequest() { EditorApplication.update += Check; }
    static void Check() {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if(!File.Exists("BuildHouse.request")) return;
        File.Delete("BuildHouse.request");
        try { BuildReferenceHouse.Build(); EditorSceneManagerCompat(); }
        catch(System.Exception e) { UnityEngine.Debug.LogException(e); File.WriteAllText("../house-build-error.txt",e.ToString()); }
    }
    public static void Open() { EditorSceneManagerCompat(); }
    static void EditorSceneManagerCompat() {
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(File.Exists("Assets/_Game/Scenes/House_Refined.unity") ? "Assets/_Game/Scenes/House_Refined.unity" : "Assets/_Game/Scenes/House.unity");
        if(SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.LookAt(new UnityEngine.Vector3(8,0,8),UnityEngine.Quaternion.Euler(65,0,0),24);
    }
}


