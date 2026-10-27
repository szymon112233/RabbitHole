using UnityEngine;

public class KillVolume : MonoBehaviour
{
    private const string RabbitTag = "Rabbit";

    [SerializeField]
    private int PointsPerRabbit = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(RabbitTag))
        {
            GameManager.Instance.Points += PointsPerRabbit;
            other.GetComponent<RabbitController>()?.Kill();
        }
    }
}
