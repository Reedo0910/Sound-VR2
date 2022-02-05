using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniPromptController : MonoBehaviour
{
    public static MiniPromptController instance;

    private GameObject promptBoxObj = null;
    private GameObject promptTextObj = null;
    private Text promptText = null;

    private GameObject suggestionBoxObj = null;
    private GameObject suggestionTextObj = null;
    private Text suggestionText = null;

    float currentPanelFadedCountdownVal = 0f;

    IEnumerator taskCompletedCoroutine = null;
    IEnumerator conditionCompletedCoroutine = null;

    [SerializeField]
    private string suggestionTextLeft = "If you cannot locate the sound, press X / Y to skip";
    [SerializeField]
    private string suggestionTextRight = "If you cannot locate the sound, press A / B to skip";

    [SerializeField]
    private string highlightingText = "Clue: The flashing objects are interactable in the test.";
    [SerializeField]
    private string waitingText = "Please wait for the moderator to start a task...";
    [SerializeField]
    private string completingText = "Task Complete";
    [SerializeField]
    private string takeoffText = "Please kindly take off the VR headset.";

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

    // Start is called before the first frame update
    void Start()
    {
        promptBoxObj = GameObject.Find("MiniPromptPanel");
        promptTextObj = GameObject.Find("MiniPromptPanel/PromptText");
      
        promptText = promptTextObj.GetComponent<Text>();

        suggestionBoxObj = GameObject.Find("SuggestionPanel");
        suggestionTextObj = GameObject.Find("SuggestionPanel/SuggestionText");
        suggestionText = suggestionTextObj.GetComponent<Text>();

        suggestionBoxObj.SetActive(false);

        TaskHolding();
        HideTaskPrompt();
    }

    public void HighlightingPreTask()
    {
        suggestionBoxObj.SetActive(true);
        suggestionText.text = highlightingText;
    }

    public void TaskHolding()
    {
        suggestionBoxObj.SetActive(true);
        suggestionText.text = waitingText;
    }

    public void TestPreparing(float countdown)
    {
        suggestionBoxObj.SetActive(true);
        suggestionText.text = "Ready to begin in " + countdown + " s";
    }

    public void TaskStarting(float countdown)
    {
        suggestionBoxObj.SetActive(true);
        suggestionText.text = "Task starts in " + countdown + " s";
    }

    public void TaskStarted()
    {
        suggestionBoxObj.SetActive(false);
    }

    public void ShowTaskPrompt(string indicatorText)
    {
        promptBoxObj.SetActive(true);
        promptText.text = "Task:\n" + indicatorText;

        suggestionBoxObj.SetActive(false);
    }

    public void ShowSuggestionText()
    {
        suggestionBoxObj.SetActive(true);
        if ((AppManager.instance.activeController == OVRInput.Controller.Touch && !AppManager.instance.isRightHanded) || AppManager.instance.activeController == OVRInput.Controller.LTouch)
        {
            suggestionText.text = suggestionTextLeft;
        }
        else
        {
            suggestionText.text = suggestionTextRight;
        }
    }

    public void HideSuggestionText()
    {
        suggestionBoxObj.SetActive(false);
        suggestionText.text = "";
    }

    public void HideTaskPrompt()
    {
        promptBoxObj.SetActive(false);
        promptText.text = "";
    }

    public void TaskCompleted(bool isTaskEnd = false)
    {
        suggestionText.text = completingText;
        suggestionBoxObj.SetActive(true);
        if (isTaskEnd)
        {
            taskCompletedCoroutine = PanelFadedCountdown(HeadsetTakeOff);
        }
        else
        {
            taskCompletedCoroutine = PanelFadedCountdown(TaskHolding);
        }

        StartCoroutine(taskCompletedCoroutine);
    }

    public void HeadsetTakeOff()
    {
        suggestionText.text = takeoffText;
        suggestionBoxObj.SetActive(true);

        conditionCompletedCoroutine = PanelFadedCountdown(TaskHolding, 15);

        StartCoroutine(conditionCompletedCoroutine);
    }

    public IEnumerator PanelFadedCountdown(Action myMethod, float panelFadedCountdownVal = 5)
    {
        currentPanelFadedCountdownVal = panelFadedCountdownVal;
        while (currentPanelFadedCountdownVal > 0)
        {
            yield return new WaitForSeconds(1.0f);
            currentPanelFadedCountdownVal--;
        }
        myMethod();
    }
}
