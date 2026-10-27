using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Shovel : Tool
{
    public float SwingSpeed_DegreePerSecond;
    public float MaxSwingRoataion = 115.0f;
    public float SwingStrength = 6;
    public Transform FrontPoint; 

    public override ToolType Type => ToolType.SHOVEL;
    public override bool UsesHandGraphics => false;

    // [NonSerialized]
    public bool isSwinging;

    // [NonSerialized]
    public int dir;

    public float _original_rotation;

    private List<RabbitController> rabbits;

    void Awake()
    {
        _original_rotation = transform.localEulerAngles.z;
        rabbits = new List<RabbitController>();
        
    }

    public override void Activate()
    {
        base.Activate();
    }

    public override void Use_Main()
    {
        if(isSwinging)
        {
            return;
        }

        ThrowRabbits();
        isSwinging = true;
        dir = 1;
    }

    void ThrowRabbits()
    {
        foreach(var rabbit in rabbits)
        {
            Vector3 forward_force = (FrontPoint.position - transform.position).normalized * SwingStrength*2;
            forward_force.y = 0;

            rabbit.Rigidbody.AddForce(forward_force, ForceMode.Impulse);
            rabbit.Rigidbody.AddForce(Vector3.up * SwingStrength*0.7f, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        if (!isSwinging)
        {
            return;
        }

        if (dir > 0)
        {

            if(transform.localEulerAngles.z >= _original_rotation + MaxSwingRoataion)
            {
                Vector3 euler = transform.localEulerAngles;
                euler.z = _original_rotation + MaxSwingRoataion;
                transform.localEulerAngles = euler;
                dir = -1;
                return;
            }

            float angle = SwingSpeed_DegreePerSecond * Time.deltaTime;
            Quaternion newRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.localRotation = (transform.localRotation * newRotation);
        }
        else
        {
            if(transform.localEulerAngles.z <= _original_rotation)
            {
                Vector3 euler = transform.localEulerAngles;
                euler.z = _original_rotation;
                transform.localEulerAngles = euler;
                isSwinging = false;
                return;
            }

            float angle = SwingSpeed_DegreePerSecond * Time.deltaTime;
            Quaternion newRotation = Quaternion.AngleAxis(angle, Vector3.back);
            transform.localRotation = (transform.localRotation * newRotation);
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.body.CompareTag("Rabbit"))
        {
            rabbits.Add(collision.body.GetComponent<RabbitController>());
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.body.CompareTag("Rabbit"))
        {
            rabbits.Remove(collision.body.GetComponent<RabbitController>());
        }
    }
}
