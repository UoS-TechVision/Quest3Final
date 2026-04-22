using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    MyGrid map;
    Player player;
    private float playerZ; // Store the player's Z-axis position
    private int lastGeneratedZ = 0; // Keep track of last map generation point
    public TMP_Text scoreText;

    private int score = 0;
    private float previousZPosition;

    // Start is called before the first frame update
    void Start()
    {
        map = new MyGrid(20, 16, 3);
        player = new Player();
        playerZ = player.GetPlayerZPosition(); // Initial player Z position
        previousZPosition = playerZ; // Initialize previous position
        lastGeneratedZ = Mathf.FloorToInt(playerZ); // Initialize the last generated point
        UpdateScoreText();
    }

    void Update()
    {
        // Continuously monitor player position
        float currentZ = player.GetPlayerZPosition(); // Get the current Z position of the player

        // Check if the player has moved forward by 1.6f units
        if (currentZ - previousZPosition >= 1.6f) {
            score += 1; // Increment score by 1
            previousZPosition = currentZ; // Update previous Z position to current position
            UpdateScoreText(); // Update the score text in UI
        }

        // Check if player has moved 15 tiles ahead for map generation
        if (currentZ - lastGeneratedZ >= 24f) {
            lastGeneratedZ = Mathf.FloorToInt(currentZ); // Update last generation point
            map.SetOffset(20); // Increase map offset by 20
            map.GenerateMap();  // Generate new map chunks
        }
    }
    // Function to update the score UI
    public void UpdateScoreText() {
        scoreText.text = "Score: " + score.ToString(); // Update the score text in the UI
    }
}
