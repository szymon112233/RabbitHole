using TMPro;
using UnityEngine;

public class PointsDisplay : MonoBehaviour
{
    public TextMeshProUGUI rabbitsText;
    public TextMeshProUGUI moniText;

    // Update is called once per frame
    void Update()
    {
        rabbitsText.text = $"Rabbits to deal with: {GameManager.Instance.GetUndisposedRabbitCount().ToString()}";
        moniText.text = $"{GameManager.Instance.Money.ToString()} $";
    }
}
