using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

public class MultipleBoardGameSpawner : MonoBehaviour
{
    [Header("Prefabs à instancier")]
    public List<GameObject> BoardGamesList = new List<GameObject>();

    [Header("Labels")]
    public MRUKAnchor.SceneLabels spawnLabelsBoardGames;

    public void SpawnBoardGames()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("MRUKRoom introuvable !");
            return;
        }

        // Step 1: Find all tables with the label
        MRUKAnchor biggestTable = null;
        float maxSurface = 0f;
        List<MRUKAnchor> validAnchors = new List<MRUKAnchor>();

        foreach (var anchor in room.Anchors)
        {
            if (anchor.HasAnyLabel(spawnLabelsBoardGames))
            {
                MeshFilter meshFilter = anchor.GetComponentInChildren<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    Bounds bounds = meshFilter.sharedMesh.bounds;
                    Vector3 scale = anchor.transform.lossyScale;

                    float scaledX = bounds.size.x * scale.x;
                    float scaledZ = bounds.size.z * scale.z;
                    float surface = scaledX * scaledZ;

                    validAnchors.Add(anchor);

                    if (surface > maxSurface)
                    {
                        maxSurface = surface;
                        biggestTable = anchor;
                    }
                }
            }
        }

        // Step 2: Remove the biggest table
        if (biggestTable != null)
        {
            validAnchors.Remove(biggestTable);
        }

        // Step 3: Shuffle available tables
        if (validAnchors.Count == 0)
        {
            Debug.LogWarning("Aucune table disponible (autre que la plus grande).");
            return;
        }

        // Shuffle table list
        Shuffle(validAnchors);

        // Step 4: Spawn each board game prefab on one table
        int spawnCount = Mathf.Min(BoardGamesList.Count, validAnchors.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            MRUKAnchor anchor = validAnchors[i];
            GameObject prefab = BoardGamesList[i];

            Vector3 pos = anchor.transform.position + Vector3.up * 0.01f;
            Quaternion rot = Quaternion.LookRotation(anchor.transform.forward, Vector3.up);

            Instantiate(prefab, pos, rot);
            Debug.Log($"Instancié: {prefab.name} sur {anchor.name}");
        }
    }

    // Fisher-Yates shuffle
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            int randIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randIndex];
            list[randIndex] = temp;
        }
    }
}
