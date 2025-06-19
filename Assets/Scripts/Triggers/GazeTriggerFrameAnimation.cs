using UnityEngine;

public class GazeTriggerFrameAnimation : MonoBehaviour
{
    private int layerMask;
    public string targetTag = "Frame";
    public float gazeTimeRequired = 2f;
    public float cooldownTime = 15f;
    public string animationName = "root|rootAction";

    public AudioClip paperNoise;
    public AudioClip delayedSound; // <--- Second sound played after 2 seconds of gaze

    private float gazeTimer = 0f;
    private Transform lastHitTransform = null;
    private float nextAllowedTime = 0f;

    private Camera mainCamera;
    private bool hasTriggeredDelayedSound = false;

    private void Start()
    {
        mainCamera = Camera.main;
        layerMask = LayerMask.GetMask("Interractable");

        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found.");
        }

        Debug.Log("GazeTriggerAnimation initialized with targetTag: " + targetTag);
    }

    private void Update()
    {
        if (mainCamera == null)
            return;

        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.green, 0.1f);

        if (Physics.Raycast(ray, out hit, 10f, layerMask))
        {
            if (hit.transform.CompareTag(targetTag))
            {
                if (hit.transform == lastHitTransform)
                {
                    gazeTimer += Time.deltaTime;

                    if (!hasTriggeredDelayedSound && gazeTimer >= gazeTimeRequired)
                    {
                        if (delayedSound != null)
                        {
                            PlaySound(hit.transform, delayedSound);
                        }
                        hasTriggeredDelayedSound = true;
                    }

                    if (gazeTimer >= gazeTimeRequired && Time.time >= nextAllowedTime)
                    {
                        Animator anim = hit.transform.GetComponent<Animator>();
                        if (anim != null)
                        {
                            anim.Play(animationName, 0, 0f);
                        }

                        if (paperNoise != null)
                        {
                            PlaySound(hit.transform, paperNoise);
                        }

                        nextAllowedTime = Time.time + cooldownTime;
                        gazeTimer = -Mathf.Infinity;
                        hasTriggeredDelayedSound = false;
                    }
                }
                else
                {
                    lastHitTransform = hit.transform;
                    gazeTimer = 0f;
                    hasTriggeredDelayedSound = false;
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

    private void ResetGaze()
    {
        gazeTimer = 0f;
        lastHitTransform = null;
        hasTriggeredDelayedSound = false;
    }

    private void PlaySound(Transform target, AudioClip clip)
    {
        AudioSource tempSource = target.gameObject.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.spatialBlend = 1.0f; // 3D sound
        tempSource.minDistance = 1f;
        tempSource.maxDistance = 15f;
        tempSource.rolloffMode = AudioRolloffMode.Linear;

        tempSource.Play();
        Destroy(tempSource, clip.length + 0.1f); // Clean up
    }
}
