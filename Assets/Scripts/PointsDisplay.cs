using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointsDisplay : MonoBehaviour
{
    public Text pointsText;
    public Text rabbitsText;
    public Text moniText;

    // Update is called once per frame
    void Update()
    {
        pointsText.text = GameManager.Instance.DisposedRabbits.ToString();
        rabbitsText.text = GameManager.Instance.GetUndisposedRabbitCount().ToString();
        moniText.text = GameManager.Instance.Money.ToString();
    }
}
