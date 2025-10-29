using TMPro;
using UnityEngine;

public class UI_InteractLabel : MonoBehaviour
{
    private PlayerController _player;
    
    
    [SerializeField] private TextMeshProUGUI textBox;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.FindFirstObjectByType<PlayerController>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_player != null)
        {
            textBox.gameObject.SetActive(_player.NumInteractables > 0);
            textBox.SetText(_player.GetInteractionText());
        }
    }
}
