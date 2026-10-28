using UnityEngine;
using EasyPeasyFirstPersonController;
public class RabbitStomper : CachedMonoBehaviour
{
    [SerializeField] private FirstPersonController m_FirstPersonController;

    private bool m_CanStomp = false;
    private float lastYMoveDir = 0f;

    private void Update()
    {
        float currentYMoveDir = m_FirstPersonController.moveDirection.y;

        if (currentYMoveDir < lastYMoveDir)
        {
            m_CanStomp = true;
        }

        if (m_FirstPersonController.isGrounded || currentYMoveDir > 0f)
        {
            m_CanStomp = false;
        }

        lastYMoveDir = currentYMoveDir;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rabbit") && m_CanStomp /*m_FirstPersonController.moveDirection.y < 0f && !m_FirstPersonController.isGrounded*/)
        {
            Physics.Raycast(other.transform.position, Vector3.down, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Ground"));
            other.GetComponent<RabbitController>().GetFlattened(hitInfo.point + hitInfo.normal * 0.05F, hitInfo.normal, 0.1f);
        }
    }
}
