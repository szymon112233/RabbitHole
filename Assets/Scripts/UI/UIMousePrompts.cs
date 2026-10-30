using TMPro;
using UnityEngine;


public class UIMousePrompts : MonoBehaviour
{
    public TextMeshProUGUI LPMText;
    public GameObject RMBVisibility;
    public TextMeshProUGUI RMBText;
    
    
    public void SetEmptyHand()
    {
        ResetVisibility();
        LPMText.text = "Grab";
        RMBVisibility.SetActive(false);
    }

    public void SetBasicHand()
    {
        ResetVisibility();
        LPMText.text = "Release";
        RMBText.text = "Release";
    }
    public void SetStrongHand()
    {
        ResetVisibility();
        LPMText.text = "KICK!";
        RMBText.text = "Release";
    }
    public void SetRake()
    {
        ResetVisibility();
        LPMText.text = "Scoop";
        RMBVisibility.SetActive(false);
    }
    public void SetVacum()
    {
        ResetVisibility();
        LPMText.text = "Suck";
        RMBText.text = "Shoot";
    }
    public void SetTV()
    {
        ResetVisibility();
        LPMText.text = "Change channel";
        RMBText.text = "BUY";
    }
    
    public void SetTVnoBuy()
    {
        ResetVisibility();
        LPMText.text = "Change channel";
        RMBVisibility.SetActive(false);
    }

    public void ResetVisibility()
    {
        RMBVisibility.SetActive(true);
    }
}
