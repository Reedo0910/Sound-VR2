using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Threading;

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

    public bool isTestStarted = false;
    private bool isTaskStarted = false;

    private string currentPlayingClip = "";
    private int currentAssignedTaskIndex = -1;

    public bool isDemoOnly = false;


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
    IEnumerator testPrepareCoundownCoroutine = null;
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

    [HideInInspector]
    public int currentAttempt = 0;

    [NonSerialized]
    public DateTime? testStartDate = null;

    private bool isSkippable = false;

    [NonSerialized]
    public List<Vector3> currentUserPositions = new List<Vector3>();
    [NonSerialized]
    public List<Vector3> currentUserRotations = new List<Vector3>();

    // record within selection
    public class UserPosition
    {
        public List<Vector3> positions;
    }
    // record within selection
    public class UserRotation
    {
        public List<Vector3> rotations;
    }

    [NonSerialized]
    public List<UserPosition> userPositionsRecord = new List<UserPosition>(); // all the record in an attempt
    [NonSerialized]
    public List<UserRotation> userRotationsRecord = new List<UserRotation>(); // all the record in an attempt

    // Interval of position/rotation tracking
    [SerializeField]
    private float trackingScale = 0.1f;

    [NonSerialized]
    public DateTime? lastTimestamp = null;
    [NonSerialized]
    public List<double> selectingDurations = new List<double>();

    [NonSerialized]
    public List<string> userSelectObjectNames = new List<string>();
    [NonSerialized]
    public List<Vector3> userSelectPositions = new List<Vector3>();

    [NonSerialized]
    public List<string> playingSourceObjectNames = new List<string>();
    [NonSerialized]
    public List<Vector3> playingSourceObjectPositions = new List<Vector3>();

    [NonSerialized]
    public List<string> playingClipNames = new List<string>();

    public class OtherPlayingClipNames
    {
        public List<string> clipNames;
    }

    [NonSerialized]
    public List<OtherPlayingClipNames> otherPlayingClipNamesList = new List<OtherPlayingClipNames>();

    public class OtherPlayingSourceObjectNames
    {
        public List<string> objectNames;
    }
    [NonSerialized]
    public List<OtherPlayingSourceObjectNames> otherPlayingSourceObjectNamesList = new List<OtherPlayingSourceObjectNames>();


    public class OtherPlayingSourceObjectPositions
    {
        public List<Vector3> objectPositions;
    }
    [NonSerialized]
    public List<OtherPlayingSourceObjectPositions> otherPlayingSourceObjectPositionsList = new List<OtherPlayingSourceObjectPositions>();


    public bool isUIDisabled = false;

    [SerializeField]
    private Canvas UICanvas = null;

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
            isBackgroundMusicMuted = bool.Parse(ConfigManager.instance.useMuteBackgroundMusicStr);
        }

        if (ConfigManager.instance.useDemoStr != null)
        {
            isDemoOnly = bool.Parse(ConfigManager.instance.useDemoStr);
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
        if (isDemoOnly)
        {
            if (Input.GetKeyUp(KeyCode.V) || OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
            {
                //NextMethod();
                ToggleViz();
            }

            if (Input.GetKeyUp(KeyCode.O))
            {
                StartTest(1);
            }

            if (Input.GetKeyUp(KeyCode.P))
            {
                StopTest();
            }

            if (Input.GetKeyUp(KeyCode.Q))
            {
                DisableUI();
            }
        }

        activeController = OVRInput.GetActiveController();

        // Dual controllers
        if (activeController == OVRInput.Controller.Touch)
        {
            if (isRightHanded)
            {
                // Detect button inputs on the right controller
                if (OVRInput.GetDown(OVRInput.Button.Two) || OVRInput.GetDown(OVRInput.Button.One))
                {
                    if (isTestStarted && isTaskStarted && isSkippable)
                    {
                        CompleteATask(null);
                    }
                }
            }
            else
            {
                // Detect button inputs on the left controller
                if (OVRInput.GetDown(OVRInput.Button.Four) || OVRInput.GetDown(OVRInput.Button.Three))
                {
                    if (isTestStarted && isTaskStarted && isSkippable)
                    {
                        CompleteATask(null);
                    }
                }

            }
        }
        // Individual controller
        else
        {
            if (OVRInput.GetDown(OVRInput.Button.Two) || OVRInput.GetDown(OVRInput.Button.One))
            {
                if (isTestStarted && isTaskStarted && isSkippable)
                {
                    CompleteATask(null);
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

    private void ToggleViz()
    {
        int curState = (int)myTestState;
        if (curState != 0)
        {
            curState = 0;
        }
        else
        {
            if (ConfigManager.instance.useDemoDefaultStr != null && int.TryParse(ConfigManager.instance.useDemoDefaultStr, out int configDefaultState))
            {
                curState = configDefaultState;
            }
            else
            {
                curState = 3;
            }
        }

        SetTestState(curState);
    }

    private void DisableUI()
    {
        if (UICanvas == null)
        {
            return;
        }

        if (!isUIDisabled)
        {
            // Disable UI
            UICanvas.enabled = false;
        }
        else
        {
            // Enable UI
            UICanvas.enabled = true;
        }

        isUIDisabled = !isUIDisabled;
    }

    public void StartTest(int attempt)
    {
        ResetTestState();

        isTestStarted = true;
        testStartDate = DateTime.UtcNow;
        currentAttempt = attempt;

        int myCountDown = 5;

        if (isDemoOnly)
        {
            // No count down on demo
            myCountDown = 0;
        }

        testPrepareCoundownCoroutine = TestPrepareCountdown(myCountDown);
        StartCoroutine(testPrepareCoundownCoroutine);
    }

    public IEnumerator TestPrepareCountdown(float countdownVal = 5)
    {
        currentCountDownVal = countdownVal;
        while (currentCountDownVal > 0)
        {
            MiniPromptController.instance.TestPreparing(currentCountDownVal);
            yield return new WaitForSeconds(1.0f);
            currentCountDownVal--;
        }
        StartATaskItr();
    }

    public void SubmitTestData()
    {
        bool isConditionEnd = currentAttempt == 2;

        isTestStarted = false;
        DateTime testEndDate = DateTime.UtcNow;

        if (!isDemoOnly)
        {
            // submit result if test only
            SocketModule.instance.TestInfoUpdate((DateTime)testStartDate, testEndDate);
        }

        if (trackingCoroutine != null)
        {
            StopCoroutine(trackingCoroutine);
        }

        ResetTestState();

        if (!isDemoOnly)
        {
            // show test finish prompt if test only
            MiniPromptController.instance.TaskCompleted(isConditionEnd);
        }
        else
        {
            // looping the test if demo only
            StartTest(1);
        }
    }

    public void StopTest()
    {
        if (testPrepareCoundownCoroutine != null)
        {
            StopCoroutine(testPrepareCoundownCoroutine);
        }

        if (taskCoundownCoroutine != null)
        {
            StopCoroutine(taskCoundownCoroutine);
        }

        if (trackingCoroutine != null)
        {
            StopCoroutine(trackingCoroutine);
        }

        if (suggestionCoroutine != null)
        {
            StopCoroutine(suggestionCoroutine);
        }

        DisableSuggestion();

        isTestStarted = false;
        isTaskStarted = false;
        ResetTestState();

        if (!isDemoOnly)
        {
            // show task holding prompt if test only
            MiniPromptController.instance.TaskHolding();
        }
    }

    private void StartATaskItr()
    {
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

            int myCountDown = 7;

            if (isDemoOnly)
            {
                // no count down if demo only
                myCountDown = 0;
            }

            taskCoundownCoroutine = StartCountdown(myCountDown);
            StartCoroutine(taskCoundownCoroutine);
        }
    }

    private void TaskStarter()
    {
        if (taskCoundownCoroutine != null)
        {
            StopCoroutine(taskCoundownCoroutine);
        }

        trackingCoroutine = TrackUserMovement();
        StartCoroutine(trackingCoroutine);

        // All the audio clips that play in this quest 
        List<string> playingClips = new List<string>();

        playingClips.Add(currentQuest.tarSoundClip);

        currentPlayingClip = currentQuest.tarSoundClip;

        playingClipNames.Add(currentQuest.tarSoundClip);

        List<string> myOtherPlayingClipNames = new List<string>();
        List<string> myOtherPlayingSourceObjectNames = new List<string>();
        List<Vector3> myOtherPlayingSourceObjectPositions = new List<Vector3>();

        if (currentQuest.otherSoundClips.Count > 0)
        {
            int randomPick = currentQuest.randomPlayAmount > 0 ? currentQuest.randomPlayAmount : currentQuest.otherSoundClips.Count;

            List<string> newOtherSoundClips = new List<string>(currentQuest.otherSoundClips);

            newOtherSoundClips.Shuffle();

            int randomPickCount = 0;

            for (int i = 0; i < newOtherSoundClips.Count; i++)
            {
                if (randomPickCount >= randomPick)
                {
                    break;
                }

                string otherAudioClipTitle = newOtherSoundClips[i];

                bool hasSameSoundSource = false;

                for (int j = 0; j < playingClips.Count; j++)
                {
                    if (ReferenceEquals(GetGameObjectbySoundClipTitle(playingClips[j]), GetGameObjectbySoundClipTitle(otherAudioClipTitle)))
                    {
                        hasSameSoundSource = true;
                        break;
                    }
                }

                if (hasSameSoundSource)
                {
                    continue;
                }

                playingClips.Add(otherAudioClipTitle);

                myOtherPlayingClipNames.Add(otherAudioClipTitle);

                GameObject otherPlayingSourceObject = GetGameObjectbySoundClipTitle(otherAudioClipTitle);
                myOtherPlayingSourceObjectNames.Add(otherPlayingSourceObject.name);
                myOtherPlayingSourceObjectPositions.Add(otherPlayingSourceObject.transform.position);

                randomPickCount++;
            }
        }

        OtherPlayingClipNames otherPlayingClipNames = new OtherPlayingClipNames
        {
            clipNames = myOtherPlayingClipNames
        };
        otherPlayingClipNamesList.Add(otherPlayingClipNames);

        OtherPlayingSourceObjectNames otherPlayingSourceObjectNames = new OtherPlayingSourceObjectNames
        {
            objectNames = myOtherPlayingSourceObjectNames
        };
        otherPlayingSourceObjectNamesList.Add(otherPlayingSourceObjectNames);

        OtherPlayingSourceObjectPositions otherPlayingSourceObjectPositions = new OtherPlayingSourceObjectPositions
        {
            objectPositions = myOtherPlayingSourceObjectPositions
        };
        otherPlayingSourceObjectPositionsList.Add(otherPlayingSourceObjectPositions);

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

        isTaskStarted = true;

        suggestionCoroutine = SuggestionCountdown();
        StartCoroutine(suggestionCoroutine);

        currentUserPositions.Clear();
        currentUserRotations.Clear();
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

        if ("Source_" + currentPlayingClip == player.name)
        {
            lastTimestamp = DateTime.UtcNow;
        }
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
        MiniPromptController.instance.HideTaskPrompt();
        isSkippable = false;
    }

    public void CompleteATask(GameObject selectedObject)
    {
        if (!isTestStarted || !isTaskStarted) { return; }

        isTaskStarted = false;

        string tarObjName = "";
        Vector3 tarObjPosition = new Vector3(0, 0, 0);

        if (GetGameObjectbySoundClipTitle(currentQuest.tarSoundClip))
        {
            GameObject tarObj = GetGameObjectbySoundClipTitle(currentQuest.tarSoundClip);
            tarObjName = tarObj.name;
            tarObjPosition = tarObj.transform.position;
        }

        DateTime myTimestamp = DateTime.UtcNow;
        double myDuration = 0f;

        if (lastTimestamp == null)
        {
            myDuration = -1f;
        }
        else
        {
            if (DateTime.Compare((DateTime)lastTimestamp, myTimestamp) > 0)
            {
                myDuration = -1f;
            }
            else
            {
                myDuration = myTimestamp.Subtract((DateTime)lastTimestamp).TotalSeconds;
            }
        }

        selectingDurations.Add(myDuration);

        lastTimestamp = null;

        List<Vector3> newPositions = new List<Vector3>(currentUserPositions);
        UserPosition userPosition = new UserPosition
        {
            positions = newPositions
        };
        userPositionsRecord.Add(userPosition);

        List<Vector3> newRotations = new List<Vector3>(currentUserRotations);
        UserRotation userRotation = new UserRotation
        {
            rotations = newRotations
        };
        userRotationsRecord.Add(userRotation);

        string selectedObjectName = "";
        Vector3 selectedObjectPosition = new Vector3(0, 0, 0);

        if (selectedObject != null)
        {
            selectedObjectName = selectedObject.name;
            selectedObjectPosition = selectedObject.transform.position;

            if (isDemoOnly)
            {
                if (selectedObjectName.Equals(tarObjName))
                {
                    startHapticFeedback();
                }
            }
            else
            {
                startHapticFeedback();
            }
        }

        userSelectObjectNames.Add(selectedObjectName);
        userSelectPositions.Add(selectedObjectPosition);
        playingSourceObjectNames.Add(tarObjName);
        playingSourceObjectPositions.Add(tarObjPosition);

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

        currentPlayingClip = "";
        currentQuest = null;

        if (objectRandomList.Count == 0)
        {
            SubmitTestData();
        }
        else
        {
            StartATaskItr();
        }

    }

    //private bool PositionCheck(GameObject firstObject, GameObject secondObject)
    //{
    //    if (firstObject != null && secondObject != null)
    //    {
    //        return firstObject.transform.localPosition.x == secondObject.transform.localPosition.x
    //        && firstObject.transform.localPosition.y == secondObject.transform.localPosition.y
    //        && firstObject.transform.localPosition.z == secondObject.transform.localPosition.z;
    //    }
    //    return false;
    //}

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

            go.transform.Find("Tag Container").localPosition = new Vector3(0, obj.indicatorHeight, 0);

            var objMusicPlayer = go.GetComponentInChildren<MusicPlayer>();
            objMusicPlayer.audioClip = obj.audioClip;
            objMusicPlayer.isLoop = false;
            if (ConfigManager.instance.useLoopStr != null && bool.TryParse(ConfigManager.instance.useLoopStr, out bool parseBool))
            {
                if (parseBool)
                {
                    objMusicPlayer.isLoop = obj.isLoop;
                }
            }
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

        float waitingInterval = .1f;

        if (isDemoOnly)
        {
            waitingInterval = .2f;
        }

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

        yield return new WaitForSeconds(waitingInterval);

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
                if (!isDemoOnly)
                {
                    MiniPromptController.instance.HighlightingPreTask();
                }
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

            if (!isDemoOnly)
            {
                MiniPromptController.instance.TaskHolding();
            }
        }
    }

    public void ResetTestState()
    {
        isTaskStarted = false;
        isTestStarted = false;

        DisableSuggestion();

        GenerateRandomList();

        StopAllCoroutines();

        StopAllPlaying();

        userSelectObjectNames.Clear();
        userSelectPositions.Clear();

        playingSourceObjectNames.Clear();
        playingSourceObjectPositions.Clear();

        playingClipNames.Clear();

        otherPlayingClipNamesList.Clear();
        otherPlayingSourceObjectNamesList.Clear();
        otherPlayingSourceObjectPositionsList.Clear();


        selectingDurations.Clear();
        currentUserPositions.Clear();
        currentUserRotations.Clear();
        userPositionsRecord.Clear();
        userRotationsRecord.Clear();

        lastTimestamp = null;

        currentPlayingClip = "";

        currentAttempt = 0;
        testStartDate = null;
        taskCoundownCoroutine = null;
        trackingCoroutine = null;
        testPrepareCoundownCoroutine = null;
    }

    public IEnumerator TrackUserMovement()
    {
        while (isTestStarted)
        {
            currentUserPositions.Add(centerEyeAnchor.transform.localPosition);
            currentUserRotations.Add(playerObj.transform.localRotation.eulerAngles);
            yield return new WaitForSeconds(trackingScale);
        }
    }

    private void StopAllPlaying()
    {
        for (int i = 0; i < audioSourceList.Count; i++)
        {
            MusicPlayer myMusicPlayer = audioSourceList[i].GetComponentInChildren<MusicPlayer>();
            myMusicPlayer.StopPlaying();
        }
    }
}

// Shuffle a List: https://stackoverflow.com/questions/273313/randomize-a-listt
public static class ThreadSafeRandom
{
    [ThreadStatic] private static System.Random Local;

    public static System.Random ThisThreadsRandom
    {
        get { return Local ?? (Local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
    }
}
static class MyExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}