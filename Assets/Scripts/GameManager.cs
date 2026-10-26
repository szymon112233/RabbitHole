using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private RabbitSpawnManager RabbitManager;

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
        if (RabbitManager == null)
        {
            RabbitManager = FindAnyObjectByType<RabbitSpawnManager>();
        }

        if (RabbitManager == null)
        {
            return -1;
        }

        return RabbitManager.GetRabbitCount();
    }
    
}
