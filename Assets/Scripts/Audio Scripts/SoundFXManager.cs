using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;

    [SerializeField] private AudioSource soundFXObject;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public AudioSource PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume, bool loop = false)
    {
        // spawn object
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign clip
        audioSource.clip = audioClip;

        // assign volume
        audioSource.volume = volume;

        // set audio to loop
        if (loop)
        {
            audioSource.loop = true;
        }

        // play sound
        audioSource.Play();

        // get length of clip
        float clipLength = audioSource.clip.length;

        // destroy object
        if (!loop)
        {
            Destroy(audioSource.gameObject, clipLength);
        }

        return audioSource;
    }

    public AudioSource PlayRandomSoundFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume, bool loop = false)
    {
        //assign a random index
        int rand = Random.Range(0, audioClip.Length);
        // spawn object
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign clip
        audioSource.clip = audioClip[rand];

        // assign volume
        audioSource.volume = volume;

        // set audio to loop
        if (loop)
        {
            audioSource.loop = true;
        }

        // play sound
        audioSource.Play();

        // get length of clip
        float clipLength = audioSource.clip.length;

        // destroy object
        if (!loop)
        {
            Destroy(audioSource.gameObject, clipLength);
        }

        return audioSource;
    }

    public void StopAudioClip(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            if (audioSource.isPlaying)
            {
                Destroy(audioSource.gameObject);
            }
        }
    }
}
