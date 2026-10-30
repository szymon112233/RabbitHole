using UnityEngine;
using System;

public class EvilBun : MonoBehaviour
{    
    
    [SerializeField]
    protected Rigidbody Rbody;

    public void Hop(Transform playerTransform)
    {
        float hopPower = 20;

        Vector3 forwardDir = playerTransform.position - Rbody.position;
        forwardDir = forwardDir.normalized;

        Rbody.AddForce(forwardDir * hopPower, ForceMode.Impulse);
        Rbody.AddForce(Vector3.up * hopPower/1.7f, ForceMode.Impulse);
        
        Quaternion lookRotation = Rbody.rotation * Quaternion.FromToRotation(-Rbody.transform.forward, forwardDir);
        Rbody.MoveRotation(lookRotation);

    }

    Quaternion startRot;
    Quaternion targetRot;
    public void RotateByStepTowards(Transform player, float time_step)
    {
        if(time_step == 0)
        {
            Vector3 forwardDir = -new Vector3(player.position.x, 0, player.position.z) + new Vector3(transform.position.x, 0, transform.position.z);
            forwardDir = forwardDir.normalized;
            targetRot = Quaternion.FromToRotation(-transform.forward, forwardDir);
            startRot = Rbody.rotation;
            return;
        }

        Quaternion upRotation = Quaternion.Lerp(startRot, targetRot, time_step);
        Rbody.MoveRotation(upRotation);

        if(time_step >= 1)
        {
            transform.LookAt(player);
        }
    }
}
