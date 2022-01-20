using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Source: https://www.youtube.com/watch?v=1usnKHMquH0

public class Interactable : MonoBehaviour
{
    public void Pressed()
    {
        if (gameObject.GetComponent<Click>())
        {
            gameObject.GetComponent<Click>().onClick();
        }

        AppManager.instance.CompleteATask("Source_" + gameObject.name);

        //for (int i = 0; i < AppManager.instance.audioSourceList.Count; i++)
        //{
        //    var tar = AppManager.instance.audioSourceList[i];
        //    if (tar.name.Equals("Source_" + gameObject.name))
        //    {
        //        tar.GetComponentInChildren<MusicPlayer>().StopPlaying();
        //        tar.GetComponentInChildren<MusicPlayer>().StartPlaying();
        //        break;
        //    }
        //}
    }
}
