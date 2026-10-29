using System.Collections.Generic;
using UnityEngine;

public class RabbitPoolManager : MonoBehaviour
{
    [SerializeField] private RabbitController RabbitPrefab;
    [SerializeField] private int MaxRabbitCount = 1500;
    [SerializeField] private List<RabbitController> RabbitPool = new();
    [SerializeField] private List<RabbitController> CurrentlyActiveRabbits = new();

    [SerializeField] private Dictionary<ERabbitType, float> m_RabbitTypeChanceDictionary;

    public List<RabbitController> GetCurrentlyActiveRabbits()
    {
        return CurrentlyActiveRabbits;
    }

    public void ReturnToPool(RabbitController rabbit)
    {
        // set position?
        rabbit.gameObject.SetActive(false);
        RabbitPool.Add(rabbit);
        CurrentlyActiveRabbits.Remove(rabbit);
    }

    public RabbitController RequestRabbit()
    {
        return RequestRabbit(Vector3.zero, GetRandomRabbitType());
    }

    public RabbitController RequestRabbit(Vector3 position, ERabbitType rabbitType)
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
        
        CurrentlyActiveRabbits.Add(rabbitToRelease);

        if (rabbitToRelease)
        {
            rabbitToRelease.Initialize(rabbitType);
            rabbitToRelease.transform.position = position;
        }

        return rabbitToRelease;
    }

    private RabbitController SpawnRabbit()
    {
        return Instantiate(RabbitPrefab).GetComponent<RabbitController>();
    }

    public ERabbitType GetRandomRabbitType()
    {
        float rabbitChancesSum = 0;

        foreach (float v in m_RabbitTypeChanceDictionary.Values)
        {
            rabbitChancesSum += v;
        }

        float randomValue = Random.Range(0f, rabbitChancesSum);
        float currentChance = 0f;

        foreach (ERabbitType rt in m_RabbitTypeChanceDictionary.Keys)
        {
            currentChance += m_RabbitTypeChanceDictionary[rt];
            if (randomValue <= currentChance)
            {
                return rt;
            }
        }

        return ERabbitType.Default;
    }
}
