using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainSpawner : MonoBehaviour
{
    public GameObject[] spawnableObjects;
    private float minSpawnInterval = 5f;    // Minimum time between spawns
    private float maxSpawnInterval = 10f;    // Maximum time between spawns
    private float spawnXOffset = 30f;        // Adjust Z position if necessary

    // Start is called before the first frame update
    void Start()
    {
        // Randomly choose an object from the array to spawn
        GameObject objectToSpawn = spawnableObjects[Random.Range(0, spawnableObjects.Length)];
        StartCoroutine(SpawnCar(objectToSpawn));
    }

    private IEnumerator SpawnCar(GameObject objectToSpawn) {
        while (true) {
            // Adjust offset based on the spawner's facing direction
            float offset = (transform.rotation.eulerAngles.y == 180f) ? -spawnXOffset : spawnXOffset;

            // Spawn the object with the calculated offset and rotation
            GameObject spawnedObject = Instantiate(objectToSpawn, transform.position + new Vector3(offset, 0, 0), transform.rotation);

            // Wait for a random interval before spawning the next object
            float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}