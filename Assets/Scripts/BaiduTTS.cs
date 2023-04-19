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
using UnityEngine.Events;

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
    private string Speak = "�ҵ����ֽ���Ԭ��������һ��22��Ĵ�ѧ����ϲ����������rap";
    private const string grant_Type = "client_credentials";
    //�ٶ�appkey
    private string client_ID = "";
    //�ٶ�Secret Key
    private string client_Secret = "";
    //��ȡ�ٶ����Ƶ�url
    private const string getTokenAPIPath = "https://aip.baidubce.com/oauth/2.0/token?";
    #endregion

    bool bTokenReady { get { return tok!=null; } }
    [HideInInspector]
    public UnityEvent<byte[]> OnGotSpeech = new UnityEvent<byte[]>();

    private void Awake()
    {
        var data = File.ReadAllLines("./Assets/BaiduTTS_Key.txt");
        client_ID = data[0];
        client_Secret = data[1];
        StartCoroutine(GetToken(getTokenAPIPath));
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
    /// �ϴ�������
    /// </summary>
    /// <param name="url">URL.</param>
    private IEnumerator Loading(string url)
    {
        if(!bTokenReady) 
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
        //�ı�����
        StringToEncodingUTF8(Speak);

        StartCoroutine(Loading(url));
    }
}