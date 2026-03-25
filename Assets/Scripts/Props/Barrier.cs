using UnityEngine;
using UnityEngine.Serialization;

public class Barrier : MonoBehaviour
{
    [SerializeField] private GameObject room;
    private PlayerController _player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.FindFirstObjectByType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowRoom()
    {
        if (room != null)
        {
            room.SetActive(true);
            Room roomComponent = room.GetComponent<Room>();

            if (roomComponent != null)
            {
                roomComponent.OnSpawned();
            }

            if (_player != null)
            {
                _player.OnRoomSpawned();
            }
            
            gameObject.SetActive(false);
        }
    }

    public void HideRoom()
    {
        if (room != null)
        {
            room.SetActive(false);
        }
    }
}
