using System;
using UnityEngine;

public class CachedMonoBehaviour : MonoBehaviour
{
    [NonSerialized]
    private Transform m_Transform;
    public Transform Transform
    {
        get
        {
            if (m_Transform == null)
            {
                m_Transform = transform;
            }
            return m_Transform;
        }
    }
}
