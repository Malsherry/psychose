using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class Whispers : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip whisperClip;
    [Range(0f, 1f)] public float baseVolume = 0.5f;
    public float volumeVariationRange = 0.3f;
    public float volumeChangeSpeed = 0.2f;

    [Header("Spatial Settings")]
    [Range(-1f, 1f)] public float basePan = 0f; // 0 = centre, -1 = gauche, 1 = droite
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
            }
        }
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

            // Si on s'approche trop de la cible, on en choisit une autre (mouvement continu)
            if (Mathf.Abs(audioSource.volume - targetVolume) < 0.01f &&
                Mathf.Abs(audioSource.panStereo - targetPan) < 0.01f)
            {
                SetNewTargets();
            }

            yield return null;
        }
    }
}
