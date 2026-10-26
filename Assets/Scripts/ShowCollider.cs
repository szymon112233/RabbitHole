using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCollider : MonoBehaviour 
{
#if (UNITY_EDITOR)
    public bool show = true;
    public bool allChildren = false;
    public Color color = Color.red;

    private void OnDrawGizmos()
    {
        if (show)
        {
            BoxCollider[] boxColliders;
            if (allChildren)
            {
                boxColliders = GetComponentsInChildren<BoxCollider>();
                
            }
            else
            {
                boxColliders = GetComponents<BoxCollider>();
            }

            foreach (var collider in boxColliders)
            {
                DrawBoxCollider(collider.transform, color, collider);
            }
        }
    }

    public static void DrawBoxCollider(Transform transform, Color gizmoColor, BoxCollider boxCollider, float alphaForInsides = 0.3f)
    {
        //Save the color in a temporary variable to not overwrite changes in the inspector (if the sent-in color is a serialized variable).
        var color = gizmoColor;

        //Change the gizmo matrix to the relative space of the boxCollider.
        //This makes offsets with rotation work
        //Source: https://forum.unity.com/threads/gizmo-rotation.4817/#post-3242447
        Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(boxCollider.center), transform.rotation, transform.lossyScale);

        //Draws the edges of the BoxCollider
        //Center is Vector3.zero, since we've transformed the calculation space in the previous step.
        Gizmos.color = color;
        Gizmos.DrawWireCube(Vector3.zero, boxCollider.size);

        //Draws the sides/insides of the BoxCollider, with a tint to the original color.
        color.a *= alphaForInsides;
        Gizmos.color = color;
        Gizmos.DrawCube(Vector3.zero, boxCollider.size);
    }

    public static void DrawSphereCollider(Transform transform, Color gizmoColor, SphereCollider sphereCollider, float alphaForInsides = 0.3f)
    {
        Color color = gizmoColor;

        Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(sphereCollider.center), transform.rotation, transform.lossyScale);

        Gizmos.color = color;
        Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius);

        color.a *= alphaForInsides;
        Gizmos.color = color;
        Gizmos.DrawSphere(Vector3.zero, sphereCollider.radius);
    }
#endif
}