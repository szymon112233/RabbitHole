using DefaultNamespace;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    protected TelezakupyPlayerState PlayerState; 

    [SerializeField] private RabbitSpawnManager m_RabbitSpawnManager;
    public RabbitSpawnManager RabbitSpawnManager { get => m_RabbitSpawnManager; }
    [SerializeField] private RabbitPoolManager m_RabbitPoolManager;
    public RabbitPoolManager RabbitPoolManager { get => m_RabbitPoolManager; }
    [SerializeField] private RabbitHoldController m_RabbitHoldController;
    public RabbitHoldController RabbitHoldController { get => m_RabbitHoldController; }
    
    public UIMousePrompts mouseUI;

    public bool DebugEnabled;

    private VacuumCleaner _vacuumCleaner;
    private SoundManager _soundManager;

    private void Awake()
    {
        _vacuumCleaner = FindAnyObjectByType<VacuumCleaner>(FindObjectsInactive.Include);
        _soundManager = FindAnyObjectByType<SoundManager>(FindObjectsInactive.Include);
        // If there is an instance, and it's not me, delete myself.
    
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        {
            Instance = this;
        } 
    }

    void Start()
    {
        PlayerState.Clear();
        
        InputSystem.actions.FindAction("GibMoni").performed += OnCheatMoniPerformed;
    }

    private void OnCheatMoniPerformed(InputAction.CallbackContext obj)
    {
        AddMoney(100);
    }


    public int DisposedRabbits;
    public int Money;


    public void AddMoney(int moni)
    {
        Money += moni;
        _soundManager.PlayCashMoneySound();
    }

    public bool HasMoney(int moni)
    {
        return Money >= moni;
    }
    
    public bool TrySpendMoney(int moni)
    {
        if (HasMoney(moni))
        {
            Money -= moni;
            return true;
        }

        return false;
    }

    public int GetUndisposedRabbitCount()
    {
        return m_RabbitPoolManager.GetCurrentlyActiveRabbits().Count + _vacuumCleaner.GetVacuumedBunniesCount();
    }
}
