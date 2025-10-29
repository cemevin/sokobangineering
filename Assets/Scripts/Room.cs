using System;
using UnityEngine;

public class Room : MonoBehaviour
{

    private GameObject _roomSpawnVFX;
    [SerializeField] private GameObject vfxSpawnPos;
    private float _killVFXTime;
    public Vector3 vfxScale = Vector3.one;

    private void Start()
    {
        if (vfxSpawnPos == null)
        {
            vfxSpawnPos = transform.Find("VFXSpawnPos").gameObject;
        }
    }

    public void OnSpawned()
    {
        _roomSpawnVFX = Instantiate(GameManager.instance.roomSpawnVFX, vfxSpawnPos.transform.position, transform.rotation);
        _roomSpawnVFX.transform.localScale = vfxScale;
        _killVFXTime = Time.time + _roomSpawnVFX.GetComponent<ParticleSystem>().main.duration;
    }

    private void Update()
    {
        if (Time.time > _killVFXTime)
        {
            Destroy(_roomSpawnVFX);
        }
    }
}
