using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioBackgroundManager : MonoBehaviour
{
    public static AudioBackgroundManager instance;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    public void MuteBackgroundAudio(bool b)
    {
        GetComponent<AudioSource>().mute = b;
        AudioManager.instance.GetComponentInChildren<AudioSource>().mute = b;
    }
}
