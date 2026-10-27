using UnityEngine;
using UnityEngine.InputSystem.Android;

public abstract class Tool : MonoBehaviour
{
    
    [SerializeField]
    protected Rigidbody Rigidbody;

    
    public virtual bool UsesHandGraphics => true;
    public virtual ToolType Type => ToolType.HAND;

    public virtual void Use_Main()
    {
        
    }

    public virtual void Use_Alt()
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
