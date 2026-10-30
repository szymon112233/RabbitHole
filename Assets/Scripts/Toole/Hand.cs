using UnityEngine;
using DefaultNamespace;
using System.Collections;

public class Hand : Tool
{
    public Joint joint;
    public Camera cam;

    public bool snapPosition = false;
    public float maxPickupDistance = 2000.0f;
    public float throwPower = 1000.0f;

    private HandController _handController;

    private bool ThrowingEnabled = false;

    RabbitController m_HeldRabbit;
    
    private SoundManager _soundManager;

    void Start()
    {
        _soundManager = FindAnyObjectByType<SoundManager>(FindObjectsInactive.Include);
        _handController = FindAnyObjectByType<HandController>();

        if (_handController == null)
        {
            Debug.LogError("BROOO! Put HandController on the scene!");
        }

        this.UpgradeNode.RuntimeFields.UpgradeBought += () => ThrowingEnabled = true;
    }

    private void Update()
    {
        if (m_HeldRabbit)
        {
            m_HeldRabbit.LeftEarTransform.position = joint.transform.position;
            m_HeldRabbit.RightEarTransform.position = joint.transform.position;

            Vector3 fromRabbitToJoint = (joint.transform.position - m_HeldRabbit.Transform.position).normalized;
            m_HeldRabbit.LeftEarTransform.up = fromRabbitToJoint;
            m_HeldRabbit.RightEarTransform.up = fromRabbitToJoint;
        }
    }

    public override void Use_Main()
    {
        if (IsHoldingARabbit())
        {
            if (ThrowingEnabled)
            {
                ThrowRabbit();
                
            }
            else
            {
                DropRabbit();
            }
        }
        else
        {
            TryHoldRabbit();
        }
    }

    public override void Use_Alt()
    {
        DropRabbit();
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
                maxPickupDistance, layerMask, QueryTriggerInteraction.Collide))
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
        m_HeldRabbit = rabbit.GetComponent<RabbitController>();
        m_HeldRabbit.Grabbed();
        m_HeldRabbit.OnKilled += OnHeldRabbitKilled;
        _handController.EnterGrabState();
        joint.connectedBody = rabbit;

        if (ThrowingEnabled)
        {
            GameManager.Instance.mouseUI.SetStrongHand();
        }
        else
        {
            GameManager.Instance.mouseUI.SetBasicHand();
        }
        

        return true;
    }

    private void OnHeldRabbitKilled()
    {
        _handController.ExitGrabState();
        joint.connectedBody = null;
        GameManager.Instance.mouseUI.SetEmptyHand();

        if (m_HeldRabbit != null)
        {
            m_HeldRabbit.OnKilled -= OnHeldRabbitKilled;
            m_HeldRabbit = null;
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
        joint.connectedBody.GetComponent<RabbitController>().OnKilled -= OnHeldRabbitKilled;
        _handController.ExitGrabState();
        joint.connectedBody = null;
        
        rabbit.position = joint.transform.position;
        rabbit.linearVelocity = Vector3.zero;
        rabbit.AddForce(cam.transform.forward * throwPower);
        m_HeldRabbit = null;
        GameManager.Instance.mouseUI.SetEmptyHand();
        StartCoroutine(HandleKickCoroutine());
    }

    private IEnumerator HandleKickCoroutine()
    {
        _handController.EnableKick();
        _soundManager.PlayPunchSound();
        
        yield return new WaitForSeconds(.3f);
        
        _handController.DisableKick();
    }

    public void DropRabbit()
    {
        if (joint.connectedBody == null)
        {
            return;
        }
        
        joint.connectedBody.GetComponent<RabbitController>().Thrown();
        joint.connectedBody.GetComponent<RabbitController>().OnKilled -= OnHeldRabbitKilled;
        _handController.ExitGrabState();
        joint.connectedBody = null;
        m_HeldRabbit = null;
        
        GameManager.Instance.mouseUI.SetEmptyHand();
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
        
        GameManager.Instance.mouseUI.SetEmptyHand();
    }

    public override void Activate()
    {
        enabled = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.mouseUI.SetEmptyHand();
        }
    }

    public override void Deactivate()
    {
        enabled = false;
    }
}
