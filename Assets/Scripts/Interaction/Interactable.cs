using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteraction
{
    
    [Tooltip("Optional prompt text for UI, e.g. 'Press E to open door'")]
    public string promptText = "Interact [E]";

    [Tooltip("Cooldown between uses (seconds)")]
    public float cooldown = 0f;

    public bool canInteractOnce = false;

    public bool triggerBaseInteraction = false;

    [Tooltip("Cooldown between uses (seconds)")]
    public float interactionDistance = 0.25f;

    public bool canInteractWithoutLeavingRange = true;

    private float lastUsedTime;
    
    private bool bInRange = false;
    private bool bInteracted = false;

    public bool CanInteract => (lastUsedTime == 0 || Time.time >= lastUsedTime + cooldown) && !(bInteracted && canInteractOnce) && (canInteractWithoutLeavingRange || !bInteracted);

    protected PlayerController _player;
    
    protected void MarkUsed()
    {
        lastUsedTime = Time.time;
        bInteracted = true;
    }
    
    protected void ResetUsed()
    {
        bInteracted = false;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        _player = GameObject.FindFirstObjectByType<PlayerController>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _player.gameObject && triggerBaseInteraction && CanInteract)
        {
            Interact();
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject == _player.gameObject && triggerBaseInteraction)
        {
            StopInteract();
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (_player != null && !triggerBaseInteraction)
        {
            bool bIsInRange = (_player.gameObject.transform.position - transform.position).sqrMagnitude <
                interactionDistance * interactionDistance;
            
            if (CanInteract && bIsInRange)
            {
                _player.RegisterInteractable(this);
            }
            else
            {
                _player.DeregisterInteractable(this);
            }
            
            if (bInteracted && !bIsInRange)
            {
                StopInteract();
            }

            bInRange = bIsInRange;
        }
    }

    public abstract void Interact();
    public abstract void StopInteract();
}
