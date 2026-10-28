using System;
using System.Collections.Generic;
using UnityEngine;

public class RabbitPileDisplay : MonoBehaviour
{
    public List<GameObject> ObjectInOrderToShow = new List<GameObject>();

    private int Counter;

    private void Start()
    {
        Clear();
    }


    public void Clear()
    {
        foreach (GameObject go in ObjectInOrderToShow)
        {
            go.SetActive(false);
        }

        Counter = 0;
    }

    public void Increment()
    {
        if (Counter >= ObjectInOrderToShow.Count)
        {
            Debug.LogWarning($"Cannot increse the display {gameObject.name}- at max");
            return;
        }
        ObjectInOrderToShow[Counter].SetActive(true);
        Counter++;
    }

    public void SetAmountDisplayed(int amount)
    {
        Clear();
        for (int i = 0; i < amount; i++)
        {
            Increment();
        }
    }
    
}