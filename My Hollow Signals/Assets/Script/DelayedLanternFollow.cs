using UnityEngine;

public class DelayedLanternFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The target transform to follow (usually the camera or head)")]
    public Transform target;

    [Header("Position Settings")]
    [Tooltip("Follow target position")]
    public bool followPosition = true;

    [Tooltip("How quickly the lantern follows position (lower = more delay)")]
    [Range(0.1f, 10f)]
    public float positionFollowSpeed = 2f;

    [Tooltip("Position offset from target")]
    public Vector3 positionOffset = Vector3.zero;

    [Header("Rotation Settings")]
    [Tooltip("Follow target rotation")]
    public bool followRotation = true;

    [Tooltip("How quickly the lantern follows rotation (lower = more delay)")]
    [Range(0.1f, 10f)]
    public float rotationFollowSpeed = 2f;

    [Tooltip("Additional rotation offset for the lantern")]
    public Vector3 rotationOffset = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        // Follow position with delay
        if (followPosition)
        {
            Vector3 targetPosition = target.position + target.TransformDirection(positionOffset);
            transform.position = Vector3.Lerp(transform.position, targetPosition, positionFollowSpeed * Time.deltaTime);
        }

        // Follow rotation with delay
        if (followRotation)
        {
            Quaternion targetRotation = target.rotation * Quaternion.Euler(rotationOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationFollowSpeed * Time.deltaTime);
        }
    }
}
