using System.Threading;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class CubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab; //cube qu'on fait spawn
    public float spawnTimer = 1;
    private float timer;
    public float off;
    //private float off = 0.3f;

    // paramètres dont on a besoin pour spawn des objets aléatoirement dans la pièce
    public float minEdgeDistance = 0.3f;
    public MRUKAnchor.SceneLabels spawnLabels;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!MRUK.Instance && !MRUK.Instance.IsInitialized) return;
      
        timer += Time.deltaTime;
        if (timer > spawnTimer)
        {
            SpawnCubes();
            timer -= spawnTimer;
        }
    }

    public void SpawnCubes()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        room.GenerateRandomPositionOnSurface(MRUK.SurfaceType.FACING_UP, minEdgeDistance,new LabelFilter(spawnLabels), out Vector3 pos,out Vector3 norm);
        Debug.Log($"Spawn position: {pos}, Normal: {norm}");
        
        Vector3 randomPosition = pos+ norm*off;
        Instantiate(cubePrefab, randomPosition, Quaternion.identity);
       
    }
}
