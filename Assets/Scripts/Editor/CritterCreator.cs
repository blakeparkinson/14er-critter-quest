using UnityEngine;
using UnityEditor;

public class CritterCreator : EditorWindow
{
    [MenuItem("14er Critter Quest/Create All Critter Data")]
    public static void CreateAllCritters()
    {
        string path = "Assets/ScriptableObjects/Critters";
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Critters");

        foreach (var template in CritterDatabase.Templates)
        {
            string assetPath = $"{path}/{template.name.Replace(" ", "_")}.asset";
            if (AssetDatabase.LoadAssetAtPath<CritterData>(assetPath) != null)
                continue;

            var data = ScriptableObject.CreateInstance<CritterData>();
            data.critterName = template.name;
            data.sillyTitle = template.sillyTitle;
            data.fieldGuideEntry = template.entry;
            data.rarity = template.rarity;
            data.personality = template.personality;
            data.sillyActions = template.sillyActions;

            AssetDatabase.CreateAsset(data, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Created {CritterDatabase.Templates.Length} critter ScriptableObjects!");
    }
}
