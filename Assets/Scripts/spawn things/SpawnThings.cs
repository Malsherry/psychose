using System.Threading;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;
using Oculus.Interaction.DebugTree;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEditor;
using System.Runtime.CompilerServices;

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
    public static bool spawnMenu = true; // Pour le script SpiderSpawner

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
    public GameObject menuPrefab;

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


    [Header("Menu")]
    public static bool menu = true;

    void Start()
    {
        alreadySpawnedColliders.Clear(); // reset before new spawn session
        StartCoroutine(WaitForRoomInitialization());


    }


    private IEnumerator WaitForRoomInitialization()
    {
        // Wait for MRUK init
        while (!MRUK.Instance || !MRUK.Instance.IsInitialized)
            yield return null;

        // Wait for room and anchors
        MRUKRoom room = null;
        while (room == null || room.Anchors == null || room.Anchors.Count == 0)
        {
            room = MRUK.Instance.GetCurrentRoom();
            yield return null;
        }

        Debug.Log("[SpawnThings] Room and anchors initialized, setting up obstacles.");

        if (room != null)
        {
            foreach (var anchor in room.GetComponentsInChildren<MRUKAnchor>(true))
            {
                if (!anchor.HasAnyLabel(windowFrameLabel)) continue;

                GameObject window = anchor.gameObject;
                if (window.name.Contains("DOOR_FRAME") || window.name.Contains("WINDOW_FRAME"))
                {
                    window.tag = "wall_avoid"; // or "wall_avoid" or "window_spots" depending on your avoid list
                }

                string effectMeshName = window.name.Contains("DOOR_FRAME") ? "DOOR_FRAME_EffectMesh" : "WINDOW_FRAME_EffectMesh";
                
                Transform obstacleBox = window.transform.Find("ObstacleBox");
                if (obstacleBox == null)
                {
                    GameObject obstacleBoxGO = new GameObject("ObstacleBox");
                    obstacleBoxGO.transform.SetParent(window.transform, false);
                    obstacleBox = obstacleBoxGO.transform;
                }

                Transform effectMesh = window.transform.Find(effectMeshName);
                if (effectMesh == null)
                {
                    Debug.LogWarning($"{effectMeshName} non trouvé dans {window.name}");
                    continue;
                }

                MeshCollider meshCollider = effectMesh.GetComponent<MeshCollider>();
                if (meshCollider == null)
                {
                    Debug.LogWarning($"MeshCollider manquant sur {effectMesh.name}");
                    continue;
                }

                Transform cubeChild = obstacleBox.Find("Cube");
                GameObject cubeGO = cubeChild == null
                    ? GameObject.CreatePrimitive(PrimitiveType.Cube)
                    : cubeChild.gameObject;

                cubeGO.name = "Cube";
                cubeGO.transform.SetParent(obstacleBox, false);

                var meshRenderer = cubeGO.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                    meshRenderer.enabled = false;

                var boxCollider = cubeGO.GetComponent<BoxCollider>() ?? cubeGO.AddComponent<BoxCollider>();
                boxCollider.enabled = true;

                Bounds meshBounds = meshCollider.sharedMesh.bounds;
                cubeGO.transform.localPosition = effectMesh.localPosition + meshBounds.center;
                cubeGO.transform.localRotation = Quaternion.identity;

                Vector3 scale = meshBounds.size;
                scale.z = 0.5f;
                cubeGO.transform.localScale = scale;

                if (!cubeGO.TryGetComponent(out Rigidbody rb))
                {
                    rb = cubeGO.AddComponent<Rigidbody>();
                }
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // Let Unity physics register new colliders
            yield return null;
            Debug.Log("[SpawnThings] windows anchors initialized, launching functions.");

            // Safe to spawn now

            SpawnWindowSpots();

            if (spawnWallDecoration)
                SpawnWallDecoration();

            SpawnMenu();

            if (spawnPorteIFMS)
                SpawnDoor();

            if (spawnCamera)
                SpawnCamera();

            Debug.Log("[SpawnThings] All objects spawned successfully.");
            


        }
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
    public float animationSpeed = 1f; // 1.0 = normal, < 1 = ralenti, > 1 = accéléré

    public void SpawnWindowSpots()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("SpawnWindowSpots: MRUKRoom introuvable !");
            return;
        }

        bool foundAny = false;

        foreach (var anchor in room.Anchors)
        {
            if (anchor.HasAnyLabel(spawnLabelWindowSpots))
            {
                Vector3 spawnPos = anchor.transform.position + anchor.transform.forward * 0.05f;
                Quaternion rotation = anchor.transform.rotation;

                GameObject spot = Instantiate(windowSpotsPrefab, spawnPos, rotation);
                spot.SetActive(true);

                // Réaligner tous les enfants
                foreach (Transform child in spot.GetComponentsInChildren<Transform>())
                {
                    child.rotation = rotation;
                }

                Animator animator = spot.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.speed = animationSpeed;
                }

                Debug.Log($"SpawnWindowSpots: Spot instancié avec bonne orientation sur {anchor.name}");
                foundAny = true;
                return; // On s'arrête après le premier anchor trouvé
                // PLUS DE return ici pour continuer la boucle
            }
        }

        if (!foundAny)
        {
            Debug.LogWarning("SpawnWindowSpots: Aucun anchor avec le label spécifié trouvé.");
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


    public bool drawWallGizmo = true;
    public static bool avoidSpawnWallDecoration = true;
    public int maxWallAttempts = 15;

    private class TemporaryColliderGO : System.IDisposable
    {
        public GameObject GO { get; private set; }
        public BoxCollider Collider { get; private set; }

        public TemporaryColliderGO(BoxCollider source, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            GO = new GameObject("TempCollider");
            GO.hideFlags = HideFlags.HideAndDontSave;
            Collider = GO.AddComponent<BoxCollider>();
            Collider.size = source.size;
            Collider.center = source.center;
            GO.transform.SetPositionAndRotation(pos, rot);
            GO.transform.localScale = scale;
        }

        public void Dispose()
        {
            if (GO != null) GameObject.DestroyImmediate(GO);
        }
    }

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

        // Validate prefab collider
        if (!prefab.TryGetComponent(out BoxCollider prefabCollider))
        {
            Debug.LogError($"[Spawn] Prefab \"{prefab.name}\" missing BoxCollider!");
            return false;
        }

        Vector3 localCenter = prefabCollider.center;
        Vector3 localHalfExtents = prefabCollider.size * 0.5f;
        Vector3 prefabScale = prefab.transform.lossyScale;
        Vector3 worldHalfExtents = Vector3.Scale(localHalfExtents, prefabScale);

        Quaternion baseRotation = Quaternion.Euler(0, 90, 0);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Try to get a surface position
            if (!room.GenerateRandomPositionOnSurface(
                MRUK.SurfaceType.VERTICAL, minEdgeDist, placementLabels, out Vector3 pos, out Vector3 norm))
            {
                Debug.LogWarning($"[Spawn] No valid wall position on attempt {attempt + 1}.");
                return false;
            }

            Quaternion rotation = Quaternion.LookRotation(norm) * baseRotation;
            Vector3 depthDirection = -norm.normalized;

            float depthFromWall = localHalfExtents.x * prefabScale.x;
            Vector3 spawnPos = pos + depthDirection * (depthFromWall + depthOffset);

            // Apply vertical offset
            spawnPos.y = verticalOffset;

            // Adjust center
            Vector3 rotatedCenter = rotation * localCenter;
            Vector3 worldCenter = spawnPos + rotatedCenter;

#if UNITY_EDITOR
            // Optional Gizmo Debug
            var gizmoGO = new GameObject($"SpawnBoxGizmo_{attempt + 1}_{prefab.name}");
            var gizmo = gizmoGO.AddComponent<GizmoDebugSpawnBox>();
            gizmo.worldCenter = worldCenter;
            gizmo.worldHalfExtents = worldHalfExtents == Vector3.zero ? Vector3.one * 0.1f : worldHalfExtents;
            gizmo.rotation = rotation;
#endif

            // Check for overlap with avoid-tag objects or disallowed anchor labels
            bool hasBadCollision = false;
            foreach (var col in Physics.OverlapBox(worldCenter, worldHalfExtents, rotation))
            {
                if (col.transform.IsChildOf(prefab.transform)) continue;

                if (avoidTags.Contains(col.tag))
                {
                    Debug.Log($"[Spawn] Blocked by tag: {col.name} ({col.tag})");
                    hasBadCollision = true;
                    break;
                }

                if (col.GetComponentInParent<MRUKAnchor>() is MRUKAnchor anchor &&
                    anchor.HasAnyLabel(spawnAvoidLabelsWall))
                {
                    Debug.Log($"[Spawn] Blocked by anchor: {anchor.name}");
                    hasBadCollision = true;
                    break;
                }
            }

            // Further validation using ComputePenetration
            if (!hasBadCollision)
            {
                using (var tempGO = new TemporaryColliderGO(prefabCollider, worldCenter, rotation, prefabScale))
                {
                    foreach (var previousCol in alreadySpawnedColliders)
                    {
                        if (Physics.ComputePenetration(
                            tempGO.Collider, worldCenter, rotation,
                            previousCol, previousCol.transform.position, previousCol.transform.rotation,
                            out _, out float dist))
                        {
                            Debug.Log($"[Spawn] Overlap with previous object: {previousCol.name} (dist: {dist})");
                            hasBadCollision = true;
                            break;
                        }
                    }
                }
            }

            if (!hasBadCollision)
            {
                spawned = GameObject.Instantiate(prefab, spawnPos, rotation);
                spawned.SetActive(true);
                alreadySpawnedColliders.AddRange(spawned.GetComponentsInChildren<Collider>());
                Debug.Log($"[Spawn] Successfully spawned \"{prefab.name}\" on attempt {attempt + 1}");
                return true;
            }
        }

        Debug.LogWarning($"[Spawn] Failed to find a valid position for \"{prefab.name}\" after {maxAttempts} attempts.");
        return false;
    }



    public float wallOffset; // `off` dans ton code

    public void SpawnDoor()
    {
        float doorOffset = -0.5f; 
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
        "wall_avoid",
        "window_spots",
    };
        BoxCollider prefabCollider = porteIFMS.GetComponent<BoxCollider>();
        Vector3 localCenter = prefabCollider != null ? prefabCollider.center : Vector3.zero;
        Vector3 localHalfExtents = prefabCollider != null ? prefabCollider.size * 0.5f : Vector3.one * 0.5f;


        if (TrySpawnVerticalPrefab(
            porteIFMS,
            room,
            placementLabels,
            avoidTags,
            minEdgeDistance,
            doorOffset,
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


    public void SpawnMenu()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("SpawnMenu: MRUKRoom introuvable !");
            return;
        }

        LabelFilter placementLabels = new LabelFilter(spawnLabelsWall);

        List<string> avoidTags = new List<string>
    {
        "Frame",
        "wall_avoid",
        "window_spots",
    };
        BoxCollider prefabCollider =menuPrefab.GetComponent<BoxCollider>();
        if (prefabCollider == null)
        {
            Debug.LogError("menuPrefab n’a pas de BoxCollider !");
            return;
        }

        for (int i = 0; i < nb_frames; i++)
        {

            if (TrySpawnVerticalPrefab(
                menuPrefab,
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

                Debug.Log($"SpawnMenu: Décoration {i + 1} instanciée.");
            }
            else
            {
                Debug.LogWarning($"SpawnMenu: Échec pour la décoration {i + 1}.");
            }
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
        "wall_avoid",
        "window_spots",
    };
        BoxCollider prefabCollider = wallPrefab.GetComponent<BoxCollider>();
        if (prefabCollider == null)
        {
            Debug.LogError("wallPrefab n’a pas de BoxCollider !");
            return;
        }

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

    // Gizmos visibles dans la scène
   

}


