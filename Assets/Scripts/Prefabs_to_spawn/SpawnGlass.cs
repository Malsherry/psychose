using UnityEngine;
using Meta.XR.MRUtilityKit;

public class GlassSpawner : MonoBehaviour
{
    [Header("Prefab à instancier")]
    public GameObject glassPrefab;

    [Header("Labels")]
    public MRUKAnchor.SceneLabels spawnLabelsTable;

    [Header("Audio")]
    public AudioClip glassSound;
    [Range(0f, 1f)] public float soundVolume = 0.8f;


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
            Vector3 pos = chosenTable.transform.position + Vector3.up * 0.01f;
            Quaternion rot = chosenTable.transform.rotation * Quaternion.Euler(90f, 0f, 0f);
            GameObject glass = Instantiate(glassPrefab, pos, rot);

            // Animation
            Animator animator = glass.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = true;
                animator.gameObject.SetActive(true);
            }
            // Ajouter et jouer le son une fois
            if (glassSound != null)
            {
                AudioSource source = glass.AddComponent<AudioSource>();
                source.clip = glassSound;
                source.volume = soundVolume;
                source.playOnAwake = false;
                source.spatialBlend = 0.5f; // 0 = 2D, 1 = 3D
                source.Play();
                Debug.Log($"[GlassSpawner] Son joué : {glassSound.name}, volume : {soundVolume}");

            }

            Debug.Log($"Verre instancié sur : {chosenTable.name}");
        }
        else
        {
            Debug.LogWarning("Aucune table disponible pour le verre (autre que la plus grande).");
        }
    }
}
