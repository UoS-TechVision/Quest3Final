using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    private int currentWidth, currentHeight;
    private GameObject prefab;
    private GameObject playerObject;
    private float previousZPosition;
    public int score;
    
    public Player() {
        currentWidth = 8;
        currentHeight = 0;
        prefab = Resources.Load<GameObject>("Prefabs/penguin");
        playerObject = Object.Instantiate(prefab, new Vector3(currentWidth * 1.6f, 0.3f, currentHeight * 1.6f), Quaternion.identity);
        previousZPosition = playerObject.transform.position.z; // Store initial Z position
        score = 0; // Initialize score
    }

    public int GetCurrentWidth() => currentWidth;
    public int GetCurrentHeight() => currentHeight;
    public GameObject GetPlayerObject() => playerObject;
    public void SetCurrentWidth() => currentWidth += 1;
    public void SetCurrentHeight() => currentHeight += 1;

    // Returns players current z position
    public float GetPlayerZPosition() {
        return playerObject.transform.position.z;
    }

    public void UpdateScore() {
        float currentZPosition = playerObject.transform.position.z;

        // Check if the player has moved forward by 1.6f
        if (currentZPosition - previousZPosition >= 1.6f) {
            score += 1;
            previousZPosition = currentZPosition; // Update to new Z position
            Debug.Log("Score: " + score);
        }
    }
}