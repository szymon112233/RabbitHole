using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PerspectiveSwitcher : MonoBehaviour
{
    private bool isThirdPerson = false;

    public List<GameObject> FirstPersonObjectsToToggle;
    
    public List<GameObject> ThirdPersonObjectsToToggle;
    public VehicleContoller VehicleContollerToToggle;

    private bool isPlayerInsidie;


    private void Awake()
    {
        InputSystem.actions.FindAction("EnterVehicle").performed += OnEnterVehiclePerformed;
    }

    private void OnEnterVehiclePerformed(InputAction.CallbackContext obj)
    {
        if (isPlayerInsidie)
        {
            TogglePerspective();
        }
    }

    public void TogglePerspective()
    {
        isThirdPerson = !isThirdPerson;

        foreach (var go in FirstPersonObjectsToToggle)
        {
            go.SetActive(!isThirdPerson);
        }
        foreach (var go in ThirdPersonObjectsToToggle)
        {
            go.SetActive(isThirdPerson);
        }
        VehicleContollerToToggle.enabled = isThirdPerson;
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInsidie = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInsidie = false;
        }
    }
}
