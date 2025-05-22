using System.Threading;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;
using Oculus.Interaction.DebugTree;
using UnityEngine.Audio;

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

    public GameObject cameraPrefab; // cam�ra � faire spawn
    public GameObject cubePrefab; //cube qu'on fait spawn
    public GameObject wallPrefab; // truc qu'on fait spawn aux murs
    public GameObject footballPrefab; // babyfoot a faire spawn
    public GameObject BoardGames; // jeux de soci�t� � faire spawn
    public GameObject porteIFMS; //porte a faire spawn
    

    public float spawnTimer = 0.5f;
    private float timer;
    public int nb_frames; // nombre de fois qu'on fait spawn de l'art mural
    private float off = 0f;


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
    public AudioClip OutsideNoise;
    public AudioClip DoorKeyNoise;
    public AudioMixerGroup doorKeyMixerGroup;
    public AudioMixer doorKeyMixer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(WaitForRoomInitialization());
    }

    private IEnumerator WaitForRoomInitialization() // On attend que la salle soit initialis�e avant de faire quoi que ce soit
    {
            // Attendre que MRUK.Instance soit initialis�
            while (!MRUK.Instance || !MRUK.Instance.IsInitialized)
            {
                yield return null;
            }

            // Une fois la salle initialis�e, lancer les spawns conditionnels
            if (spawnPorteIFMS)
                SpawnPorteIFMS2();

            if (spawnWallDecoration)
            {
                for (int i = 0; i < nb_frames; i++)
                {
                    SpawnWallDecoration();
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

        randomPosition.y = 1.5f;         // Ajuste la position verticale pour qu'elle soit � 1,50 m du sol
        randomPosition.x = randomPosition.x - 0.05f; // Ajustement horizontal

        Quaternion rotation = Quaternion.LookRotation(norm) * Quaternion.Euler(0, 90, 0); // Calcule la rotation en fonction de la normale et ajoute une rotation de 90 degr�s en Y
        Instantiate(wallPrefab, randomPosition, rotation); // Utilise la rotation calcul�e
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
                            Debug.Log($"Porte : Trop proche de {anchor.name} via {col.name} (dist : {dist})");
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
                MeshFilter meshFilter = anchor.GetComponentInChildren<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
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


    public void SpawnOutsideNoiseOnWindow()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        foreach (var anchor in room.Anchors)
        {
            if (anchor.HasAnyLabel(window))
            {
                Debug.Log($"AUDIO Fen�tre trouv�e : {anchor.name}");
                // Position l�g�rement en avant de la fen�tre
                Vector3 soundPosition = anchor.transform.position + anchor.transform.forward * 0.05f;

                // Cr�er un GameObject qui porte le son
                GameObject soundEmitter = new GameObject("WindowOutsideNoise");
                soundEmitter.transform.position = soundPosition;
                soundEmitter.transform.rotation = anchor.transform.rotation;
                soundEmitter.transform.SetParent(anchor.transform); // Pour qu�il suive la fen�tre
                Debug.Log("Position du son : " + soundPosition);
                // Ajouter un AudioSource
                AudioSource audioSource = soundEmitter.AddComponent<AudioSource>();
                audioSource.clip = OutsideNoise;
                audioSource.loop = true;
                Debug.Log("Son : " + OutsideNoise);
                audioSource.spatialBlend = 1.0f; // Son 3D
                audioSource.minDistance = 1.0f;
                audioSource.maxDistance = 10.0f;
                audioSource.Play();

                Debug.Log("Son ext�rieur ajout� � la fen�tre : " + anchor.name);
                return; // On ne prend que la premi�re fen�tre
            }
        }

        Debug.LogWarning("Aucune fen�tre trouv�e pour jouer le son.");
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
            doorKeyMixer.SetFloat("volume", volumeInDb);

            // Distorsion al�atoire : 30% de chances d'appliquer un effet
            bool applyDistortion = Random.value < 0.3f;

            if (applyDistortion)
            {
                float distortionValue = Random.Range(0.1f, 1f);
                doorKeyMixer.SetFloat("distortion", distortionValue);
                Debug.Log($"Distorsion activ�e : {distortionValue}");
            }
            else
            {
                doorKeyMixer.SetFloat("distortion", 0f);
                Debug.Log("Distorsion d�sactiv�e");
            }
            audioSource.Play();
            Debug.Log($"Son jou� avec volume lin�aire : {volume} / dB : {volumeInDb}");
            yield return new WaitForSeconds(55f);
        }
    }




}


