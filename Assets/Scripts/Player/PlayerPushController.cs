using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerPushController : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private LayerMask pushableLayerMask = ~0;
    [SerializeField] private float pushDetectionDistance = 1f;
    [SerializeField] private float pushSearchYOffset = 0f;

    private CharacterController _controller;
    private bool _isPushing = false;
    private PushableBox _currentPushableBox;
    private Vector3 _currentPushDirection;
    private Vector3 _offsetToBox;
    private float _pushCooldownTimer = 0f;
    private const float PUSH_COOLDOWN = 0.1f;

    // Direction vectors (will be set by PlayerController)
    private Vector3 _fixedForward = Vector3.forward;
    private Vector3 _fixedRight = Vector3.right;
    private Vector3 _fixedBackward = Vector3.back;
    private Vector3 _fixedLeft = Vector3.left;

    public bool IsPushing => _isPushing;
    public PushableBox CurrentPushableBox => _currentPushableBox;
    public Vector3 CurrentPushDirection => _currentPushDirection;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (_pushCooldownTimer > 0f)
            _pushCooldownTimer -= Time.deltaTime;
    }

    public void SetDirectionVectors(Vector3 forward, Vector3 right)
    {
        _fixedForward = forward.normalized;
        _fixedRight = right.normalized;
        _fixedBackward = -_fixedForward;
        _fixedLeft = -_fixedRight;
    }

    public void HandlePushing(Vector3 worldDir)
    {
        if (worldDir.magnitude < 0.1f)
        {
            StopPushing();
            return;
        }

        // If already pushing, continue pushing in the same direction
        if (_isPushing && _currentPushableBox != null)
        {
            // Check if we're still trying to push in a valid direction
            Vector3 constrainedInputDir = GetConstrainedDirection(worldDir);
            if (constrainedInputDir == _currentPushDirection)
            {
                // Continue pushing
                Vector3 myRayOrigin = _controller.transform.position + Vector3.up * pushSearchYOffset;
                RaycastHit myHit;
                if (Physics.Raycast(myRayOrigin, worldDir, out myHit, pushDetectionDistance, pushableLayerMask))
                {
                    if (myHit.collider.gameObject != _currentPushableBox.gameObject)
                    {
                        StopPushing();
                    }
                }
                return;
            }
            else
            {
                // Direction changed or stopped, stop pushing
                StopPushing();
                return;
            }
        }

        // Don't start new push during cooldown
        if (_pushCooldownTimer > 0f)
            return;

        // Only do raycast detection when not currently pushing
        Vector3 rayOrigin = _controller.transform.position + Vector3.up * pushSearchYOffset;
        RaycastHit hit;
        
        if (Physics.Raycast(rayOrigin, worldDir, out hit, pushDetectionDistance, pushableLayerMask))
        {
            PushableBox pushableBox = hit.collider.GetComponent<PushableBox>();
            if (pushableBox != null)
            {
                if (pushableBox.CanBePushed(_controller.transform.position, worldDir))
                {
                    StartPushing(pushableBox, worldDir);
                }
            }
        }
    }

    public Vector3 HandlePushMovement()
    {
        if (_currentPushableBox != null && _currentPushableBox.IsBeingPushed)
        {
            // Return velocity values for animation system
            float pushSpeed = _currentPushableBox.PushSpeed;
            return _currentPushDirection * pushSpeed;
        }
        else
        {
            StopPushing();
            return Vector3.zero;
        }
    }

    private void StartPushing(PushableBox box, Vector3 pushDirection)
    {
        if (box.StartPush(_controller.transform.position, pushDirection))
        {
            _isPushing = true;
            _currentPushableBox = box;
            _currentPushDirection = GetConstrainedDirection(pushDirection);
        
            // Face the box
            Vector3 directionToBox = (box.transform.position - transform.position).normalized;
            directionToBox.y = 0f;
            if (directionToBox.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(directionToBox);
            }
        
            // Disable CharacterController and parent to box
            _controller.enabled = false;
            transform.SetParent(box.transform);
        }
    }

    public void StopPushing()
    {
        if (_isPushing)
        {
            _isPushing = false;
            _pushCooldownTimer = PUSH_COOLDOWN;
        
            // Unparent and re-enable CharacterController
            transform.SetParent(null);
            _controller.enabled = true;
        
            if (_currentPushableBox != null && _currentPushableBox.IsBeingPushed)
            {
                _currentPushableBox.StopPush();
                _currentPushableBox = null;
            }
            _currentPushDirection = Vector3.zero;
        }
    }

    private Vector3 GetConstrainedDirection(Vector3 inputDirection)
    {
        // Same logic as PushableBox - constrain to cardinal directions
        Vector3 dir = inputDirection.normalized;
        Vector3 constrainedDir = Vector3.zero;

        // Find the dominant axis
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
        {
            // X-axis dominant
            if (dir.x > 0)
                constrainedDir = _fixedRight;
            else
                constrainedDir = _fixedLeft;
        }
        else
        {
            // Z-axis dominant
            if (dir.z > 0)
                constrainedDir = _fixedForward;
            else
                constrainedDir = _fixedBackward;
        }

        return constrainedDir;
    }
}