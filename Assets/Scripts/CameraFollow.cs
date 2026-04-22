using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Vector3 _offset;
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime;
    private Vector3 _currentVelocity = Vector3.zero;

    private void Awake() {
        _offset = transform.position - target.position;
        // Look for the object with the Movement script (which is on your Penguin)
        GameObject player = GameObject.FindObjectOfType<Movement>().gameObject;
        
        if (player != null) {
            target = player.transform;
        } else {
            Debug.LogWarning("Camera couldn't find the player!");
        }
    }

    private void LateUpdate() {
        Vector3 targetPosition = target.position + _offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, smoothTime);
    }
}
