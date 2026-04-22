using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGrid
{
    // Instantiate basic variables
    private int height;
    private int width;
    private int offset;
    private GameObject prefab;
    
    // Constructor
    public MyGrid(int height, int width, int offset) {
        this.height = height;
        this.width = width;
        this.offset = offset;
        this.StartArea();
        this.GenerateMap();
    }

    public int GetHeight() => height;
    public int GetWidth() => width;
    public int GetOffset() => offset;
    public void SetOffset(int addOffset) => this.offset += addOffset;

    public void StartArea() {
        prefab = Resources.Load<GameObject>($"Prefabs/start-area");
        Object.Instantiate(prefab, new Vector3(12.8f, 0, 0), Quaternion.identity);
    }
    public void GenerateMap() {
        int[] chunk = GenerateChunk();
        string[] row = {"light-grass", "light-road", "rail", "light-river"}; //ight-grass
        for(int z = this.GetOffset(); z < this.GetHeight() + this.GetOffset(); z++) {
            for (int x = 0; x < this.GetWidth(); x++) {
                prefab = Resources.Load<GameObject>($"Prefabs/{row[chunk[z - this.GetOffset()] - 1]}");
                if (chunk[z - this.GetOffset()] == 2) {
                    Object.Instantiate(prefab, new Vector3(x * 1.6f + 0.8f, 0, z * 1.6f + 0.8f), Quaternion.identity);
                    x++;
                } else {
                    Object.Instantiate(prefab, new Vector3(x * 1.6f, 0, z * 1.6f), Quaternion.identity);
                    placeObstacle(chunk[z - this.GetOffset()],x, z);
                }

                if (x == this.GetWidth() - 1 && chunk[z - this.GetOffset()] != 1) {
                    InstantiateSpawners(x, z, chunk);
                }
            }
            if (chunk[z - this.GetOffset()] == 2) {
                z++;
            }
        }
    }

    // Instantiates object spawner
    private void InstantiateSpawners(int x, int z, int[] chunk) {
        float spawnX = Random.Range(0, 2) == 0 ? x * 1.6f + 5f : -5f;
        float destroyX = spawnX == -5f ? x * 1.6f + 30f : -30f;
        Quaternion spawnRotation;

        if (chunk[z - this.GetOffset()] == 2) {
            prefab = Resources.Load<GameObject>("Prefabs/DestroyObject");
            Object.Instantiate(prefab, new Vector3(destroyX, 0, z * 1.6f + 1.6f), Quaternion.identity);
            prefab = Resources.Load<GameObject>("Prefabs/car-spawner");
            spawnRotation = spawnX == -5f ? Quaternion.Euler(0, 180f, 0) : Quaternion.identity;
            Object.Instantiate(prefab, new Vector3(spawnX, 0, z * 1.6f + 1.6f), spawnRotation);
        } else if (chunk[z - this.GetOffset()] == 3 ) {
            prefab = Resources.Load<GameObject>("Prefabs/train-spawner");
        } else if (chunk[z - this.GetOffset()] == 4 ) {
            prefab = Resources.Load<GameObject>("Prefabs/log-spawner");
        }

        // Instantiates spawner and adjusts rotation
        spawnX = Random.Range(0, 2) == 0 ? x * 1.6f + 5f : -5f;
        spawnRotation = spawnX == -5f ? Quaternion.Euler(0, 180f, 0) : Quaternion.identity;
        Object.Instantiate(prefab, new Vector3(spawnX, 0, z * 1.6f), spawnRotation);

        // Instantiate gameobject which destroys
        destroyX = spawnX == -5f ? x * 1.6f + 30f : -30f;
        GameObject destroyPrefab = Resources.Load<GameObject>("Prefabs/DestroyObject");
        Object.Instantiate(destroyPrefab, new Vector3(destroyX, 0, z * 1.6f), Quaternion.identity);
    }

    // Generates a number string where numbers 1234 represent a terrain
    // 1: light-grasss
    // 2: light-road
    // 3: rail
    // 4: light-river
    private int[] GenerateChunk() {
        System.Random random = new System.Random();
        int length = GetHeight(), i = 0;
        int[] numberArray = new int[length];

        while (i < length) {
            int digit = random.Next(1, 5);

            if (digit == 2) {
                if (i < length - 1) {
                    numberArray[i] = 2;
                    numberArray[i + 1] = 2;
                    i += 2; 
                }
            } else if (digit == 4) {
                if (i < length - 1) {
                    numberArray[i] = 4;
                    i ++;
                }
            } else {
                numberArray[i] = digit;
                i++;
            }
        }
        return numberArray;
    }

    private void placeObstacle(int terrain, int x, int z) {
        if (terrain == 1) {
            System.Random random = new System.Random();
            float chance = random.Next(0, 100); 

            // 35% chance of an obstacle appearing on grass
            if (chance < 5) {
                prefab = Resources.Load<GameObject>($"Prefabs/rock");
                Object.Instantiate(prefab, new Vector3(x * 1.6f, 0.2f, z * 1.6f), Quaternion.identity);
            } else if (chance >= 5 && chance < 15) {
                prefab = Resources.Load<GameObject>($"Prefabs/small-tree");
                Object.Instantiate(prefab, new Vector3(x * 1.6f, 1.1f, z * 1.6f), Quaternion.identity);
            } else if (chance >= 15 && chance < 25) {
                prefab = Resources.Load<GameObject>($"Prefabs/med-tree");
                Object.Instantiate(prefab, new Vector3(x * 1.6f, 1.1f, z * 1.6f), Quaternion.identity);
            } else if (chance >= 25 && chance < 35) {
                prefab = Resources.Load<GameObject>($"Prefabs/big-tree"); //big-tree
                Object.Instantiate(prefab, new Vector3(x * 1.6f, 1.1f, z * 1.6f), Quaternion.identity);
            }
        }
    }

    // Add death and point system
    // Tweak car, train and log speeds
    // Add train and car collisions
    // done with the basic game

    // EXTRA FEATURES
    // Implement better camera
    // Traffic light
    // Lilypads
    // Sound effects
}
