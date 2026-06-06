using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneSetup
{
    [MenuItem("14er Critter Quest/Create Top-Down Pixel Art Scene (Play Immediately)")]
    public static void CreateTopDownScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // remove directional light (not needed for 2D)
        var light = Object.FindFirstObjectByType<Light>();
        if (light != null) Object.DestroyImmediate(light.gameObject);

        var bootstrap = new GameObject("TopDownBootstrap");
        bootstrap.AddComponent<TopDownBootstrap>();

        string scenePath = "Assets/Scenes/TopDown.unity";
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, scenePath);

        var scenes = new EditorBuildSettingsScene[] {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = scenes;

        Debug.Log("Top-down pixel art scene created! Press Play.");
    }
}
