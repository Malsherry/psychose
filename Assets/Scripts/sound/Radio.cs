using UnityEngine;

public class Radio : MonoBehaviour
{
    [Header("Radio")]
    public AudioClip[] playlist;
    public bool playRandom = false;
    public static bool ActiveRadio = true;

    [Header("Ambiance café")]
    public AudioClip ambianceCafeClip;
    public static bool ambianceCafe = true;

    private AudioSource radioSource;
    private AudioSource ambianceSource;
    private int currentIndex = 0;

    void Start()
    {
        // Gestion de la radio
        if (ActiveRadio)
        {
            radioSource = gameObject.AddComponent<AudioSource>();
            radioSource.loop = false;
            radioSource.playOnAwake = false;
            radioSource.spatialBlend = 0f;
            PlayNext();
        }

        // Gestion de l'ambiance café
        if (ambianceCafe && ambianceCafeClip != null)
        {
            ambianceSource = gameObject.AddComponent<AudioSource>();
            ambianceSource.clip = ambianceCafeClip;
            ambianceSource.loop = true;
            ambianceSource.playOnAwake = false;
            ambianceSource.volume = 0.2f;
            ambianceSource.spatialBlend = 0f;
            ambianceSource.Play();
        }
    }

    void Update()
    {
        if (ActiveRadio && radioSource != null && !radioSource.isPlaying)
        {
            PlayNext();
        }
    }

    void PlayNext()
    {
        if (playlist.Length == 0) return;

        if (playRandom)
            currentIndex = Random.Range(0, playlist.Length);
        else
            currentIndex = (currentIndex + 1) % playlist.Length;

        radioSource.volume = 0.07f;
        radioSource.clip = playlist[currentIndex];
        radioSource.Play();
    }
}
