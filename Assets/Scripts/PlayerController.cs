using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerPushController))]
public class PlayerController : MonoBehaviour
{
    private static readonly int InteractAnimState = Animator.StringToHash("interact");
    private static readonly int BuildAnimState = Animator.StringToHash("build");

    [Header("Movement Input")]
#if ENABLE_INPUT_SYSTEM
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference interactAction;
    [Tooltip("If no InputActionReference is assigned, the script will try to fetch an action named 'Move' from a PlayerInput on this GameObject.")]
    [SerializeField] private string fallbackMoveActionName = "Move";
#endif
    
    [Header("Movement Settings")]
    [Tooltip("Interpret input relative to the camera's facing direction.")]
    [SerializeField] private bool cameraRelative = true;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float moveInputDeadzone = 0.1f;

    [Header("Fixed Directions")]
    [SerializeField] private Vector3 fixedForward = Vector3.forward;
    [SerializeField] private Vector3 fixedRight = Vector3.right;
    [SerializeField] private bool showDirectionGizmos = true;
    [SerializeField] private float gizmoLength = 2f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;

    [Header("Rotation")]
    [Tooltip("Rotate the player to face the movement direction")]
    [SerializeField] private bool rotateToMoveDirection = true;
    [SerializeField] private float turnSpeed = 12f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [Tooltip("Animator float parameter name to drive movement speed.")]
    [SerializeField] private string speedParam = "speed";
    [Tooltip("Animator bool parameter name for push state.")]
    [SerializeField] private string pushParam = "isPushing";
    [Tooltip("Normalize speed (0..1) by moveSpeed before sending to Animator.")]
    [SerializeField] private bool normalizeSpeed = true;
    [Tooltip("Damping time for Animator.SetFloat.")]
    [SerializeField, Min(0f)] private float speedDampTime = 0.1f;

    private HashSet<Interactable> _interactables;
    private string interactionText = "";
    
    public int NumInteractables => _interactables.Count;

#if ENABLE_INPUT_SYSTEM
    private InputAction _move;
#endif
    private CharacterController _controller;
    private PlayerPushController _pushController;
    private Camera _mainCamera;
    private Vector3 _velocity;
    private Vector3 _horizontalVelocity;

    // Calculated direction vectors
    private Vector3 _fixedBackward;
    private Vector3 _fixedLeft;

    private float lastTimeInteracted = 0;
    private float lastTimeBuilt = 0;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _pushController = GetComponent<PlayerPushController>();
        
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Calculate other directions based on fixed forward and right
        CalculateDirectionVectors();
        
        _interactables =  new HashSet<Interactable>();
    }

    private void CalculateDirectionVectors()
    {
        fixedForward = fixedForward.normalized;
        fixedRight = fixedRight.normalized;
        _fixedBackward = -fixedForward;
        _fixedLeft = -fixedRight;

        // Update push controller's direction vectors
        if (_pushController != null)
        {
            _pushController.SetDirectionVectors(fixedForward, fixedRight);
        }
    }

    private void OnValidate()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void OnDrawGizmos()
    {
        if (!showDirectionGizmos) return;

        Vector3 center = transform.position + Vector3.up * 0.1f; // Slightly above ground
        
        // Draw Forward arrow (Blue)
        Gizmos.color = Color.blue;
        DrawArrow(center, fixedForward.normalized * gizmoLength);
        
        // Draw Right arrow (Red)
        Gizmos.color = Color.red;
        DrawArrow(center, fixedRight.normalized * gizmoLength);
        
        // Draw Backward arrow (Cyan)
        Gizmos.color = Color.cyan;
        DrawArrow(center, _fixedBackward * gizmoLength);
        
        // Draw Left arrow (Magenta)
        Gizmos.color = Color.magenta;
        DrawArrow(center, _fixedLeft * gizmoLength);
        
        // Draw labels
        #if UNITY_EDITOR
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.Label(center + fixedForward.normalized * (gizmoLength + 0.3f), "Forward");
        
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.Label(center + fixedRight.normalized * (gizmoLength + 0.3f), "Right");
        
        UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.Label(center + _fixedBackward * (gizmoLength + 0.3f), "Backward");
        
        UnityEditor.Handles.color = Color.magenta;
        UnityEditor.Handles.Label(center + _fixedLeft * (gizmoLength + 0.3f), "Left");
        #endif
    }

    private void DrawArrow(Vector3 start, Vector3 direction)
    {
        Vector3 end = start + direction;
        
        // Draw main line
        Gizmos.DrawLine(start, end);
        
        // Draw arrowhead
        Vector3 arrowHeadLength = direction.normalized * 0.3f;
        Vector3 arrowRight = Vector3.Cross(Vector3.up, direction.normalized) * 0.15f;
        Vector3 arrowLeft = -arrowRight;
        
        Gizmos.DrawLine(end, end - arrowHeadLength + arrowRight);
        Gizmos.DrawLine(end, end - arrowHeadLength + arrowLeft);
    }

    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        if (moveAction != null)
        {
            _move = moveAction.action;
        }
        else
        {
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null && playerInput.actions != null && !string.IsNullOrEmpty(fallbackMoveActionName))
            {
                _move = playerInput.actions.FindAction(fallbackMoveActionName, throwIfNotFound: false);
            }
        }
        if (_move != null) _move.Enable();
#endif
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        if (_move != null) _move.Disable();
#endif
    }

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        bool isInteractionCooldown = lastTimeInteracted != 0 && Time.time - lastTimeInteracted < 1f;
        bool isBuildCooldown = lastTimeBuilt != 0 && Time.time - lastTimeBuilt < 1f;
        if (!isInteractionCooldown && !isBuildCooldown)
        {
            
            Vector2 moveInput = ReadMoveInput();
        
            Vector3 worldDir = ToWorldDirection(moveInput);
            
            _pushController.HandlePushing(worldDir);
            
            if (!_pushController.IsPushing)
            {
                HandleMovement(worldDir);
                HandleGravity();
                
                // Apply movement
                _controller.Move(_velocity * Time.deltaTime);
                
                MaybeRotate(worldDir);
            }
            else
            {
                Vector3 pushVelocity = _pushController.HandlePushMovement();
                _horizontalVelocity = new Vector3(pushVelocity.x, 0f, pushVelocity.z);
                _velocity.x = _horizontalVelocity.x;
                _velocity.z = _horizontalVelocity.z;
                // Player moves with box via parenting, CharacterController is disabled
            }
        }
        else
        {
            _horizontalVelocity = _velocity = Vector3.zero;
        }
        
        UpdateAnimatorSpeed();
        UpdateAnimatorPushState();

        if (CanInteract())
        {
            UpdateInteraction();
        }
    }
    
    public string GetInteractionText()
    {
        return interactionText;
    }

    private void UpdateInteraction()
    {
        if (_interactables.Count > 0)
        {
            Interactable closest = _interactables.ToList()
                .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
                .First();

            if (closest != null)
            {
                bool interacted = false;
                foreach (Interactable i in closest.gameObject.GetComponents<Interactable>())
                {
                    if (i.isActiveAndEnabled && i.CanInteract)
                    {
                        interactionText = i.promptText;
                        if (interactAction != null && interactAction.action.triggered)
                        {
                            i.Interact();
                            interacted = true;
                            lastTimeInteracted = Time.time;
                        }
                    }
                }

                if (interacted && animator != null)
                {
                    animator.SetTrigger(InteractAnimState);
                }
            }
        }
    }

    private bool CanInteract()
    {
        bool isInteractionCooldown = lastTimeInteracted != 0 && Time.time - lastTimeInteracted < 1f;
        bool isBuildCooldown = lastTimeBuilt != 0 && Time.time - lastTimeBuilt < 2f;
        
        return !_pushController.IsPushing && !isInteractionCooldown && !isBuildCooldown;
    }

    private Vector2 ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        Vector2 input = _move != null ? _move.ReadValue<Vector2>() : Vector2.zero;
        if (input.sqrMagnitude < moveInputDeadzone * moveInputDeadzone)
        {
            return Vector2.zero;
        }

        return input;
#else
        return Vector2.zero;
#endif
    }

    private Vector3 ToWorldDirection(Vector2 input)
    {
        Vector3 dir;

        if (cameraRelative && _mainCamera != null)
        {
            // Normal camera-relative movement
            Vector3 camForward = _mainCamera.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = _mainCamera.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            dir = camForward * input.y + camRight * input.x;
        }
        else
        {
            // Fixed world directions
            dir = fixedForward * input.y + fixedRight * input.x;
        }

        if (dir.sqrMagnitude > 1f)
            dir.Normalize();

        return dir;
    }

    private void HandleMovement(Vector3 worldDir)
    {
        Vector3 targetVelocity = worldDir * moveSpeed;
        
        if (worldDir.magnitude > 0.1f)
        {
            // Accelerating
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            // Decelerating
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        // Update horizontal components of velocity
        _velocity.x = _horizontalVelocity.x;
        _velocity.z = _horizontalVelocity.z;
    }

    private void HandleGravity()
    {
        if (_controller.isGrounded && _velocity.y < 0f)
        {
            _velocity.y = -2; // Small negative value to keep grounded
        }
        else
        {
            _velocity.y += gravity * Time.deltaTime;
        }
    }

    private void MaybeRotate(Vector3 worldDir)
    {
        if (!rotateToMoveDirection) return;

        Vector3 planar = worldDir;
        planar.y = 0f;

        if (planar.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(planar, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
    }

    private void UpdateAnimatorSpeed()
    {
        if (animator == null) return;

        float speed = _horizontalVelocity.magnitude;

        if (normalizeSpeed)
        {
            float maxSpeed = _pushController.IsPushing ? _pushController.CurrentPushableBox.PushSpeed : moveSpeed;
            if (maxSpeed > 0.0001f)
            {
                speed = Mathf.Clamp01(speed / maxSpeed);
            }
        }

        animator.SetFloat(speedParam, speed, speedDampTime, Time.deltaTime);
    }
    
    public void OnRoomSpawned()
    {
        if (animator == null) return;
        
        animator.SetTrigger(BuildAnimState);
        lastTimeBuilt =  Time.time;
    }

    private void UpdateAnimatorPushState()
    {
        if (animator == null) return;
        
        animator.SetBool(pushParam, _pushController.IsPushing);
    }

    public void RegisterInteractable(Interactable interactable)
    {
        _interactables.Add(interactable);
    }

    public void DeregisterInteractable(Interactable interactable)
    {
        _interactables.Remove(interactable);
    }

    public Vector3 Velocity => _velocity;
    public float HorizontalSpeed => _horizontalVelocity.magnitude;
    public bool IsPushing => _pushController.IsPushing;

}