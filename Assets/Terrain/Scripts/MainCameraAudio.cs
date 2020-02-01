using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraAudio : MonoBehaviour
{
    void Start()
    {
        GetComponent<AudioSource>().volume = AudioController.volume;
    }

    void Update()
    {
        if (!AudioController.playSounds)
            GetComponent<AudioSource>().Stop();
    }
}
