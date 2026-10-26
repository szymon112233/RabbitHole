using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitSpawnManager : MonoBehaviour
{
    [Header("Spawn Data")]
    [SerializeField] private RabbitController RabbitPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float RabbitSpawnDeltaTime = 0.1f;
    [Tooltip("Number of rabbits that will be checked every RabbitSpawnDeltaTime if they can have a baby!:)")]
    [SerializeField] private int QueryRabbitCount = 5;
    [SerializeField] private int MaxRabbitCount = 500;
    
    private List<RabbitController> RabbitList = new List<RabbitController>();
    private Transform T;

    void Start()
    {
        T = transform;
        StartCoroutine(SpawnRabbitCoroutine());
    }

    void Update()
    {
        
    }
    
    public int GetRabbitCount()
    {
        return RabbitList.Count;
    }

    private IEnumerator SpawnRabbitCoroutine()
    {
        if (RabbitList.Count == 0)
        {
            SpawnRabbit(T.position);
            SpawnRabbit(T.position);
        }
        else
        {
            List<int> uniqueRabbitIDs = new List<int>();

            // [SO] It actually does *not* guarantee to query the nr of rabbits specified below, because there's no check for double randomized number;
            // maybe I'll fix this later.
            for (int i = 0; i < Mathf.Min(QueryRabbitCount, RabbitList.Count); i++)
            {
                int randomID = Random.Range(0, RabbitList.Count);

                if (!uniqueRabbitIDs.Contains(randomID))
                {
                    uniqueRabbitIDs.Add(randomID);
                }
            }

            for (int i = 0; i < uniqueRabbitIDs.Count; i++)
            {
                RabbitController rabbit = RabbitList[uniqueRabbitIDs[i]];
                if (!rabbit.IsVisible())
                {
                    SpawnRabbit(RabbitList[uniqueRabbitIDs[i]].transform.position);
                }
            }
        }
        // bonus: check if rabbit is out of view

        yield return new WaitForSeconds(RabbitSpawnDeltaTime);
        StartCoroutine(SpawnRabbitCoroutine());
    }

    private void SpawnRabbit(Vector3 position, bool addRandomOffset = true)
    {
        Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
        RabbitList.Add(Instantiate(RabbitPrefab, addRandomOffset ? position + randomOffset : position, Quaternion.Euler(0f, Random.Range(-180f, 180f), 0f), T).GetComponent<RabbitController>());
    }
}
