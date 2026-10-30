using System;
using UnityEngine;

public class KillVolume : MonoBehaviour
{
    private const string RabbitTag = "Rabbit";
    public int MoneyForThisTrigger = 0;
    
    [Header("Capacity")]
    public bool bUsesCapacity;

    public bool bDontReset;
    
    public int Capacity = 0;
    public RabbitPileDisplay display;
    public float Cooldown;
    public GameObject ObjectToDisableOnCooldown;
    
    private int Count = 0;

    private Timer cooldownTimer;
    private Vector3 cachedPosition;

    private void Awake()
    {
        cooldownTimer = new Timer();
    }

    private void Update()
    {
        if (bUsesCapacity)
        {
            cooldownTimer.Tick();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(RabbitTag))
        {
            return;
        }
        
        RabbitController rabbitController = other.GetComponent<RabbitController>();
            
        if (rabbitController == null)
        {
            return;
        }

        GameManager.Instance.DisposedRabbits += 1;
        GameManager.Instance.AddMoney(rabbitController.RabbitType == ERabbitType.Golden ? MoneyForThisTrigger * 5 : MoneyForThisTrigger);
        Instantiate(GameManager.Instance.CashMoneyParticles, rabbitController.transform.position, Quaternion.identity);
        rabbitController.Kill();
        
        if (bUsesCapacity)
        {
            Count++;
            display.Increment();

            if (!bDontReset && Count >= Capacity)
            {
                StartCooldown();
            }
        }
    }

    private void StartCooldown()
    {
        cooldownTimer.Start(Cooldown);
        cooldownTimer.CompletedEvent += OnTimerFinished;
        cachedPosition = ObjectToDisableOnCooldown.transform.position;
        //Almost as good as disbling it bout it gets the callback
        ObjectToDisableOnCooldown.transform.position = new Vector3(100000, 100000, 100000);

    }

    private void OnTimerFinished()
    {
        Debug.Log("TIme up!");
        Count = 0;
        display.Clear();
        ObjectToDisableOnCooldown.transform.position = cachedPosition;
        cooldownTimer.Stop();
        cooldownTimer.CompletedEvent -= OnTimerFinished;
    }
}
