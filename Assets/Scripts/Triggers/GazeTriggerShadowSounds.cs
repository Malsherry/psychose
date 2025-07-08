using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class GazeTriggerShadowSounds : MonoBehaviour
{
    public string targetTag = "window_spots";
    public float gazeTimeRequired = 2f;
    public float cooldownTime = 8f;

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
            StartCoroutine(PulseMaterial(target)); // Démarre la pulsation
            yield return new WaitForSeconds(firstSoundDuration + 1f);
        }
        else
        {
            StartCoroutine(PulseMaterial(target)); // Même sans son, on pulse
            yield return new WaitForSeconds(0.5f);
        }

        if (secondSound != null)
        {
            PlaySound(target, secondSound);
        }
        // Quand tu joues un son prioritaire


}

    private float PlaySound(Transform target, AudioClip clip)
    {
        AudioSource tempSource = target.gameObject.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.spatialBlend = 1.0f;
        tempSource.minDistance = 1f;
        tempSource.maxDistance = 15f;
        tempSource.rolloffMode = AudioRolloffMode.Linear;

        SoundManager.Instance?.RegisterPrioritySoundStart();
        tempSource.Play();
        StartCoroutine(EndSoundAfter(tempSource.clip.length));

        Destroy(tempSource, clip.length + 0.1f);

        return clip.length;
    }
    private IEnumerator EndSoundAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        SoundManager.Instance?.RegisterPrioritySoundEnd();
    }

    private IEnumerator PulseMaterial(Transform target)
    {
        Debug.Log("Starting material pulse on: " + target.name);
        Renderer targetRenderer = target.GetComponentInChildren<Renderer>();
        if (targetRenderer == null || targetRenderer.material == null)
            yield break;

        Material mat = targetRenderer.material;
        Color originalColor = mat.color;
        Debug.Log("pulse Original color: " + originalColor);

        float duration = 20f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float intensity = Mathf.Lerp(0.4f, 1f, Mathf.PingPong(Time.time * 1f, 1f));
            Color newColor = originalColor * intensity;
            

            mat.color = newColor;

            elapsed += Time.deltaTime;
            yield return null;
        }

        mat.color = originalColor;
    }

}
