using UnityEngine;
using UnityEngine.InputSystem;

public class RabbitHoldController : MonoBehaviour
{

    public SpringJoint joint;

    public Camera cam;
    
    
    public float maxPickupDistance = 2000.0f;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputSystem.actions.FindAction("Attack").performed += OnAttackPerformed;
        InputSystem.actions.FindAction("Drop").performed += OnDropPerformed;
        InputSystem.actions.FindAction("Interact").performed += OnInteractPerformed;
    }

    private void OnInteractPerformed(InputAction.CallbackContext obj)
    {
        var layerMask = LayerMask.GetMask("Rabbit");
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        
        if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out hit,
                maxPickupDistance, layerMask))
        {
            // Debug.DrawRay(cam.transform.position, cam.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            // Debug.Log("Did Hit");
            TryHoldRabbit(hit.rigidbody);
        }
        else
        {
            // Debug.DrawRay(cam.transform.position, cam.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            // Debug.Log("Did not Hit");
        }
    }

    private void OnDropPerformed(InputAction.CallbackContext obj)
    {
        DropRabbit();
    }

    private void OnAttackPerformed(InputAction.CallbackContext obj)
    {
        ThrowRabbit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    bool TryHoldRabbit(Rigidbody rabbit)
    {
        if (joint.connectedBody != null)
        {
            return false;
        }

        if (rabbit == null)
        {
            return false;
        }
        
        rabbit.position = transform.position;
        rabbit.linearVelocity = Vector3.zero;
        joint.connectedBody = rabbit;

        return true;
    }

    void DropRabbit()
    {
        if (joint.connectedBody == null)
        {
            return;
        }
        
        joint.connectedBody = null;
    }

    void ThrowRabbit()
    {
        if (joint.connectedBody == null)
        {
            return;
        }

        var rabbit = joint.connectedBody;
        
        joint.connectedBody = null;
        
        rabbit.AddForce(cam.transform.forward * 1000);
    }
}
