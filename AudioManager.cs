using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
   [SerializeField]
   private List <AudioClip> audios = new List<AudioClip>();
    private AudioSource audioSource;


    public void PlayAudio(string name, Vector3 point)
    {
        AudioClip newClip = null;
        foreach (AudioClip clip in audios)
        {
            if(clip.name == name)
                newClip = clip;
        }

        if (newClip == null)
            return;
        AudioSource.PlayClipAtPoint(newClip, point);
    }

}
