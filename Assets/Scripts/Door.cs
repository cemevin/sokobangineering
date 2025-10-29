using System;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private int doorId = 0;
    [SerializeField] private float doorOpenCloseAnimTime = 0.5f;
    [SerializeField] private Vector3 slideDirection = Vector3.right; // Direction to slide the door
    [SerializeField] private float slideDistance = 1f; // Distance to slide
    [SerializeField] private bool isOpenByDefault = false;
    [SerializeField] private bool turnOffParentCollision = false;
    
    private bool _isOpen = false;
    private Vector3 _initialPosition;
    private Vector3 _finalPosition;
    private float _animationProgress = 0f;
    private bool _isAnimating = false;
    private Coroutine _animationCoroutine;

    void Start()
    {
        _initialPosition = transform.position;
        _finalPosition = _initialPosition + transform.rotation * slideDirection.normalized * slideDistance;
        _isOpen = false;
        
        if (isOpenByDefault)
        {
            Open();
        }
        else
        {
            Close();
        }
    }
    
    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 ab = b - a;
        Vector3 av = value - a;
        return av.magnitude / ab.magnitude;
    }

    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        // Only draw in the Scene view, not in Game view
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 14;
        style.alignment = TextAnchor.MiddleCenter;

        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, doorId + "", style);
        #endif
    }

    void Update()
    {
        if (_isAnimating)
        {
            _animationProgress += Time.deltaTime / doorOpenCloseAnimTime;
            
            if (_animationProgress >= 1f)
            {
                _animationProgress = 1f; 
                _isAnimating = false;
            }

            if (_isOpen)
            {
                // Interpolate between initial and target positions
                transform.position = Vector3.Lerp(_initialPosition, _finalPosition, _animationProgress);

                
            }
            else
            {
                transform.position = Vector3.Lerp(_finalPosition, _initialPosition, _animationProgress);
            }
        }
    }

    public void Open()
    {
        if (_isOpen || _isAnimating) return;
        
        _isOpen = true;
        _isAnimating = true;
        
        _animationProgress = InverseLerp(_initialPosition, _finalPosition, transform.position);

        if (transform.parent != null && turnOffParentCollision)
        {
            BoxCollider box = transform.parent.gameObject.GetComponent<BoxCollider>();
            if (box)
            {
                box.enabled = false;
            }
        }
    }

    public void Close()
    {
        if (!_isOpen || _isAnimating) return;
        
        _isOpen = false;
        _isAnimating = true;
        _animationProgress = 0f;
        
        _animationProgress = 1 - InverseLerp(_initialPosition, _finalPosition, transform.position);

        if (transform.parent != null && turnOffParentCollision)
        {
            BoxCollider box = transform.parent.gameObject.GetComponent<BoxCollider>();
            if (box)
            {
                box.enabled = true;
            }
        }
    }

    public void ToggleDoor()
    {
        if (_isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public bool IsOpen => _isOpen;
    public bool IsOpenByDefault => isOpenByDefault;
    public int DoorId => doorId;

    // Called when something enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        // You can add logic here for automatic opening based on player or other triggers
        // For example, open door when player enters trigger area
        // Open();
    }

    // Called when something exits the trigger
    private void OnTriggerExit(Collider other)
    {
        // You can add logic here for automatic closing
        // For example, close door when player leaves trigger area
        // Close();
    }
}