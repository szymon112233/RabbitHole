using System.Collections.Generic;
using UnityEngine;

public class RabbitPoolManager : MonoBehaviour
{
    [SerializeField] private RabbitController RabbitPrefab;
    [SerializeField] private int MaxRabbitCount = 1500;
    [SerializeField] private List<RabbitController> RabbitPool = new List<RabbitController>();

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void ReturnToPool(RabbitController rabbit)
    {
        // set position?
        rabbit.gameObject.SetActive(false);
        RabbitPool.Add(rabbit);
    }

    public RabbitController RequestRabbit()
    {
        return RequestRabbit(Vector3.zero);
    }

    public RabbitController RequestRabbit(Vector3 position)
    {
        RabbitController rabbitToRelease = null;

        if (RabbitPool.Count > 0)
        {
            rabbitToRelease = RabbitPool[RabbitPool.Count - 1];
            rabbitToRelease.gameObject.SetActive(true);
            RabbitPool.Remove(rabbitToRelease);
        }
        else 
        {
            rabbitToRelease = SpawnRabbit();
        }

        if (rabbitToRelease)
        {
            rabbitToRelease.Initialize();
            rabbitToRelease.transform.position = position;
        }

        return rabbitToRelease;
    }

    private RabbitController SpawnRabbit()
    {
        return Instantiate(RabbitPrefab).GetComponent<RabbitController>();
    }
}
