using UnityEngine;

namespace ProjectQuest.Levels
{
    /// <summary>
    /// Procedurally generates a 3D tilemap grid mixing snow and ice tiles on the XZ plane.
    /// </summary>
    public class TilemapGenerator : MonoBehaviour
    {
        [SerializeField]
        private GameObject snowTilePrefab;

        [SerializeField]
        private GameObject iceTilePrefab;

        [SerializeField]
        private int gridWidth = 50;

        [SerializeField]
        private int gridDepth = 50;

        [SerializeField]
        [Range(0f, 1f)]
        private float iceRatio = 0.3f;

        [SerializeField]
        private int randomSeed = 42;

        [SerializeField]
        private float tileSize = 5f;

        private GameObject _tilemapRoot;

        private void Start()
        {
            GenerateTilemap();
        }

        /// <summary>
        /// Generates the tilemap by instantiating tiles in a grid pattern.
        /// </summary>
        [ContextMenu("Generate Tilemap")]
        public void GenerateTilemap()
        {
            if (snowTilePrefab == null || iceTilePrefab == null)
            {
                Debug.LogError(
                    "TilemapGenerator: Both snowTilePrefab and iceTilePrefab must be assigned.");
                return;
            }

            // Clear existing tilemap
            ClearTilemap();

            // Create or find TilemapRoot
            _tilemapRoot = new GameObject("TilemapRoot");
            _tilemapRoot.transform.SetParent(transform);
            _tilemapRoot.transform.localPosition = Vector3.zero;

            // Create deterministic random generator
            System.Random rng = new System.Random(randomSeed);

            // Generate grid
            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    // Decide which tile to place
                    GameObject prefabToUse = rng.NextDouble() < iceRatio ? iceTilePrefab : snowTilePrefab;

                    // Calculate world position
                    Vector3 position = new Vector3(x * tileSize, 0, z * tileSize);

                    // Instantiate and parent
                    GameObject tile = Instantiate(prefabToUse, position, Quaternion.identity, _tilemapRoot.transform);
                    tile.name = $"Tile_{x}_{z}";
                }
            }

            Debug.Log($"TilemapGenerator: Generated {gridWidth}×{gridDepth} tilemap ({gridWidth * gridDepth} tiles total).");
        }

        /// <summary>
        /// Clears all tiles by destroying the TilemapRoot.
        /// </summary>
        [ContextMenu("Clear Tilemap")]
        public void ClearTilemap()
        {
            if (_tilemapRoot != null)
            {
                DestroyImmediate(_tilemapRoot);
                _tilemapRoot = null;
                Debug.Log("TilemapGenerator: Tilemap cleared.");
            }
            else
            {
                // Also check for existing TilemapRoot in children
                Transform existingRoot = transform.Find("TilemapRoot");
                if (existingRoot != null)
                {
                    DestroyImmediate(existingRoot.gameObject);
                    Debug.Log("TilemapGenerator: Existing TilemapRoot cleared.");
                }
            }
        }
    }
}
