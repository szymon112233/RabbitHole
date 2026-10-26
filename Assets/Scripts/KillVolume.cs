using UnityEngine;

public class KillVolume : MonoBehaviour
{
    private const string RabbitTag = "Rabbit";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(RabbitTag))
        {
            other.GetComponent<RabbitController>()?.Kill();
        }
    }
}
