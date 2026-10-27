using UnityEngine;

public abstract class Tool : MonoBehaviour
{
    
    [SerializeField]
    protected Rigidbody Rigidbody;

    
    public virtual ToolType Type => ToolType.HAND;

    public virtual void Use_Main()
    {
        
    }

    public virtual void Use_Alt()
    {
        
    }

    
}
