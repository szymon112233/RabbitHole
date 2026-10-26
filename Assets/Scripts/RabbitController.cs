using UnityEngine;

public class RabbitController : MonoBehaviour
{
    [SerializeField] private Renderer Renderer;

    void Start()
    {
    }   

    void Update()
    {
    }

    public bool IsVisible()
    {
        return Renderer.isVisible;
    }
}
