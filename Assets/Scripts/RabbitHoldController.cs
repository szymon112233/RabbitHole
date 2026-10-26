using UnityEngine;

public class RabbitHoldController : MonoBehaviour
{

    public SpringJoint joint;

    public Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
