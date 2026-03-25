using Unity.VisualScripting;
using UnityEngine;

public class InteractionShowNote : Interactable
{
    [SerializeField] private string noteText;
    public override void Interact()
    {
        Debug.Log("interact : show note");
        if (UIManager.instance != null)
        {
            UIManager.instance.ShowNote(noteText);
        }
        MarkUsed();
    }
    
    public override void StopInteract()
    {
        Debug.Log("Stop interact : show note");
        if (UIManager.instance != null)
        {
            UIManager.instance.HideNote();
        }
        ResetUsed();
    }
}
