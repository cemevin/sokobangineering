using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PushableBox : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float pushSpeed = 2f;
    [SerializeField] private LayerMask obstacleLayerMask = ~0;
    [SerializeField] private float pushDetectionDistance = 0.1f;
    [SerializeField] private float pushAngleThreshold = 45f;
    
    [Header("Movement Constraints")]
    [SerializeField] private bool canMoveForward = true;
    [SerializeField] private bool canMoveBackward = true;
    [SerializeField] private bool canMoveLeft = true;
    [SerializeField] private bool canMoveRight = true;
    
    private bool _isBeingPushed = false;
    private Vector3 _pushDirection;
    private Vector3 _targetPosition;
    private Vector3 _startPosition;
    private Collider _boxCollider;

    public bool IsBeingPushed => _isBeingPushed;
    public float PushSpeed => pushSpeed;

    private void Awake()
    {
        _boxCollider = GetComponent<Collider>();
    }

    public bool CanBePushed(Vector3 origin, Vector3 pushDirection)
    {
        Vector3 dir = transform.position - origin;
        dir.y = 0;
        dir.Normalize();

        if (Vector3.Dot(pushDirection, dir) < Mathf.Cos(Mathf.Deg2Rad * pushAngleThreshold))
        {
            return false;
        }
            
        Vector3 normalizedDir = GetConstrainedDirection(dir);
        
        if (normalizedDir == Vector3.zero)
            return false;

        // Check if there's space to move in that direction
        Vector3 checkPosition = _boxCollider.bounds.center + normalizedDir * pushDetectionDistance;
        
        // Temporarily disable this box's collider to avoid self-detection
        _boxCollider.enabled = false;
        
        bool hasObstacle = Physics.CheckBox(checkPosition, _boxCollider.bounds.extents, transform.rotation, obstacleLayerMask, QueryTriggerInteraction.Ignore);

        if (hasObstacle)
        {
            RaycastHit hit; 
            if (Physics.Raycast(_boxCollider.bounds.center, normalizedDir, out hit, pushDetectionDistance, obstacleLayerMask))
            {
                Debug.Log(hit.distance);
            }
        }
        // Re-enable the collider
        _boxCollider.enabled = true;
        
        return !hasObstacle;
    }

    public bool StartPush(Vector3 origin, Vector3 pushDirection)
    {
        Debug.Log("Start Push");
        if (_isBeingPushed || !CanBePushed(origin, pushDirection))
            return false;
        
        Vector3 dir = transform.position - origin;
        dir.y = 0;
        dir.Normalize();

        _pushDirection = GetConstrainedDirection(dir);
        if (_pushDirection == Vector3.zero)
            return false;

        _isBeingPushed = true;
        _startPosition = transform.position;
        _targetPosition = _startPosition + _pushDirection;

        return true;
    }

    public void StopPush()
    {
        Debug.Log("Stop Push");
        
        _isBeingPushed = false;

        // First, check if any player is still parented to this box and unparent them
        foreach (Transform child in transform)
        {
            PlayerPushController playerPushController = child.GetComponent<PlayerPushController>();
            if (playerPushController != null)
            {
                playerPushController.StopPushing();
            }
        }

    }

    private void Update()
    {
        if (_isBeingPushed)
        {
            if (!CanBePushed(transform.position - _pushDirection * pushDetectionDistance, _pushDirection))
            {
                StopPush();
            }
            else
            {
                transform.position += _pushDirection * (pushSpeed * Time.deltaTime);
            }
            /*
            _pushProgress += pushSpeed * Time.deltaTime;
            
            if (_pushProgress >= 1f)
            {
                transform.position = _targetPosition;
                StopPush();
            }
            else
            {
                transform.position = Vector3.Lerp(_startPosition, _targetPosition, _pushProgress);
                
            }*/
        }
    }

    private Vector3 GetConstrainedDirection(Vector3 inputDirection)
    {
        // Normalize to cardinal directions only
        
        Vector3 dir = inputDirection.normalized;
        Vector3 constrainedDir = Vector3.zero;

        // Find the dominant axis
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
        {
            // X-axis dominant
            if (dir.x > 0 && canMoveRight)
                constrainedDir = Vector3.right;
            else if (dir.x < 0 && canMoveLeft)
                constrainedDir = Vector3.left;
        }
        else
        {
            // Z-axis dominant
            if (dir.z > 0 && canMoveForward)
                constrainedDir = Vector3.forward;
            else if (dir.z < 0 && canMoveBackward)
                constrainedDir = Vector3.back;
        }

        return constrainedDir;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (_boxCollider != null)
        {
            Gizmos.DrawWireCube(transform.position, _boxCollider.bounds.size);
        }
        
        if (_isBeingPushed)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _targetPosition);
        }
    }
}