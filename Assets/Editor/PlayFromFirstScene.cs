using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class PlayFromFirstScene
{
    static PlayFromFirstScene()
    {
        // 設定在按下 Play 時，自動載入 Build Settings 中編號為 0 的場景
        var scene = EditorBuildSettings.scenes[0];
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
    }
}