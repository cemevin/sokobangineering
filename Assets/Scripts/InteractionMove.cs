using UnityEngine;

public class InteractionMove : Interactable
{
    private bool isMoving = false;

    public Vector3 moveDir = Vector3.up;

    public float moveSpeed = 1;

    public float moveOffset = 1;

    public bool undoOnStopInteraction = false;

    private Vector3 startPos;
    private Vector3 endPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start() 
    {
        base.Start();
        if (_player == null)
        {
            _player = GameObject.FindFirstObjectByType<PlayerController>();
        }
        startPos = transform.position;
        endPos = transform.position + moveDir * moveOffset;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos, moveSpeed * Time.deltaTime);
        }
        else if (undoOnStopInteraction)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, moveSpeed * Time.deltaTime);
        }
    }
    
    public override void Interact()
    {
        isMoving = true;
    }

    public override void StopInteract()
    {
        if (undoOnStopInteraction)
        {
            isMoving = false;
        }
    }
}
