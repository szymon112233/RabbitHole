using UnityEngine;

public class RabbitStressTester : MonoBehaviour
{
    // Settings
    public GameObject RabbitPrefab;
    public int RabbitCountPerSpawn = 10;
    public float SpawnYOffset = 1.5f;
    
    // Internal data
    private Transform T;
    private int RabbitCount = 0;


    void Start()
    {
        T = transform;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            for (int i = 0; i < RabbitCountPerSpawn; i++)
            {
                Vector3 position = new Vector3(Random.Range(-1f, 1f), i * SpawnYOffset, Random.Range(-1f, 1f));
                Instantiate(RabbitPrefab, position, Quaternion.identity, T);
            }

            RabbitCount += RabbitCountPerSpawn;
            Debug.Log($"Current Rabbit Count: {RabbitCount}");
        }
    }
}
