using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI noteTextBox;
    public static UIManager instance;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        noteTextBox.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ShowNote(string noteText)
    {
        noteTextBox.SetText(noteText);
        noteTextBox.gameObject.SetActive(true);
    }

    public void HideNote()
    {
        noteTextBox.gameObject.SetActive(false);
    }
}
