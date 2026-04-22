using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCollide : MonoBehaviour
{
    public GameObject canvas; // Assign your canvas in the Unity editor

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit something: " + collision.gameObject.name + " with tag: " + collision.gameObject.tag);
        // Check if the player collides with an object tagged "Vehicle"
        if (collision.gameObject.CompareTag("Vehicle"))
        {
            // Show the restart button
            canvas.SetActive(true);
            
            // Pause the game
            Time.timeScale = 0f;
        }
    }
}
