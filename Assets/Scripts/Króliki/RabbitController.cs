using System;
using System.Collections;
using UnityEngine;

public class RabbitController : MonoBehaviour
{
    [SerializeField] private Renderer Renderer;

    [SerializeField]
    protected float HopPower = 10.0f;

    [SerializeField]
    protected float MaximumHopCooldown = 20.0f;

    [SerializeField]
    protected float LeashLength = 20.0f;

    [SerializeField]
    protected float MaximumAngleRotationTwoardsGround = 5.0f;

    [SerializeField]
    protected Rigidbody Rbody;
	
    [SerializeField]
    protected float MinVelocityToSlap = 1.5f;

    [SerializeField]
    protected SphereCollider SphereCollider;

    [SerializeField]
    protected BoxCollider BoxCollider;

    protected bool frozen;
    protected float timeToHop;
    protected bool readyToHop;
    protected bool hitWall;

    private Transform T;
    private Vector3 lastWallNormal;

    void Awake()
    {
        T = transform;
        SphereCollider = GetComponent<SphereCollider>();
        BoxCollider = GetComponent<BoxCollider>();
        Initialize();
    }

    public void Initialize()
    {
        timeToHop = RandomHopCD();
        frozen = false;
        readyToHop = false;
    }

    public void Kill()
    {
        frozen = true;
        Rbody.angularVelocity = Vector3.zero;
        Rbody.linearVelocity = Vector3.zero;
        // this could be merged somehow ig
        GameManager.Instance.RabbitPoolManager.ReturnToPool(this);
        GameManager.Instance.RabbitSpawnManager.RemoveRabbit(this);
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
            RotateTowardsGround();
            return;
        }
        else
        {
            Hop();
        }

    }

    protected float RandomHopCD()
    {
        return UnityEngine.Random.Range(MaximumHopCooldown * 0.5f, MaximumHopCooldown);
    }

    protected float RandomHopPower()
    {
        return UnityEngine.Random.Range(HopPower * 0.4f, HopPower);
    }

    protected void Hop()
    {
        float hopPower = RandomHopPower();
        double randomAngle = UnityEngine.Random.Range(0.0f, 2.0f) * Math.PI;
        double x = Math.Cos(randomAngle);
        double z = Math.Sin(randomAngle);


        if(Vector3.Distance(transform.position, GameManager.Instance.RabbitSpawnManager.transform.position) >= LeashLength)
        {
            Vector3 dir = (GameManager.Instance.RabbitSpawnManager.transform.position - transform.position).normalized;
            x = dir.x;
            z = dir.z;
        }

        Vector3 forwardDir = new Vector3((float)x, 0, (float)z);

        Rbody.AddForce(forwardDir * hopPower, ForceMode.Impulse);
        Rbody.AddForce(Vector3.up * hopPower, ForceMode.Impulse);
        
        Quaternion lookRotation = Rbody.rotation * Quaternion.FromToRotation(-Rbody.transform.forward, forwardDir);
        Rbody.MoveRotation(lookRotation);

        readyToHop = false;
        timeToHop = RandomHopCD();
    }

    protected void RotateTowardsGround()
    {
        Quaternion upRotation = Quaternion.RotateTowards(Rbody.rotation, Quaternion.Euler(0, Rbody.transform.eulerAngles.y, 0), MaximumAngleRotationTwoardsGround);
        Rbody.MoveRotation(upRotation);
    }

    public void Grabbed()
    {
        Rbody.linearVelocity = Vector3.zero;
        Rbody.angularVelocity = Vector3.zero;
        timeToHop = RandomHopCD();
        frozen = true;
    }

    public void Thrown()
    {
        frozen = false;
    }

    public bool IsVisible()
    {
        return Renderer.isVisible;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hitWall && Rbody.linearVelocity.magnitude > MinVelocityToSlap && Vector3.Dot(collision.GetContact(0).normal, Vector3.up) < 0.5f)
        {
            //Debug.Log($"Rbody.linearVelocity.magnitude: {Rbody.linearVelocity.magnitude}");

            hitWall = true;

            T.localScale = new Vector3(2.4f, 1.43f, 0.0056f);
            Rbody.angularVelocity = Vector3.zero;
            Rbody.linearVelocity = Vector3.zero;
            Rbody.isKinematic = true;

            lastWallNormal = collision.GetContact(0).normal;

            T.forward = UnityEngine.Random.value > 0.5f ? collision.GetContact(0).normal : -collision.GetContact(0).normal;
            T.position = collision.GetContact(0).point + collision.GetContact(0).normal * 0.035f;
            T.RotateAround(T.position, T.forward, UnityEngine.Random.Range(-20f, 20f));

            SphereCollider.enabled = false;
            BoxCollider.enabled = true;

            frozen = true;

            if (GameManager.Instance.RabbitHoldController.ActiveTool.Type == ToolType.HAND)
            {
                ((Hand)GameManager.Instance.RabbitHoldController.ActiveTool).DropRabbit(Rbody);
                
            }

            StartCoroutine(FallOffCoroutine());
        }
    }

    private IEnumerator FallOffCoroutine()
    {
        yield return new WaitForSeconds(1f);

        Rbody.isKinematic = false;
        Rbody.AddForceAtPosition(lastWallNormal * 40f, T.position + Vector3.up * 1.1f);

        yield return new WaitForSeconds(1.5f);

        SphereCollider.enabled = true;
        BoxCollider.enabled = false;
        T.localScale = Vector3.one;
        hitWall = false;
        Rbody.AddForce(new Vector3(UnityEngine.Random.Range(-.75f, .75f), 1f, UnityEngine.Random.Range(-.75f, .75f)).normalized * 5f, ForceMode.Impulse);

    }

}
