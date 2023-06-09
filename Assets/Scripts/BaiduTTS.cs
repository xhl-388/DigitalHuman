using LitJson;
using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// 用来转换语音，将文字转成语音。
/// </summary>
public class BaiduTTS : MonoBehaviour
{
    private ImageConfig config;

    #region 字段、属性
    private string tex;
    private string lan = "zh";
    private string tok;
    private string ctp = "1";
    private string aue = "6";
    //用户唯一标识，这里建议使用机器 MAC 地址或 IMEI 码
    private string cuid = "unity";      //待加入的功能

    //上传数据的url，
    private string url;
    //所需要转成语音的信息文本
    private string Speak = "";
    private const string grant_Type = "client_credentials";
    //百度appkey
    private string client_ID = "";
    //百度Secret Key
    private string client_Secret = "";
    //获取百度令牌的url
    private const string getTokenAPIPath = "https://aip.baidubce.com/oauth/2.0/token?";
    #endregion

    bool bTokenReady { get { return tok != null; } }
    ConfigController ccontroller;
    [HideInInspector]
    public UnityEvent<byte[]> OnGotSpeech = new UnityEvent<byte[]>();

    private void Awake()
    {
        var data = File.ReadAllLines("./Assets/BaiduTTS_Key.txt");
        client_ID = data[0];
        client_Secret = data[1];
        StartCoroutine(GetToken(getTokenAPIPath));
        ccontroller = GameObject.FindWithTag("Config").GetComponent<ConfigController>();
        config = ccontroller.CurImageConfig;
    }

    /// <summary>
    /// 将所需要说的话进行编码
    /// </summary>
    /// <returns>The to encoding UT f8.</returns>
    /// <param name="str">String.输入您想说的话</param>
    private void StringToEncodingUTF8(string str)
    {

        byte[] tempByte = Encoding.UTF8.GetBytes(str);
        for (int i = 0; i < tempByte.Length; i++)
        {
            //UrlEncode编码规则
            tex += (@"%" + Convert.ToString(tempByte[i], 16));

        }
        //拼接上传的url
        url = "http://tsn.baidu.com/text2audio?tex=" + tex
            + "&lan=" + lan
            + "&cuid=" + cuid
            + "&ctp=" + ctp
            + "&tok=" + tok
            + "&per=" + config.per
            + "&spd=" + config.spd
            + "&pit=" + config.pit
            + "&aue=" + aue
            + "&vol=" + config.vol + "";
        Debug.Log("Token:" + tok);
        Debug.Log("Person:" + config.per);
    }

    /// <summary>
    /// 获取百度用户令牌，否则无法使用API
    /// </summary>
    /// <param name="url">获取的url</param>
    /// <returns></returns>
    private IEnumerator GetToken(string url)
    {
        WWWForm TokenForm = new WWWForm();
        TokenForm.AddField("grant_type", grant_Type);
        TokenForm.AddField("client_id", client_ID);
        TokenForm.AddField("client_secret", client_Secret);

        using (UnityWebRequest getTW = UnityWebRequest.Post(url, TokenForm))
        {
            getTW.downloadHandler = new DownloadHandlerBuffer();
            yield return getTW.SendWebRequest();
            if (getTW.isDone)
            {
                //Debug.Log (getTW.text);

                if (getTW.error == null)
                {
                    tok = JsonMapper.ToObject(getTW.downloadHandler.text)["access_token"].ToString();
                }
                else
                {
                    Debug.LogError(getTW.error);
                }
            }
        }
    }

    /// <summary>
    /// 上传和下载
    /// </summary>
    /// <param name="url">URL.</param>
    private IEnumerator Loading(string url)
    {
        if (!bTokenReady)
        {
            Debug.LogError("Baidu TTS not ready but you are still accessing it");
            yield break;
        }
        using (UnityWebRequest loadingAudio = UnityWebRequest.Get(url))
        {
            loadingAudio.downloadHandler = new DownloadHandlerBuffer();
            yield return loadingAudio.SendWebRequest();
            if (loadingAudio.error == null)
            {
                if (loadingAudio.isDone)
                {
                    OnGotSpeech?.Invoke(loadingAudio.downloadHandler.data);
                }
                else
                {
                    Debug.LogError(loadingAudio.error);
                }
            }
        }
    }

    public void TextToSpeech(string input)
    {
        tex = "";
        Speak = input;
        Debug.Log(Speak);
        //文本编码
        StringToEncodingUTF8(Speak);

        StartCoroutine(Loading(url));
    }

    public void ChangeConfig(string prop, int value)
    {
        switch (prop)
        {
            case "spd":
                config.spd = value.ToString();
                break;
            case "pit":
                config.pit = value.ToString();
                break;
            case "vol":
                config.vol = value.ToString();
                break;
            case "per":
                config.per = value.ToString();
                break;
            default:
                Debug.LogError("Invalid input of string prop");
                break;
        }
        Debug.LogFormat("Prop {0} changed to {1}", prop, value);
    }

    public void SaveTTSConfig()
    {
        ccontroller.SaveTTSConfig(config);
    }
}