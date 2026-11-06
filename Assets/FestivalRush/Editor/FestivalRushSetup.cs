using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FestivalRush.EditorTools
{
    /// <summary>Menu: Festival Rush > Create Game Scene. Makes a ready-to-play scene in one click.</summary>
    public static class FestivalRushSetup
    {
        const string ScenePath = "Assets/FestivalRush/FestivalRush_Generated.unity";

        [MenuItem("Festival Rush/Create Game Scene")]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("FestivalRush");
            go.AddComponent<FestivalRush.GameManager>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuild(ScenePath);

            EditorUtility.DisplayDialog("Festival Rush",
                "Scene created and added to Build Settings.\n\nPress PLAY to run the game!", "OK");
        }

        [MenuItem("Festival Rush/Add Scenes To Build Settings")]
        public static void AddPackagedScene()
        {
            AddToBuild("Assets/FestivalRush/FestivalRush.unity");
            Debug.Log("Festival Rush: scene added to Build Settings.");
        }

        static void AddToBuild(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) return;
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            list.RemoveAll(s => s.path == path);
            list.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
