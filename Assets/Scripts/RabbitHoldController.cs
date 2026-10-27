using DefaultNamespace;
using UnityEngine;
using UnityEngine.InputSystem;

public class RabbitHoldController : MonoBehaviour
{
    public Joint joint;
    public Camera cam;
    
    public bool snapPosition = false;
    public float maxPickupDistance = 2000.0f;
    public float throwPower = 1000.0f;

    private HandController _handController;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputSystem.actions.FindAction("Attack").performed += OnAttackPerformed;
        InputSystem.actions.FindAction("Drop").performed += OnDropPerformed;
        // InputSystem.actions.FindAction("Interact").performed += OnInteractPerformed;

        _handController = FindAnyObjectByType<HandController>();

        if (_handController == null)
        {
            Debug.LogError("BROOO! Put HandController on the scene!");
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext obj)
    {
        TryHoldRabbit();
    }

    private void OnDropPerformed(InputAction.CallbackContext obj)
    {
        DropRabbit();
    }

    private void OnAttackPerformed(InputAction.CallbackContext obj)
    {
        if (IsHoldingARabbit())
        {
            ThrowRabbit();
        }
        else
        {
            TryHoldRabbit();
        }
        
    }

    bool IsHoldingARabbit()
    {
        return joint.connectedBody != null && joint.connectedBody.gameObject.CompareTag("Rabbit");
    }
    
    bool TryHoldRabbit()
    {

        Rigidbody rabbit;
        
        var layerMask = LayerMask.GetMask("Rabbit");
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        
        if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out hit,
                maxPickupDistance, layerMask))
        {
            // Debug.DrawRay(cam.transform.position, cam.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            rabbit = hit.rigidbody;
        }
        else
        {
            // Debug.DrawRay(cam.transform.position, cam.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            // Debug.Log("Did not Hit");
            
            return false;
        }
        
        if(snapPosition)
        {
            rabbit.position = transform.position;        
        }
        
        rabbit.linearVelocity = Vector3.zero;
        rabbit.GetComponent<RabbitController>().Grabbed();
        _handController.EnterGrabState();
        joint.connectedBody = rabbit;

        return true;
    }

    public void DropRabbit()
    {
        if (joint.connectedBody == null)
        {
            return;
        }
        
        joint.connectedBody.GetComponent<RabbitController>().Thrown();
        _handController.ExitGrabState();
        joint.connectedBody = null;

    }

    public void DropRabbit(Rigidbody rabbitRB)
    {
        if (joint.connectedBody == null)
        {
            return;
        }

        if (rabbitRB == joint.connectedBody)
        { 
            joint.connectedBody = null;
        }
    }

    void ThrowRabbit()
    {
        if (joint.connectedBody == null)
        {
            return;
        }

        var rabbit = joint.connectedBody;
        
        joint.connectedBody.GetComponent<RabbitController>().Thrown();
        _handController.ExitGrabState();
        joint.connectedBody = null;
        
        rabbit.AddForce(cam.transform.forward * throwPower);
    }
}
