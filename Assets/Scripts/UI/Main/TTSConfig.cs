using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TTSConfig : MonoBehaviour
{
    readonly static Dictionary<string, int> SPEAKER_DICT = new Dictionary<string, int>()
    {
        {"xiaoyu", 1},
        {"xiaomei",0 },
        {"xiaoyao",3 },
        {"yaya",4 },
        {"xiaoyao(2)",5003 },
        {"xiaolu",5118 },
        {"bowen",106 },
        {"xiaotong",110 },
        {"xiaomeng",111 },
        {"miduo",103 },
        {"xiaojiao",5 }
    };
    BaiduTTS baiduTTS;
    [SerializeField] Slider sliderSpd;
    [SerializeField] Slider sliderPit;
    [SerializeField] Slider sliderVol;
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] Button button;
    private void Awake()
    {
        baiduTTS = GameObject.FindWithTag("GameController").GetComponent<BaiduTTS>();
        sliderSpd.onValueChanged.AddListener(OnSpeedChanged);
        sliderPit.onValueChanged.AddListener(OnPitchChanged);
        sliderVol.onValueChanged.AddListener(OnVolumeChanged);
        dropdown.onValueChanged.AddListener(OnSpeakerChanged);
        button.onClick.AddListener(OnSaveTTSConfig);
    }
    public void OnSpeedChanged(float val)
    {
        int ival = (int)val;
        baiduTTS.ChangeConfig("spd", ival);
    }

    public void OnPitchChanged(float val)
    {
        int ival = (int)val;
        baiduTTS.ChangeConfig("pit", ival);
    }
    public void OnVolumeChanged(float val)
    {
        int ival = (int)val;
        baiduTTS.ChangeConfig("vol", ival);
    }
    public void OnSpeakerChanged(int id)
    {
        var option = dropdown.options[id];
        Debug.Log(option.text);
        baiduTTS.ChangeConfig("per", SPEAKER_DICT[option.text]);
    }
    public void OnSaveTTSConfig()
    {
        baiduTTS.SaveTTSConfig();
    }
}
