using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InteractionToggleDoor : Interactable
{
    [SerializeField] private List<int> doorIdsToToggle = new List<int>();

    public override void Interact()
    {
        Door[] Doors = GameObject.FindObjectsByType<Door>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Door door in Doors)
        {
            if (door.isActiveAndEnabled && doorIdsToToggle.Contains(door.DoorId))
            {
                door.ToggleDoor();
            }
        }
        
        MarkUsed();
    }

    public override void StopInteract()
    {
        ResetUsed();
    }
}
