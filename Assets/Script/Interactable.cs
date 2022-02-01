using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Source: https://www.youtube.com/watch?v=1usnKHMquH0
// Source: https://www.youtube.com/watch?v=7ybz28Py0-U

public class Interactable : MonoBehaviour
{
    private Color32 originColor;
    private bool isFlashing = false;
    private bool isFlashingIn = true;
    private int redColor;
    private int greenColor;
    private int blueColor;

    IEnumerator flashingCoroutine = null;

    private void Awake()
    {
        originColor = gameObject.GetComponent<Renderer>().material.color;
    }
    private void LateUpdate()
    {
        if (AppManager.instance.myTestState == AppManager.TestType.Off)
        {
            HighlightObject();
        }
        else
        {
            RestoreColor();
        }
    }

    private void HighlightObject()
    {
        gameObject.GetComponent<Renderer>().material.color = new Color32((byte)redColor, (byte)greenColor, (byte)blueColor, 255);

        if (!isFlashing)
        {
            isFlashing = true;
            flashingCoroutine = FlashObject();
            StartCoroutine(flashingCoroutine);
        }
    }

    IEnumerator FlashObject()
    {
        while (AppManager.instance.myTestState == AppManager.TestType.Off)
        {
            yield return new WaitForSeconds(0.1f);
            if (isFlashingIn)
            {
                if (greenColor <= 50)
                {
                    isFlashingIn = false;
                }
                else
                {
                    redColor -= 5;
                    greenColor -= 25;
                    blueColor -= 5;
                }
            }
            else
            {
                if (greenColor >= 220)
                {
                    isFlashingIn = true;
                }
                else
                {
                    redColor += 5;
                    greenColor += 25;
                    blueColor += 5;
                }
            }
        }
    }


    private void RestoreColor()
    {
        isFlashing = false;

        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
        }

        gameObject.GetComponent<Renderer>().material.color = originColor;
    }

    public void Pressed()
    {
        if (gameObject.GetComponent<Click>())
        {
            gameObject.GetComponent<Click>().onClick();
        }

        AppManager.instance.CompleteATask(gameObject);
    }
}
