using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class ChatGPT : MonoBehaviour
{
    private const string URL = "https://api.openai.com/v1/completions";
    private string API_KEY = "";
    private BaiduTTS baiduTTS = null;

    private void Awake()
    {
        API_KEY = File.ReadAllLines("./Assets/ChatGPT_Key.txt")[0];

        baiduTTS = GetComponent<BaiduTTS>();
    }
    private void Start()
    {
        StartCoroutine(PostRequest("Say this is a test"));
    }
    IEnumerator PostRequest(string question)
    {
        // 创建要上传的json文本
        Hashtable tab = new Hashtable();
        tab.Add("model", "text-davinci-003");
        tab.Add("prompt", "我该怎么赚钱？");
        tab.Add("max_tokens", 4000);
        string uploadStr = JsonMapper.ToJson(tab);
        Debug.Log(uploadStr);

        // 创建POST请求
        UnityWebRequest webRequest = new UnityWebRequest(URL,"POST");
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Authorization", "Bearer " + API_KEY);
        webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(uploadStr));
        webRequest.downloadHandler = new DownloadHandlerBuffer();

        // 发送请求
        yield return webRequest.SendWebRequest();

        // 处理响应
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            string responseText = webRequest.downloadHandler.text;
            string answer =(string)JsonMapper.ToObject(responseText)["choices"][0]["text"];
            Debug.Log("Response: " + responseText);
            Debug.Log("Ans: " + answer);

            baiduTTS.TestTTS(answer);
        }
        else
        {
            Debug.LogError("Error: " + webRequest.error);
        }
    }

}
