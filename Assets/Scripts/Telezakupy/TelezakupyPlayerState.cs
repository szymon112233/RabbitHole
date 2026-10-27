using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;

namespace UnityEngine.Rendering.Universal {
[CreateAssetMenu(fileName = "PlayerTelezakupyState", menuName = "RabbitHole/TVMango/PlayerTelezakupyState")]
[Serializable]
public class TelezakupyPlayerState : ScriptableObject {
    [SerializeField]
    protected List<string> _PurchasedUpgradeIds;
    
    public ImmutableList<string> PurchasedUpgradeIds => _PurchasedUpgradeIds.ToImmutableList();
    
    public void SavePurchasedUpgrade(PurchasableUpgrade upgrade) {
        if (!_PurchasedUpgradeIds.Contains(upgrade.ID)) {
            _PurchasedUpgradeIds.Add(upgrade.ID);
        }
    }
    
    public bool IsUpgradePurchased(PurchasableUpgrade upgrade) {
        return IsUpgradePurchased(upgrade.ID);
    }
    
    public bool IsUpgradePurchased(string id) {
        return _PurchasedUpgradeIds.Contains(id);
    }

}
}