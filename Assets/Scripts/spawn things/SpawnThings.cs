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
    private List<Collider> alreadySpawnedColliders = new List<Collider>();

    public static bool spawnCamera = true; // pour faire spawn la cam�ra
    public static bool spawnPorteIFMS = true;
    public static bool spawnWallDecoration = true;
    public static bool spawnFootball = true;
    public static bool spawnBoardGames = true;
    public static bool spawnWindowNoise = true;
    public static bool spawnDoorNoises = true;
    public static bool spawnCubes = true; // pour faire spawn des cubes al�atoirement dans la pi�ce 

    [Header("Claquement de porte")]
    public AudioClip randomNoiseClip; // Clip s�lectionn� dans l'inspecteur
    public float noiseMinInterval = 30f;
    public float noiseMaxInterval = 90f;

    private AudioSource randomNoiseSource;
    [Header("Prefabs des diff�rents �lements")]

    public GameObject cameraPrefab; // cam�ra � faire spawn
    public GameObject cubePrefab; //cube qu'on fait spawn
    public GameObject wallPrefab; // truc qu'on fait spawn aux murs
    public GameObject footballPrefab; // babyfoot a faire spawn
    public GameObject BoardGames; // jeux de soci�t� � faire spawn
    public GameObject porteIFMS; //porte a faire spawn
    [Header("Param�tres")]


    public float spawnTimer = 0.5f;
    private float timer;
    public int nb_frames; // nombre de fois qu'on fait spawn de l'art mural
    private float off = 0f;



    [Header("Scene labels")]

    // param�tres dont on a besoin pour spawn des mesh al�atoirement dans la pi�ce
    public float minEdgeDistance;
    public MRUKAnchor.SceneLabels spawnLabelsCamera;
    public MRUKAnchor.SceneLabels spawnLabels;
    public MRUKAnchor.SceneLabels spawnLabelsWall;
    public MRUKAnchor.SceneLabels spawnLabelsFootball;
    public MRUKAnchor.SceneLabels avoid; // labels � �viter pour le spawn du babyfoot
    public MRUKAnchor.SceneLabels spawnLabelsBoardGames;
    public MRUKAnchor.SceneLabels spawnLabelsPorteIFMS;
    public MRUKAnchor.SceneLabels window;
    public MRUKAnchor.SceneLabels door_frame; // label de la porte
    public MRUKAnchor.SceneLabels spawnAvoidLabelsWall;
    public MRUKAnchor.SceneLabels windowFrameLabel;

    [Header("Autres bruits")]

    public AudioClip OutsideNoise;
    public AudioClip DoorKeyNoise;
    public AudioMixerGroup doorKeyMixerGroup;
    public AudioMixer AudioMixer; //psychose Mixuer
    public AudioMixerGroup OutsideMixerGroup; // Le group o� le son passera


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        alreadySpawnedColliders.Clear(); // reset before new spawn session
        StartCoroutine(WaitForRoomInitialization());
        // Pr�parer la source audio pour le bruit al�atoire
        randomNoiseSource = gameObject.AddComponent<AudioSource>();
        randomNoiseSource.clip = randomNoiseClip;
        randomNoiseSource.playOnAwake = false;
        randomNoiseSource.spatialBlend = 0f;

        // Lancer la coroutine
        //StartCoroutine(PlayRandomNoise());

    }
    IEnumerator DelayedSpawnWallDecoration(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnWallDecoration();
    }

    private IEnumerator WaitForRoomInitialization()
    {
        // Attendre que MRUK soit initialis�
        while (!MRUK.Instance || !MRUK.Instance.IsInitialized)
            yield return null;

        // Attendre que la room soit cr��e et qu'il y ait au moins une ancre d�tect�e
        MRUKRoom room = null;
        while (room == null || room.Anchors == null || room.Anchors.Count == 0)
        {
            room = MRUK.Instance.GetCurrentRoom();
            yield return null;
        }

        Debug.Log("[SpawnThings] Room et anchors initialis�s, lancement des spawns.");

        // Une fois la salle initialis�e, lancer les spawns conditionnels



        // A FAIRE: ajuster le cube, v�rifier que ca marche sur les portes, refaire marcher la d�tection sur le mesh de porte ifms, faire marcher la d�tection entre deux cadres instanci�s
        if (spawnWallDecoration)
        {
            if (room != null)
            {
                foreach (var anchor in room.GetComponentsInChildren<MRUKAnchor>(true))
                {
                    if (!anchor.HasAnyLabel(windowFrameLabel)) continue;

                    GameObject window = anchor.gameObject;

                    // Chercher ou cr�er l'enfant ObstacleBox
                    Transform obstacleBox = window.transform.Find("ObstacleBox");
                    if (obstacleBox == null)
                    {
                        GameObject obstacleBoxGO = new GameObject("ObstacleBox");
                        obstacleBoxGO.transform.SetParent(window.transform, false);
                        obstacleBox = obstacleBoxGO.transform;
                    }

                    // Chercher ou cr�er l'enfant cube dans ObstacleBox
                    Transform cubeChild = obstacleBox.Find("Cube");
                    if (cubeChild == null)
                    {
                        GameObject cubeGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cubeGO.name = "Cube";
                        cubeGO.transform.SetParent(obstacleBox, false);

                        // D�sactiver le MeshRenderer
                        var meshRenderer = cubeGO.GetComponent<MeshRenderer>();
                        if (meshRenderer != null)
                            meshRenderer.enabled = false;

                        // Activer le BoxCollider
                        var boxCollider = cubeGO.GetComponent<BoxCollider>();
                        if (boxCollider != null)
                            boxCollider.enabled = true;
                        Debug.Log($"BoxCollider activ� ? {boxCollider.enabled}");

                        Rigidbody rb = cubeGO.GetComponent<Rigidbody>();
                        if (rb == null)
                        {
                            rb = cubeGO.AddComponent<Rigidbody>();
                            rb.isKinematic = true; // Pour ne pas que la physique le d�place
                            rb.useGravity = false;
                        }

                    }
                    else
                    {
                        // Si l'enfant existe d�j�, s'assurer que le MeshRenderer est d�sactiv� et le BoxCollider activ�
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

        if (spawnFootball)
                SpawnFootball();

            if (spawnBoardGames)
                SpawnBoardGames();

            if (spawnWindowNoise)
                SpawnOutsideNoiseOnWindow();

            if (spawnCamera)
                SpawnCamera();

            if (spawnDoorNoises)
                SpawnDoorKeyNoiseOnDoor();
    }


    // Update is called once per frame
    void Update()
    {
        if (!MRUK.Instance || !MRUK.Instance.IsInitialized) return;

        timer += Time.deltaTime;
        if ((timer > spawnTimer) && spawnCubes) // si le timer est sup�rieur � spawnTimer et qu'on fait spawn des cubes
        {
            SpawnCubes();
            timer -= spawnTimer;
        }
    }
    private IEnumerator PlayRandomNoise()
    {
        while (true)
        {
            float waitTime = Random.Range(noiseMinInterval, noiseMaxInterval);
            yield return new WaitForSeconds(waitTime);

            if (randomNoiseClip != null && randomNoiseSource != null)
            {
                randomNoiseSource.Play();
            }
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
    public float wallOffset = 0.05f; // `off` dans ton code

    //old function that works but they're kinda messy ngl
    /*

     public void SpawnWallDecoration()
     {
         MRUKRoom room = MRUK.Instance.GetCurrentRoom();
         if (room == null)
         {
             Debug.LogError("MRUKRoom introuvable !");
             return;
         }

         BoxCollider prefabCollider = wallPrefab.GetComponentInChildren<BoxCollider>();
         if (prefabCollider == null)
         {
             Debug.LogError("Le prefab mural n'a pas de BoxCollider !");
             return;
         }

         Vector3 localCenter = prefabCollider.center;
         Vector3 localHalfExtents = prefabCollider.size * 0.5f;

         List<GameObject> spawnedDecorations = new List<GameObject>();

         for (int i = 0; i < nb_frames; i++)
         {
             Debug.Log("on commence a vouloir mettre le mur cadre");
             bool spawned = false;

             for (int attempt = 0; attempt < maxWallAttempts; attempt++)
             {
                 if (!room.GenerateRandomPositionOnSurface(
                    MRUK.SurfaceType.VERTICAL, 1,
                    new LabelFilter(spawnLabelsWall),
                    out Vector3 pos, out Vector3 norm))
                 {
                     Debug.LogWarning("D�coration murale : impossible de g�n�rer une position.");
                     return;
                 }

                 Vector3 randomPosition = pos + norm * wallOffset;
                 randomPosition.y = 1.5f;
                 randomPosition.x -= 0.05f;

                 Quaternion rotation = Quaternion.LookRotation(norm) * Quaternion.Euler(0, 90, 0);

                 Vector3 worldCenter = randomPosition + rotation * localCenter;
                 Vector3 worldHalfExtents = Vector3.Scale(localHalfExtents, wallPrefab.transform.lossyScale);

                 Collider[] overlaps = Physics.OverlapBox(worldCenter, worldHalfExtents, rotation);
                 bool hasBadCollision = false;

                 Debug.Log($"mur OverlapBox d�tecte {overlaps.Length} colliders � la position {worldCenter}");

                 foreach (var col in overlaps)
                 {
                     Debug.Log($"mur OverlapBox d�tecte collider : {col.name} (Tag: {col.tag})");

                     if (col.transform.IsChildOf(wallPrefab.transform)) continue;

                     // Recherche r�cursive de MRUKAnchor
                     Transform t = col.transform;
                     MRUKAnchor anchor = null;
                     while (t != null && anchor == null)
                     {
                         anchor = t.GetComponent<MRUKAnchor>();
                         t = t.parent;
                     }

                     if (anchor != null && anchor.HasAnyLabel(spawnAvoidLabelsWall))
                     {
                         Debug.Log($"[mur Tentative {attempt + 1}] Collision avec '{anchor.name}' (label interdit)");
                         hasBadCollision = true;
                         break;
                     }

                     if (col.CompareTag("wall_avoid") || col.CompareTag("Frame"))
                     {
                         Debug.Log($"[mur Tentative {attempt + 1}] Collision avec '{col.name}' (tag wall_avoid ou Frame)");
                         hasBadCollision = true;
                         break;
                     }
                 }

                 if (!hasBadCollision)
                 {
                     GameObject wallInstance = Instantiate(wallPrefab, randomPosition, rotation);
                     wallInstance.SetActive(true);
                     spawnedDecorations.Add(wallInstance);
                     spawned = true;

                     Animator animator = wallInstance.GetComponentInChildren<Animator>(true);
                     if (animator != null)
                     {
                         animator.enabled = true;
                         animator.gameObject.SetActive(true);
                         Debug.Log($"[SpawnWallDecoration] Animator activ� : {animator.name}");
                     }

                     Debug.Log($"D�coration murale {i + 1} instanci�e � la tentative {attempt + 1}");
                     Debug.Log("d�co mur instanci�e");
                     break;
                 }
             }

             if (!spawned)
             {
                 Debug.LogWarning($"D�coration murale {i + 1} : aucune position valide trouv�e apr�s {maxWallAttempts} tentatives.");
             }
         }
     }


     public void SpawnPorteIFMS2()
     {
         MRUKRoom room = MRUK.Instance.GetCurrentRoom();
         if (room == null)
         {
             Debug.LogError("MRUKRoom introuvable !");
             return;
         }

         BoxCollider doorCollider = porteIFMS.GetComponentInChildren<BoxCollider>();
         if (doorCollider == null)
         {
             Debug.LogError("Le prefab de porte n'a pas de BoxCollider !");
             return;
         }

         Vector3 localCenter = doorCollider.center;
         Vector3 localHalfExtents = doorCollider.size * 0.5f;

         for (int attempt = 0; attempt < maxWallAttempts; attempt++)
         {
             if (!room.GenerateRandomPositionOnSurface(
                 MRUK.SurfaceType.VERTICAL,
                 minEdgeDistance,
                 new LabelFilter(spawnLabelsPorteIFMS),
                 out Vector3 pos,
                 out Vector3 norm))
             {
                 Debug.LogWarning("Porte : Impossible de g�n�rer une position.");
                 return;
             }

             // Slightly offset the door forward and adjust vertical height
             Vector3 offsetPosition = pos + norm * wallOffset;
             offsetPosition.y = 0.05f;

             Quaternion rotation = Quaternion.LookRotation(norm) * Quaternion.Euler(0, 90, 0);

             // Compute world-space bounds of the door
             Vector3 worldCenter = offsetPosition + rotation * localCenter;
             Vector3 worldHalfExtents = Vector3.Scale(localHalfExtents, porteIFMS.transform.lossyScale);

             Collider[] overlaps = Physics.OverlapBox(worldCenter, worldHalfExtents, rotation);
             bool hasBadCollision = false;

             foreach (var col in overlaps)
             {
                 if (col.transform.IsChildOf(porteIFMS.transform)) continue;

                 // Check MRUKAnchor label rejection
                 Transform t = col.transform;
                 MRUKAnchor anchor = null;
                 while (t != null && anchor == null)
                 {
                     anchor = t.GetComponent<MRUKAnchor>();
                     t = t.parent;
                 }

                 if (anchor != null && anchor.HasAnyLabel(avoid))
                 {
                     hasBadCollision = true;
                     Debug.Log($"[Porte Tentative {attempt + 1}] Collision avec '{anchor.name}' (label interdit)");
                     break;
                 }

                 // Optional: tag-based blocking
                 if (col.CompareTag("wall_avoid") || col.CompareTag("Frame"))
                 {
                     hasBadCollision = true;
                     Debug.Log($"[Porte Tentative {attempt + 1}] Collision avec tag bloquant : {col.name}");
                     break;
                 }
             }

             if (!hasBadCollision)
             {
                 GameObject doorInstance = Instantiate(porteIFMS, offsetPosition, rotation);
                 doorInstance.SetActive(true);
                 Debug.Log($"///////////////////PORTE INSTANCIEE TERMINADO � la tentative {attempt + 1}////////////////////////////");
                 return;
             }
         }

         Debug.LogWarning("Porte : Aucune position valide trouv�e apr�s plusieurs tentatives.");
     }
     */


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
                Debug.LogWarning("TrySpawnVerticalPrefab: Pas de position murale trouv�e.");
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
                Debug.Log($"[Spawn] Objet instanci� avec succ�s � la tentative {attempt + 1}");
                return true;
            }
        }

        Debug.LogWarning("[Spawn] Aucune position valide trouv�e apr�s toutes les tentatives.");
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
            Debug.Log("SpawnDoor: Porte instanci�e avec succ�s !");
        }
        else
        {
            Debug.LogWarning("SpawnDoor: Aucune porte n'a pu �tre instanci�e.");
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

                Debug.Log($"SpawnWallDecoration: D�coration {i + 1} instanci�e.");
            }
            else
            {
                Debug.LogWarning($"SpawnWallDecoration: �chec pour la d�coration {i + 1}.");
            }
        }
    }




    public void SpawnCamera()
    {
        Debug.Log("Spawn Camera");
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        // G�n�re une position sur une surface correspondant � spawnLabelsCamera
        if (room.GenerateRandomPositionOnSurface(
            MRUK.SurfaceType.FACING_DOWN,
            minEdgeDistance,
            new LabelFilter(spawnLabelsCamera),
            out Vector3 pos,
            out Vector3 norm)
            )
        {
            Debug.Log("Spawn Camera 2");

            Vector3 spawnPosition = pos + norm * 0.05f; // L�g�rement au-dessus de la surface

            // Ajoute une rotation de 180� sur X pour remettre la cam�ra � l'endroit
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, norm) * Quaternion.Euler(180, 0, 0);

            GameObject cameraObj = Instantiate(cameraPrefab, spawnPosition, rotation);

            // Ajoute le script de clignotement si besoin
            if (cameraObj.GetComponent<CameraBlink>() == null)
                cameraObj.AddComponent<CameraBlink>();
        }
        else
        {
            Debug.LogWarning("Impossible de g�n�rer une position pour la cam�ra.");
        }
    }


    public void SpawnBoardGames()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        MRUKAnchor biggestTable = null;
        float maxSurface = 0f;
        
        foreach (var anchor in room.Anchors)
        {
            Debug.Log($"BoardAnchor: {anchor.name}, {spawnLabelsBoardGames}");
            if (anchor.HasAnyLabel(spawnLabelsBoardGames))
            {
                Debug.Log($"Table d�tect�e : {anchor.name} avec le label {spawnLabelsBoardGames}");
                MeshFilter meshFilter = anchor.GetComponentInChildren<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    Debug.Log($"table: Mesh trouv� pour l'ancre {anchor.name}, taille : {meshFilter.sharedMesh.bounds.size}");
                    Bounds bounds = meshFilter.sharedMesh.bounds;

                    // On utilise la taille projet�e au sol (X et Z, en local)
                    Vector3 scale = anchor.transform.lossyScale;
                    float scaledX = bounds.size.x * scale.x;
                    float scaledZ = bounds.size.z * scale.z;
                    float surface = scaledX * scaledZ;

                    Debug.Log($"Table d�tect�e : {anchor.name}, surface = {surface}");

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
            Debug.Log($"Plus grande table trouv�e : {biggestTable.name}, surface = {maxSurface}");

            // On place le jeu de soci�t� au-dessus de la table
            Vector3 pos = biggestTable.transform.position + Vector3.up * 0.1f; // ajuster si besoin
            Quaternion rot = Quaternion.LookRotation(biggestTable.transform.forward, Vector3.up); // orient� comme la table

            Instantiate(BoardGames, pos, rot);
        }
        else
        {
            Debug.LogWarning("Aucune table d�tect�e pour placer les jeux de soci�t�.");
        }
    }

    public int maxAttempts = 20;
    public bool drawGizmos = true;

    private Vector3 lastGizmoCenter;
    private Vector3 lastGizmoHalfExtents;
    private Quaternion lastGizmoRotation = Quaternion.identity;
    private bool lastGizmoOverlap;

    public void SpawnFootball()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("MRUKRoom introuvable !");
            return;
        }

        // R�cup�re un collider dans la hi�rarchie
        BoxCollider prefabCollider = footballPrefab.GetComponentInChildren<BoxCollider>();
        if (prefabCollider == null)
        {
            Debug.LogError("Le prefab du babyfoot n'a pas de BoxCollider !");
            return;
        }

        // Valeurs locales du collider
        Vector3 localCenter = prefabCollider.center;
        Vector3 localHalfExtents = prefabCollider.size * 0.5f;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (!room.GenerateRandomPositionOnSurface(
                MRUK.SurfaceType.FACING_UP,
                minEdgeDistance,
                new LabelFilter(spawnLabelsFootball),
                out Vector3 pos,
                out Vector3 norm))
            {
                Debug.LogWarning("baby Impossible de g�n�rer une position.");
                return;
            }
            pos.y = 0f;
            norm.y = 0f; // On ignore la composante Y de la normale pour le babyfoot
            Vector3 spawnPos = pos + norm;

            Quaternion baseRotation = Quaternion.LookRotation(Vector3.forward, norm);
            Quaternion modelCorrection = Quaternion.Euler(-90, 0, 0); // Adapter si besoin
            Quaternion finalRotation = baseRotation * modelCorrection;

            Vector3 worldCenter = spawnPos + finalRotation * localCenter;
            Vector3 worldHalfExtents = Vector3.Scale(localHalfExtents, footballPrefab.transform.lossyScale);

            Collider[] overlaps = Physics.OverlapBox(worldCenter, worldHalfExtents, finalRotation);
            bool hasBadCollision = false;

            foreach (var col in overlaps)
            {
                if (col.transform.IsChildOf(footballPrefab.transform)) continue;

                MRUKAnchor anchor = col.GetComponentInParent<MRUKAnchor>();
                if (anchor != null && anchor.HasAnyLabel(avoid))
                {
                    Debug.Log($"[baby Tentative {attempt + 1}] Collision avec '{anchor.name}'");
                    hasBadCollision = true;
                    break;
                }
            }

            // Stocker les gizmos de debug
            lastGizmoCenter = worldCenter;
            lastGizmoHalfExtents = worldHalfExtents;
            lastGizmoRotation = finalRotation;
            lastGizmoOverlap = hasBadCollision;

            if (!hasBadCollision)
            {
                GameObject instance = Instantiate(footballPrefab, spawnPos, finalRotation);
                Debug.Log($"Babyfoot instanci� � la tentative {attempt + 1}.");
                return;
            }
        }

        Debug.LogWarning("Impossible de trouver une position valide pour le babyfoot apr�s plusieurs tentatives.");
    }

    // Gizmos visibles dans la sc�ne
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // V�rifie que la rotation est valide avant de l'utiliser
        if (lastGizmoRotation == Quaternion.identity || lastGizmoRotation.w == 0f)
        {
            return; // On �vite de faire planter la sc�ne Unity
        }

        Gizmos.color = lastGizmoOverlap ? Color.red : Color.green;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(lastGizmoCenter, lastGizmoRotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, lastGizmoHalfExtents * 2);
    }


    /// <summary>
    /// Bruits qui apparaissent sur les portes/ fen�tre qui cr��ent parfois des hallucinations sonores
    /// </summary>
    /// 




    // <summary>
    /// Partie pour g�rer diff�rents bruits dans la sc�ne
    public void SpawnOutsideNoiseOnWindow()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        foreach (var anchor in room.Anchors)
        {
            if (anchor.HasAnyLabel(window))
            {
                Debug.Log($"AUDIO Fen�tre trouv�e : {anchor.name}");

                Vector3 soundPosition = anchor.transform.position + anchor.transform.forward * 0.05f;

                GameObject soundEmitter = new GameObject("WindowOutsideNoise");
                soundEmitter.transform.position = soundPosition;
                soundEmitter.transform.rotation = anchor.transform.rotation;
                soundEmitter.transform.SetParent(anchor.transform);

                AudioSource audioSource = soundEmitter.AddComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = OutsideMixerGroup;
                audioSource.clip = OutsideNoise;
                audioSource.loop = true;
                audioSource.spatialBlend = 1.0f;
                audioSource.minDistance = 1.0f;
                audioSource.maxDistance = 10.0f;
                audioSource.volume = 0.5f;

                audioSource.Play();

                Debug.Log("Son ext�rieur ajout� � la fen�tre : " + anchor.name);

                StartCoroutine(HandleOutsideNoiseCycle(audioSource));
                return;
            }
        }

        Debug.LogWarning("Aucune fen�tre trouv�e pour jouer le son.");
    }

    private IEnumerator OutsideNoiseEffect(AudioSource source)
    {
        float rampDuration = 0.3f;
        float holdDuration = 0.7f;
        float timer = 0f;

        // Effet progressif
        while (timer < rampDuration)
        {
            timer += Time.deltaTime;
            float t = timer / rampDuration;

            source.volume = Mathf.Lerp(0.5f, 2f, t); // au-del� de 1.0 pour exag�rer
            AudioMixer.SetFloat("DistortionLevel", Mathf.Lerp(-80f,0.8f, t)); // 0dB = distorsion max

            yield return null;
        }

        yield return new WaitForSeconds(holdDuration);

        // Retour � la normale
        timer = 0f;
        while (timer < rampDuration)
        {
            timer += Time.deltaTime;
            float t = timer / rampDuration;

            source.volume = Mathf.Lerp(1.2f, 0.5f, t);
            AudioMixer.SetFloat("DistortionLevel", Mathf.Lerp(0f, -80f, t));

            yield return null;
        }

        AudioMixer.SetFloat("DistortionLevel", -80f);
    }


    private IEnumerator HandleOutsideNoiseCycle(AudioSource source)
    {
        while (true)
        {
            if (Random.value < 2f / 3f)
            {
                //Debug.Log(" Distorsion temporaire !");
                yield return StartCoroutine(OutsideNoiseEffect(source));
            }

            // Attendre avant le prochain cycle
            yield return new WaitForSeconds(10f);
        }
    }


    public void SpawnDoorKeyNoiseOnDoor()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        foreach (var anchor in room.Anchors)
        {
            if (anchor.HasAnyLabel(door_frame))
            {
                Debug.Log($"AUDIO Porte trouv�e : {anchor.name}");

                // Position l�g�rement en avant de la porte
                Vector3 soundPosition = anchor.transform.position + anchor.transform.forward * 0.05f;

                // Cr�er un GameObject pour �mettre le son
                GameObject soundEmitter = new GameObject("DoorKeyNoiseEmitter");
                soundEmitter.transform.position = soundPosition;
                soundEmitter.transform.rotation = anchor.transform.rotation;
                soundEmitter.transform.SetParent(anchor.transform); // Pour qu�il suive la porte
                //Debug.Log("Position du son : " + soundPosition);

                // Ajouter un AudioSource
                AudioSource audioSource = soundEmitter.AddComponent<AudioSource>();
                audioSource.clip = DoorKeyNoise;
                audioSource.loop = false; // Pas en boucle
                audioSource.spatialBlend = 1.0f; // Son 3D
                audioSource.minDistance = 1.0f;
                audioSource.maxDistance = 10.0f;
                audioSource.outputAudioMixerGroup = doorKeyMixerGroup;


                // Lancer une coroutine pour jouer le son toutes les 40 secondes avec volume al�atoire
                StartCoroutine(PlayDoorKeyNoisePeriodically(audioSource));

                //Debug.Log("Son de cl� ajout� � la porte : " + anchor.name);
                return; // On ne prend que la premi�re porte
            }
        }

        Debug.LogWarning("Aucune porte trouv�e pour jouer le son.");
    }

    private IEnumerator PlayDoorKeyNoisePeriodically(AudioSource audioSource)
    {
        while (true)
        {
            // Volume al�atoire entre 0.3 et 1.0
            float volume = Random.Range(0.1f, 1.0f);
            audioSource.volume = volume; // Pour spatial blend (non mix�)

            // Convertir volume [0.0, 1.0] en dB [-80, 0]
            float safeVolume = Mathf.Max(volume, 0.0001f);
            float volumeInDb = Mathf.Log10(safeVolume) * 20f;
            AudioMixer.SetFloat("volume", volumeInDb);

            // Distorsion al�atoire : 30% de chances d'appliquer un effet
            bool applyDistortion = Random.value < 0.3f;

            if (applyDistortion)
            {
                float distortionValue = Random.Range(0.1f, 1f);
                AudioMixer.SetFloat("distortion", distortionValue);
                Debug.Log($"Distorsion activ�e : {distortionValue}");
            }
            else
            {
                AudioMixer.SetFloat("distortion", 0f);
                Debug.Log("Distorsion d�sactiv�e");
            }
            audioSource.Play();
            Debug.Log($"Son jou� avec volume lin�aire : {volume} / dB : {volumeInDb}");
            yield return new WaitForSeconds(55f);
        }
    }




}


