using UnityEngine;

public class SpawnRoom : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private Vector3 rotationOffset = Vector3.zero; // Rotation offset in degrees
    [SerializeField] private Vector3 locationOffset = Vector3.zero; // location offset
    [SerializeField] private LayerMask barrierLayerMask = ~0; // Layer mask for barrier objects
    [SerializeField] private string pivotObjectName = "Pivot"; // Name of the pivot object to search for
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Barrier b = other.GetComponent<Barrier>();
        if (b)
        {
            PushableBox box = GetComponent<PushableBox>();
            if (box != null)
            {
                // Stop pushing the box
                box.StopPush();
            }
            
            
            
            b.ShowRoom();
            Destroy(gameObject);
        }
            
        /*// Check if the colliding object is on the barrier layer
        if (IsOnBarrierLayer(other.gameObject))
        {
            DestroyBoxAndCreateRoom(other.transform);
        }*/
    }

    /*private void OnCollisionEnter(Collision other)
    {
        Barrier b = other.gameObject.GetComponent<Barrier>();
        if (b)
        {
            PushableBox box = GetComponent<PushableBox>();
            if (box != null)
            {
                // Stop pushing the box
                box.StopPush();
            }
                
            b.ShowRoom();
            Destroy(gameObject);
        }
        /#1#/ Check if the colliding object is on the barrier layer
        if (IsOnBarrierLayer(collision.gameObject))
        {
            DestroyBoxAndCreateRoom(collision.transform);
        }#1#
    }*/

    private bool IsOnBarrierLayer(GameObject obj)
    {
        // Check if the object's layer is included in the barrier layer mask
        return (barrierLayerMask.value & (1 << obj.layer)) != 0;
    }

    private void DestroyBoxAndCreateRoom(Transform barrierTransform)
    {
        PushableBox box = GetComponent<PushableBox>();
        if (box != null)
        {
            // Stop pushing the box
            box.StopPush();
        }
        
        // Step 1: Calculate spawn rotation (barrier rotation + rotation offset)
        Quaternion barrierRotation = barrierTransform.rotation;
        Quaternion offsetRotation = Quaternion.Euler(rotationOffset);
        Quaternion spawnRotation = barrierRotation * offsetRotation;

        // Step 2: Create the room at the barrier position first
        GameObject newRoom = null;
        if (roomPrefab != null)
        {
            // offset it like crazy because of momentary collision
            newRoom = Instantiate(roomPrefab, barrierTransform.position + new Vector3(100,100,100), spawnRotation); 
        }

        // Step 3: Find the Pivot object and adjust position
        if (newRoom != null)
        {
            barrierTransform.gameObject.SetActive(false);
            Transform pivotTransform = FindPivotInChildren(newRoom.transform);
            if (pivotTransform != null)
            {
                // Calculate the offset between the pivot and the barrier
                Vector3 pivotToBarrierOffset = barrierTransform.position - pivotTransform.position + spawnRotation*locationOffset;
                
                // Move the entire room by this offset so the pivot aligns with the barrier
                newRoom.transform.position += pivotToBarrierOffset;

                Room room = newRoom.GetComponent<Room>();
                if (room != null)
                {
                    room.OnSpawned();
                }
            }
            else
            {
                Debug.LogWarning($"Pivot object '{pivotObjectName}' not found in room prefab. Room positioned at barrier location.");
            }
            
        }

        // Step 4: Destroy the box
        Destroy(gameObject);
    }

    private Transform FindPivotInChildren(Transform parent)
    {
        // First check if the parent itself is the pivot
        if (parent.name.Equals(pivotObjectName, System.StringComparison.OrdinalIgnoreCase))
        {
            return parent;
        }

        // Search through all children recursively
        foreach (Transform child in parent)
        {
            if (child.name.Equals(pivotObjectName, System.StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }

            // Recursively search in grandchildren
            Transform foundInChild = FindPivotInChildren(child);
            if (foundInChild != null)
            {
                return foundInChild;
            }
        }

        return null; // Pivot not found
    }
}