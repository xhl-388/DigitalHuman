using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.Text;
using System;
using UnityEngine.UI;
using System.IO;
using NAudio;
using NAudio.Wave;
using UnityEngine.Networking;

/// <summary>
/// 用来转换语音，将文字转成语音。
/// </summary>
public class BaiduTTS : MonoBehaviour
{
    //语速 0-9 5为中语速
    public string spd = "5";
    //音调 0-9 5为中语调
    public string pit = "5";
    //音量 0-9 5为中音量
    public string vol = "5";
    //发音 0-4 发音人选择, 0为女声，1为男声，3为情感合成-度逍遥，4为情感合成-度丫丫，默认为普通女声
    public string per = "103";

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
    private string Speak = "我的名字叫做袁靖轩，是一名22岁的大学生，喜欢唱、跳、rap";
    private const string grant_Type = "client_credentials";
    //百度appkey
    private string client_ID = "";
    //百度Secret Key
    private string client_Secret = "";
    //获取百度令牌的url
    private const string getTokenAPIPath = "https://aip.baidubce.com/oauth/2.0/token?";
    #endregion
    //测试
    public InputField inputField;
    AudioSource aud;

    bool bTokenReady { get { return tok!=null; } }
    bool bUsed = false;

    string wavOutputPath = "./test.wav";

    private void Awake()
    {
        var data = File.ReadAllLines("./Assets/BaiduTTS_Key.txt");
        client_ID = data[0];
        client_Secret = data[1];

        if (GetComponent<AudioSource>() == null)
        {
            aud = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            aud = gameObject.GetComponent<AudioSource>();
        }
        aud.playOnAwake = false;
        StartCoroutine(GetToken(getTokenAPIPath));

    }

    private void Update()
    {
        // for test
        //if (bTokenReady && !bUsed)
        //{
        //    bUsed = true;
        //    TestTTS(Speak);
        //}
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
            + "&lan=" +lan 
            + "&cuid=" + cuid 
            + "&ctp=" + ctp 
            + "&tok=" + tok 
            + "&per=" + per 
            + "&spd=" + spd 
            + "&pit=" + pit 
            + "&aue=" + aue
            + "&vol=" + vol + "";
        Debug.Log("Token:" + tok);
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

        UnityWebRequest getTW = UnityWebRequest.Post(url, TokenForm);
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

    /// <summary>
    /// 上传和下载
    /// </summary>
    /// <param name="url">URL.</param>
    private IEnumerator Loading(string url)
    {
        UnityWebRequest loadingAudio = UnityWebRequest.Get(url);
        loadingAudio.downloadHandler = new DownloadHandlerBuffer();
        yield return loadingAudio.SendWebRequest();
        if (loadingAudio.error == null)
        {
            if (loadingAudio.isDone)
            {
                aud.clip = WavUtility.ToAudioClip(loadingAudio.downloadHandler.data);
                // 输出音频文件
                using (WaveFileWriter writer =
                    new WaveFileWriter(wavOutputPath, new WaveFormat(aud.clip.frequency, aud.clip.channels)))
                {
                    writer.Write(loadingAudio.downloadHandler.data, 0, loadingAudio.downloadHandler.data.Length);
                }
                aud.Play();    
            }
            else
            {
                Debug.LogError(loadingAudio.error);
            }
        }

    }

    //Button响应事件
    public void StartStringToAudio()
    {
        tex = "";
        Speak = inputField.text;
        Debug.Log(Speak);
        //文本编码
        StringToEncodingUTF8(Speak);
        //Debug.Log ("编码后得到的信息："+tex);

        StartCoroutine(Loading(url));
    }   //MP3 --- wav

    public void TestTTS(string input)
    {
        tex = "";
        Speak = input;
        Debug.Log(Speak);
        //文本编码
        StringToEncodingUTF8(Speak);

        StartCoroutine(Loading(url));
    }
}