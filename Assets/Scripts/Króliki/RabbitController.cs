using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;


public enum ERabbitType
{ 
    Default,
    Golden,
}


public class RabbitController : CachedMonoBehaviour
{
    [SerializeField] private Renderer Renderer;
    [SerializeField] private Renderer Renderer2;
    [SerializeField] private Transform m_ModelTransform;
    [SerializeField] private Transform m_LeftEarTransform;
    public Transform LeftEarTransform => m_LeftEarTransform;
    [SerializeField] private Transform m_RightEarTransform;
    public Transform RightEarTransform => m_RightEarTransform;
    private Vector3 m_RightEarPos;
    private Quaternion m_RightEarRot;
    private Vector3 m_LeftEarPos;
    private Quaternion m_LeftEarRot;

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

    [SerializeField] private Material _defualtRabbitMaterial;
    [SerializeField] private Material _goldRabbitMaterial;

    public Action OnKilled;

    protected bool frozen;
    protected float timeToHop;
    protected bool readyToHop;
    protected bool hitWall;

    private bool m_IsVacuumed = false;
    public bool m_IsSlappable = false;
    public bool IsVacuumed => m_IsVacuumed;
    public Rigidbody Rigidbody => Rbody;

    private Vector3 m_LastWallNormal;
    private Vector3 m_LastUnflattenedPosition;
    [HideInInspector]
    public UnityEvent onVacuumed;

    [SerializeField]
    private ERabbitType m_RabbitType;
    public ERabbitType RabbitType => m_RabbitType;

    private Sequence m_DoTweenSequence;

    public float m_Duration = .25f;
    public float m_Strength = 10f;
    public int m_Vibrato = 20;
    public float m_Randomness = 45;


    void Awake()
    {
        SphereCollider = GetComponent<SphereCollider>();
        BoxCollider = GetComponent<BoxCollider>();
        Initialize(ERabbitType.Default);

        m_RightEarPos = m_RightEarTransform.localPosition;
        m_LeftEarPos = m_LeftEarTransform.localPosition;
        m_RightEarRot = m_RightEarTransform.localRotation;
        m_LeftEarRot = m_LeftEarTransform.localRotation;

        //AnimateReadyToHopAndHop();
    }

    public void Initialize(ERabbitType rabbitType)
    {
        timeToHop = RandomHopCD();
        Transform.localScale = Vector3.one;
        Rbody.isKinematic = false;
        frozen = false;
        readyToHop = false;
        m_IsVacuumed = false;
        hitWall = false;
        m_IsSlappable = false;
        m_RabbitType = rabbitType;
        SphereCollider.enabled = true;
        BoxCollider.enabled = false;
        switch (m_RabbitType)
        {
            case ERabbitType.Golden:
                Renderer.material = _goldRabbitMaterial;
                Renderer2.material = _goldRabbitMaterial;
                break;
            default:
                Renderer.material = _defualtRabbitMaterial;
                Renderer2.material = _defualtRabbitMaterial;
                break;

        }
    }

    public void Kill()
    {
        frozen = true;
        Rbody.angularVelocity = Vector3.zero;
        Rbody.linearVelocity = Vector3.zero;
        // this could be merged somehow ig
        ResetEars();
        GameManager.Instance.RabbitPoolManager.ReturnToPool(this);
        OnKilled?.Invoke();
    }
    
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.U))
        {
            AnimateReadyToHopAndHop();
        }*/

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
            AnimateReadyToHopAndHop();
            //Hop();
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

    protected void AnimateReadyToHopAndHop()
    {
        readyToHop = false;

        m_DoTweenSequence = DOTween.Sequence();
        m_DoTweenSequence
            .Append(m_ModelTransform.DOScaleY(.6f, m_Duration))
            .Join(m_ModelTransform.DOShakeRotation(m_Duration, m_Strength, m_Vibrato, m_Randomness, false))
            .Append(m_ModelTransform.DOScaleY(1f, .1f))
            .AppendCallback(Hop)
            //.Append(m_ModelTransform.DOScaleY(1.1f, .05f))
            //.Append(m_ModelTransform.DOScaleY(1f, .05f))
            ;

        //m_DoTweenSequence.SetLoops(-1);
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
        m_IsSlappable = true;
        frozen = false;

        ResetEars();
    }

    private void ResetEars()
    {
        m_RightEarTransform.localPosition = m_RightEarPos;
        m_LeftEarTransform.localPosition = m_LeftEarPos;
        m_RightEarTransform.localRotation = m_RightEarRot;
        m_LeftEarTransform.localRotation = m_LeftEarRot;
    }

    public bool IsVisible()
    {
        return Renderer.isVisible;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hitWall && !frozen && m_IsSlappable && Vector3.Dot(collision.GetContact(0).normal, Vector3.up) < 0.5f && !collision.collider.CompareTag("Rabbit"))
        {
            
            m_LastWallNormal = collision.GetContact(0).normal;

            GetFlattened(collision.GetContact(0).point + collision.GetContact(0).normal * 0.035f, UnityEngine.Random.value > 0.5f ? collision.GetContact(0).normal : -collision.GetContact(0).normal);

            StartCoroutine(FallOffCoroutine());
            StartCoroutine(PopCoroutine(2.5f));
        }
    }

    public void GetFlattened(Vector3 rabbitPosition, Vector3 normalVector, float delay = 0f)
    {
        hitWall = true;
        m_IsSlappable = false;
        Rbody.angularVelocity = Vector3.zero;
        Rbody.linearVelocity = Vector3.zero;
        Rbody.isKinematic = true;
        frozen = true;

        if (GameManager.Instance.RabbitHoldController.ActiveTool.Type == ToolType.HAND)
        {
            ((Hand)GameManager.Instance.RabbitHoldController.ActiveTool).DropRabbit(Rbody);
        }

        SphereCollider.enabled = false;
        StartCoroutine(GetFlattenedCoroutine(rabbitPosition, normalVector, delay));
    }

    private IEnumerator GetFlattenedCoroutine(Vector3 rabbitPosition, Vector3 normalVector, float delay)
    { 
        yield return new WaitForSeconds(delay);
        Transform.localScale = new Vector3(2.4f, 1.43f, 0.0056f);
        Renderer.gameObject.SetActive(false);
        Renderer2.gameObject.SetActive(true);
        Transform.forward = normalVector;
        m_LastUnflattenedPosition = Transform.position;
        Transform.position = rabbitPosition;
        Transform.RotateAround(Transform.position, Transform.forward, UnityEngine.Random.Range(-20f, 20f));
        BoxCollider.enabled = true;

        StartCoroutine(PopCoroutine(2.5f));

    }

    private IEnumerator FallOffCoroutine()
    {
        yield return new WaitForSeconds(1f);

        Rbody.isKinematic = false;
        Rbody.AddForceAtPosition(m_LastWallNormal * 40f, Transform.position + Vector3.up * 1.1f);
    }

    private IEnumerator PopCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        Renderer.gameObject.SetActive(true);
        Renderer2.gameObject.SetActive(false);
        Rbody.isKinematic = false;
        SphereCollider.enabled = true;
        BoxCollider.enabled = false;
        Transform.localScale = Vector3.one;
        hitWall = false;
        //Transform.position = m_LastUnflattenedPosition; // bugs out the wall slap
        Rbody.AddForce(new Vector3(UnityEngine.Random.Range(-.75f, .75f), 1f, UnityEngine.Random.Range(-.75f, .75f)).normalized * 5f, ForceMode.Impulse);

    }

    public void Vacuum(Transform vacuumPointTransform)
    {
        m_IsVacuumed = true;
        StartCoroutine(GetVacuumedCoroutine(vacuumPointTransform));
    }

    private IEnumerator GetVacuumedCoroutine(Transform vacuumPointTransform)
    {
        float length = .1f;
        float timer = 0f;

        Vector3 startScale = Transform.localScale;

        while (timer < length)
        {
            float progress = timer / length;

            Transform.position = Vector3.Lerp(Transform.position, vacuumPointTransform.position, progress);
            Transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);

            timer += Time.fixedDeltaTime;
            yield return new WaitForEndOfFrame();
        }

        onVacuumed.Invoke();
        Kill();
    }

    public void SetRabbitMaterial(Material material)
    {
        Renderer.material = material;
        Renderer2.material = material;
    }

    
}
