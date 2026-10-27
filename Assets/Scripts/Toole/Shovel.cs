using System;
using System.ComponentModel;
using UnityEngine;

public class Shovel : Tool
{
    public float SwingSpeed;
    public float MaxSwingRoataion = 115.0f;

    public override ToolType Type => ToolType.SHOVEL;
    public override bool UsesHandGraphics => false;

    // [NonSerialized]
    public bool isSwinging;

    // [NonSerialized]
    public int dir;

    public float _original_rotation;

    public override void Activate()
    {
        base.Activate();
        _original_rotation = transform.localEulerAngles.z;
    }

    public override void Use_Main()
    {
        if(isSwinging)
        {
            return;
        }

        isSwinging = true;
        dir = 1;
    }

    void FixedUpdate()
    {
        if (!isSwinging)
        {
            return;
        }

        if (dir > 0)
        {

            if(transform.localEulerAngles.z >= MaxSwingRoataion)
            {

                dir = -1;
                return;
            }

            float angle = Mathf.Clamp(transform.localEulerAngles.z + (MaxSwingRoataion - _original_rotation) * SwingSpeed/Time.fixedDeltaTime, _original_rotation, MaxSwingRoataion);
            Quaternion newRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Rigidbody.MoveRotation(newRotation);
        }
        else
        {
            if(transform.localEulerAngles.z <= _original_rotation)
            {
                isSwinging = false;
                return;
            }

            float angle = Mathf.Clamp(transform.localEulerAngles.z - (MaxSwingRoataion - _original_rotation) * SwingSpeed/Time.fixedDeltaTime, _original_rotation, MaxSwingRoataion);
                  Quaternion newRotation = Quaternion.AngleAxis(angle, Vector3.back);

            Rigidbody.MoveRotation(newRotation);
        }

    }
}
