using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class RabbitHoldController : MonoBehaviour
{

    public Tool ActiveTool;

    public List<Tool> Tools;

    private HandController _handController;
    private List<Action<InputAction.CallbackContext>> toolSwappingCallbacks;

    void Awake()
    {
        toolSwappingCallbacks = new List<Action<InputAction.CallbackContext>>();
        ActiveTool.Activate();
    }
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputSystem.actions.FindAction("Attack").performed += OnActionPerformed;
        InputSystem.actions.FindAction("Drop").started += OnAltActionPerformed;
        InputSystem.actions.FindAction("Attack").canceled += OnActionStop;
        // InputSystem.actions.FindAction("Drop").canceled += OnAltActionPerformed;

        toolSwappingCallbacks.Add((InputAction.CallbackContext x) => {OnToolSwap(0);});
        InputSystem.actions.FindAction("SwapToTool_0").performed += toolSwappingCallbacks[0];
        
        toolSwappingCallbacks.Add((InputAction.CallbackContext x) => {OnToolSwap(1);});
        InputSystem.actions.FindAction("SwapToTool_1").performed += toolSwappingCallbacks[1];
        
        toolSwappingCallbacks.Add((InputAction.CallbackContext x) => {OnToolSwap(2);});
        InputSystem.actions.FindAction("SwapToTool_2").performed += toolSwappingCallbacks[2];
        
        
        _handController = FindAnyObjectByType<HandController>();


        if (_handController == null)
        {
            Debug.LogError("BROOO! Put HandController on the scene!");
        }

    }

    void OnDestroy()
    {
        InputSystem.actions.FindAction("Attack").performed -= OnActionPerformed;
        InputSystem.actions.FindAction("Drop").performed -= OnAltActionPerformed;
        InputSystem.actions.FindAction("SwapToTool_0").performed -= toolSwappingCallbacks[0];
        InputSystem.actions.FindAction("SwapToTool_1").performed -= toolSwappingCallbacks[1];
        InputSystem.actions.FindAction("SwapToTool_2").performed -= toolSwappingCallbacks[2];
    }

    // private void OnInteractPerformed(InputAction.CallbackContext obj)
    // {
    //     TryHoldRabbit();
    // }

    private void OnAltActionPerformed(InputAction.CallbackContext obj)
    {
        ActiveTool.Use_Alt();
    }

    private void OnActionPerformed(InputAction.CallbackContext obj)
    {
        ActiveTool.Use_Main();

    }

    private void OnActionStop(InputAction.CallbackContext obj)
    {
        ActiveTool.Stop_Main();
    }

    private void OnAltActionStopped(InputAction.CallbackContext obj)
    {
        ActiveTool.Stop_Alt();
    }


    private void OnToolSwap(int id)
    {
        ActiveTool.Deactivate();
        ActiveTool = Tools[id];
        ActiveTool.Activate();

        _handController.gameObject.SetActive(ActiveTool.UsesHandGraphics);

    }
}
