using System;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace DefaultNamespace {
public class UpgradePurchaseDisplayer : MonoBehaviour {
    [SerializeField] 
    protected TelezakupyUIObserver _Observer;

    [SerializeField] 
    protected GameObject _UI;

    [SerializeField] 
    protected Text _PurchaseText;
    
    private void OnEnable() {
        _Observer.SubscribeUpgradesForPurchase(OnNewPurchaseAvailable);
    }

    private void OnDisable() {
        _Observer.UnsubscribeUpgradesForPurchase(OnNewPurchaseAvailable);
    }

    private void OnNewPurchaseAvailable(PurchasableUpgrade upgrade) {
        if (upgrade == null) {
            _PurchaseText.gameObject.SetActive(false);
        }
        else {
            _PurchaseText.gameObject.SetActive(true);
            _PurchaseText.text = "[E] Buy("+upgrade.Price+"$) " + upgrade.Name;
        }
    }
}
}