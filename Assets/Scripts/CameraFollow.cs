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
        // BUG-5 fix: find the actual player first, then compute the offset from it.
        // The original code computed the offset from the serialised Inspector reference
        // before reassigning target to the player, giving a permanently wrong offset
        // whenever those two objects were not co-located.
        GameObject player = GameObject.FindObjectOfType<Movement>().gameObject;

        if (player != null) {
            target = player.transform;
        } else {
            Debug.LogWarning("Camera couldn't find the player!");
        }

        _offset = transform.position - target.position;
    }

    private void LateUpdate() {
        Vector3 targetPosition = target.position + _offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, smoothTime);
    }
}
