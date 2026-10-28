using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class VacuumCleaner : Tool
{
    private Collider[] m_HitColliders;
    [SerializeField] private Vector3 m_VacuumHalfExtents;
    [SerializeField] private float m_ShootForce = 50;
    [SerializeField] private int m_MaxVacuumedRabbitCount = 10;

    [SerializeField] protected Transform m_ProjectileSpawnPoint;
    [SerializeField] protected ParticleSystem m_ParticleEffect;
    
    [SerializeField] protected MeshRenderer _bulb;
    [SerializeField] protected Material _greenBulbMaterial;
    [SerializeField] protected Material _redBulbMaterial;
    [SerializeField] protected RabbitPileDisplay _rabbitDisplay;

    private Sequence m_DoTweenSequence;

    public override ToolType Type => ToolType.VACUUM;
    public override bool UsesHandGraphics => false;

    private bool m_IsVacuumTriggerHeld = false;
    private bool m_IsShootTriggerHeld = false;
    private LayerMask m_LayerMask;

    private List<ERabbitType> m_VacuumedBunnies = new();

    protected void Awake()
    {
        m_HitColliders = new Collider[5];
        m_LayerMask = LayerMask.GetMask("Rabbit");
    }

    public override void Use_Main()
    {
        if (!CanUse())
        {
            return;
        }

        PullVacuumTrigger();
    }

    public override void Stop_Main()
    {
        if (!CanUse())
        {
            return;
        }

        ReleaseVacuumTrigger();
    }

    public override void Use_Alt()
    {
        if (!CanUse())
        {
            return;
        }

        PullShootTrigger();
    }

    public override void Stop_Alt()
    {
        if (!CanUse())
        {
            return;
        }

        ReleaseShootTrigger();
    }

    private void FixedUpdate()
    {
        if (!CanUse())
        {
            return;
        }

        if (m_IsVacuumTriggerHeld)
        {
            HoldVacuumTrigger();
        }

        else if (m_IsShootTriggerHeld)
        {
            HoldShootTrigger();
        }
    }

    private bool CanUse()
    {
        // check if current collected rabbits < max collected rabbit count
        return true;
    }

    protected void PullVacuumTrigger()
    {
        m_IsVacuumTriggerHeld = true;

        AnimateVacuuming();
    }

    private void PullShootTrigger()
    {
        m_IsShootTriggerHeld = true;

        AnimateShootingAndShoot();
    }

    protected void HoldVacuumTrigger()
    {
        if (m_VacuumedBunnies.Count >= m_MaxVacuumedRabbitCount)
        {
            return;
        }
        
        m_HitColliders = new Collider[5];
        Physics.OverlapBoxNonAlloc(m_ProjectileSpawnPoint.position, m_VacuumHalfExtents, m_HitColliders, Transform.rotation, m_LayerMask, QueryTriggerInteraction.Ignore);
        
        foreach (var hitCollider in m_HitColliders)
        {
            if (m_VacuumedBunnies.Count < m_MaxVacuumedRabbitCount && hitCollider != null && hitCollider.TryGetComponent(out RabbitController rabbit))
            {
                if (rabbit.IsVacuumed)
                {
                    continue;
                }

                m_VacuumedBunnies.Add(rabbit.RabbitType);
                rabbit.Vacuum(m_ProjectileSpawnPoint);
                rabbit.onVacuumed.AddListener(OnVaccumed);

                UpdateLightBulb();
            }
        }
    }

    private void HoldShootTrigger()
    {

    }

    protected void ReleaseVacuumTrigger()
    {
        if (m_IsVacuumTriggerHeld)
        {
            m_IsVacuumTriggerHeld = false;
            FinishAnimation();
        }
    }

    private void ReleaseShootTrigger()
    {
        if (m_IsShootTriggerHeld)
        {
            m_IsShootTriggerHeld = false;
            FinishAnimation();
        }
    }


    private void OnVaccumed()
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence
            .Append(Transform.DOLocalMoveX(.19f, .03f))
            .Join(Transform.DOLocalRotate(new Vector3(0, 0f, -10f), .03f))
            .Append(Transform.DOLocalMoveX(0, .18f))
            .Join(Transform.DOLocalRotate(new Vector3(0f, 0f, 0f), .18f))
            ;
    }

    private void OnDrawGizmos()
    {
        if (m_ProjectileSpawnPoint)
        {
            Color color = Color.orange;
            Gizmos.color = color;
            Gizmos.matrix = Matrix4x4.TRS(Transform.TransformPoint(m_ProjectileSpawnPoint.localPosition), m_ProjectileSpawnPoint.transform.rotation, m_ProjectileSpawnPoint.transform.lossyScale);
            
            Gizmos.DrawWireCube(Vector3.zero, m_VacuumHalfExtents * 2f);

            color.a *= .3f;
            Gizmos.color = color;
            Gizmos.DrawCube(Vector3.zero, m_VacuumHalfExtents * 2f);
        }
    }

    protected void AnimateVacuuming()
    {
        m_DoTweenSequence = DOTween.Sequence();
        m_DoTweenSequence
            .Append(Transform.DOLocalMoveY(-.02f, .13f))
            .Join(Transform.DOLocalRotate(new Vector3(-1.5f, 0f, 0f), .13f))
            .Append(Transform.DOLocalMoveY(.02f, .13f))
            .Join(Transform.DOLocalRotate(new Vector3(1.5f, 0f, 0f), .13f))
            ;

        m_DoTweenSequence.SetLoops(-1);
    }

    protected void AnimateShootingAndShoot()
    {
        m_DoTweenSequence = DOTween.Sequence();
        m_DoTweenSequence
            .Append(Transform.DOLocalMoveY(-.02f, .13f))
            .Join(Transform.DOLocalRotate(new Vector3(-1.5f, 0f, 0f), .13f))
            .Append(Transform.DOLocalMoveY(.02f, .13f))
            .Join(Transform.DOLocalRotate(new Vector3(1.5f, 0f, 0f), .13f))
            ;

        m_DoTweenSequence.InsertCallback(0, Shoot);

        m_DoTweenSequence.SetLoops(-1);
    }

    private void Shoot()
    {

        if (m_VacuumedBunnies.Count <= 0)
        {
            m_DoTweenSequence.Pause();
            m_DoTweenSequence.Kill();
            return;
        }

        RabbitController rabbit = GameManager.Instance.RabbitPoolManager.RequestRabbit(m_ProjectileSpawnPoint.position, m_VacuumedBunnies[m_VacuumedBunnies.Count - 1]);
        if (rabbit)
        {
            rabbit.m_IsSlappable = true;
            Rigidbody rabbitBody = rabbit?.GetComponent<Rigidbody>();
            // get proper vector
            rabbitBody.AddForce(Camera.main.transform.forward * m_ShootForce, ForceMode.Impulse);
            m_VacuumedBunnies.RemoveAt(m_VacuumedBunnies.Count - 1);
            //UnityEditor.EditorApplication.isPaused = true;
            UpdateLightBulb();
        }
    }

    protected void FinishAnimation()
    {
        m_DoTweenSequence.Kill();
        m_DoTweenSequence = DOTween.Sequence();
        m_DoTweenSequence
            .Append(Transform.DOLocalMoveY(0f, .1f));
    }

    private void UpdateLightBulb()
    {
        _bulb.material = m_VacuumedBunnies.Count >= m_MaxVacuumedRabbitCount ? _redBulbMaterial : _greenBulbMaterial;
        _rabbitDisplay.SetAmountDisplayed(m_VacuumedBunnies.Count);
    }
}
