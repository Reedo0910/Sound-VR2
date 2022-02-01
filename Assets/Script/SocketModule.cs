using System.Collections.Generic;
using UnityEngine;
using System;
using BestHTTP.SocketIO3;
using BestHTTP.SocketIO3.Events;
using System.Linq;

public class SocketModule : MonoBehaviour
{
    [SerializeField]
    private string prodUrl = "https://vrsoundmap2.velascamp.cn";
    [SerializeField]
    private string devUrl = "http://127.0.0.1:8002";
    [SerializeField]
    private bool isDev = false;
    [SerializeField]
    private string token = "";

    [NonSerialized]
    public static SocketModule instance;

    private SocketManager Manager;

    class TestState
    {
        public int state;
    }

    class AttemptState : TestState
    {
        public int attempt;
    }

    class SelectInfo
    {
        public string objectName;
        public string position;
        public double durationTime;
        public double distanceTotal;
        public double rotationPerSec;
        public List<string> userPositionRecord;
        public List<string> userRotationRecord;
    }

    class AnswerInfo
    {
        public string objectName;
        public string position;
        public string clipName;
        public List<string> otherClipNames;
        public List<string> otherObjectNames;
        public List<string> otherObjectPositions;
    }

    class TestInfo : AttemptState
    {
        public List<SelectInfo> select;
        public List<AnswerInfo> ans;
        public double avg_distance;
        public double avg_rotation_per_sec;
        public float cr;
        public double toc;
        public double tdt;
        public DateTime start_date;
        public DateTime end_date;
    }

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
    }

    void OnDestroy()
    {
        if (this.Manager != null)
        {
            // Leaving this sample, close the socket
            this.Manager.Close();
            this.Manager = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Read Config
        if (ConfigManager.instance.useTokenStr != null)
        {
            token = ConfigManager.instance.useTokenStr;
        }
        if (ConfigManager.instance.useDevStr != null)
        {
            isDev = bool.Parse(ConfigManager.instance.useDevStr);
        }

        string url = isDev ? devUrl : prodUrl;

        SocketOptions options = new SocketOptions();
        //options.AutoConnect = false;
        options.Auth = (manager, socket) => new { token = token };
        options.AdditionalQueryParams = new PlatformSupport.Collections.ObjectModel.ObservableDictionary<string, string>();
        options.AdditionalQueryParams.Add("role", "client");

        Manager = new SocketManager(new Uri(url), options);

        // Accessing the root ("/") socket
        var root = Manager.Socket;

        Manager.Socket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        Manager.Socket.On<Error>(SocketIOEventTypes.Error, OnError);
        Manager.Socket.On("disconnect", () => Debug.Log("disconnect!"));

        Manager.Socket.On("inited", () => Debug.Log("inited!"));
        Manager.Socket.On<TestState>("test update", TestUpdateHandler);
        Manager.Socket.On<AttemptState>("test start", TestStartHandler);
        Manager.Socket.On("test stop", TestStopHandler);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnConnected(ConnectResponse resp)
    {
        Debug.Log("[SocketIO] Open received: " + resp.sid);

        TestState initinfo = new TestState
        {
            state = (int)AppManager.instance.myTestState
        };

        Manager.Socket.Emit("init state", initinfo);
    }

    private void TestUpdateHandler(TestState data)
    {
        int testId = data.state;
        if (Enum.IsDefined(typeof(AppManager.TestType), testId))
        {
            AppManager.instance.SetTestState(testId);
            Debug.Log("Switched to Test" + testId);
        }
    }

    private void TestStartHandler(AttemptState data)
    {
        AppManager.instance.StartTest(data.attempt);
    }

    private void TestStopHandler()
    {
        AppManager.instance.StopTest();
    }

    private void OnError(Error error)
    {
        Debug.Log(error);
    }

    public void TestInfoUpdate(DateTime startdate, DateTime enddate)
    {
        List<string> userSelectList = AppManager.instance.userSelectObjectNames.ToList();
        List<string> answerList = AppManager.instance.playingSourceObjectNames.ToList();

        List<double> userDuration = AppManager.instance.selectingDurations.ToList();

        // calculate correct rate
        int matches = 0;

        for (int i = 0; i < userSelectList.Count; i++)
        {
            if (i >= answerList.Count)
            {
                break;
            }
            if (userSelectList[i] == answerList[i] && userDuration[i] >= 0)
            {
                matches++;
            }
        }

        float correctRate = (float)matches / (float)answerList.Count;

        // calculate time of completion
        double timeOfCompletion = enddate.Subtract(startdate).TotalSeconds;

        // caculate total duration time
        double totalDurationTime = 0f;

        List<SelectInfo> selectInfos = new List<SelectInfo>();
        List<AnswerInfo> answerInfos = new List<AnswerInfo>();

        for (int i = 0; i < userSelectList.Count; i++)
        {
            double myDuration = AppManager.instance.selectingDurations[i];
            List<Vector3> myPositionRecord = AppManager.instance.userPositionsRecord[i].positions;
            List<Vector3> myRotationRecord = AppManager.instance.userRotationsRecord[i].rotations;
            SelectInfo selectInfo = new SelectInfo
            {
                objectName = userSelectList[i],
                position = AppManager.instance.userSelectPositions[i].ToString(),
                durationTime = myDuration,
                distanceTotal = GetTotalDistanceOnHorizontal(myPositionRecord),
                rotationPerSec = myDuration > 0 ? GetDiffRotationOnYAxis(myRotationRecord) / myDuration : 0,
                userPositionRecord = NormalizeVector3List(myPositionRecord),
                userRotationRecord = NormalizeVector3List(myRotationRecord)
            };

            selectInfos.Add(selectInfo);

            totalDurationTime += myDuration;
        }

        for (int i = 0; i < answerList.Count; i++)
        {
            AnswerInfo answerInfo = new AnswerInfo
            {
                objectName = answerList[i],
                position = AppManager.instance.playingSourceObjectPositions[i].ToString(),
                clipName = AppManager.instance.playingClipNames[i],
                otherClipNames = AppManager.instance.otherPlayingClipNamesList[i].clipNames,
                otherObjectNames = AppManager.instance.otherPlayingSourceObjectNamesList[i].objectNames,
                otherObjectPositions = NormalizeVector3List(AppManager.instance.otherPlayingSourceObjectPositionsList[i].objectPositions)
            };

            answerInfos.Add(answerInfo);
        }

        double myTotalDistance = 0;
        double myTotalRotationPerSec = 0;

        for (int i = 0; i < selectInfos.Count; i++)
        {
            myTotalDistance += selectInfos[i].distanceTotal;
            myTotalRotationPerSec += selectInfos[i].rotationPerSec;
        }

        double myAvgDistance = myTotalDistance / selectInfos.Count;
        double myAvgRotationPerSec = myTotalRotationPerSec / selectInfos.Count;

        TestInfo testInfo = new TestInfo
        {
            state = (int)AppManager.instance.myTestState,
            attempt = AppManager.instance.currentAttempt,
            select = selectInfos,
            ans = answerInfos,
            avg_distance = myAvgDistance,
            avg_rotation_per_sec = myAvgRotationPerSec,
            cr = correctRate,
            toc = timeOfCompletion,
            tdt = totalDurationTime,
            start_date = startdate,
            end_date = enddate
        };

        Manager.Socket.Emit("test info", testInfo);
    }

    private double GetTotalDistanceOnHorizontal(List<Vector3> positions)
    {
        Vector2 lastPosition = new Vector2(0, 0);
        double total = 0f;
        for (int i = 0; i < positions.Count; i++)
        {
            if (i == 0)
            {
                lastPosition = new Vector2(positions[i].x, positions[i].y);
                continue;
            }

            Vector2 currentPosition = new Vector2(positions[i].x, positions[i].y);

            float distance = Vector2.Distance(lastPosition, currentPosition);
            lastPosition = currentPosition;
            total += distance;
        }
        return total;
    }

    private double GetDiffRotationOnYAxis(List<Vector3> rotations)
    {
        Vector3 startY = new Vector3(0, rotations[0].y, 0);
        Vector3 endY = new Vector3(0, rotations[rotations.Count - 1].y, 0);
        double diff = Quaternion.Angle(Quaternion.Euler(endY), Quaternion.Euler(startY));
        return diff;
    }

    private List<string> NormalizeVector3List(List<Vector3> list)
    {
        List<string> newList = new List<string>();
        for (int i = 0; i < list.Count; i++)
        {
            newList.Add(list[i].ToString());
        }
        return newList;
    }
}