using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleContoller : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Rigidbody controlledRigidbody;
    public Transform camTransform;
    public float speed;
    public float rotateSpeed;
    public Action OnVehicleEntered;

    private Vector3 camrafrombodydiff;
    

    private void Start()
    {
        camrafrombodydiff = camTransform.position - controlledRigidbody.transform.position;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void FixedUpdate()
    {
        Vector2 moveInput = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();

        Vector3 moveVector = controlledRigidbody.transform.forward * moveInput.y * speed;
        Vector3 rotateVector = new Vector3(0.0f, moveInput.x * rotateSpeed, 0.0f);
        // controlledRigidbody.linearVelocity = moveVector;
        // controlledRigidbody.angularVelocity = rotateVector;
        
        controlledRigidbody.MovePosition(controlledRigidbody.position + moveVector);
        controlledRigidbody.MoveRotation(controlledRigidbody.rotation * Quaternion.Euler(rotateVector));
        
        // camTransform.position = controlledRigidbody.position + camrafrombodydiff;
        
        // Debug.Log(moveVector.ToString());
    }

    private void OnEnable()
    {
        OnVehicleEntered?.Invoke();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.body.CompareTag("Rabbit"))
        {
            collision.body.GetComponent<RabbitController>().Grabbed();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.body.CompareTag("Rabbit"))
        {
            collision.body.GetComponent<RabbitController>().Thrown();
        }
    }
}
