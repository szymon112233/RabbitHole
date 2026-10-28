using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RabbitPoolManager : MonoBehaviour
{
    [SerializeField] private RabbitController RabbitPrefab;
    [SerializeField] private int MaxRabbitCount = 1500;
    [SerializeField] private List<RabbitController> RabbitPool = new List<RabbitController>();

    [SerializeField] private Dictionary<ERabbitType, float> m_RabbitTypeChanceDictionary = new Dictionary<ERabbitType, float>();


    private void Awake()
    {
        m_RabbitTypeChanceDictionary.Add(ERabbitType.Default, 1f);
        m_RabbitTypeChanceDictionary.Add(ERabbitType.Golden, 0.005f);
    }
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
