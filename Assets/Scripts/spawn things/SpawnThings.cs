using System.Threading;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;

public class SpawnThings : MonoBehaviour
{
    public GameObject cubePrefab; //cube qu'on fait spawn
    public GameObject wallPrefab; // truc qu'on fait spawn aux murs
    public GameObject footballPrefab; // babyfoot a faire spawn

    public float spawnTimer = 0.5f;
    private float timer;
    public int nb_frames;
    public bool spawnFootball;
    private float off = 0f;

    // paramètres dont on a besoin pour spawn des mesh aléatoirement dans la pièce
    public float minEdgeDistance;
    public MRUKAnchor.SceneLabels spawnLabels;
    public MRUKAnchor.SceneLabels spawnLabelsWall;
    public MRUKAnchor.SceneLabels spawnLabelsFootball;
    public MRUKAnchor.SceneLabels avoid; // labels à éviter pour le spawn du babyfoot


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(WaitForRoomInitialization());
    }

    private IEnumerator WaitForRoomInitialization()
    {
        // Attendre que MRUK.Instance soit initialisé
        while (!MRUK.Instance || !MRUK.Instance.IsInitialized)
        {
            yield return null;
        }

        // Une fois la salle initialisée, lancer les spawns
        for (int i = 0; i < nb_frames; i++)
        {
            SpawnWallDecoration();
        }
        SpawnFootball();
    }

    // Update is called once per frame
    void Update()
    {
        if (!MRUK.Instance || !MRUK.Instance.IsInitialized) return;

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
        room.GenerateRandomPositionOnSurface(MRUK.SurfaceType.FACING_UP, minEdgeDistance, new LabelFilter(spawnLabels), out Vector3 pos, out Vector3 norm);
        Debug.Log($"Spawn position: {pos}, Normal: {norm}");

        Vector3 randomPosition = pos + norm * off;
        Instantiate(cubePrefab, randomPosition, Quaternion.identity);
    }

    public void SpawnWallDecoration()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        Debug.Log("Getting the room: " + room);
        room.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL, minEdgeDistance, new LabelFilter(spawnLabelsWall), out Vector3 pos, out Vector3 norm);
        Debug.Log($"Spawn position: {pos}, Normal: {norm}");

        Vector3 randomPosition = pos + norm * off;

        randomPosition.y = 1.5f;         // Ajuste la position verticale pour qu'elle soit à 1,50 m du sol
        Quaternion rotation = Quaternion.LookRotation(norm) * Quaternion.Euler(0, 90, 0); // Calcule la rotation en fonction de la normale et ajoute une rotation de 90 degrés en Y
        Instantiate(wallPrefab, randomPosition, rotation); // Utilise la rotation calculée
    }

    public void SpawnFootball()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        // Distance minimale à respecter par rapport aux éléments avec les labels à éviter
        float minDistanceFromAvoidLabels = 0.3f; // Par exemple, 2 mètres
        int maxAttempts = 20; // Nombre maximum de tentatives pour trouver une position valide

        Vector3 pos = Vector3.zero;
        Vector3 norm = Vector3.up;
        bool positionFound = false;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Génère une position en utilisant les labels pour le babyfoot
            if (!room.GenerateRandomPositionOnSurface(
                MRUK.SurfaceType.FACING_UP,
                minEdgeDistance,
                new LabelFilter(spawnLabelsFootball),
                out pos,
                out norm))
            {
                Debug.LogWarning("Impossible de générer une position pour le babyfoot.");
                return;
            }

            // Vérifie la distance par rapport aux éléments avec les labels à éviter
            bool isFarEnough = true;
            foreach (var anchor in room.Anchors)
            {
                if (anchor.HasAnyLabel(avoid) && Vector3.Distance(anchor.transform.position, pos) < minDistanceFromAvoidLabels)
                {
                    isFarEnough = false;
                    Debug.Log($"Position trop proche d'un élément avec le label à éviter : {anchor.name}");
                    break;
                }
            }

            if (isFarEnough)
            {
                positionFound = true;
                Debug.Log($"Position valide trouvée pour le babyfoot : {pos}, Normale : {norm}");
                break;
            }
        }

        if (!positionFound)
        {
            Debug.LogWarning("Impossible de trouver une position valide pour le babyfoot après plusieurs tentatives.");
            return;
        }

        Vector3 randomPosition = pos + norm * off;

        // Base : on fait en sorte qu'il "colle" au sol
        Quaternion baseRotation = Quaternion.LookRotation(Vector3.forward, norm);

        // Correction de l'axe si le prefab a les pieds en l'air (ajuste si c'est X ou Z qui est à l'envers)
        Quaternion modelCorrection = Quaternion.Euler(-90, 0, 0);

        Quaternion finalRotation = baseRotation * modelCorrection;

        randomPosition.y = 0.75f; // Ajuste la position verticale pour qu'elle soit à 0,75 m du sol
        Instantiate(footballPrefab, randomPosition, finalRotation);
    }



}


