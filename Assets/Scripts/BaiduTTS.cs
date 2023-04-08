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
    //用户唯一标识，这里建议使用机器 MAC 地址或 IMEI 码
    private string cuid = "unity";      //待加入的功能

    //上传数据的url，
    private string url;
    //所需要转成语音的信息文本
    private string Speak = "卧槽，冰冰冰";
    private string grant_Type = "client_credentials";
    //百度appkey
    private string client_ID = "FqgX7zARn2AiCDRYBNMG0B4E";
    //百度Secret Key
    private string client_Secret = "8cw3MgENUX5cd8RTmwWLKGBjc6DAQoPO";
    //获取百度令牌的url
    private string getTokenAPIPath = "https://aip.baidubce.com/oauth/2.0/token?";
    #endregion
    //测试
    public InputField inputField;
    AudioSource aud;

    bool bTokenReady { get { return tok!=null; } }
    bool bUsed = false;

    private void Awake()
    {
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
        if (bTokenReady && !bUsed)
        {
            bUsed = true;
            TestTTS(Speak);
        }
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

        WWW getTW = new WWW(url, TokenForm);
        yield return getTW;
        if (getTW.isDone)
        {
            //Debug.Log (getTW.text);

            if (getTW.error == null)
            {
                tok = JsonMapper.ToObject(getTW.text)["access_token"].ToString();

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
        WWW loadingAudio = new WWW(url);
        yield return loadingAudio;
        if (loadingAudio.error == null)
        {
            if (loadingAudio.isDone)
            {
                //下载该音频 /* PC下需要对MP3格式转码，手机端则使用MP3*/
#if UNITY_EDITOR_WIN
                aud.clip = FromMp3Data(loadingAudio.bytes);
                aud.Play();
#elif UNITY_STANDALONE_WIN
                    aud.clip = FromMp3Data(loadingAudio.bytes);
                    aud.Play ();
#elif UNITY_ANDROID
                    aud.clip = loadingAudio.GetAudioClip (false,true,AudioType.MPEG);
                    aud.Play ();
#endif          
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

    private void TestTTS(string input)
    {
        tex = "";
        Speak = input;
        Debug.Log(Speak);
        //文本编码
        StringToEncodingUTF8(Speak);

        StartCoroutine(Loading(url));
    }

    public static AudioClip FromMp3Data(byte[] data)
    {
        //加载数据进入流
        MemoryStream mp3stream = new MemoryStream(data);
        //流中的数据转换为WAV格式
        Mp3FileReader mp3audio = new Mp3FileReader(mp3stream);
        WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(mp3audio);
        //转换WAV数据
        WAV wav = new WAV(AudioMemStream(waveStream).ToArray());
        AudioClip audioClip = AudioClip.Create("testSound", wav.SampleCount, 1, wav.Frequency, false);
        audioClip.SetData(wav.LeftChannel, 0);
        return audioClip;
    }

    private static MemoryStream AudioMemStream(WaveStream waveStream)
    {
        MemoryStream outputStream = new MemoryStream();
        using (WaveFileWriter waveFileWriter = new WaveFileWriter(outputStream, waveStream.WaveFormat))
        {
            byte[] bytes = new byte[waveStream.Length];
            waveStream.Position = 0;
            waveStream.Read(bytes, 0, Convert.ToInt32(waveStream.Length));
            waveFileWriter.Write(bytes, 0, bytes.Length);
            waveFileWriter.Flush();
        }
        return outputStream;
    }
}

public class WAV
{
    // 两个字节转换为一个浮动范围在-1到1
    static float bytesToFloat(byte firstByte, byte secondByte)
    {
        //两个字节转换为一个短(小端字节序)
        short s = (short)((secondByte << 8) | firstByte);
        //将范围从-1到1
        return s / 32768.0F;
    }

    static int bytesToInt(byte[] bytes, int offset = 0)
    {
        int value = 0;
        for (int i = 0; i < 4; i++)
        {
            value |= ((int)bytes[offset + i]) << (i * 8);
        }
        return value;
    }

    // 属性
    public float[] LeftChannel { get; internal set; }
    public float[] RightChannel { get; internal set; }
    public int ChannelCount { get; internal set; }
    public int SampleCount { get; internal set; }
    public int Frequency { get; internal set; }    /// <summary>
                                                   /// 自定义Wav格式
                                                   /// </summary>
                                                   /// <param name="wav">Wav.</param>
    public WAV(byte[] wav)
    {

        //确定单声道或立体声
        ChannelCount = wav[22];     // 23(99.999%)往后丢弃

        //得到的频率
        Frequency = bytesToInt(wav, 24);

        int pos = 12;   //第一个子块ID从12-16

        // 继续迭代,直到找到数据块 (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
        while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
        {
            pos += 4;
            int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
            pos += 4 + chunkSize;
        }
        pos += 8;

        //定位实际声音开始的数据.
        SampleCount = (wav.Length - pos) / 2;     // 2字节/采样 (16 bit 单声道)
        if (ChannelCount == 2) SampleCount /= 2;        // 4字节/采样 (16 bit 音响)

        //分配内存(右将null如果只有单声道声音)
        LeftChannel = new float[SampleCount];
        if (ChannelCount == 2) RightChannel = new float[SampleCount];
        else RightChannel = null;

        //写入双数组
        int i = 0;
        while (pos < wav.Length)
        {
            LeftChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
            pos += 2;
            if (ChannelCount == 2)
            {
                RightChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
                pos += 2;
            }
            i++;
        }
    }

}