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
    private string aue = "6";
    //�û�Ψһ��ʶ�����ｨ��ʹ�û��� MAC ��ַ�� IMEI ��
    private string cuid = "unity";      //������Ĺ���

    //�ϴ����ݵ�url��
    private string url;
    //����Ҫת����������Ϣ�ı�
    private string Speak = "��ǰ�αؾ�˯�������Ի᳤��";
    private const string grant_Type = "client_credentials";
    //�ٶ�appkey
    private const string client_ID = "FqgX7zARn2AiCDRYBNMG0B4E";
    //�ٶ�Secret Key
    private const string client_Secret = "8cw3MgENUX5cd8RTmwWLKGBjc6DAQoPO";
    //��ȡ�ٶ����Ƶ�url
    private const string getTokenAPIPath = "https://aip.baidubce.com/oauth/2.0/token?";
    #endregion
    //����
    public InputField inputField;
    AudioSource aud;

    bool bTokenReady { get { return tok!=null; } }
    bool bUsed = false;

    string wavOutputPath = "./test.wav";

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
            + "&aue=" + aue
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
                aud.clip = WavUtility.ToAudioClip(loadingAudio.bytes);
                Debug.Log(string.Format("Channels:{0}", aud.clip.channels));
                // �����Ƶ�ļ�
                using (WaveFileWriter writer =
                    new WaveFileWriter(wavOutputPath, new WaveFormat(aud.clip.frequency, aud.clip.channels)))
                {
                    writer.Write(loadingAudio.bytes, 0, loadingAudio.bytes.Length);
                }
                aud.Play();    
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
}