using System.Threading;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;
using Oculus.Interaction.DebugTree;

public class SpawnThings : MonoBehaviour
{
    public GameObject cubePrefab; //cube qu'on fait spawn
    public GameObject wallPrefab; // truc qu'on fait spawn aux murs
    public GameObject footballPrefab; // babyfoot a faire spawn
    public GameObject BoardGames; // jeux de soci�t� � faire spawn
    public GameObject porteIFMS; //porte a faire spawn
    

    public float spawnTimer = 0.5f;
    private float timer;
    public int nb_frames; // nombre de fois qu'on fait spawn de l'art mural
    public bool spawnFootball; // si on fait spawn le babyfoot ou pas
    private float off = 0f;


    // param�tres dont on a besoin pour spawn des mesh al�atoirement dans la pi�ce
    public float minEdgeDistance;
    public MRUKAnchor.SceneLabels spawnLabels;
    public MRUKAnchor.SceneLabels spawnLabelsWall;
    public MRUKAnchor.SceneLabels spawnLabelsFootball;
    public MRUKAnchor.SceneLabels avoid; // labels � �viter pour le spawn du babyfoot
    public MRUKAnchor.SceneLabels spawnLabelsBoardGames;
    public MRUKAnchor.SceneLabels spawnLabelsPorteIFMS;
    public MRUKAnchor.SceneLabels window;
    public AudioClip OutsideNoise;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(WaitForRoomInitialization());
    }

    private IEnumerator WaitForRoomInitialization()
    {
        // Attendre que MRUK.Instance soit initialis�
        while (!MRUK.Instance || !MRUK.Instance.IsInitialized)
        {
            yield return null;
        }

        // Une fois la salle initialis�e, lancer les spawns
        SpawnPorteIFMS2();
        for (int i = 0; i < nb_frames; i++)
        {
            SpawnWallDecoration();
        }
        SpawnFootball();
        SpawnBoardGames();
        SpawnOutsideNoiseOnWindow();
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

        randomPosition.y = 1.5f;         // Ajuste la position verticale pour qu'elle soit � 1,50 m du sol
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

        // Positionnement et rotation
        Vector3 spawnPosition = pos + norm * off;
        spawnPosition.y = 0.05f; // Ajustement vertical
        spawnPosition.x -= 0.08f; // Ajustement horizontal
        Quaternion rotation = Quaternion.LookRotation(norm) * Quaternion.Euler(0, 90f, 0);

        Instantiate(porteIFMS, spawnPosition, rotation);
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

        // Distance minimale � respecter par rapport aux �l�ments avec les labels � �viter
        float minDistanceFromAvoidLabels = 0.3f; // Par exemple, 2 m�tres
        int maxAttempts = 20; // Nombre maximum de tentatives pour trouver une position valide

        Vector3 pos = Vector3.zero;
        Vector3 norm = Vector3.up;
        bool positionFound = false;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // G�n�re une position en utilisant les labels pour le babyfoot
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

            // V�rifie la distance par rapport aux �l�ments avec les labels � �viter
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

        // Base : on fait en sorte qu'il "colle" au sol
        Quaternion baseRotation = Quaternion.LookRotation(Vector3.forward, norm);

        // Correction de l'axe si le prefab a les pieds en l'air (ajuste si c'est X ou Z qui est � l'envers)
        Quaternion modelCorrection = Quaternion.Euler(-90, 0, 0);

        Quaternion finalRotation = baseRotation * modelCorrection;

        randomPosition.y = 0.75f; // Ajuste la position verticale pour qu'elle soit � 0,75 m du sol
        Instantiate(footballPrefab, randomPosition, finalRotation);
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




}


