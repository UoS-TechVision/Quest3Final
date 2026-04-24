using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private bool isMoving;
    private Vector3 origPos, targetPos, playerOrigPos, playerTargerPos;
    private float timeToMove = 0.125f;
    private float dist = 1.6f; //Adjust this to match your grid size
    public LayerMask obstacleLayer; //Layer for obstacles
    public LayerMask logLayer; // Layer for logs
    public LayerMask waterLayer;
    private ObjMovement currentLog = null; // Reference to the ObjMovement script on the log
    public float abilityRadius = 2.0f; // How far the "shiver" reaches
    private float lastAbilityTime = -5f; // Track cooldown
    public float cooldownTime = 3f; // Seconds between uses
    void Update() {
        // Prevents multiple coroutines to occur at the same time
        if(isMoving) {
            return;
        }
        // Check for key releases and trigger movement
        if (Input.GetKey(KeyCode.W)) {
            if (CanMove(Vector3.forward)) {
                StartCoroutine(MovePlayer(new Vector3(0, 0, dist)));
            }
        }
        else if (Input.GetKey(KeyCode.S)) {
            if (transform.position.z > 0f && CanMove(Vector3.back)) {
                StartCoroutine(MovePlayer(new Vector3(0, 0, -dist)));
            }
        }
        else if (Input.GetKey(KeyCode.A)) {
            if (CanMove(Vector3.left)) {
                StartCoroutine(MovePlayer(new Vector3(-dist, 0, 0)));
            }
        }
        else if (Input.GetKey(KeyCode.D)) {
            if (CanMove(Vector3.right)) {
                StartCoroutine(MovePlayer(new Vector3(dist, 0, 0)));
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (Time.time >= lastAbilityTime + cooldownTime) {
                UseSpecialAbility();
                lastAbilityTime = Time.time;
            } else {
                Debug.Log("Ability is on cooldown!");
            }
        }

         // If player is on a log, move with the log using its manual speed
        if (currentLog != null) {
            // Adjust movement based on the log's direction
            Vector3 logMovementDirection = currentLog.transform.right; // Assume logs move along their local X-axis
            transform.position += logMovementDirection * -currentLog.speed * Time.deltaTime;

            // BUG-3 fix: if the log has carried the player outside the map's X boundary, trigger death.
            if (transform.position.x < MapMinX || transform.position.x > MapMaxX) {
                Die("Carried off map by log");
                return;
            }

            DetectLogUnderneath();
        }
    }

    private void UseSpecialAbility() {
        Debug.Log("Ice Burst! Clearing obstacles...");

        // Find all colliders within the radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, abilityRadius, obstacleLayer);

        foreach (var hitCollider in hitColliders) {
            // Destroy the obstacle
            // Note: We use hitCollider.gameObject.transform.root to make sure we 
            // destroy the whole prefab, not just one branch of it.
            Destroy(hitCollider.gameObject.transform.root.gameObject);
            
            Debug.Log("Destroyed: " + hitCollider.name);
        }
        StartCoroutine(FlashPenguin());
        // Optional: Add a visual effect or sound here later!
    }

    private IEnumerator FlashPenguin() {
        Transform model = transform.Find("default");
        model.localScale *= 1.5f; // Make it big for a moment
        yield return new WaitForSeconds(0.1f);
        model.localScale /= 1.5f; // Back to normal
    }
    // Map bounds: 16 cells wide, each cell is 1.6f. Valid X range is [0, 15 * 1.6f].
    private const float MapMinX = 0f;
    private const float MapMaxX = 15 * 1.6f; // 24f

    private bool CanMove(Vector3 direction) {
        // BUG-1 fix: reject moves that would take the player outside the left/right map boundary.
        Vector3 candidate = transform.position + direction;
        if (candidate.x < MapMinX || candidate.x > MapMaxX) {
            return false;
        }

        // Perform a raycast in the desired direction to detect obstacles
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, dist, obstacleLayer)) {
            // If the raycast hits something, the player can't move
            Debug.Log("Obstacle in the way: " + hit.collider.name);
            return false;
        }
        // No obstacle detected, allow movement
        return true;
    }
    
    private IEnumerator MovePlayer(Vector3 direction) {
        isMoving = true;
        float elapsedTime = 0;
        
        origPos = transform.position;
        targetPos = origPos + direction;

        // Get the child model
        Transform model = transform.Find("default");
        Vector3 modelOrigLocalPos = model.localPosition;

        while(elapsedTime < timeToMove) {
            float lerpFactor = elapsedTime / timeToMove;
            
            // Move the parent root forward
            transform.position = Vector3.Lerp(origPos, targetPos, lerpFactor);
            
            // Create a "hop" by moving the child up and then back down using a sine wave
            float hopHeight = Mathf.Sin(lerpFactor * Mathf.PI) * 0.5f;
            model.localPosition = new Vector3(modelOrigLocalPos.x, modelOrigLocalPos.y + hopHeight, modelOrigLocalPos.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Finalize position and reset model height
        transform.position = SnapToGrid(targetPos);
        model.localPosition = modelOrigLocalPos; // Ensure penguin is back on the ground

        DetectLogUnderneath();
        isMoving = false;
    }

    private Vector3 SnapToGrid(Vector3 position) {
        // Round the x and z coordinates to the nearest multiple of 1.6
        float snappedX = Mathf.Round(position.x / 1.6f) * 1.6f;
        float snappedZ = Mathf.Round(position.z / 1.6f) * 1.6f;
        
        // Return the new position, y remains the same
        return new Vector3(snappedX, position.y, snappedZ);
    }

    // private void DetectLogUnderneath() {
    //     RaycastHit hit;
    //     if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f, logLayer)) {
    //         ObjMovement log = hit.collider.GetComponent<ObjMovement>();

    //         if (log != null) {
    //             SnapLog(log);
    //             currentLog = log; // Store the log's ObjMovement reference
    //         }
    //     } else {
    //         currentLog = null; // If no log is detected, set currentLog to null
    //     }
    // }
    private void DetectLogUnderneath() {
        RaycastHit hit;
        
        // 1. Check for logs first (safety)
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f, logLayer)) {
            ObjMovement log = hit.collider.GetComponent<ObjMovement>();
            if (log != null) {
                SnapLog(log);
                currentLog = log;
            }
        } 
        else {
            currentLog = null; 

            // 2. If NOT on a log, check if we are on water
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f, waterLayer)) {
                // We are on a water tile without a log!
                Die("Drowned");
            }
        }
    }

// Simple death function to trigger the UI
    private void Die(string message) {
        Debug.Log(message);
        Time.timeScale = 0f; // Freeze the game
        // Find the VehicleCollide script to reuse its canvas activation
        GetComponent<VehicleCollide>().canvas.SetActive(true);
    }

    private void SnapLog(ObjMovement log) {
        Vector3 logPosition = log.transform.position;
        Vector3 snappedPosition = new Vector3(logPosition.x, transform.position.y, transform.position.z); // Adjust only X
        transform.position = snappedPosition;
    }
}
