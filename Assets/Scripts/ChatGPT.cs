using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.Events;

public class ChatGPT : MonoBehaviour
{
    private const string URL = "https://api.openai.com/v1/completions";
    private string API_KEY = "";

    [HideInInspector]
    public UnityEvent<string> OnGotAns = new UnityEvent<string>();

    private void Awake()
    {
        API_KEY = File.ReadAllLines("./Assets/ChatGPT_Key.txt")[0];
    }

    IEnumerator PostRequest(string question)
    {
        // 创建要上传的json文本
        Hashtable tab = new Hashtable();
        tab.Add("model", "text-davinci-003");
        tab.Add("prompt", question);
        tab.Add("max_tokens", 4000);
        string uploadStr = JsonMapper.ToJson(tab);
        Debug.Log(uploadStr);

        // 创建POST请求
        using (UnityWebRequest webRequest = new UnityWebRequest(URL, "POST"))
        {
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
                string answer = (string)JsonMapper.ToObject(responseText)["choices"][0]["text"];
                Debug.Log("Ans: " + answer);

                OnGotAns?.Invoke(answer);
            }
            else
            {
                Debug.LogError("Error: " + webRequest.error);
            }
        }
    }

    public void StartProcess(string str)
    {
        StartCoroutine(PostRequest(str));
    }
}
