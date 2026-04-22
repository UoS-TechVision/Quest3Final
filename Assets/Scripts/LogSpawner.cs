using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogSpawner : MonoBehaviour
{
    public GameObject[] spawnableObjects;
    private float minSpawnInterval = 0.5f;    // Minimum time between spawns
    private float maxSpawnInterval = 2f;    // Maximum time between spawns
    private float spawnZOffset = 0f;        // Adjust Z position if necessary

    // Start is called before the first frame update
    void Start()
    {
        // Randomly choose an object from the array to spawn
        GameObject objectToSpawn = spawnableObjects[Random.Range(0, spawnableObjects.Length)];
        StartCoroutine(SpawnCar(objectToSpawn));
    }

    private IEnumerator SpawnCar(GameObject objectToSpawn) {
        while (true) {
            // Spawn the object at the defined spawn point
            GameObject spawnedObject = Instantiate(objectToSpawn, transform.position + new Vector3(0, 0, spawnZOffset), transform.rotation);

            // Wait for a random interval before spawning the next object
            float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}