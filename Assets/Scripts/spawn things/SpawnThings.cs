using System.Threading;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;
using Oculus.Interaction.DebugTree;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEditor;

public class SpawnThings : MonoBehaviour
{
    // --- État du spawn ---
    public static bool spawnCamera = true;
    public static bool spawnPorteIFMS = true;
    public static bool spawnWallDecoration = true;
    public static bool spawnFootball = true;
    public static bool spawnBoardGames = true;
    public static bool spawnWindowNoise = true;
    public static bool spawnDoorNoises = true;
    public static bool spawnCubes = true;

    // --- Variables internes ---
    private List<Collider> alreadySpawnedColliders = new List<Collider>();
    private float timer;
    private float off = 0f;

    [Header("Paramètres")]
    public float spawnTimer = 0.5f;
    public int nb_frames;

    // --- Prefabs ---
    [Header("Prefabs des différents éléments")]
    public GameObject cameraPrefab;
    public GameObject cubePrefab;
    public GameObject wallPrefab;
    // public GameObject footballPrefab;
    // public GameObject BoardGames;
    public GameObject porteIFMS;
    public GameObject windowSpotsPrefab;

    // --- Labels de scène ---
    [Header("Scene labels")]
    public float minEdgeDistance;
    public MRUKAnchor.SceneLabels spawnLabelsCamera;
    public MRUKAnchor.SceneLabels spawnLabels;
    public MRUKAnchor.SceneLabels spawnLabelsWall;
    public MRUKAnchor.SceneLabels avoid;
    public MRUKAnchor.SceneLabels spawnLabelsPorteIFMS;
    public MRUKAnchor.SceneLabels spawnAvoidLabelsWall;
    public MRUKAnchor.SceneLabels windowFrameLabel;
    public MRUKAnchor.SceneLabels spawnLabelWindowSpots;



    void Start()
    {
        alreadySpawnedColliders.Clear(); // reset before new spawn session
        StartCoroutine(WaitForRoomInitialization());

    }
    IEnumerator DelayedSpawnWallDecoration(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnWallDecoration();
    }

    private IEnumerator WaitForRoomInitialization()
    {
        // Attendre que MRUK soit initialisé
        while (!MRUK.Instance || !MRUK.Instance.IsInitialized)
            yield return null;

        // Attendre que la room soit créée et qu'il y ait au moins une ancre détectée
        MRUKRoom room = null;
        while (room == null || room.Anchors == null || room.Anchors.Count == 0)
        {
            room = MRUK.Instance.GetCurrentRoom();
            yield return null;
        }

        Debug.Log("[SpawnThings] Room et anchors initialisés, lancement des spawns.");

        // Une fois la salle initialisée, lancer les spawns conditionnels

        if (spawnWallDecoration)
        {
            if (room != null)
            {
                foreach (var anchor in room.GetComponentsInChildren<MRUKAnchor>(true))
                {
                    if (!anchor.HasAnyLabel(windowFrameLabel)) continue;

                    GameObject window = anchor.gameObject;

                    // Chercher ou créer l'enfant ObstacleBox
                    Transform obstacleBox = window.transform.Find("ObstacleBox");
                    if (obstacleBox == null)
                    {
                        GameObject obstacleBoxGO = new GameObject("ObstacleBox");
                        obstacleBoxGO.transform.SetParent(window.transform, false);
                        obstacleBox = obstacleBoxGO.transform;
                    }

                    // Chercher ou créer l'enfant cube dans ObstacleBox
                    Transform cubeChild = obstacleBox.Find("Cube");
                    if (cubeChild == null)
                    {
                        GameObject cubeGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cubeGO.name = "Cube";
                        cubeGO.transform.SetParent(obstacleBox, false);

                        // Désactiver le MeshRenderer
                        var meshRenderer = cubeGO.GetComponent<MeshRenderer>();
                        if (meshRenderer != null)
                            meshRenderer.enabled = false;

                        // Activer le BoxCollider
                        var boxCollider = cubeGO.GetComponent<BoxCollider>();
                        if (boxCollider != null)
                            boxCollider.enabled = true;
                        Debug.Log($"BoxCollider activé ? {boxCollider.enabled}");

                        Rigidbody rb = cubeGO.GetComponent<Rigidbody>();
                        if (rb == null)
                        {
                            rb = cubeGO.AddComponent<Rigidbody>();
                            rb.isKinematic = true; // Pour ne pas que la physique le déplace
                            rb.useGravity = false;
                        }

                    }
                    else
                    {
                        // Si l'enfant existe déjà, s'assurer que le MeshRenderer est désactivé et le BoxCollider activé
                        var meshRenderer = cubeChild.GetComponent<MeshRenderer>();
                        if (meshRenderer != null)
                            meshRenderer.enabled = false;

                        var boxCollider = cubeChild.GetComponent<BoxCollider>();
                        if (boxCollider != null)
                            boxCollider.enabled = true;
                    }
                }
            }

            StartCoroutine(DelayedSpawnWallDecoration(1f));
        }
        if (spawnPorteIFMS)
            SpawnDoor();

        SpawnWindowSpots();

        if (spawnCamera)
            SpawnCamera();

    }

    // Update is called once per frame
    void Update()
    {
        if (!MRUK.Instance || !MRUK.Instance.IsInitialized) return;

        timer += Time.deltaTime;
        if ((timer > spawnTimer) && spawnCubes) // si le timer est supérieur à spawnTimer et qu'on fait spawn des cubes
        {
            SpawnCubes();
            timer -= spawnTimer;
        }
    }
    public float animationSpeed = 0.3f; // 1.0 = normal, < 1 = ralenti, > 1 = accéléré

    public void SpawnWindowSpots()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("SpawnWindowSpots: MRUKRoom introuvable !");
            return;
        }

        foreach (var anchor in room.Anchors)
        {
            if (anchor.HasAnyLabel(spawnLabelWindowSpots))
            {
                Vector3 spawnPos = anchor.transform.position + anchor.transform.forward * 0.05f;
                Quaternion rotation = Quaternion.LookRotation(Vector3.forward, anchor.transform.forward);

                GameObject spot = Instantiate(windowSpotsPrefab, spawnPos, rotation);
                spot.SetActive(true);

                Animator animator = spot.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.speed = animationSpeed;
                }

                Debug.Log($"SpawnWindowSpots: Spot instancié avec bonne orientation et animation ralentie sur {anchor.name}");
                return;
            }
        }

        Debug.LogWarning("SpawnWindowSpots: Aucun anchor avec le label spécifié trouvé.");
    }


    public void SpawnCubes()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        room.GenerateRandomPositionOnSurface(MRUK.SurfaceType.FACING_UP, minEdgeDistance, new LabelFilter(spawnLabels), out Vector3 pos, out Vector3 norm);
        Debug.Log($"Spawn position: {pos}, Normal: {norm}");

        Vector3 randomPosition = pos + norm * off;
        Instantiate(cubePrefab, randomPosition, Quaternion.identity);
    }


    public bool drawWallGizmo = true;
    public static bool avoidSpawnWallDecoration = true;
    public int maxWallAttempts = 15;
    public float wallOffset = 0.05f; // `off` dans ton code


    public bool TrySpawnVerticalPrefab(
      GameObject prefab,
      MRUKRoom room,
      LabelFilter placementLabels,
      List<string> avoidTags,
      float minEdgeDist,
      float depthOffset,
      float verticalOffset,
      int maxAttempts,
      out GameObject spawned)
    {
        spawned = null;

        if (prefab == null || room == null)
        {
            Debug.LogError("TrySpawnVerticalPrefab: prefab ou room est null.");
            return false;
        }

        BoxCollider prefabCollider = prefab.GetComponentInChildren<BoxCollider>();
        if (prefabCollider == null)
        {
            Debug.LogError("TrySpawnVerticalPrefab: Le prefab n'a pas de BoxCollider !");
            return false;
        }

        Vector3 localCenter = prefabCollider.center;
        Vector3 localHalfExtents = prefabCollider.size * 0.5f;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (!room.GenerateRandomPositionOnSurface(
                MRUK.SurfaceType.VERTICAL,
                minEdgeDist,
                placementLabels,
                out Vector3 pos,
                out Vector3 norm))
            {
                Debug.LogWarning("TrySpawnVerticalPrefab: Pas de position murale trouvée.");
                return false;
            }

            Vector3 spawnPos = pos + norm * depthOffset;
            spawnPos.y = verticalOffset;

            Quaternion rotation = Quaternion.LookRotation(norm) * Quaternion.Euler(0, 90, 0);
            Vector3 worldCenter = spawnPos + rotation * localCenter;
            Vector3 worldHalfExtents = Vector3.Scale(localHalfExtents, prefab.transform.lossyScale);

            Collider[] overlaps = Physics.OverlapBox(worldCenter, worldHalfExtents, rotation);
            bool badCollision = false;

            foreach (var col in overlaps)
            {
                if (col.transform.IsChildOf(prefab.transform)) continue;

                foreach (string avoidTag in avoidTags)
                {
                    if (col.CompareTag(avoidTag))
                    {
                        badCollision = true;
                        Debug.Log($"[Spawn] Collision avec tag bloquant: {col.name}");
                        break;
                    }
                }

                if (badCollision) break;

                MRUKAnchor anchor = col.GetComponentInParent<MRUKAnchor>();
                if (anchor != null && anchor.HasAnyLabel(spawnAvoidLabelsWall))
                {
                    badCollision = true;
                    Debug.Log($"[Spawn] Collision avec anchor label interdit: {anchor.name}");
                    break;
                }
            }

            //  Extra custom overlap check with previous spawns
            if (!badCollision)
            {
                // Create a temporary clone collider for penetration testing
                GameObject temp = new GameObject("TempCheck");
                temp.hideFlags = HideFlags.HideAndDontSave;
                BoxCollider tempCol = temp.AddComponent<BoxCollider>();
                tempCol.size = prefabCollider.size;
                tempCol.center = prefabCollider.center;

                temp.transform.position = worldCenter;
                temp.transform.rotation = rotation;
                temp.transform.localScale = prefab.transform.lossyScale;

                foreach (Collider previousCol in alreadySpawnedColliders)
                {
                    if (Physics.ComputePenetration(
                        tempCol, temp.transform.position, temp.transform.rotation,
                        previousCol, previousCol.transform.position, previousCol.transform.rotation,
                        out Vector3 dir, out float dist))
                    {
                        Debug.Log($"[Spawn] ComputePenetration: Overlap detected with {previousCol.name} (dist: {dist})");
                        badCollision = true;
                        break;
                    }
                }

                GameObject.DestroyImmediate(temp); // Clean up test object
            }

            if (!badCollision)
            {
                spawned = GameObject.Instantiate(prefab, spawnPos, rotation);
                spawned.SetActive(true);

                alreadySpawnedColliders.AddRange(spawned.GetComponentsInChildren<Collider>());
                Debug.Log($"[Spawn] Objet instancié avec succès à la tentative {attempt + 1}");
                return true;
            }
        }

        Debug.LogWarning("[Spawn] Aucune position valide trouvée après toutes les tentatives.");
        return false;
    }


    public void SpawnDoor()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("SpawnDoor: MRUKRoom introuvable !");
            return;
        }

        LabelFilter placementLabels = new LabelFilter(spawnLabelsPorteIFMS);

        List<string> avoidTags = new List<string>
    {
        "Frame",
        "wall_avoid"
    };

        if (TrySpawnVerticalPrefab(
            porteIFMS,
            room,
            placementLabels,
            avoidTags,
            minEdgeDistance,
            wallOffset,
            0.05f, // vertical offset for door
            maxWallAttempts,
            out GameObject spawnedDoor))
        {
            Debug.Log("SpawnDoor: Porte instanciée avec succès !");
        }
        else
        {
            Debug.LogWarning("SpawnDoor: Aucune porte n'a pu être instanciée.");
        }
    }



    public void SpawnWallDecoration()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("SpawnWallDecoration: MRUKRoom introuvable !");
            return;
        }

        LabelFilter placementLabels = new LabelFilter(spawnLabelsWall);

        List<string> avoidTags = new List<string>
    {
        "Frame",
        "wall_avoid"
    };

        for (int i = 0; i < nb_frames; i++)
        {
            if (TrySpawnVerticalPrefab(
                wallPrefab,
                room,
                placementLabels,
                avoidTags,
                minEdgeDistance,
                wallOffset,
                1.5f, // vertical offset for wall frame
                maxWallAttempts,
                out GameObject decoration))
            {
                Animator animator = decoration.GetComponentInChildren<Animator>(true);
                if (animator != null)
                {
                    animator.enabled = true;
                    animator.gameObject.SetActive(true);
                }

                Debug.Log($"SpawnWallDecoration: Décoration {i + 1} instanciée.");
            }
            else
            {
                Debug.LogWarning($"SpawnWallDecoration: Échec pour la décoration {i + 1}.");
            }
        }
    }

    public void SpawnCamera()
    {
        Debug.Log("Spawn Camera");
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        // Génère une position sur une surface correspondant à spawnLabelsCamera
        if (room.GenerateRandomPositionOnSurface(
            MRUK.SurfaceType.FACING_DOWN,
            minEdgeDistance,
            new LabelFilter(spawnLabelsCamera),
            out Vector3 pos,
            out Vector3 norm)
            )
        {
            Debug.Log("Spawn Camera 2");

            Vector3 spawnPosition = pos + norm * 0.05f; // Légèrement au-dessus de la surface

            // Ajoute une rotation de 180° sur X pour remettre la caméra à l'endroit
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, norm) * Quaternion.Euler(180, 0, 0);

            GameObject cameraObj = Instantiate(cameraPrefab, spawnPosition, rotation);

            // Ajoute le script de clignotement si besoin
            if (cameraObj.GetComponent<CameraBlink>() == null)
                cameraObj.AddComponent<CameraBlink>();
        }
        else
        {
            Debug.LogWarning("Impossible de générer une position pour la caméra.");
        }
    }

    public int maxAttempts = 20;
    public bool drawGizmos = true;

    private Vector3 lastGizmoCenter;
    private Vector3 lastGizmoHalfExtents;
    private Quaternion lastGizmoRotation = Quaternion.identity;
    private bool lastGizmoOverlap;


    // Gizmos visibles dans la scène
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // Vérifie que la rotation est valide avant de l'utiliser
        if (lastGizmoRotation == Quaternion.identity || lastGizmoRotation.w == 0f)
        {
            return; // On évite de faire planter la scène Unity
        }

        Gizmos.color = lastGizmoOverlap ? Color.red : Color.green;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(lastGizmoCenter, lastGizmoRotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, lastGizmoHalfExtents * 2);
    }

}


