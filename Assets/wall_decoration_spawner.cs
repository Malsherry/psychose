using UnityEngine;
using Meta.XR.MRUtilityKit;

public class WallFrameSpawner : MonoBehaviour
{
    public GameObject framePrefab; // prefab du cadre à faire apparaître
    public float spawnTimer = 1f;
    private float timer;
    public float offset = 0.01f; // pour éviter que le cadre soit à l’intérieur du mur

    public float minEdgeDistance = 0.3f;
    public MRUKAnchor.SceneLabels spawnLabels;

    public int maxFrames = 5;
   // private int currentFrameCount = 0;
   void Start()
    {
       SpawnFrame();
    }
    void Update()
    {
        /*if (!MRUK.Instance || !MRUK.Instance.IsInitialized) return;

        if (currentFrameCount >= maxFrames) return;

        timer += Time.deltaTime;
        if (timer > spawnTimer)
        {
            SpawnFrame();
            timer = 0f;
        }*/
    }

    void SpawnFrame()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null) return;

        bool success = room.GenerateRandomPositionOnSurface(
            MRUK.SurfaceType.VERTICAL,
            minEdgeDistance,
            new LabelFilter(spawnLabels),
            out Vector3 pos,
            out Vector3 norm
        );

        if (!success)
        {
            Debug.LogWarning("Aucune position valide trouvée pour le cadre.");
            return;
        }

        Vector3 spawnPosition = pos + norm * offset;

        // Important : pour orienter le cadre "face à la pièce", on inverse la normale du mur
        Quaternion rotation = Quaternion.LookRotation(-norm, Vector3.up);

        Instantiate(framePrefab, spawnPosition, rotation);
    }
}
