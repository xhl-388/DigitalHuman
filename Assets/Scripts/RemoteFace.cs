using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using LitJson;
using UnityEngine.Video;
using UnityEngine.Events;

public class RemoteFace : MonoBehaviour
{
    string REMOTE_NAME;
    byte[] targetImage;
    [HideInInspector]
    public UnityEvent<byte[]> OnGotVideo = new UnityEvent<byte[]>();

    private void Awake()
    {
        REMOTE_NAME = File.ReadAllLines("./Assets/remote_host.txt")[0];
        targetImage = File.ReadAllBytes("./Assets/Resources/happy.png");
    }

    IEnumerator _GetFaceData(byte[] wav)
    {
        Hashtable hashtable = new Hashtable();
        hashtable.Add("wav", Convert.ToBase64String(wav));
        hashtable.Add("pic", Convert.ToBase64String(targetImage));
        string data = JsonMapper.ToJson(hashtable);

        Debug.Log(data);

        using(UnityWebRequest request = new UnityWebRequest(REMOTE_NAME,"POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
            request.downloadHandler = new DownloadHandlerBuffer();

            request.timeout = 3600;
            request.useHttpContinue = false;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string mp4Str = (string)JsonMapper.ToObject(request.downloadHandler.text)["mp4"];
                byte[] mp4Data = Convert.FromBase64String(mp4Str);
                OnGotVideo?.Invoke(mp4Data);
            }else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }

    public void GetFaceData(byte[] wav)
    {
        StartCoroutine(_GetFaceData(wav));
    }
}
