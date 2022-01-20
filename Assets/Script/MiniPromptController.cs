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

    //[SerializeField]
    //private string suggestionTextLeft = "If you cannot locate the sound, press X or Y button to skip";
    //[SerializeField]
    //private string suggestionTextRight = "If you cannot locate the sound, press A or B button to skip";

    [SerializeField]
    private string indicationText = "Please point out the sounding object";

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

        promptBoxObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowSuggestionText()
    {
        promptBoxObj.SetActive(true);
        promptText.text = indicationText;
        //if ((AppManager.instance.activeController == OVRInput.Controller.Touch && !AppManager.instance.isRightHanded) || AppManager.instance.activeController == OVRInput.Controller.LTouch)
        //{
        //    promptText.text = suggestionTextLeft;
        //}
        //else
        //{
        //    promptText.text = suggestionTextRight;
        //}
    }

    public void HideSuggestionText()
    {
        promptBoxObj.SetActive(false);
        promptText.text = "";
    }
}
