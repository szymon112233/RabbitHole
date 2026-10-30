using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEditor;
using UnityEngine.UI;

namespace UnityEngine.Rendering.Universal {
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
[Serializable]
[CreateAssetMenu(fileName = "PurchasableUpgradeConfig", menuName = "RabbitHole/TVMango/PurchasableUpgradeConfig")]
public class PurchasableUpgrade : ScriptableObject {

    [SerializeField] 
    private string _ID;
    
    [SerializeField, Tooltip("Video file name inside Assets/StreamingAssets/Video, e.g. strong_ad.mp4")]
    protected string _AdVideo;

    [SerializeField] 
    protected string _Name;
    
    [SerializeField] 
    protected int _Price;
    
    [SerializeField] 
    protected Sprite _UpgradeIcon;
    
    [SerializeField] 
    protected List<PurchasableUpgrade> _Unlocks;
    
    #region Public API
    public string ID => _ID;
    
    /// <summary>File name of the ad inside StreamingAssets/Video.</summary>
    public string AdVideo => _AdVideo;
    
    public Sprite UpgradeIcon => _UpgradeIcon;
    
    public string Name => _Name;
    
    public int Price => _Price;
    
    public ImmutableList<PurchasableUpgrade> Unlocks => _Unlocks.ToImmutableList();

    [NonSerialized]
    public Runtime RuntimeFields;

    #endregion

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_ID))
        {
            _ID = System.Guid.NewGuid().ToString();
        #if UNITY_EDITOR
            // Tells Unity the asset has changed so it saves the ID to the disk file
            UnityEditor.EditorUtility.SetDirty(this);
        #endif
        }
    }

    void OnEnable()
    {
        RuntimeFields = new Runtime();
    }


    public class Runtime
    {
        public event Action UpgradeBought;

        public void Bought()
        {
            UpgradeBought?.Invoke();
        }
    }
}
}