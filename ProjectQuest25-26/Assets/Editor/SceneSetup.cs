using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectQuest.Levels;
using ProjectQuest.Player;

/// <summary>
/// Editor script to set up the SampleScene with TilemapGenerator and camera positioning.
/// </summary>
public static class SceneSetup
{
    [MenuItem("ProjectQuest/Setup Ice Age Level", validate = true)]
    public static bool SetupIceAgeLevelValidate() => !EditorApplication.isPlaying;

    [MenuItem("ProjectQuest/Setup Ice Age Level")]
    public static void SetupIceAgeLevel()
    {
        // Load SampleScene if not already loaded
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

        if (!scene.isLoaded)
        {
            Debug.LogError("Failed to load SampleScene.unity");
            return;
        }

        // Find or create TilemapGenerator GameObject
        GameObject tilemapGenGO = GameObject.Find("TilemapGenerator");
        if (tilemapGenGO == null)
        {
            tilemapGenGO = new GameObject("TilemapGenerator");
            EditorSceneManager.MarkSceneDirty(scene);
        }

        // Ensure the TilemapGenerator component is attached
        TilemapGenerator tilemapGen = tilemapGenGO.GetComponent<TilemapGenerator>();
        if (tilemapGen == null)
        {
            tilemapGen = tilemapGenGO.AddComponent<TilemapGenerator>();
        }

        // Wire up prefab references
        GameObject snowTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/snow_tile.prefab");
        GameObject iceTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ice_tile.prefab");

        if (snowTilePrefab == null || iceTilePrefab == null)
        {
            Debug.LogError("Could not find snow_tile.prefab or ice_tile.prefab in Assets/Prefabs/");
            return;
        }

        // Set serialized fields via SerializedObject so Unity properly saves them to the scene
        SerializedObject so = new SerializedObject(tilemapGen);
        so.FindProperty("snowTilePrefab").objectReferenceValue = snowTilePrefab;
        so.FindProperty("iceTilePrefab").objectReferenceValue = iceTilePrefab;
        so.ApplyModifiedProperties();

        // Place penguin at the centre of the generated tilemap
        GameObject penguinGO = GameObject.Find("penguin");
        if (penguinGO == null)
        {
            GameObject penguinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/penguin.prefab");
            if (penguinPrefab != null)
            {
                // Derive centre from the TilemapGenerator's serialized grid settings
                SerializedObject tilemapSo = new SerializedObject(tilemapGen);
                float gridWidth = tilemapSo.FindProperty("gridWidth").intValue;
                float gridDepth = tilemapSo.FindProperty("gridDepth").intValue;
                float tileSize  = tilemapSo.FindProperty("tileSize").floatValue;

                float centerX = (gridWidth * tileSize) / 2f;
                float centerZ = (gridDepth * tileSize) / 2f;

                penguinGO = PrefabUtility.InstantiatePrefab(penguinPrefab) as GameObject;
                penguinGO.transform.position = new Vector3(centerX, 2f, centerZ);
                EditorSceneManager.MarkSceneDirty(scene);
            }
            else
            {
                Debug.LogWarning("penguin.prefab not found in Assets/Prefabs/ — skipping placement.");
            }
        }

        // Ensure a Main Camera exists with CameraFollow targeting the penguin
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            mainCamera = cameraGO.AddComponent<Camera>();
            cameraGO.AddComponent<AudioListener>();
        }

        CameraFollow follow = mainCamera.GetComponent<CameraFollow>() ?? mainCamera.gameObject.AddComponent<CameraFollow>();

        if (penguinGO != null)
        {
            SerializedObject cameraSo = new SerializedObject(follow);
            cameraSo.FindProperty("target").objectReferenceValue = penguinGO.transform;
            cameraSo.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(scene);

        // Frame the Scene view over the centre of the tilemap (250×250 world units)
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            // Pivot at grid centre, angled down so the full grid is visible
            sceneView.pivot = new Vector3(125f, 0f, 125f);
            sceneView.rotation = Quaternion.Euler(50f, 0f, 0f);
            sceneView.size = 180f; // orthographic-equivalent zoom to fit 250-unit grid
            sceneView.Repaint();
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("Ice Age Level setup complete! Scene view framed over tilemap. Ready to generate via context menu.");
    }
}
