using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointsDisplay : MonoBehaviour
{
    public Text pointsText;
    public Text rabbitsText;

    // Update is called once per frame
    void Update()
    {
        pointsText.text = GameManager.Instance.Points.ToString();
        rabbitsText.text = GameManager.Instance.GetUndisposedRabbitCount().ToString();
    }
}
