using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mario : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;


    public void PlayAudio(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

}
