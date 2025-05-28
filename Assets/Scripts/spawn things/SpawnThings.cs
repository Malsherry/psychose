using System.Threading;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;
using Oculus.Interaction.DebugTree;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SpawnThings : MonoBehaviour
{
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

    [Header("Autres bruits")]

    public AudioClip OutsideNoise;
    public AudioClip DoorKeyNoise;
    public AudioMixerGroup doorKeyMixerGroup;
    public AudioMixer AudioMixer; //psychose Mixuer
    public AudioMixerGroup OutsideMixerGroup; // Le group o� le son passera


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(WaitForRoomInitialization());
        // Pr�parer la source audio pour le bruit al�atoire
        randomNoiseSource = gameObject.AddComponent<AudioSource>();
        randomNoiseSource.clip = randomNoiseClip;
        randomNoiseSource.playOnAwake = false;
        randomNoiseSource.spatialBlend = 0f;

        // Lancer la coroutine
        //StartCoroutine(PlayRandomNoise());

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
        if (spawnPorteIFMS)
                SpawnPorteIFMS2();

            if (spawnWallDecoration)
            {
                for (int i = 0; i < nb_frames; i++)
                {
                //SpawnWallDecoration();
                Debug.Log("youhou c'est moi le cadre");
                }
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

    public void SpawnWallDecoration()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        Debug.Log("Getting the room: " + room);
        room.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL, minEdgeDistance, new LabelFilter(spawnLabelsWall), out Vector3 pos, out Vector3 norm);
        Debug.Log($"Spawn position: {pos}, Normal: {norm}");

        Vector3 randomPosition = pos + norm * off;
        randomPosition.y = 1.5f;
        randomPosition.x = randomPosition.x - 0.05f;

        Quaternion rotation = Quaternion.LookRotation(norm) * Quaternion.Euler(0, 90, 0);

        GameObject wallInstance = Instantiate(wallPrefab, randomPosition, rotation);

        // Activation forc�e r�cursive (pour �tre s�r)
        wallInstance.SetActive(true);
        foreach (var animator in wallInstance.GetComponentsInChildren<Animator>(true))
        {
            animator.enabled = true;
            animator.gameObject.SetActive(true); // just in case
            Debug.Log($"[SpawnWallDecoration] Animator trouv�: {animator.name} | ActiveInHierarchy: {animator.gameObject.activeInHierarchy}");
        }

    }


    public void SpawnPorteIFMS2()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        float minDistanceFromAvoidLabels = 1.3f; // Distance minimale � respecter
        int maxAttempts = 40; // Nombre maximum de tentatives pour trouver une position valide

        Vector3 pos = Vector3.zero;
        Vector3 norm = Vector3.forward;
        bool positionFound = false;

        Vector3 porteSize = new Vector3(1.0f, 2.0f, 0.1f); // Dimensions approximatives de la porte (largeur, hauteur, profondeur)

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // G�n�re une position sur un mur (VERTICAL)
            if (!room.GenerateRandomPositionOnSurface(
                MRUK.SurfaceType.VERTICAL,
                minEdgeDistance,
                new LabelFilter(spawnLabelsPorteIFMS),
                out pos,
                out norm))
            {
                Debug.LogWarning("Porte : Impossible de g�n�rer une position (aucun mur trouv�)");
                return;
            }

            // V�rifie la distance par rapport aux �l�ments avec les labels � �viter
            bool isFarEnough = true;

            foreach (var anchor in room.Anchors)
            {
                // Ignore les objets avec le label "wall_face"
                if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.WALL_FACE))
                    continue;

                if (!anchor.HasAnyLabel(avoid))
                    continue;

                // V�rifie si l'objet a un ou plusieurs colliders
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
                            // Sinon, utilisez une v�rification de distance simple avec transform.position
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
                    // Cas fallback : pas de collider, v�rification simple avec transform.position
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

            // V�rifie les collisions avec d'autres objets dans la zone de la porte
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
                        // V�rifie si l'objet chevauch� a un label dans "avoid"
                        MRUKAnchor anchor = overlapCollider.GetComponentInParent<MRUKAnchor>();
                        if (anchor != null && anchor.HasAnyLabel(avoid))
                        {
                            isFarEnough = false;
                            Debug.Log($"Porte : Position rejet�e car elle chevauche un objet � �viter : {overlapCollider.name} (parent : {overlapCollider.transform.parent?.name})");
                            break;
                        }
                    }
                }
            }

            if (isFarEnough)
            {
                positionFound = true;
                Debug.Log($"Porte : Position valide trouv�e : {pos}, Normale : {norm}");
                break;
            }
        }

        if (!positionFound)
        {
            Debug.LogWarning("Porte : Aucune position valide trouv�e apr�s plusieurs tentatives.");
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
        Debug.Log("Getting the boardroom: " + room);

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


    public void SpawnFootball()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        float minDistanceFromAvoidLabels = 0.3f;
        int maxAttempts = 20;

        Vector3 pos = Vector3.zero;
        Vector3 norm = Vector3.up;
        bool positionFound = false;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (!room.GenerateRandomPositionOnSurface(
                MRUK.SurfaceType.FACING_UP,
                minEdgeDistance,
                new LabelFilter(spawnLabelsFootball),
                out pos,
                out norm))
            {
                Debug.LogWarning("Impossible de g�n�rer une position pour le babyfoot.");
                return;
            }

            bool isFarEnough = true;
            foreach (var anchor in room.Anchors)
            {
                if (anchor.HasAnyLabel(avoid) && Vector3.Distance(anchor.transform.position, pos) < minDistanceFromAvoidLabels)
                {
                    isFarEnough = false;
                    Debug.Log($"Position trop proche d'un �l�ment avec le label � �viter : {anchor.name}");
                    break;
                }
            }

            if (isFarEnough)
            {
                positionFound = true;
                Debug.Log($"Position valide trouv�e pour le babyfoot : {pos}, Normale : {norm}");
                break;
            }
        }

        if (!positionFound)
        {
            Debug.LogWarning("Impossible de trouver une position valide pour le babyfoot apr�s plusieurs tentatives.");
            return;
        }

        Vector3 randomPosition = pos + norm * off;

        Quaternion baseRotation = Quaternion.LookRotation(Vector3.forward, norm);
        Quaternion modelCorrection = Quaternion.Euler(-90, 0, 0);
        Quaternion finalRotation = baseRotation * modelCorrection;

        randomPosition.y = 0.75f;

        GameObject babyfootInstance = Instantiate(footballPrefab, randomPosition, finalRotation);
        babyfootInstance.tag = "FilterTarget";
        babyfootInstance.layer = LayerMask.NameToLayer("Interractable");

        // AJOUT D'UN COLLIDER SI AUCUN N'EST PR�SENT
        if (babyfootInstance.GetComponent<Collider>() == null)
        {
            // Essaye d�ajouter un BoxCollider, ou tu peux changer pour MeshCollider si besoin
            MeshCollider meshCollider = babyfootInstance.AddComponent<MeshCollider>();
            meshCollider.convex = true; // Important si l�objet doit d�tecter des rayons en tant que Rigidbody
            Debug.Log("Collider ajout� automatiquement au babyfoot.");
        }
    }
    /// <summary>
    /// Bruits qui apparaissent sur les portes/ fen�tre qui cr��ent parfois des hallucinations sonores
    /// </summary>
    /// 

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
                Debug.Log(" Distorsion temporaire !");
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
                Debug.Log("Position du son : " + soundPosition);

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

                Debug.Log("Son de cl� ajout� � la porte : " + anchor.name);
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


