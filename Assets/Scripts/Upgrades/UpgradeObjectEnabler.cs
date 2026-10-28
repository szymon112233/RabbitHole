using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UpgradeObjectEnabler : MonoBehaviour
{
    [SerializeField]
    protected PurchasableUpgrade Upgrade;

    void Start()
    {
        Upgrade.RuntimeFields.UpgradeBought += () => gameObject.SetActive(true);
    }
}
