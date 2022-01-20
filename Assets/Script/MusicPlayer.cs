using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SOurce code: https://answers.unity.com/questions/1167177/how-do-i-get-the-current-volume-level-amplitude-of.html

public class MusicPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    //public AudioClip[] audioClips;

    public float updateStep = 0.1f;
    public int sampleDataLength = 1024;

    private float currentUpdateTime = 0f;

    public float clipLoudness;
    private float[] clipSampleData;

    public AudioClip audioClip;

    public GameObject mapIndicator;
    private Transform loudnessIndicator;

    public GameObject soundIndicatorOnTag;

    public float minIndicatorScale = 0.2f;
    public float maxIndicatorScale = 3.0f;

    public float scaleStep = 0.5f;

    public bool isAutoPlay = false;

    public bool isLoop = false;

    public Sprite indicatorIcon = null;


    void Awake()
    {
        clipSampleData = new float[sampleDataLength];
    }

    // Start is called before the first frame update
    void Start()
    {
        if (mapIndicator)
        {
            loudnessIndicator = mapIndicator.transform.Find("Iconic Loudness Indicator");

            if (indicatorIcon)
            {
                loudnessIndicator.GetComponent<SpriteRenderer>().sprite = indicatorIcon;
            }

            mapIndicator.SetActive(false);
        }

        if (soundIndicatorOnTag)
        {
            soundIndicatorOnTag.SetActive(false);
        }

        if (isAutoPlay)
        {
            StartPlaying();
        }

        audioSource.loop = isLoop;
    }

    // Update is called once per frame
    void Update()
    {
        if (audioSource.isPlaying)
        {
            currentUpdateTime += Time.deltaTime;
            if (currentUpdateTime >= updateStep)
            {
                currentUpdateTime = 0f;

                //read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
                audioSource.clip.GetData(clipSampleData, audioSource.timeSamples);
                clipLoudness = 0f;
                foreach (var sample in clipSampleData)
                {
                    clipLoudness += Mathf.Abs(sample);
                }
                clipLoudness /= sampleDataLength;
            }

            if (mapIndicator)
            {
                mapIndicator.SetActive(true);
            }

            if (soundIndicatorOnTag && AppManager.instance.onObjectIndicatorState)
            {
                soundIndicatorOnTag.SetActive(true);
            }

            if (soundIndicatorOnTag && !AppManager.instance.onObjectIndicatorState)
            {
                soundIndicatorOnTag.SetActive(false);
            }
        }
        else
        {
            if (mapIndicator)
            {
                mapIndicator.SetActive(false);
            }

            if (soundIndicatorOnTag)
            {
                soundIndicatorOnTag.SetActive(false);
            }
        }

        AnimateLoudnessIndicator();
    }

    public void StartPlaying()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }

    public void StopPlaying()
    {
        audioSource.Stop();
    }

    void AnimateLoudnessIndicator()
    {
        float tarScale = minIndicatorScale + (maxIndicatorScale - minIndicatorScale) * clipLoudness;

        float TagScale = 0.35f;

        if (mapIndicator && mapIndicator.activeSelf)
        {
            loudnessIndicator.localScale = Vector3.Lerp(new Vector3(tarScale, tarScale, loudnessIndicator.localScale.z), loudnessIndicator.localScale, scaleStep);
        }

        if (soundIndicatorOnTag && soundIndicatorOnTag.activeSelf)
        {
            soundIndicatorOnTag.transform.localScale = Vector3.Lerp(new Vector3(tarScale * TagScale, soundIndicatorOnTag.transform.localScale.y, tarScale * TagScale), soundIndicatorOnTag.transform.localScale, scaleStep);
        }
    }
}
