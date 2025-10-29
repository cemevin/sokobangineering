using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] public GameObject roomSpawnVFX;
    [SerializeField] private InputActionReference resetInteraction;
    public static GameManager instance;
    private PlayerController _player;
    public float killY = -10;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _player = GameObject.FindFirstObjectByType<PlayerController>();
        foreach (Barrier b in GameObject.FindObjectsByType<Barrier>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            b.HideRoom();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_player.gameObject.transform.position.y < killY || resetInteraction != null && resetInteraction.action.triggered)
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}
