using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class AppManager : MonoBehaviour
{
    [NonSerialized]
    public static AppManager instance;

    private GameObject HUDMap;

    private GameObject playerObj;
    private GameObject centerEyeAnchor;
    private GameObject playerIndicatorOnMap;
    [SerializeField]
    private GameObject soundSourcePrefab;

    public bool isRightHanded = true; // use which controller by default

    private bool isTaskStarted = false;

    public string currentPlayingObject = "";
    private int currentPlayingObjectIndex = -1;


    [Serializable]
    class MySoundClip
    {
        public string title = "untitled";
        public Sprite sprite;
        public GameObject tarObject;
        public AudioClip audioClip;
        public bool isLoop = false;
        //public bool isPlayOnWake = false;
        public Vector3 scale = new Vector3(0.1f, 0.1f, 0.1f);
        public float indicatorHeight = 0f;
    }

    [SerializeField]
    private List<MySoundClip> MySoundClipList;

    private List<int> objectRandomList = new List<int>();

    private List<GameObject> audioSourceList = new List<GameObject>();

    public enum TestType
    {
        Off = 0,
        IconMapWithIconTag = 1,
        IconMapWithTextTag = 2,
        TextMapWithIconTag = 3,
        TextMapWithTextTag = 4
    };

    public bool onObjectIndicatorState = false;
    public bool isIconicTagIndicator = false;
    public bool isTextTagIndicator = false;

    public bool isIconicMapIndicator = false;
    public bool isTextMapIndicator = false;

    public TestType myTestState = TestType.Off;

    [NonSerialized]
    public OVRInput.Controller activeController;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(transform.gameObject);
        }

        // Play in 90Hz
        if (OVRManager.display.displayFrequenciesAvailable.Contains(90f))
        {
            OVRManager.display.displayFrequency = 90f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        activeController = OVRInput.GetActiveController();

        HUDMap = GameObject.Find("HUDMiniMap");
        centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
        playerObj = GameObject.Find("OVRPlayerController");
        playerIndicatorOnMap = GameObject.Find("Player on Map");

        SetTestState((int)myTestState);
        BindSoundSource();

        GenerateRandomList();
    }

    // Update is called once per frame
    void Update()
    {
        activeController = OVRInput.GetActiveController();

        // Dual controllers
        if (activeController == OVRInput.Controller.Touch)
        {
            if (isRightHanded)
            {
                // Detect button inputs on the right controller
                if (OVRInput.GetDown(OVRInput.Button.Two))
                {
                    NextMethod();
                }
                else if (OVRInput.GetDown(OVRInput.Button.One))
                {
                    StartATask();
                }
            }
            else
            {
                // Detect button inputs on the left controller
                if (OVRInput.GetDown(OVRInput.Button.Four))
                {
                    NextMethod();
                }
                else if (OVRInput.GetDown(OVRInput.Button.Three))
                {
                    StartATask();
                }

            }
        }
        // Individual controller
        else
        {
            if (OVRInput.GetDown(OVRInput.Button.Two))
            {
                NextMethod();
            }
            else if (OVRInput.GetDown(OVRInput.Button.One))
            {
                StartATask();
            }
        }
    }

    private void LateUpdate()
    {
        UpdateSoundSourcePosition();
    }

    private void NextMethod()
    {
        int curState = (int)myTestState;
        if (curState >= 4)
        {
            curState = 0;
        }
        else
        {
            curState++;
        }

        SetTestState(curState);
    }

    //private void PreviousMethod()
    //{
    //    int curState = (int)myTestState;
    //    if (curState <= 0)
    //    {
    //        curState = 4;
    //    }
    //    else
    //    {
    //        curState--;
    //    }

    //    SetTestState(curState);
    //}

    private void StartATask()
    {
        if (!isTaskStarted)
        {
            if (objectRandomList.Count > 0)
            {
                MiniPromptController.instance.ShowSuggestionText();

                currentPlayingObjectIndex = objectRandomList[UnityEngine.Random.Range(0, objectRandomList.Count)];
                objectRandomList.Remove(currentPlayingObjectIndex);

                audioSourceList[currentPlayingObjectIndex].GetComponentInChildren<MusicPlayer>().StopPlaying();
                audioSourceList[currentPlayingObjectIndex].GetComponentInChildren<MusicPlayer>().StartPlaying();

                currentPlayingObject = audioSourceList[currentPlayingObjectIndex].name;

                isTaskStarted = true;
            }
        }
    }

    public void CompleteATask(string name)
    {
        if (isTaskStarted)
        {
            if (name == currentPlayingObject)
            {
                startHapticFeedback();
            }

            MiniPromptController.instance.HideSuggestionText();

            audioSourceList[currentPlayingObjectIndex].GetComponentInChildren<MusicPlayer>().StopPlaying();

            currentPlayingObject = "";

            isTaskStarted = false;
        }
    }

    public void startHapticFeedback()
    {
        StartCoroutine(TriggerVibration());
    }

    private void BindSoundSource()
    {
        Transform container = GameObject.Find("SourceGroup").transform;

        for (int i = 0; i < MySoundClipList.Count; i++)
        {
            MySoundClip obj = MySoundClipList[i];

            GameObject tar = obj.tarObject;

            GameObject go = Instantiate(soundSourcePrefab, tar.transform.position, Quaternion.identity);
            go.name = "Source_" + tar.name;
            go.transform.localScale = obj.scale;

            go.transform.Find("Tag Conatiner").localPosition = new Vector3(0, obj.indicatorHeight, 0);

            var objMusicPlayer = go.GetComponentInChildren<MusicPlayer>();
            objMusicPlayer.audioClip = obj.audioClip;
            objMusicPlayer.isLoop = obj.isLoop;
            //objMusicPlayer.isAutoPlay = obj.isPlayOnWake;
            objMusicPlayer.indicatorIcon = obj.sprite;
            objMusicPlayer.indicatorText = obj.title;

            go.transform.SetParent(container);

            audioSourceList.Add(go);
        }
    }

    private void UpdateSoundSourcePosition()
    {
        for (int i = 0; i < audioSourceList.Count; i++)
        {
            var obj = audioSourceList[i];
            var tar = MySoundClipList[i];
            if (obj.name.Equals("Source_" + tar.tarObject.name))
            {
                obj.transform.position = tar.tarObject.transform.position;
            }
        }
    }

    private void GenerateRandomList()
    {
        objectRandomList.Clear();
        for (int i = 0; i < audioSourceList.Count; i++)
        {
            objectRandomList.Add(i);
        }
    }


    IEnumerator TriggerVibration()
    {
        OVRInput.Controller activeControllerMask;

        if (activeController == OVRInput.Controller.Touch)
        {
            if (!isRightHanded)
            {
                activeControllerMask = OVRInput.Controller.LTouch;
            }
            else
            {
                activeControllerMask = OVRInput.Controller.RTouch;
            }
        }
        else
        {
            activeControllerMask = activeController;
        }

        // Start Vibration
        OVRInput.SetControllerVibration(.7f, .2f, activeControllerMask);

        yield return new WaitForSeconds(.1f);

        // Stop Vibration
        OVRInput.SetControllerVibration(0, 0, activeControllerMask);
    }

    public void SetTestState(int testState)
    {
        InactivateMaps();

        switch ((TestType)testState)
        {
            case TestType.IconMapWithIconTag:
                HUDMap.SetActive(true);
                onObjectIndicatorState = true;
                isIconicMapIndicator = true;
                isIconicTagIndicator = true;
                myTestState = TestType.IconMapWithIconTag;
                break;

            case TestType.IconMapWithTextTag:
                HUDMap.SetActive(true);
                onObjectIndicatorState = true;
                isIconicMapIndicator = true;
                isTextTagIndicator = true;
                myTestState = TestType.IconMapWithTextTag;
                break;

            case TestType.TextMapWithIconTag:
                HUDMap.SetActive(true);
                onObjectIndicatorState = true;
                isTextMapIndicator = true;
                isIconicTagIndicator = true;
                myTestState = TestType.TextMapWithIconTag;
                break;

            case TestType.TextMapWithTextTag:
                HUDMap.SetActive(true);
                onObjectIndicatorState = true;
                isTextMapIndicator = true;
                isTextTagIndicator = true;
                myTestState = TestType.TextMapWithTextTag;
                break;

            default:
                myTestState = TestType.Off;
                break;
        }

        void InactivateMaps()
        {
            HUDMap.SetActive(false);

            onObjectIndicatorState = false;

            isIconicMapIndicator = false;
            isIconicTagIndicator = false;
            isTextMapIndicator = false;
            isTextTagIndicator = false;

            playerIndicatorOnMap.SetActive(true);
        }
    }
}
