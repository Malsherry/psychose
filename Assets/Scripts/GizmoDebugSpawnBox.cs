using UnityEngine;

public class GizmoDebugSpawnBox : MonoBehaviour
{
    public Vector3 worldCenter;
    public Vector3 worldHalfExtents;
    public Quaternion rotation = Quaternion.identity;

    public bool drawGizmo = true;
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.4f);

    private void OnDrawGizmos()
    {
        if (!drawGizmo) return;

        Gizmos.color = gizmoColor;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(worldCenter, rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;

        Gizmos.DrawCube(Vector3.zero, worldHalfExtents * 2f);
    }
}
