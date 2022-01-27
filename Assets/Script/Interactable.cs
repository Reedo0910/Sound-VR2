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

        AppManager.instance.CompleteATask(gameObject);
    }
}
