using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private int prioritySoundCount = 0;

    public bool IsPrioritySoundPlaying => prioritySoundCount > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void RegisterPrioritySoundStart()
    {
        prioritySoundCount++;
    }

    public void RegisterPrioritySoundEnd()
    {
        prioritySoundCount = Mathf.Max(0, prioritySoundCount - 1);
    }
}
