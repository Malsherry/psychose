using UnityEngine;
using System.Collections;

public class GazeTriggerGameSounds : MonoBehaviour
{
    public string targetTag = "Frame";
    public float gazeTimeRequired = 5f;
    public float cooldownTime = 10f;

    public AudioClip firstSound;
    public AudioClip secondSound;

    private float gazeTimer = 0f;
    private Transform lastHitTransform = null;
    private float nextAllowedTime = 0f;

    private bool isTriggered = false;

    private Camera mainCamera;
    private int layerMask;

    private void Start()
    {
        mainCamera = Camera.main;
        layerMask = LayerMask.GetMask("Interractable");

        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found.");
        }
    }

    private void Update()
    {
        if (mainCamera == null)
            return;

        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.blue, 0.1f);

        if (Physics.Raycast(ray, out hit, 10f, layerMask))
        {
            if (hit.transform.CompareTag(targetTag))
            {
                if (hit.transform == lastHitTransform)
                {
                    gazeTimer += Time.deltaTime;

                    if (gazeTimer >= gazeTimeRequired && !isTriggered && Time.time >= nextAllowedTime)
                    {
                        StartCoroutine(PlaySequence(hit.transform));
                        isTriggered = true;
                        nextAllowedTime = Time.time + cooldownTime;
                    }
                }
                else
                {
                    lastHitTransform = hit.transform;
                    gazeTimer = 0f;
                    isTriggered = false;
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
        isTriggered = false;
    }

    private IEnumerator PlaySequence(Transform target)
    {
        if (firstSound != null)
        {
            float firstSoundDuration = PlaySound(target, firstSound);
            yield return new WaitForSeconds(firstSoundDuration + 3f); // Wait for first sound + 3s delay
        }
        else
        {
            yield return new WaitForSeconds(3f); // Still wait 3s if no first sound
        }

        if (secondSound != null)
        {
            PlaySound(target, secondSound);
        }
    }

    private float PlaySound(Transform target, AudioClip clip)
    {
        AudioSource tempSource = target.gameObject.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.spatialBlend = 1.0f; // 3D sound
        tempSource.minDistance = 1f;
        tempSource.maxDistance = 15f;
        tempSource.rolloffMode = AudioRolloffMode.Linear;

        tempSource.Play();
        Destroy(tempSource, clip.length + 0.1f); // Cleanup

        return clip.length;
    }
}
