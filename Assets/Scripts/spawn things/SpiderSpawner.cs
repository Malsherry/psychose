using System.Threading;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class SpiderSpawner : MonoBehaviour
{
    public static bool spawnSpiders = true; //bool pour savoir si on fait spawn les spider ou pas
    public GameObject spiderPrefab; //spider qu'on fait spawn
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
        //si spawnSpiders est faux, on ne fait rien
        if (!spawnSpiders) return;
        // On vérifie si MRUK est initialisé
        if (!MRUK.Instance || !MRUK.Instance.IsInitialized)
        {
            Debug.LogError("MRUK is not initialized. Please initialize MRUK before spawning objects.");
            return;
        }
        // On vérifie si la pièce actuelle est valide
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("Current room is null. Please ensure you are in a valid MRUK room.");
            return;
        }
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
        Instantiate(spiderPrefab, randomPosition, Quaternion.identity);
       
    }
}
