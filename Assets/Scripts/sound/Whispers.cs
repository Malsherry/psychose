using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class Whispers : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip whisperClip;
    [Tooltip("List of possible secondary whisper clips (picked randomly if triggered)")]
    public List<AudioClip> secondaryWhisperClips = new List<AudioClip>();
    [Range(0f, 1f)] public float baseVolume = 0.5f;
    [Range(0f, 1f)] public float secondaryVolume = 0.5f;
    public float volumeVariationRange = 0.3f;
    public float volumeChangeSpeed = 0.2f;

    [Header("Spatial Settings")]
    [Range(-1f, 1f)] public float basePan = 0f;
    public float panVariationRange = 0.6f;
    public float panChangeSpeed = 0.2f;

    [Header("Playback Settings")]
    public float intervalBetweenPlays = 120f;

    private AudioSource audioSource;
    private float targetVolume;
    private float targetPan;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = whisperClip;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D audio
        audioSource.volume = baseVolume;
        audioSource.panStereo = basePan;

        targetVolume = baseVolume;
        targetPan = basePan;

        StartCoroutine(PlayLoop());
        StartCoroutine(UpdateVolumeAndPan());
    }

    private IEnumerator PlayLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervalBetweenPlays);

            if (whisperClip != null)
            {
                audioSource.Play();
                SetNewTargets();

                if (secondaryWhisperClips != null && secondaryWhisperClips.Count > 0 && Random.value < 0.5f)
                {
                    float maxDelay = whisperClip.length - 5f;
                    if (maxDelay > 0f)
                    {
                        float delay = 5f + Random.Range(0f, maxDelay);
                        StartCoroutine(PlaySecondaryWhisperAfterDelay(delay));
                    }
                }
            }
        }
    }

    private IEnumerator PlaySecondaryWhisperAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        AudioClip clip = secondaryWhisperClips[Random.Range(0, secondaryWhisperClips.Count)];
        if (clip == null) yield break;

        AudioSource tempSource = gameObject.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = secondaryVolume;
        tempSource.panStereo = 0f; // centered stereo
        tempSource.spatialBlend = 0f; // 2D sound
        tempSource.playOnAwake = false;
        tempSource.loop = false;

        tempSource.Play();
        Debug.Log($"Playing secondary whisper: {clip.name} at volume {secondaryVolume}");

        Destroy(tempSource, clip.length + 0.1f);
    }

    private void SetNewTargets()
    {
        targetVolume = Mathf.Clamp(
            baseVolume + Random.Range(-volumeVariationRange, volumeVariationRange),
            0f, 1f
        );

        targetPan = Mathf.Clamp(
            basePan + Random.Range(-panVariationRange, panVariationRange),
            -1f, 1f
        );
    }

    private IEnumerator UpdateVolumeAndPan()
    {
        while (true)
        {
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, volumeChangeSpeed * Time.deltaTime);
            audioSource.panStereo = Mathf.MoveTowards(audioSource.panStereo, targetPan, panChangeSpeed * Time.deltaTime);

            if (Mathf.Abs(audioSource.volume - targetVolume) < 0.01f &&
                Mathf.Abs(audioSource.panStereo - targetPan) < 0.01f)
            {
                SetNewTargets();
            }

            yield return null;
        }
    }
}
