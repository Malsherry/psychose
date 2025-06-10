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
    public static bool spawnCamera = true; // pour faire spawn la caméra
    public static bool spawnPorteIFMS = true;
    public static bool spawnWallDecoration = true;
    public static bool spawnFootball = true;
    public static bool spawnBoardGames = true;
    public static bool spawnWindowNoise = true;
    public static bool spawnDoorNoises = true;
    public static bool spawnCubes = true; // pour faire spawn des cubes aléatoirement dans la pièce 

    [Header("Claquement de porte")]
    public AudioClip randomNoiseClip; // Clip sélectionné dans l'inspecteur
    public float noiseMinInterval = 30f;
    public float noiseMaxInterval = 90f;

    private AudioSource randomNoiseSource;
    [Header("Prefabs des différents élements")]

    public GameObject cameraPrefab; // caméra à faire spawn
    public GameObject cubePrefab; //cube qu'on fait spawn
    public GameObject wallPrefab; // truc qu'on fait spawn aux murs
    public GameObject footballPrefab; // babyfoot a faire spawn
    public GameObject BoardGames; // jeux de société à faire spawn
    public GameObject porteIFMS; //porte a faire spawn
    [Header("Paramètres")]


    public float spawnTimer = 0.5f;
    private float timer;
    public int nb_frames; // nombre de fois qu'on fait spawn de l'art mural
    private float off = 0f;

    [Header("Scene labels")]

    // paramètres dont on a besoin pour spawn des mesh aléatoirement dans la pièce
    public float minEdgeDistance;
    public MRUKAnchor.SceneLabels spawnLabelsCamera;
    public MRUKAnchor.SceneLabels spawnLabels;
    public MRUKAnchor.SceneLabels spawnLabelsWall;
    public MRUKAnchor.SceneLabels spawnLabelsFootball;
    public MRUKAnchor.SceneLabels avoid; // labels à éviter pour le spawn du babyfoot
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
    public AudioMixerGroup OutsideMixerGroup; // Le group où le son passera


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(WaitForRoomInitialization());
        // Préparer la source audio pour le bruit aléatoire
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
        if (spawnPorteIFMS)
                SpawnPorteIFMS2();



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
        if ((timer > spawnTimer) && spawnCubes) // si le timer est supérieur à spawnTimer et qu'on fait spawn des cubes
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
                    Debug.LogWarning("Décoration murale : impossible de générer une position.");
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

                Debug.Log($"mur OverlapBox détecte {overlaps.Length} colliders à la position {worldCenter}");

                foreach (var col in overlaps)
                {
                    Debug.Log($"mur OverlapBox détecte collider : {col.name} (Tag: {col.tag})");

                    if (col.transform.IsChildOf(wallPrefab.transform)) continue;

                    // Recherche récursive de MRUKAnchor
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
                        Debug.Log($"[SpawnWallDecoration] Animator activé : {animator.name}");
                    }

                    Debug.Log($"Décoration murale {i + 1} instanciée à la tentative {attempt + 1}");
                    Debug.Log("déco mur instanciée");
                    break;
                }
            }

            if (!spawned)
            {
                Debug.LogWarning($"Décoration murale {i + 1} : aucune position valide trouvée après {maxWallAttempts} tentatives.");
            }
        }
    }





    public void SpawnPorteIFMS2()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        float minDistanceFromAvoidLabels = 1.3f; // Distance minimale à respecter
        int maxAttempts = 40; // Nombre maximum de tentatives pour trouver une position valide

        Vector3 pos = Vector3.zero;
        Vector3 norm = Vector3.forward;
        bool positionFound = false;

        Vector3 porteSize = new Vector3(1.0f, 2.0f, 0.1f); // Dimensions approximatives de la porte (largeur, hauteur, profondeur)

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Génère une position sur un mur (VERTICAL)
            if (!room.GenerateRandomPositionOnSurface(
                MRUK.SurfaceType.VERTICAL,
                minEdgeDistance,
                new LabelFilter(spawnLabelsPorteIFMS),
                out pos,
                out norm))
            {
                Debug.LogWarning("Porte : Impossible de générer une position (aucun mur trouvé)");
                return;
            }

            // Vérifie la distance par rapport aux éléments avec les labels à éviter
            bool isFarEnough = true;

            foreach (var anchor in room.Anchors)
            {
                // Ignore les objets avec le label "wall_face"
                if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.WALL_FACE))
                    continue;

                if (!anchor.HasAnyLabel(avoid))
                    continue;

                // Vérifie si l'objet a un ou plusieurs colliders
                Collider[] colliders = anchor.GetComponentsInChildren<Collider>(true);
                if (colliders.Length > 0)
                {
                    foreach (var col in colliders)
                    {
                        float dist;

                        // Si le collider est compatible avec ClosestPoint, utilisez-le
                        if (col is BoxCollider || col is SphereCollider || col is CapsuleCollider || (col is MeshCollider meshCol && meshCol.convex))
                        {
                            dist = Vector3.Distance(col.ClosestPoint(pos), pos);
                        }
                        else
                        {
                            // Sinon, utilisez une vérification de distance simple avec transform.position
                            dist = Vector3.Distance(anchor.transform.position, pos);
                        }

                        if (dist < minDistanceFromAvoidLabels)
                        {
                            isFarEnough = false;
                            //Debug.Log($"Porte : Trop proche de {anchor.name} via {col.name} (dist : {dist})");
                            break;
                        }
                    }
                }
                else
                {
                    // Cas fallback : pas de collider, vérification simple avec transform.position
                    float fallbackDist = Vector3.Distance(anchor.transform.position, pos);
                    if (fallbackDist < minDistanceFromAvoidLabels)
                    {
                        isFarEnough = false;
                        Debug.Log($"Porte : Trop proche de {anchor.name} (fallback sans collider, dist : {fallbackDist})");
                        break;
                    }
                }

                if (!isFarEnough)
                    break;
            }

            // Vérifie les collisions avec d'autres objets dans la zone de la porte
            if (isFarEnough)
            {
                Collider[] overlapColliders = Physics.OverlapBox(
                    pos, // Centre de la zone
                    porteSize / 2, // Demi-dimensions de la porte
                    Quaternion.LookRotation(norm) // Orientation de la porte
                );

                if (overlapColliders.Length > 0)
                {
                    foreach (var overlapCollider in overlapColliders)
                    {
                        // Vérifie si l'objet chevauché a un label dans "avoid"
                        MRUKAnchor anchor = overlapCollider.GetComponentInParent<MRUKAnchor>();
                        if (anchor != null && anchor.HasAnyLabel(avoid))
                        {
                            isFarEnough = false;
                            Debug.Log($"Porte : Position rejetée car elle chevauche un objet à éviter : {overlapCollider.name} (parent : {overlapCollider.transform.parent?.name})");
                            break;
                        }
                    }
                }
            }

            if (isFarEnough)
            {
                positionFound = true;
                Debug.Log($"Porte : Position valide trouvée : {pos}, Normale : {norm}");
                break;
            }
        }

        if (!positionFound)
        {
            Debug.LogWarning("Porte : Aucune position valide trouvée après plusieurs tentatives.");
            return;
        }

        float depthOffset = 0.01f; // Distance pour ressortir un peu la porte

        // Le "x" de la porte doit pointer dans la direction +norm
        Quaternion rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, norm), Vector3.up);

        // On pousse un peu dans la direction +norm pour ressortir du mur
        Vector3 spawnPosition = pos + (norm * depthOffset);
        spawnPosition.y = 0.05f; // Ajustement vertical

        Instantiate(porteIFMS, spawnPosition, rotation);


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
                Debug.Log($"Table détectée : {anchor.name} avec le label {spawnLabelsBoardGames}");
                MeshFilter meshFilter = anchor.GetComponentInChildren<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    Debug.Log($"table: Mesh trouvé pour l'ancre {anchor.name}, taille : {meshFilter.sharedMesh.bounds.size}");
                    Bounds bounds = meshFilter.sharedMesh.bounds;

                    // On utilise la taille projetée au sol (X et Z, en local)
                    Vector3 scale = anchor.transform.lossyScale;
                    float scaledX = bounds.size.x * scale.x;
                    float scaledZ = bounds.size.z * scale.z;
                    float surface = scaledX * scaledZ;

                    Debug.Log($"Table détectée : {anchor.name}, surface = {surface}");

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
            Debug.Log($"Plus grande table trouvée : {biggestTable.name}, surface = {maxSurface}");

            // On place le jeu de société au-dessus de la table
            Vector3 pos = biggestTable.transform.position + Vector3.up * 0.1f; // ajuster si besoin
            Quaternion rot = Quaternion.LookRotation(biggestTable.transform.forward, Vector3.up); // orienté comme la table

            Instantiate(BoardGames, pos, rot);
        }
        else
        {
            Debug.LogWarning("Aucune table détectée pour placer les jeux de société.");
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

        // Récupère un collider dans la hiérarchie
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
                Debug.LogWarning("baby Impossible de générer une position.");
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
                Debug.Log($"Babyfoot instancié à la tentative {attempt + 1}.");
                return;
            }
        }

        Debug.LogWarning("Impossible de trouver une position valide pour le babyfoot après plusieurs tentatives.");
    }

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


    /// <summary>
    /// Bruits qui apparaissent sur les portes/ fenêtre qui crééent parfois des hallucinations sonores
    /// </summary>
    /// 




    // <summary>
    /// Partie pour gérer différents bruits dans la scène
    public void SpawnOutsideNoiseOnWindow()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        foreach (var anchor in room.Anchors)
        {
            if (anchor.HasAnyLabel(window))
            {
                Debug.Log($"AUDIO Fenêtre trouvée : {anchor.name}");

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

                Debug.Log("Son extérieur ajouté à la fenêtre : " + anchor.name);

                StartCoroutine(HandleOutsideNoiseCycle(audioSource));
                return;
            }
        }

        Debug.LogWarning("Aucune fenêtre trouvée pour jouer le son.");
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

            source.volume = Mathf.Lerp(0.5f, 2f, t); // au-delà de 1.0 pour exagérer
            AudioMixer.SetFloat("DistortionLevel", Mathf.Lerp(-80f,0.8f, t)); // 0dB = distorsion max

            yield return null;
        }

        yield return new WaitForSeconds(holdDuration);

        // Retour à la normale
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
                Debug.Log($"AUDIO Porte trouvée : {anchor.name}");

                // Position légèrement en avant de la porte
                Vector3 soundPosition = anchor.transform.position + anchor.transform.forward * 0.05f;

                // Créer un GameObject pour émettre le son
                GameObject soundEmitter = new GameObject("DoorKeyNoiseEmitter");
                soundEmitter.transform.position = soundPosition;
                soundEmitter.transform.rotation = anchor.transform.rotation;
                soundEmitter.transform.SetParent(anchor.transform); // Pour qu’il suive la porte
                //Debug.Log("Position du son : " + soundPosition);

                // Ajouter un AudioSource
                AudioSource audioSource = soundEmitter.AddComponent<AudioSource>();
                audioSource.clip = DoorKeyNoise;
                audioSource.loop = false; // Pas en boucle
                audioSource.spatialBlend = 1.0f; // Son 3D
                audioSource.minDistance = 1.0f;
                audioSource.maxDistance = 10.0f;
                audioSource.outputAudioMixerGroup = doorKeyMixerGroup;


                // Lancer une coroutine pour jouer le son toutes les 40 secondes avec volume aléatoire
                StartCoroutine(PlayDoorKeyNoisePeriodically(audioSource));

                //Debug.Log("Son de clé ajouté à la porte : " + anchor.name);
                return; // On ne prend que la première porte
            }
        }

        Debug.LogWarning("Aucune porte trouvée pour jouer le son.");
    }

    private IEnumerator PlayDoorKeyNoisePeriodically(AudioSource audioSource)
    {
        while (true)
        {
            // Volume aléatoire entre 0.3 et 1.0
            float volume = Random.Range(0.1f, 1.0f);
            audioSource.volume = volume; // Pour spatial blend (non mixé)

            // Convertir volume [0.0, 1.0] en dB [-80, 0]
            float safeVolume = Mathf.Max(volume, 0.0001f);
            float volumeInDb = Mathf.Log10(safeVolume) * 20f;
            AudioMixer.SetFloat("volume", volumeInDb);

            // Distorsion aléatoire : 30% de chances d'appliquer un effet
            bool applyDistortion = Random.value < 0.3f;

            if (applyDistortion)
            {
                float distortionValue = Random.Range(0.1f, 1f);
                AudioMixer.SetFloat("distortion", distortionValue);
                Debug.Log($"Distorsion activée : {distortionValue}");
            }
            else
            {
                AudioMixer.SetFloat("distortion", 0f);
                Debug.Log("Distorsion désactivée");
            }
            audioSource.Play();
            Debug.Log($"Son joué avec volume linéaire : {volume} / dB : {volumeInDb}");
            yield return new WaitForSeconds(55f);
        }
    }




}


