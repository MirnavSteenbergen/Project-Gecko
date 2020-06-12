using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.audioMixerGroup;
        }
    }

    private void Start()
    {

    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("name " + name + " wasn't found!");
            return;
        }
        s.source.Play();
    }

    public void Play(string name, float pitch)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("name " + name + " wasn't found!");
            return;
        }
        s.source.pitch = pitch;
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("name " + name + " wasn't found!");
            return;
        }
        s.source.Stop();
    }

    public void UnpauseLoop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("name " + name + " wasn't found!");
            return;
        }

        if (!s.source.loop)
        {
            Debug.LogWarning("Sound " + name + " is not a loop!");
            return;
        }

        if (s.source.isPlaying)
        {
            Debug.Log("Loop is already playing");
            return;
        }
        
        s.source.UnPause();
    }

    public void PauseLoop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("name " + name + " wasn't found!");
            return;
        }

        if (!s.source.loop)
        {
            Debug.LogWarning("Sound " + name + " is not a loop!");
            return;
        }

        if (!s.source.isPlaying)
        {
            Debug.Log("Loop is not playing");
            return;
        }

        s.source.Pause();
    }
}