using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    [SerializeField] private RabbitSpawnManager m_RabbitSpawnManager;
    public RabbitSpawnManager RabbitSpawnManager { get => m_RabbitSpawnManager; }
    [SerializeField] private RabbitPoolManager m_RabbitPoolManager;
    public RabbitPoolManager RabbitPoolManager { get => m_RabbitPoolManager; }

    private void Awake() 
    { 
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
    
    
    
    
    public int Points;

    public int GetUndisposedRabbitCount()
    {
        if (m_RabbitSpawnManager == null)
        {
            m_RabbitSpawnManager = FindAnyObjectByType<RabbitSpawnManager>();
        }

        if (m_RabbitSpawnManager == null)
        {
            return -1;
        }

        return m_RabbitSpawnManager.GetRabbitCount();
    }
    
}
