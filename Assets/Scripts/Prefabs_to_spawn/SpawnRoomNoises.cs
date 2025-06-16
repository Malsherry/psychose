using Meta.XR.MRUtilityKit;
using System.Collections;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Audio;



public class SpawnRoomNoises : MonoBehaviour
{
    public MRUKAnchor.SceneLabels window;
    public MRUKAnchor.SceneLabels door_frame;
    public static bool spawnWindowNoise = true; // Pour activer/désactiver le spawn de bruit extérieur  
    public static bool spawnDoorNoises = true; // Pour activer/désactiver le spawn de bruit de porte

    // --- Bruits de claquement de porte ---
    [Header("Claquement de porte")]
    public AudioClip randomNoiseClip;
    public float noiseMinInterval = 30f;
    public float noiseMaxInterval = 90f;
    private AudioSource randomNoiseSource;

    // --- Autres bruits ---
    [Header("Autres bruits")]
    public AudioClip OutsideNoise;
    public AudioClip DoorKeyNoise;
    public AudioMixerGroup doorKeyMixerGroup;
    public AudioMixer AudioMixer;
    public AudioMixerGroup OutsideMixerGroup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Préparer la source audio pour le bruit aléatoire
        StartCoroutine(WaitForRoomInitialization());

        randomNoiseSource = gameObject.AddComponent<AudioSource>();
        randomNoiseSource.clip = randomNoiseClip;
        randomNoiseSource.playOnAwake = false;
        randomNoiseSource.spatialBlend = 0f;
        StartCoroutine(PlayRandomNoise());

        if (spawnWindowNoise)
            SpawnOutsideNoiseOnWindow();
        if (spawnDoorNoises)
            SpawnDoorKeyNoiseOnDoor();

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
    }

    public void SpawnOutsideNoiseOnWindow()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        Debug.Log($"SpawnRoomNoises: AUDIO SpawnOutsideNoiseOnWindow - Room: {room.name}, Anchors: {room.Anchors.Count}");
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

    private IEnumerator PlayRandomNoise() //slamming door noise
    {
        Debug.Log("SpawnRoomNoises: Démarrage de la coroutine pour jouer le bruit aléatoire de porte.");
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

    private IEnumerator OutsideNoiseEffect(AudioSource source)
    {
        Debug.Log("SpawnRoomNoises: Démarrage de l'effet de distorsion temporaire sur le bruit extérieur.");
        float rampDuration = 0.3f;
        float holdDuration = 0.7f;
        float timer = 0f;

        // Effet progressif
        while (timer < rampDuration)
        {
            timer += Time.deltaTime;
            float t = timer / rampDuration;

            source.volume = Mathf.Lerp(0.5f, 2f, t); // au-delà de 1.0 pour exagérer
            AudioMixer.SetFloat("DistortionLevel", Mathf.Lerp(-80f, 0.8f, t)); // 0dB = distorsion max

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
        Debug.Log("SpawnRoomNoises: Démarrage du cycle de bruit extérieur sur la fenêtre.");
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
        Debug.Log($"SpawnRoomNoises: AUDIO SpawnDoorKeyNoiseOnDoor - Room: {room.name}, Anchors: {room.Anchors.Count}");
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
        Debug.Log("SpawnRoomNoises: Démarrage de la coroutine pour jouer le bruit de clé toutes les 40 secondes.");
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
