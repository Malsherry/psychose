using UnityEngine;

public class GazeTriggeredSounds : MonoBehaviour
{
    public string targetTag = "Frame";
    public float gazeTimeRequired = 5f;
    public float cooldownTime = 15f;

    public AudioClip[] primarySounds;       // Folder 1
    public AudioClip[] secondarySounds;     // Folder 2 (optional 50% chance)

    private int layerMask;
    private float gazeTimer = 0f;
    private Transform lastHitTransform = null;
    private float nextAllowedTime = 0f;
    private Camera mainCamera;

    public static bool ActiveGazeSounds = true; // Static variable to control activation

    private void Start()
    {
        mainCamera = Camera.main;
        layerMask = LayerMask.GetMask("Interractable");

        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found.");
        }

        Debug.Log("GazeTriggeredSounds initialized with tag: " + targetTag);
    }

    private void Update()
    {
        if(ActiveGazeSounds)
            {
            
        
            if (mainCamera == null) return;

            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10f, layerMask))
            {
                if (hit.transform.CompareTag(targetTag))
                {
                    if (hit.transform == lastHitTransform)
                    {
                        gazeTimer += Time.deltaTime;

                        if (gazeTimer >= gazeTimeRequired && Time.time >= nextAllowedTime)
                        {
                            PlayPrimarySound(hit.transform);

                            // 50% chance to play secondary sound
                            if (Random.value < 0.5f)
                            {
                                PlaySecondarySound(hit.transform);
                            }

                            nextAllowedTime = Time.time + cooldownTime;
                            gazeTimer = -Mathf.Infinity;
                        }
                    }
                    else
                    {
                        lastHitTransform = hit.transform;
                        gazeTimer = 0f;
                    }
                }
                else
                {
                    ResetGaze();
                }
            }
            else
            {
                ResetGaze();
            }
        }
    }

    private void ResetGaze()
    {
        gazeTimer = 0f;
        lastHitTransform = null;
    }

    private void PlayPrimarySound(Transform target)
    {
        float volume = 1f; // Adjust volume as needed
        if (primarySounds.Length == 0) return;

        AudioClip clip = primarySounds[Random.Range(0, primarySounds.Length)];
        PlaySpatialClipAtTransform(clip, target, volume);
    }

    private void PlaySecondarySound(Transform target)
    {
        float volume = 0.3f; // Adjust volume as needed
        if (secondarySounds.Length == 0) return;

        AudioClip clip = secondarySounds[Random.Range(0, secondarySounds.Length)];
        PlaySpatialClipAtTransform(clip, target, volume);
    }

    private void PlaySpatialClipAtTransform(AudioClip clip, Transform target, float volume)
    {
        AudioSource tempSource = target.gameObject.AddComponent<AudioSource>();
        tempSource.volume = volume; // Adjust volume as needed
        tempSource.clip = clip;
        tempSource.spatialBlend = 1.0f; // 3D spatial audio
        tempSource.minDistance = 1f;
        tempSource.maxDistance = 15f;
        tempSource.rolloffMode = AudioRolloffMode.Linear;
        tempSource.Play();

        Destroy(tempSource, clip.length + 0.1f);
    }
}
