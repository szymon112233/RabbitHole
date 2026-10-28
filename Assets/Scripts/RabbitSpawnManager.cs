using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class RabbitSpawnManager : MonoBehaviour
{
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
        
        InputSystem.actions.FindAction("SpawnKrulkis").performed += OnCheatSpawnPerformed;
    }

    private void OnCheatSpawnPerformed(InputAction.CallbackContext obj)
    {
        for (int i = 0; i < 10; i++)
        {
            CreateRabbit(T.position);
        }
    }

    public int GetRabbitCount()
    {
        return RabbitList.Count;
    }

    private IEnumerator SpawnRabbitCoroutine()
    {
        if (RabbitList.Count == 0)
        {
            CreateRabbit(T.position);
            CreateRabbit(T.position);
        }
        else if (RabbitList.Count < MaxRabbitCount)
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
                    CreateRabbit(RabbitList[uniqueRabbitIDs[i]].transform.position);
                }
            }
        }
        // bonus: check if rabbit is out of view

        yield return new WaitForSeconds(RabbitSpawnDeltaTime);
        StartCoroutine(SpawnRabbitCoroutine());
    }

    private void CreateRabbit(Vector3 position, bool addRandomOffset = true)
    {
        RabbitController newRabbit = GameManager.Instance.RabbitPoolManager.RequestRabbit();

        if (newRabbit)
        {
            Vector3 spawnPosition = addRandomOffset ? position + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f)) : position;
            newRabbit.transform.position = spawnPosition;
            RabbitList.Add(newRabbit);
        }
    }

    public void RemoveRabbit(RabbitController rabbit)
    {
        RabbitList.Remove(rabbit);
    }
}
