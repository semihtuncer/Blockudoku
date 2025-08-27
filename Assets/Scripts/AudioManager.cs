using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Variables
    [Header("Sounds")]
    public Sound[] sounds;

    public static AudioManager instance;
    #endregion

    void Awake()
    {
        instance = this;

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
            return;

        s.source.Play();
    }
    public void StopSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
            return;

        if (s.source.isPlaying)
            s.source.Stop();
    }
}

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;
    [HideInInspector]
    public AudioSource source;

    public bool loop;
    [Range(0f,1f)]
    public float volume;
    [Range(0.1f, 3f)]
    public float pitch;
}