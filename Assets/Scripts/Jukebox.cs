using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jukebox : MonoBehaviour
{
    static AudioSource audioSource = null;

    public static float volume { get => audioSource.volume; }

    public void UpdateVolume(float vol)
    {
        audioSource.volume = vol; 
    }

    void Awake()
    {
        if (audioSource != null) 
        {
            Destroy(GetComponent<AudioSource>());
        }
        else
        {
            audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
    }
}
