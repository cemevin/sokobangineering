using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Follow Settings")]
    [Tooltip("Smoothly move towards the target instead of snapping.")]
    [SerializeField] private bool smooth = true;
    [SerializeField, Min(0f)] private float smoothTime = 0.12f;

    private Vector3 offset;
    private Quaternion initialRotation;
    private Vector3 velocity; // Used by SmoothDamp
    private bool hasCached;

    private void Start()
    {
        CacheDefaultsIfPossible();
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        hasCached = false;
        CacheDefaultsIfPossible();
    }

    private void CacheDefaultsIfPossible()
    {
        if (player == null || hasCached) return;

        // Cache the initial offset (camera start position - player start position)
        offset = transform.position - player.position;

        // Cache the initial camera rotation to keep it fixed
        initialRotation = transform.rotation;

        hasCached = true;
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        if (!hasCached)
            CacheDefaultsIfPossible();

        Vector3 targetPos = player.position + offset;

        if (smooth)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        }
        else
        {
            transform.position = targetPos;
        }

        // Keep the original camera rotation (no rotation following)
        transform.rotation = initialRotation;
    }
}