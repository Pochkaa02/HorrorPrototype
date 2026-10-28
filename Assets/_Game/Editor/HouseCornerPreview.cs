using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
public static class HouseCornerPreview {
    public static void Run(){
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/House.unity");
        var c=Object.FindFirstObjectByType<Camera>();c.transform.SetParent(null);c.transform.position=new Vector3(-2.8f,1.7f,-2.8f);c.transform.LookAt(new Vector3(0,.55f,0));c.fieldOfView=50;
        var rt=new RenderTexture(900,700,24);c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(900,700,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,900,700),0,0);t.Apply();File.WriteAllBytes("../output/house/House-corner.png",t.EncodeToPNG());c.targetTexture=null;RenderTexture.active=null;
    }
}
