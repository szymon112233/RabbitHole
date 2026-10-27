using System;
using UnityEngine;


public class RabbitCounterTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Rabbit")
        {
            GameManager.Instance.Points += 1;
            other.GetComponent<RabbitController>()?.Kill();
        }
    }
}