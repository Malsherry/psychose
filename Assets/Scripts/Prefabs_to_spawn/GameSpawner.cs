using UnityEngine;
using Meta.XR.MRUtilityKit;

public class GameSpawner : MonoBehaviour
{
    [Header("Prefab à instancier")]
    public GameObject BoardGames;

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

        MRUKAnchor biggestTable = null;
        float maxSurface = 0f;

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

                    if (surface > maxSurface)
                    {
                        maxSurface = surface;
                        biggestTable = anchor;
                    }
                }
            }
        }

        if (biggestTable != null)
        {
            Vector3 pos = biggestTable.transform.position + Vector3.up * 0.1f;
            Quaternion rot = Quaternion.LookRotation(biggestTable.transform.forward, Vector3.up);
            Instantiate(BoardGames, pos, rot);
            Debug.Log($"Jeu de société instancié sur : {biggestTable.name}, surface = {maxSurface}");
        }
        else
        {
            Debug.LogWarning("Aucune table détectée pour placer les jeux de société.");
        }
    }
}
