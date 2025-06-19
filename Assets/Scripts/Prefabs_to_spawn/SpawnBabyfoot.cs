using Meta.XR.MRUtilityKit;
using UnityEngine;

public class BabySpawner : MonoBehaviour
{
    [Header("Références")]
    public GameObject footballPrefab;

    [Header("MRUK Labels")]
    public MRUKAnchor.SceneLabels spawnLabelsFootball;
    public MRUKAnchor.SceneLabels avoid;

    [Header("Spawn Settings")]
    public float minEdgeDistance = 0.5f;
    public int maxAttempts = 10;

    [Header("Debug")]
    public Vector3 lastGizmoCenter;
    public Vector3 lastGizmoHalfExtents;
    public Quaternion lastGizmoRotation;
    public bool lastGizmoOverlap;

    public void SpawnBabyfoot()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("MRUKRoom introuvable !");
            return;
        }

        BoxCollider prefabCollider = footballPrefab.GetComponentInChildren<BoxCollider>();
        if (prefabCollider == null)
        {
            Debug.LogError("Le prefab du babyfoot n'a pas de BoxCollider !");
            return;
        }

        Vector3 localCenter = prefabCollider.center;
        Vector3 localHalfExtents = prefabCollider.size * 0.5f;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (!room.GenerateRandomPositionOnSurface(
                MRUK.SurfaceType.FACING_UP,
                minEdgeDistance,
                new LabelFilter(spawnLabelsFootball),
                out Vector3 pos,
                out Vector3 norm))
            {
                Debug.LogWarning("Impossible de générer une position.");
                return;
            }

            pos.y = 0f;
            norm.y = 0f;
            Vector3 spawnPos = pos + norm;

            Quaternion baseRotation = Quaternion.LookRotation(Vector3.forward, norm);
            Quaternion modelCorrection = Quaternion.Euler(-90, 0, 0);
            Quaternion finalRotation = baseRotation * modelCorrection;

            Vector3 worldCenter = spawnPos + finalRotation * localCenter;
            Vector3 worldHalfExtents = Vector3.Scale(localHalfExtents, footballPrefab.transform.lossyScale);

            Collider[] overlaps = Physics.OverlapBox(worldCenter, worldHalfExtents, finalRotation);
            bool hasBadCollision = false;

            foreach (var col in overlaps)
            {
                if (col.transform.IsChildOf(footballPrefab.transform)) continue;

                MRUKAnchor anchor = col.GetComponentInParent<MRUKAnchor>();
                if (anchor != null && anchor.HasAnyLabel(avoid))
                {
                    //Debug.Log($"[baby Tentative {attempt + 1}] Collision avec '{anchor.name}'");
                    hasBadCollision = true;
                    break;
                }
            }

            lastGizmoCenter = worldCenter;
            lastGizmoHalfExtents = worldHalfExtents;
            lastGizmoRotation = finalRotation;
            lastGizmoOverlap = hasBadCollision;

            if (!hasBadCollision)
            {
                Instantiate(footballPrefab, spawnPos, finalRotation);
                //Debug.Log($"Babyfoot instancié à la tentative {attempt + 1}.");
                return;
            }
        }

        Debug.LogWarning("Impossible de trouver une position valide pour le babyfoot après plusieurs tentatives.");
    }
}
