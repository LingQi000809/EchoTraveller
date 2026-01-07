using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip playerMoveSound;
    [SerializeField] private AudioClip dialogueTriggerSound;

    [Header("Background Music")]
    [SerializeField] private AudioClip ambientMusic;

    [Header("Fade Settings")]
    [SerializeField] private float normalMusicVolume = 0.3f;
    [SerializeField] private float dialogueMusicVolume = 0.05f;
    [SerializeField] private float sfxVolume = 0.5f;
    [SerializeField] private float fadeSpeed = 2f;

    private float targetVolume;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one AudioManager in the scene.");
            return;
        }
        instance = this;
        sfxSource.volume = sfxVolume;
    }

    private void Start()
    {
        PlayAmbientMusic();
    }

    public void PlayAmbientMusic()
    {
        if (ambientMusic != null && musicSource != null)
        {
            musicSource.clip = ambientMusic;
            musicSource.loop = true;
            musicSource.volume = normalMusicVolume;
            targetVolume = normalMusicVolume;
            musicSource.Play();
        }
    }

    public void StopAmbientMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void FadeMusicForDialogue()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeMusicTo(dialogueMusicVolume));
    }

    public void RestoreMusicAfterDialogue()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeMusicTo(normalMusicVolume));
    }

    private IEnumerator FadeMusicTo(float targetVolume)
    {
        this.targetVolume = targetVolume;

        while (Mathf.Abs(musicSource.volume - targetVolume) > 0.01f)
        {
            musicSource.volume = Mathf.Lerp(musicSource.volume, targetVolume, fadeSpeed * Time.deltaTime);
            yield return null;
        }

        musicSource.volume = targetVolume;
        fadeCoroutine = null;
    }

    public void PlayPlayerMoveSound()
    {
        if (playerMoveSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(playerMoveSound);
        }
    }

    public void PlayDialogueTriggerSound()
    {
        if (dialogueTriggerSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(dialogueTriggerSound);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
