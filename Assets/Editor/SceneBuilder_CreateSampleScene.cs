using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SceneBuilder_CreateSampleScene
{
    [MenuItem("Tools/AI/Build SampleScene (10x10)")]
    public static void BuildSampleScene()
    {
        string scenePath = "Assets/Scenes/SampleScene.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.isLoaded)
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        // Clear existing root objects except Main Camera and Directional Light if present
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != "Main Camera" && root.name != "Directional Light")
            {
                GameObject.DestroyImmediate(root);
            }
        }

        // Create AI root
        GameObject aiRoot = new GameObject("AI_Systems");
        var grid = aiRoot.AddComponent<AI.GridManager>();
        grid.Width = 10;
        grid.Height = 10;
        grid.CellSize = 1.0f;

        aiRoot.AddComponent<AI.EnemyCommunicationManager>();

        // Try to load player prefab
        string playerPath = "Assets/Survivalist/Prefab/PlayerArmature.prefab";
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPath);
        GameObject player;
        if (playerPrefab != null)
        {
            player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        }
        else
        {
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "PlayerArmature";
        }
        player.transform.position = new Vector3(0, 1, 0);
        player.tag = "Player";

        // Spawn enemies
        string enemyPath = "Assets/Survivalist/Prefab/Survivalist (2).prefab";
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPath);

        int enemyCount = 5;
        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemy;
            if (enemyPrefab != null)
            {
                enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
            }
            else
            {
                enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = "Survivalist (2)";
            }
            float x = (i % 5) - 2;
            float z = (i / 5) * 2 + 3;
            enemy.transform.position = new Vector3(x, 1, z);

            if (enemy.GetComponent<AI.EnemyController>() == null)
            {
                var ec = enemy.AddComponent<AI.EnemyController>();
                ec.Grid = grid;
            }

            // register with manager
            var comm = aiRoot.GetComponent<AI.EnemyCommunicationManager>();
            comm.RegisterEnemy(enemy.GetComponent<AI.EnemyController>());
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("SampleScene built/updated: 10x10 grid, player and enemies placed.");
    }
}
