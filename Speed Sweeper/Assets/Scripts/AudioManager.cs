using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip explosionSound;
    public AudioClip clickSound;
    public AudioSource sfxSource;
    public static AudioManager instance;

    public float vol { get; protected set; } 

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        //sfxSource = new AudioSource();
        //sfxSource.transform.parent = Camera.main.transform;
        vol = 1.0f;
    }

    public void PlayExplosionAt(Vector3 pos)
    {
        sfxSource.transform.position = pos;
        sfxSource.PlayOneShot(explosionSound);
        //AudioSource.PlayClipAtPoint(explosionSound, pos, vol);
        //sfxSource.PlayOneShot(explosionSound, vol);
    }
    public void PlayClickAt(Vector3 pos)
    {
        sfxSource.transform.position = pos;
        sfxSource.PlayOneShot(clickSound);
        //AudioSource.PlayClipAtPoint(clickSound, pos, vol);
        //AudioSource.PlayClipAtPoint(clickSound, pos, vol);
        //sfxSource.PlayOneShot(clickSound, vol);
    }
    public void MoveAudioSource(Vector3 pos)
    {
        sfxSource.transform.position = pos;
    }

    public void SetVolume(float f)
    {
        print("Volume changed to :" +  f.ToString());
        vol = f;
        sfxSource.volume = vol;
    }
}
