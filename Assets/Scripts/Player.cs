using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    private int currentWidth, currentHeight;
    private GameObject prefab;
    private GameObject playerObject;
    public Player() {
        currentWidth = 8;
        currentHeight = 0;
        prefab = Resources.Load<GameObject>("Prefabs/penguin");
        playerObject = Object.Instantiate(prefab, new Vector3(currentWidth * 1.6f, 0.3f, currentHeight * 1.6f), Quaternion.identity);
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
    // BUG-6 fix: removed dead-code score field and UpdateScore() method.
    // Scoring is handled exclusively by GameController.
}