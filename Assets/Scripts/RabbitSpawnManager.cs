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
    private Transform T;

    private bool IsActive;

    void Start()
    {
        IsActive = true;
        T = transform;
        StartCoroutine(SpawnRabbitCoroutine());
        
        InputSystem.actions.FindAction("SpawnKrulkis").performed += OnCheatSpawnPerformed;
    }

    public void Stop()
    {
        IsActive = false;
    }

    private void OnCheatSpawnPerformed(InputAction.CallbackContext obj)
    {
        for (int i = 0; i < 10; i++)
        {
            CreateRabbit(T.position);
        }
    }

    private IEnumerator SpawnRabbitCoroutine()
    {
        List<RabbitController> currentlyActiveRabbits = GameManager.Instance.RabbitPoolManager.GetCurrentlyActiveRabbits();
        
        if (currentlyActiveRabbits.Count == 0)
        {
            CreateRabbit(T.position);
            CreateRabbit(T.position);
        }
        else if (currentlyActiveRabbits.Count < MaxRabbitCount)
        {
            List<int> uniqueRabbitIDs = new List<int>();

            // [SO] It actually does *not* guarantee to query the nr of rabbits specified below, because there's no check for double randomized number;
            // maybe I'll fix this later.
            for (int i = 0; i < Mathf.Min(QueryRabbitCount, currentlyActiveRabbits.Count); i++)
            {
                int randomID = Random.Range(0, currentlyActiveRabbits.Count);

                if (!uniqueRabbitIDs.Contains(randomID))
                {
                    uniqueRabbitIDs.Add(randomID);
                }
            }

            for (int i = 0; i < uniqueRabbitIDs.Count; i++)
            {
                RabbitController rabbit = currentlyActiveRabbits[uniqueRabbitIDs[i]];
                
                if (!rabbit.IsVisible())
                {
                    CreateRabbit(currentlyActiveRabbits[uniqueRabbitIDs[i]].transform.position);
                }
            }
        }
        // bonus: check if rabbit is out of view

        yield return new WaitForSeconds(RabbitSpawnDeltaTime);

        if (IsActive)
        {
            StartCoroutine(SpawnRabbitCoroutine());
            
        }
    }

    private void CreateRabbit(Vector3 position, bool addRandomOffset = true)
    {
        RabbitController newRabbit = GameManager.Instance.RabbitPoolManager.RequestRabbit();

        if (newRabbit)
        {
            Vector3 spawnPosition = addRandomOffset ? position + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f)) : position;
            newRabbit.transform.position = spawnPosition;
        }
    }
}
