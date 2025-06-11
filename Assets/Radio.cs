using UnityEngine;

public class Radio : MonoBehaviour
{
    public AudioClip[] playlist; 
    private AudioSource audioSource;
    private int currentIndex = 0;

    public bool playRandom = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false; // On g�re la boucle nous-m�mes
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // Son global, pas spatial
        PlayNext();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
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
        {
            currentIndex = (currentIndex + 1) % playlist.Length;
        }
        audioSource.volume = 0.1f; // Volume r�duit pour un fond sonore
        audioSource.clip = playlist[currentIndex];
        audioSource.Play();
    }
}
