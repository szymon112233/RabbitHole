using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointsDisplay : MonoBehaviour
{
    public Text pointsText;

    // Update is called once per frame
    void Update()
    {
        pointsText.text = GameManager.Instance.Points.ToString();
    }
}
