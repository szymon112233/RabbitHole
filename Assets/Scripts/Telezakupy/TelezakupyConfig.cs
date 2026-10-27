using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Video;

namespace UnityEngine.Rendering.Universal {
[Serializable]
[CreateAssetMenu(fileName = "TelezakupyConfig", menuName = "RabbitHole/TVMango/TelezakupyConfig")]
public class TelezakupyConfig : ScriptableObject {
    
    [SerializeField]
    protected List<PurchasableUpgrade> _InitialUpgrades;
    
    [SerializeField] 
    protected VideoClip _NoProgramm;
    
    [SerializeField]
    protected TelezakupyPlayerState _TPlayerState;

    public VideoClip NoProgramm => _NoProgramm;
    
    public List<PurchasableUpgrade> GetPurchasedUpgrades() {
        var result = new List<PurchasableUpgrade>();

        Queue<PurchasableUpgrade> queue = new Queue<PurchasableUpgrade>(_InitialUpgrades);

        while (queue.Count > 0) {
            var test = queue.Dequeue();

            if (_TPlayerState.IsUpgradePurchased(test.ID)) {
                result.Add(test);
                test.Unlocks.ForEach(x => queue.Enqueue(x));
            }
        }
        
        return result;
    }
    
    public List<PurchasableUpgrade> GetAvailableUpgrades() {
        var result = _InitialUpgrades.Where(x => !_TPlayerState.IsUpgradePurchased(x)).ToList();

        Debug.Log("Av Purchases");
        
        GetPurchasedUpgrades()
            .ForEach(x => {
                Debug.Log(" - "+x.Name);
                x.Unlocks.ForEach(u => {
                    Debug.Log(" ---> "+u.Name+" IsP: "+_TPlayerState.IsUpgradePurchased(u));
                    if (_TPlayerState.IsUpgradePurchased(u)) return;
                    if (result.Exists(y => y.ID == u.ID))
                        return;
                    
                    result.Add(u);
                });
            });
        
        return result.ToList();
    }
    
}
}