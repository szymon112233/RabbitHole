using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Video;

namespace UnityEngine.Rendering.Universal {
public class TelezakupyController : MonoBehaviour {

    [SerializeField] 
    protected VideoClip _NoProgramm;
    
    [SerializeField] 
    protected List<VideoClip> _Programms;

    [SerializeField] 
    protected VideoPlayer _VideoPlayer;
    
    [SerializeField]
    public LayerMask _TriggerLayers;
    
    [SerializeField]
    protected float _InteractionDistance = 2;

    [SerializeField] 
    protected GameObject _SwitchProgramPrompt;
    
    [SerializeField] 
    protected GameObject _PurchasePrompt;

    private int _CurrentProgram;

    private bool _PressRegistered;

    public List<VideoClip> CurrentPrograms {
        get {
            //TODO: filter by the list of currently available programms
            
            var result = new List<VideoClip>();

            result.Add(_NoProgramm);
            result.AddRange(_Programms);
            
            return result;
        }
    }
    
    public void SwitchProgramm() {
        var currPrograms = CurrentPrograms;

        _CurrentProgram = (_CurrentProgram + 1 % currPrograms.Count);
        UpdateVideoProgram();
    }

    private void UpdateVideoProgram() {
        _VideoPlayer.clip = CurrentPrograms[_CurrentProgram % CurrentPrograms.Count];
    }

    private void Awake() {
        _CurrentProgram = 0;
        SwitchProgramm();
    }

    private void Update() {
        // 1. Define the origin and direction (e.g., forward from the camera or player)
        Vector3 origin = Camera.main.transform.position;
        Vector3 direction = Camera.main.transform.forward;
        var isInteractionReady = false;
        
        // 2. Perform the Raycast forcing it to collide with Triggers
        if (Physics.Raycast(origin, direction, out RaycastHit hit, _InteractionDistance, _TriggerLayers, QueryTriggerInteraction.Collide))
        {
            
            // 3. Double-check if the hit collider is actually a trigger
            if (hit.collider.isTrigger) {
                isInteractionReady = true;

                // Put your interaction or UI highlighting logic here
            }
        }
        
        _SwitchProgramPrompt.gameObject.SetActive(isInteractionReady);
        _PurchasePrompt.gameObject.SetActive(isInteractionReady && _CurrentProgram != 0);

        if (isInteractionReady) {
            if (Keyboard.current.qKey.isPressed && !_PressRegistered) {
                _PressRegistered = true;
                SwitchProgramm();
            }
        }

        if (!Keyboard.current.qKey.isPressed) {
            _PressRegistered = false;
        }
    }
}
}