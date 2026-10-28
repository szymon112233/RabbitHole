using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEditor;
using UnityEngine.Video;

namespace UnityEngine.Rendering.Universal {
[Serializable][InitializeOnLoad]
[CreateAssetMenu(fileName = "PurchasableUpgradeConfig", menuName = "RabbitHole/TVMango/PurchasableUpgradeConfig")]
public class PurchasableUpgrade : ScriptableObject {

    [SerializeField] 
    private string _ID;
    
    [SerializeField] 
    protected VideoClip _AdClip;

    [SerializeField] 
    protected string _Name;
    
    [SerializeField] 
    protected int _Price;
    
    [SerializeField] 
    protected List<PurchasableUpgrade> _Unlocks;
    
    #region Public API
    public string ID => _ID;
    
    public VideoClip AdClip => _AdClip;
    
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
        Debug.Log("call");
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