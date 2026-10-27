using UnityEngine;

public class KillVolume : MonoBehaviour
{
    private const string RabbitTag = "Rabbit";
    public int MoneyForThisTrigger = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(RabbitTag))
        {
            GameManager.Instance.DisposedRabbits += 1;
            GameManager.Instance.AddMoney(MoneyForThisTrigger);
            other.GetComponent<RabbitController>()?.Kill();
        }
    }
}
