using UnityEngine;

namespace ProjectQuest.Player
{
    /// <summary>
    /// Smoothly follows a target from a fixed overhead offset.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField]
        private Transform target;

        [SerializeField]
        private Vector3 offset = new Vector3(0f, 20f, -10f);

        [SerializeField]
        private float smoothSpeed = 5f;

        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.LookAt(target);
        }
    }
}
