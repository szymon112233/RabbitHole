using UnityEngine;
using UnityEngine.InputSystem.Android;
using UnityEngine.Rendering.Universal;

public abstract class Tool : CachedMonoBehaviour
{
    
    [SerializeField]
    protected Rigidbody Rigidbody;

    [SerializeField]
    protected PurchasableUpgrade _UpgradeNode;
    
    public PurchasableUpgrade UpgradeNode => _UpgradeNode;
    public virtual bool UsesHandGraphics => true;
    public virtual ToolType Type => ToolType.HAND;

    public virtual void Use_Main()
    {
        
    }

    public virtual void Stop_Main()
    {
        
    }

    public virtual void Use_Alt()
    {
        
    }

    public virtual void Stop_Alt()
    {
        
    }

    public virtual void Activate()
    {
        gameObject.SetActive(true);
    }

    public virtual void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
