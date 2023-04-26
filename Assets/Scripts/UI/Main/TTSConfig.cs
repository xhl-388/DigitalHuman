using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
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
    ConfigController ccontroller;
    private void Awake()
    {
        baiduTTS = GameObject.FindWithTag("GameController").GetComponent<BaiduTTS>();
        sliderSpd.onValueChanged.AddListener(OnSpeedChanged);
        sliderPit.onValueChanged.AddListener(OnPitchChanged);
        sliderVol.onValueChanged.AddListener(OnVolumeChanged);
        dropdown.onValueChanged.AddListener(OnSpeakerChanged);
        button.onClick.AddListener(OnSaveTTSConfig);
    }

    private void Start()
    {
        ccontroller = GameObject.FindWithTag("Config").GetComponent<ConfigController>();
        ImageConfig ic = ccontroller.CurImageConfig;
        sliderSpd.value = Convert.ToInt32(ic.spd);
        sliderPit.value = Convert.ToInt32(ic.pit);
        sliderVol.value = Convert.ToInt32(ic.vol);
        string name = "";
        foreach(var item in SPEAKER_DICT)
        {
            if(item.Value == Convert.ToInt32(ic.per))
            {
                name = item.Key;
                break;
            }
        }
        Assert.AreNotEqual(name,"");
        for(int i =0;i<dropdown.options.Count;i++)
        {
            if (dropdown.options[i].text == name)
            {
                dropdown.value = i;
                break;
            }
        }
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
