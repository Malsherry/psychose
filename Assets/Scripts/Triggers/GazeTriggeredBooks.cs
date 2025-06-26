using UnityEngine;
using System.Collections.Generic;

public class GazeTriggerPlayMeshAudio : MonoBehaviour
{
    [Header("Gaze Settings")]
    public string targetTag = "book";
    public LayerMask layerMask = ~0; // all layers
    public float gazeTimeRequired = 2f;
    public float cooldownPerTarget = 6f;

    private Camera mainCamera;
    private float gazeTimer = 0f;
    private Transform currentTarget = null;

    // Track cooldowns per object
    private Dictionary<Transform, float> cooldowns = new Dictionary<Transform, float>();

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogError("Main Camera not found.");
    }

    private void Update()
    {
        if (mainCamera == null) return;

        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 10f, layerMask))
        {
            Transform hitTransform = hit.transform;

            if (hitTransform.CompareTag(targetTag))
            {
                // Skip if this target is still in cooldown
                if (cooldowns.TryGetValue(hitTransform, out float cooldownEnd))
                {
                    if (Time.time < cooldownEnd)
                    {
                        return;
                    }
                }

                if (hitTransform == currentTarget)
                {
                    gazeTimer += Time.deltaTime;
                    if (gazeTimer >= gazeTimeRequired)
                    {
                        PlayAudioFromTarget(hitTransform);

                        // Start cooldown
                        cooldowns[hitTransform] = Time.time + cooldownPerTarget;

                        // Reset gaze
                        gazeTimer = 0f;
                        currentTarget = null;
                    }
                }
                else
                {
                    currentTarget = hitTransform;
                    gazeTimer = 0f;
                }
                return;
            }
        }

        // Gaze lost
        currentTarget = null;
        gazeTimer = 0f;
    }

    private void PlayAudioFromTarget(Transform target)
    {
        AudioSource audio = target.GetComponent<AudioSource>();
        if (audio != null && !audio.isPlaying)
        {
            audio.Play();
            Debug.Log("Audio played on: " + target.name);
        }
        else if (audio == null)
        {
            Debug.LogWarning("No AudioSource found on: " + target.name);
        }
    }
}
