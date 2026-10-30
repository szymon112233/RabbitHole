using System;
using UnityEngine;

public class WallFader : MonoBehaviour
{
    [SerializeField]

    protected MeshRenderer _mesh;

    [SerializeField]
    protected float _minimumDistanceFromPlayer;

    [SerializeField]
    protected float _maximumDistanceFromPlayer;

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 playerPos = GameManager.Instance.RabbitHoldController.transform.position;
        Vector3 wallPos = transform.position;
        wallPos.y = playerPos.y;

        float dist = Vector3.Distance(wallPos, playerPos);

        _mesh.material.SetColor("_BaseColor", new Color(1, 1, 1, Mathf.Lerp(0, 1, Mathf.Abs(Mathf.Clamp(dist/(_minimumDistanceFromPlayer - _maximumDistanceFromPlayer), 0, 1 ) - 1)) ));
    }
}
