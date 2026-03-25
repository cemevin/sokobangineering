using UnityEngine;
using UnityEngine.AI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // New Input System
#endif

public class PlayerMouseNavigation : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private NavMeshAgent agent;
    [Tooltip("Layers that can be clicked (e.g., Ground)")]
    [SerializeField] private LayerMask clickMask = ~0;
    [SerializeField] private float maxRayDistance = 1000f;

    [Header("Top-Down Rotation")]
    [Tooltip("Rotate the player to face the movement direction")]
    [SerializeField] private bool rotateToMoveDirection = true;
    [SerializeField] private float turnSpeed = 12f;

    [Header("NavMesh Sampling")]
    [Tooltip("Project clicked point to the nearest NavMesh position")]
    [SerializeField] private bool useNavMeshProjection = true;
    [SerializeField] private float maxProjectionDistance = 1.5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [Tooltip("Animator float parameter name to drive movement speed.")]
    [SerializeField] private string speedParam = "speed";
    [Tooltip("Normalize speed (0..1) by agent.speed before sending to Animator.")]
    [SerializeField] private bool normalizeSpeed = true;
    [Tooltip("Damping time for Animator.SetFloat.")]
    [SerializeField, Min(0f)] private float speedDampTime = 0.1f;

    private void Awake()
    {
        if (agent == null)
            TryGetComponent(out agent);

        // Try to find an Animator on this object or its children if not assigned
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (agent != null)
            agent.updateRotation = !rotateToMoveDirection;
    }

    private void Update()
    {
        // Pointer/click handling for both input backends
        if (TryGetClickScreenPosition(out Vector2 screenPos))
        {
            TrySetDestinationFromScreenPosition(screenPos);
        }

        if (rotateToMoveDirection)
            RotateTowardsDesiredVelocity();

        UpdateAnimatorSpeed();
    }

    private bool TryGetClickScreenPosition(out Vector2 screenPos)
    {
        screenPos = default;

        #if ENABLE_INPUT_SYSTEM
        // New Input System: Mouse
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            screenPos = mouse.position.ReadValue();
            return true;
        }

        // New Input System: Touch
        var touch = Touchscreen.current;
        if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
        {
            screenPos = touch.primaryTouch.position.ReadValue();
            return true;
        }
        #else
        // Legacy Input Manager
        if (Input.GetMouseButtonDown(0))
        {
            screenPos = Input.mousePosition;
            return true;
        }
        #endif

        return false;
    }

    private void TrySetDestinationFromScreenPosition(Vector2 screenPos)
    {
        var cam = Camera.main;
        if (cam == null || agent == null)
            return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, clickMask))
        {
            Vector3 target = hit.point;

            if (useNavMeshProjection &&
                NavMesh.SamplePosition(target, out NavMeshHit navHit, maxProjectionDistance, NavMesh.AllAreas))
            {
                target = navHit.position;
            }

            agent.SetDestination(target);
        }
    }

    private void RotateTowardsDesiredVelocity()
    {
        if (agent == null) return;

        Vector3 planarVel = agent.desiredVelocity;
        planarVel.y = 0f;

        if (planarVel.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(planarVel.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
    }

    private void UpdateAnimatorSpeed()
    {
        if (animator == null) return;

        float speed = 0f;

        if (agent != null)
        {
            // Use actual agent velocity magnitude (world units per second)
            speed = agent.velocity.magnitude;

            if (normalizeSpeed && agent.speed > 0.0001f)
            {
                speed /= agent.speed;          // normalize to 0..~1
                speed = Mathf.Clamp01(speed);  // clamp just in case
            }
        }

        // Damped set (Animator handles smoothing internally)
        animator.SetFloat(speedParam, speed, speedDampTime, Time.deltaTime);
    }

    private void OnValidate()
    {
        if (agent != null)
            agent.updateRotation = !rotateToMoveDirection;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }
}