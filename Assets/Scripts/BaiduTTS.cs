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
/// ����ת��������������ת��������
/// </summary>
public class BaiduTTS : MonoBehaviour
{
    //���� 0-9 5Ϊ������
    public string spd = "5";
    //���� 0-9 5Ϊ�����
    public string pit = "5";
    //���� 0-9 5Ϊ������
    public string vol = "5";
    //���� 0-4 ������ѡ��, 0ΪŮ����1Ϊ������3Ϊ��кϳ�-����ң��4Ϊ��кϳ�-��ѾѾ��Ĭ��Ϊ��ͨŮ��
    public string per = "103";

    #region �ֶΡ�����
    private string tex;
    private string lan = "zh";
    private string tok;
    private string ctp = "1";
    //�û�Ψһ��ʶ�����ｨ��ʹ�û��� MAC ��ַ�� IMEI ��
    private string cuid = "unity";      //������Ĺ���

    //�ϴ����ݵ�url��
    private string url;
    //����Ҫת����������Ϣ�ı�
    private string Speak = "�Բۣ�������";
    private string grant_Type = "client_credentials";
    //�ٶ�appkey
    private string client_ID = "FqgX7zARn2AiCDRYBNMG0B4E";
    //�ٶ�Secret Key
    private string client_Secret = "8cw3MgENUX5cd8RTmwWLKGBjc6DAQoPO";
    //��ȡ�ٶ����Ƶ�url
    private string getTokenAPIPath = "https://aip.baidubce.com/oauth/2.0/token?";
    #endregion
    //����
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
    /// ������Ҫ˵�Ļ����б���
    /// </summary>
    /// <returns>The to encoding UT f8.</returns>
    /// <param name="str">String.��������˵�Ļ�</param>
    private void StringToEncodingUTF8(string str)
    {

        byte[] tempByte = Encoding.UTF8.GetBytes(str);
        for (int i = 0; i < tempByte.Length; i++)
        {
            //UrlEncode�������
            tex += (@"%" + Convert.ToString(tempByte[i], 16));

        }
        //ƴ���ϴ���url
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
    /// ��ȡ�ٶ��û����ƣ������޷�ʹ��API
    /// </summary>
    /// <param name="url">��ȡ��url</param>
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
    /// �ϴ�������
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
                //���ظ���Ƶ /* PC����Ҫ��MP3��ʽת�룬�ֻ�����ʹ��MP3*/
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

    //Button��Ӧ�¼�
    public void StartStringToAudio()
    {
        tex = "";
        Speak = inputField.text;
        Debug.Log(Speak);
        //�ı�����
        StringToEncodingUTF8(Speak);
        //Debug.Log ("�����õ�����Ϣ��"+tex);

        StartCoroutine(Loading(url));
    }   //MP3 --- wav

    private void TestTTS(string input)
    {
        tex = "";
        Speak = input;
        Debug.Log(Speak);
        //�ı�����
        StringToEncodingUTF8(Speak);

        StartCoroutine(Loading(url));
    }

    public static AudioClip FromMp3Data(byte[] data)
    {
        //�������ݽ�����
        MemoryStream mp3stream = new MemoryStream(data);
        //���е�����ת��ΪWAV��ʽ
        Mp3FileReader mp3audio = new Mp3FileReader(mp3stream);
        WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(mp3audio);
        //ת��WAV����
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
    // �����ֽ�ת��Ϊһ��������Χ��-1��1
    static float bytesToFloat(byte firstByte, byte secondByte)
    {
        //�����ֽ�ת��Ϊһ����(С���ֽ���)
        short s = (short)((secondByte << 8) | firstByte);
        //����Χ��-1��1
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

    // ����
    public float[] LeftChannel { get; internal set; }
    public float[] RightChannel { get; internal set; }
    public int ChannelCount { get; internal set; }
    public int SampleCount { get; internal set; }
    public int Frequency { get; internal set; }    /// <summary>
                                                   /// �Զ���Wav��ʽ
                                                   /// </summary>
                                                   /// <param name="wav">Wav.</param>
    public WAV(byte[] wav)
    {

        //ȷ����������������
        ChannelCount = wav[22];     // 23(99.999%)������

        //�õ���Ƶ��
        Frequency = bytesToInt(wav, 24);

        int pos = 12;   //��һ���ӿ�ID��12-16

        // ��������,ֱ���ҵ����ݿ� (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
        while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
        {
            pos += 4;
            int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
            pos += 4 + chunkSize;
        }
        pos += 8;

        //��λʵ��������ʼ������.
        SampleCount = (wav.Length - pos) / 2;     // 2�ֽ�/���� (16 bit ������)
        if (ChannelCount == 2) SampleCount /= 2;        // 4�ֽ�/���� (16 bit ����)

        //�����ڴ�(�ҽ�null���ֻ�е���������)
        LeftChannel = new float[SampleCount];
        if (ChannelCount == 2) RightChannel = new float[SampleCount];
        else RightChannel = null;

        //д��˫����
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