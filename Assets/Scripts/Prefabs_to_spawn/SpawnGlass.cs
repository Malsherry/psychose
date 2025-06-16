using UnityEngine;
using Meta.XR.MRUtilityKit;

public class GlassSpawner : MonoBehaviour
{
    [Header("Prefab à instancier")]
    public GameObject glassPrefab;

    [Header("Labels")]
    public MRUKAnchor.SceneLabels spawnLabelsTable;

    public void SpawnGlass()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("MRUKRoom introuvable !");
            return;
        }

        MRUKAnchor biggestTable = null;
        float maxSurface = 0f;

        // D'abord, identifier la plus grande table
        foreach (var anchor in room.Anchors)
        {
            if (anchor.HasAnyLabel(spawnLabelsTable))
            {
                MeshFilter meshFilter = anchor.GetComponentInChildren<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    Bounds bounds = meshFilter.sharedMesh.bounds;
                    Vector3 scale = anchor.transform.lossyScale;

                    float scaledX = bounds.size.x * scale.x;
                    float scaledZ = bounds.size.z * scale.z;
                    float surface = scaledX * scaledZ;

                    if (surface > maxSurface)
                    {
                        maxSurface = surface;
                        biggestTable = anchor;
                    }
                }
            }
        }

        // Ensuite, trouver une autre table que la plus grande
        MRUKAnchor chosenTable = null;
        foreach (var anchor in room.Anchors)
        {
            if (anchor.HasAnyLabel(spawnLabelsTable) && anchor != biggestTable)
            {
                chosenTable = anchor;
                break;
            }
        }

        if (chosenTable != null)
        {
            Vector3 pos = chosenTable.transform.position + Vector3.up * 0.1f;
            Quaternion rot = Quaternion.LookRotation(chosenTable.transform.forward, Vector3.up);
            GameObject glass = Instantiate(glassPrefab, pos, rot);

            Animator animator = glass.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = true;
                animator.gameObject.SetActive(true);
            }

            Debug.Log($"Verre instancié sur : {chosenTable.name}");
        }
        else
        {
            Debug.LogWarning("Aucune table disponible pour le verre (autre que la plus grande).");
        }
    }
}
