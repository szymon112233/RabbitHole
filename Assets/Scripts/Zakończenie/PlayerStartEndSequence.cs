using System;
using UnityEngine;

public class PlayerStartEndSequence : MonoBehaviour
{
    [SerializeField]
    private GameObject OldRoom;

    [SerializeField]
    private GameObject NextRoom;

    [SerializeField]
    private Light DirectionalLight;

    [SerializeField]
    private Color Light_Color_Switch;

    [SerializeField]
    private Material SkyboxMat;

    [SerializeField]
    private Transform NextRoomOffset;

    [SerializeField]
    private AudioClip ChangeAmbient;

    public void OnTriggerEnter(Collider other)
    {
        GameManager.Instance.Ambient.Stop();
        //GameManager.Instance.Ambient.loop = false;
        GameManager.Instance.Ambient.clip = ChangeAmbient;
        GameManager.Instance.Ambient.Play();

        Invoke(nameof(ContinueWithChangingRooms), 0f);
    }

    private void ContinueWithChangingRooms()
    {
        OldRoom.SetActive(false);
        NextRoom.SetActive(true);
        // Vector3 offset = NextRoomOffset.position -  NextRoom.transform.position;
        // NextRoom.transform.position = NextRoom.transform.position + offset;
        // GameManager.Instance.Player.transform.position = NextRoomOffset.position;

        RenderSettings.skybox = SkyboxMat;
        DirectionalLight.color = Light_Color_Switch;

        GameManager.Instance.RabbitHoldController.RemoveAllTools(); 
        GameManager.Instance.RabbitSpawnManager.Stop();
        GameManager.Instance.RabbitPoolManager.ReturnToPoolALll();
    }
}
