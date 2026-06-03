using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneSetup
{
    [MenuItem("14er Critter Quest/Create Bootstrap Scene (Play Immediately)")]
    public static void CreateBootstrapScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // bootstrap object
        var bootstrap = new GameObject("GameBootstrap");
        bootstrap.AddComponent<GameBootstrap>();

        // save scene
        string scenePath = "Assets/Scenes/Game.unity";
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, scenePath);

        // add to build settings
        var scenes = new EditorBuildSettingsScene[] {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = scenes;

        Debug.Log("Bootstrap scene created at Assets/Scenes/Game.unity — press Play!");
        Debug.Log("The entire game level generates at runtime. No manual setup needed.");
    }

    [MenuItem("14er Critter Quest/Setup Ground Layer")]
    public static void SetupGroundLayer()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        // find first empty user layer (8+)
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = "Ground";
                tagManager.ApplyModifiedProperties();
                Debug.Log($"Created 'Ground' layer at index {i}");
                return;
            }
        }

        Debug.LogWarning("No empty layer slots available!");
    }
}
