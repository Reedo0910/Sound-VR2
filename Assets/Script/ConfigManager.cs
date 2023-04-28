using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IniParser;
using IniParser.Model;
using System.IO;

public class ConfigManager : MonoBehaviour
{
    public static ConfigManager instance;

    [HideInInspector]
    public string useLeftHandStr;
    [HideInInspector]
    public string useMuteBackgroundMusicStr;
    [HideInInspector]
    public string useTokenStr;
    [HideInInspector]
    public string useDevStr;
    [HideInInspector]
    public string useDemoStr;
    [HideInInspector]
    public string useDemoDefaultStr;
    [HideInInspector]
    public string useLoopStr;

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

        ReadConfigFile();
    }

    private void ReadConfigFile()
    {
        //  Read from the configuration file
        string configFile = Application.dataPath + "/config.ini";
        // Packing the "xxx_data" directory seems to have not read file permissions inside
        // So for the package, you need to put the configuration file config.ini in the same directory in EXE.
#if !UNITY_EDITOR
        configFile = System.Environment.CurrentDirectory + "/config.ini";
#endif
        if (File.Exists(configFile))
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(configFile);

            // AppManager config
            useLeftHandStr = data["AppManager"]["is_left_handed"];
            useMuteBackgroundMusicStr = data["AppManager"]["is_background_music_muted"];
            useDemoStr = data["AppManager"]["is_demo_only"];
            useDemoDefaultStr = data["AppManager"]["demo_default"];
            useLoopStr = data["AppManager"]["is_loop"];

            // SocketIO config
            useTokenStr = data["SocketIO"]["token"];
            useDevStr = data["SocketIO"]["is_dev"];
        }
    }
}
