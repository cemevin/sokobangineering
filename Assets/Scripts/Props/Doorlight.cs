using System;
using System.Collections.Generic;
using UnityEngine;

public class Doorlight : MonoBehaviour
{
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private static readonly int BaseColor = Shader.PropertyToID("_Color");
    [SerializeField] private int doorId = 0;
    [SerializeField] private float intensityOn = 30;
    [SerializeField] private float intensityOff = 0.5f;
    [SerializeField] private GameObject doorlight;


    private Door _door;
    private bool isOpen;

    void Start()
    {
        Door[] doors = GameObject.FindObjectsByType<Door>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (Door d in doors)
        {
            if (d.DoorId == doorId)
            {
                _door = d;
                break;
            }
        }

        if (_door != null)
        {
            isOpen = _door.IsOpenByDefault;
            UpdateLightMaterial();
        }
    }
    

    void Update()
    {
        if (_door != null)
        {
            bool wasOpen = isOpen;
            isOpen = _door.IsOpen;   
            
            if (wasOpen != isOpen)
            {
                UpdateLightMaterial();
            }
        }
    }

    private void UpdateLightMaterial()
    {
        if (doorlight != null)
        {
            if (isOpen)
            {
                var mat = doorlight.GetComponent<Renderer>().material;
                Color color = mat.GetColor(BaseColor);
                mat.SetColor(EmissionColor, color * intensityOn);
                mat.EnableKeyword("_EMISSION");
            }
            else
            {
                var mat = doorlight.GetComponent<Renderer>().material;
                Color color = mat.GetColor(BaseColor);
                mat.SetColor(EmissionColor, color * intensityOff);
                mat.EnableKeyword("_EMISSION");
            }
        }
    }
}