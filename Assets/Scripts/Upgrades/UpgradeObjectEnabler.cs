using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UpgradeObjectEnabler : MonoBehaviour
{
    [SerializeField]
    protected PurchasableUpgrade Upgrade;

    [SerializeField] 
    protected List<GameObject> _DisableOnUpgrade = new List<GameObject>();

    void Start()
    {
        Upgrade.RuntimeFields.UpgradeBought += () => {
            _DisableOnUpgrade.ForEach(x => x.SetActive(false));
            gameObject.SetActive(true);
        };
        gameObject.SetActive(false);
    }
}
