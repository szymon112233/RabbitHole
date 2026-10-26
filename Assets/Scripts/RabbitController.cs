using System;
using UnityEngine;

public class RabbitController : MonoBehaviour
{
    [SerializeField] private Renderer Renderer;

    [SerializeField]
    protected float HopPower = 10.0f;

    [SerializeField]
    protected float MaximumHopCooldown = 20.0f;

    [SerializeField]
    protected Rigidbody Rbody;

    protected bool frozen;
    protected float timeToHop;
    protected bool readyToHop;

    void Awake()
    {
        timeToHop = RandomHopCD();
        readyToHop = false;
    }
    
    void Update()
    {
        if(timeToHop > 0)
        {
            timeToHop -= Time.deltaTime;

            if (timeToHop <= 0)
            {
                timeToHop = 0;
                readyToHop = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (frozen)
        {
            return;
        }

        if(!readyToHop)
        {
            return;
        }
        else
        {
            Hop();
        }

    }

    protected float RandomHopCD()
    {
        return UnityEngine.Random.Range(MaximumHopCooldown * 0.2f, MaximumHopCooldown);
    }

    protected float RandomHopPower()
    {
        return UnityEngine.Random.Range(HopPower * 0.4f, HopPower);
    }

    protected void Hop()
    {
        float hopPower = RandomHopPower();
        double randomAngle = UnityEngine.Random.Range(0.0f, 2.0f) * Math.PI;
        Debug.Log(randomAngle);
        double x = Math.Cos(randomAngle);
        double z = Math.Sin(randomAngle);
        Vector3 forewardDir = new Vector3((float)x, 0, (float)z);
        
        Rbody.AddForce(forewardDir * hopPower, ForceMode.Impulse);
        Rbody.AddForce(Vector3.up * hopPower, ForceMode.Impulse);

        readyToHop = false;
        timeToHop = RandomHopCD();
    }

    public bool IsVisible()
    {
        return Renderer.isVisible;
    }
}
