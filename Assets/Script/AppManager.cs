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
    private int currentAssignedTaskIndex = -1;


    [Serializable]
    class MySoundClip
    {
        public string title = "untitled";
        public string name = "";
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

    [Serializable]
    class MyQuestItem
    {
        public int index = 0;
        public string questContent = "";
        public string tarSoundClip;
        public List<string> otherSoundClips;
        public int randomPlayAmount = 0;
    }

    [SerializeField]
    private List<MyQuestItem> MyQuestList;

    private MyQuestItem currentQuest = null;

    [Serializable]
    class MyTaskItem
    {
        public int index = 0;
        public List<int> questList;
    }

    [SerializeField]
    private List<MyTaskItem> MyTaskList;

    private List<int> objectRandomList = new List<int>();

    public string currentTaskContent = "";

    private List<GameObject> audioSourceList = new List<GameObject>();

    private List<GameObject> currentPlayingList = new List<GameObject>();

    private GameObject backgroundMusicPlayer = null;

    public bool isBackgroundMusicMuted = false;

    private float currentCountDownVal = 0f;
    private float suggestionCountDownVal = 0f;

    IEnumerator trackingCoroutine = null;
    IEnumerator taskCoundownCoroutine = null;
    IEnumerator suggestionCoroutine = null;

    List<IEnumerator> autoPlayAudioCoroutines = new List<IEnumerator>();

    public enum TestType
    {
        Off = 0,
        IconMapWithIconTag = 1,
        IconMapWithTextTag = 2,
        TextMapWithIconTag = 3,
        TextMapWithTextTag = 4
    };

    [HideInInspector]
    public bool onObjectIndicatorState = false;
    [HideInInspector]
    public bool isIconicTagIndicator = false;
    [HideInInspector]
    public bool isTextTagIndicator = false;

    [HideInInspector]
    public bool isIconicMapIndicator = false;
    [HideInInspector]
    public bool isTextMapIndicator = false;

    public TestType myTestState = TestType.Off;

    [NonSerialized]
    public OVRInput.Controller activeController;

    private bool isSkippable = false;

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

        if (ConfigManager.instance.useLeftHandStr != null)
        {
            isRightHanded = !bool.Parse(ConfigManager.instance.useLeftHandStr);
        }

        if (ConfigManager.instance.useMuteBackgroundMusicStr != null)
        {
            isBackgroundMusicMuted = !bool.Parse(ConfigManager.instance.useMuteBackgroundMusicStr);
        }

        HUDMap = GameObject.Find("HUDMiniMap");
        centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
        playerObj = GameObject.Find("OVRPlayerController");
        playerIndicatorOnMap = GameObject.Find("Player on Map");
        backgroundMusicPlayer = GameObject.Find("Speaker");

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

                    if (isTaskStarted && isSkippable)
                    {
                        CompleteATask("");
                    }
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

                    if (isTaskStarted && isSkippable)
                    {
                        CompleteATask("");
                    }
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

                if (isTaskStarted && isSkippable)
                {
                    CompleteATask("");
                }
            }
        }


        // Background Music Control
        backgroundMusicPlayer.GetComponent<AudioSource>().mute = isBackgroundMusicMuted;
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

    private void StartATask()
    {
        if (!isTaskStarted)
        {
            isTaskStarted = true;

            isSkippable = false;

            if (objectRandomList.Count > 0)
            {
                currentAssignedTaskIndex = objectRandomList[UnityEngine.Random.Range(0, objectRandomList.Count)];
                objectRandomList.Remove(currentAssignedTaskIndex);

                List<int> currentQuestList = MyTaskList[currentAssignedTaskIndex].questList;
                int currentActiveQuestIndex = currentQuestList[UnityEngine.Random.Range(0, currentQuestList.Count)];

                currentQuest = MyQuestList[currentActiveQuestIndex];
                currentTaskContent = currentQuest.questContent;

                MiniPromptController.instance.ShowTaskPrompt(currentTaskContent);

                taskCoundownCoroutine = StartCountdown();
                StartCoroutine(taskCoundownCoroutine);
            }
        }
    }

    private void TaskStarter()
    {
        // All the audio clips that play in this quest 
        List<string> playingClips = new List<string>();

        playingClips.Add(currentQuest.tarSoundClip);

        if (currentQuest.otherSoundClips.Count > 0)
        {
            int randomPick = currentQuest.randomPlayAmount > 0 ? currentQuest.randomPlayAmount : currentQuest.otherSoundClips.Count;

            List<string> newOtherSoundClips = new List<string>(currentQuest.otherSoundClips);

            for (int i = 0; i < randomPick; i++)
            {
                string otherAudioClipTitle = newOtherSoundClips[UnityEngine.Random.Range(0, newOtherSoundClips.Count)];
                newOtherSoundClips.Remove(otherAudioClipTitle);

                playingClips.Add(otherAudioClipTitle);
            }
        }

        currentPlayingList.Clear();
        autoPlayAudioCoroutines.Clear();

        for (int i = 0; i < playingClips.Count; i++)
        {
            for (int j = 0; j < audioSourceList.Count; j++)
            {
                if (audioSourceList[j].name == "Source_" + playingClips[i])
                {
                    autoPlayAudioCoroutines.Add(PlayAudioClip(audioSourceList[j]));
                    StartCoroutine(autoPlayAudioCoroutines[i]);

                    break;
                }
            }
        }

        MiniPromptController.instance.TaskStarted();

        if (taskCoundownCoroutine != null)
        {
            StopCoroutine(taskCoundownCoroutine);
        }

        suggestionCoroutine = SuggestionCountdown();
        StartCoroutine(suggestionCoroutine);
    }

    public IEnumerator StartCountdown(float countdownVal = 7)
    {
        currentCountDownVal = countdownVal;
        while (currentCountDownVal > 0)
        {
            if (currentCountDownVal <= 3)
            {
                MiniPromptController.instance.TaskStarting(currentCountDownVal);
            }

            yield return new WaitForSeconds(1.0f);
            currentCountDownVal--;
        }
        TaskStarter();
    }

    public IEnumerator PlayAudioClip(GameObject player)
    {
        float generatorTimer = UnityEngine.Random.Range(0f, 5f);
        yield return new WaitForSeconds(generatorTimer);

        player.GetComponentInChildren<MusicPlayer>().StopPlaying();
        player.GetComponentInChildren<MusicPlayer>().StartPlaying();

        currentPlayingList.Add(player);
        currentPlayingObject = player.name;
    }

    public IEnumerator SuggestionCountdown(float countdownVal = 10)
    {
        suggestionCountDownVal = countdownVal;
        while (suggestionCountDownVal > 0)
        {
            yield return new WaitForSeconds(1.0f);
            suggestionCountDownVal--;
        }
        EnableSuggestion();
    }

    private void EnableSuggestion()
    {
        MiniPromptController.instance.ShowSuggestionText();
        isSkippable = true;
    }

    private void DisableSuggestion()
    {
        MiniPromptController.instance.HideSuggestionText();
        isSkippable = false;
    }

    public void CompleteATask(string objectName)
    {
        if (isTaskStarted)
        {
            string tarObjName = "";

            if (GetGameObjectbySoundClipTitle(currentQuest.tarSoundClip))
            {
                tarObjName = GetGameObjectbySoundClipTitle(currentQuest.tarSoundClip).name;
            }

            if (objectName == tarObjName)
            {
                startHapticFeedback();
            }

            MiniPromptController.instance.HideTaskPrompt();

            if (suggestionCoroutine != null)
            {
                StopCoroutine(suggestionCoroutine);
            }

            // Stop all autoplay Coroutine
            for (int i = 0; i < autoPlayAudioCoroutines.Count; i++)
            {
                if (autoPlayAudioCoroutines[i] != null)
                {
                    StopCoroutine(autoPlayAudioCoroutines[i]);
                }
            }

            DisableSuggestion();

            for (int i = 0; i < currentPlayingList.Count; i++)
            {
                currentPlayingList[i].GetComponentInChildren<MusicPlayer>().StopPlaying();
            }

            currentPlayingList.Clear();

            currentPlayingObject = "";
            currentQuest = null;

            isTaskStarted = false;

            if (objectRandomList.Count == 0)
            {
                MiniPromptController.instance.TaskCompleted(true);
                //ResetTestState();
            }
            else
            {
                StartATask();
            }
        }
    }

    private GameObject GetGameObjectbySoundClipTitle(string title)
    {
        for (int i = 0; i < MySoundClipList.Count; i++)
        {
            if (MySoundClipList[i].title == title)
            {
                return MySoundClipList[i].tarObject;
            }
        }

        return null;
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
            go.name = "Source_" + obj.title;
            go.transform.localScale = obj.scale;

            go.transform.Find("Tag Conatiner").localPosition = new Vector3(0, obj.indicatorHeight, 0);

            var objMusicPlayer = go.GetComponentInChildren<MusicPlayer>();
            objMusicPlayer.audioClip = obj.audioClip;
            objMusicPlayer.isLoop = obj.isLoop;
            //objMusicPlayer.isAutoPlay = obj.isPlayOnWake;
            objMusicPlayer.indicatorIcon = obj.sprite;
            objMusicPlayer.indicatorText = obj.name;

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
        for (int i = 0; i < MyTaskList.Count; i++)
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

    public void ResetTestState()
    {
        DisableSuggestion();

        GenerateRandomList();

        StopAllCoroutines();

        //userSelectIds.Clear();
        //playingSourceIds.Clear();
        //playingClipIds.Clear();
        //playingClipNames.Clear();
        //userSelectPositions.Clear();
        //playingSourcePositions.Clear();
        //selectingDurations.Clear();
        //currentUserPositions.Clear();
        //currentUserRotations.Clear();
        //userPositionsRecord.Clear();
        //userRotationsRecord.Clear();

        //lastTimestamp = null;

        //currentAttempt = 0;
        //testStartDate = null;
        taskCoundownCoroutine = null;
        trackingCoroutine = null;
    }
}
