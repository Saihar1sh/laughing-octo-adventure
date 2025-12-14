using UnityEngine;


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }


    public AudioClip flipClip;
    public AudioClip matchClip;
    public AudioClip mismatchClip;
    public AudioClip gameoverClip;


    AudioSource sfxSource;


    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }


    public void PlayFlip()
    {
        if (flipClip) 
            sfxSource.PlayOneShot(flipClip);
    }

    public void PlayMatch()
    {
        if (matchClip) 
            sfxSource.PlayOneShot(matchClip);
    }

    public void PlayMismatch()
    {
        if (mismatchClip) 
            sfxSource.PlayOneShot(mismatchClip);
    }

    public void PlayGameOver()
    {
        if (gameoverClip) 
            sfxSource.PlayOneShot(gameoverClip);
    }
}