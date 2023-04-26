using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class MainController : MonoBehaviour
{
    ChatGPT chatGPT = null;
    BaiduTTS baiduTTS = null;
    RemoteFace remoteFace = null;
    [SerializeField]
    private Transform textInputT;
    TextInput textInput = null;
    bool isProcessing = false;

    [SerializeField]
    Transform targetVideoUI;
    VideoPlayer vPlayer;
    const string VIDEO_PATH = "Assets/Resources/Video/test";
    int videoCnt = 0;
    string targetVideoName { get { return VIDEO_PATH + videoCnt + ".mp4"; } }

    private void Awake()
    {
        chatGPT = GetComponent<ChatGPT>();
        baiduTTS = GetComponent<BaiduTTS>();
        textInput = textInputT.GetComponent<TextInput>();
        remoteFace = GetComponent<RemoteFace>();
        vPlayer = targetVideoUI.GetComponent<VideoPlayer>();

        textInput.OnEndEdit.AddListener(UserInput);
        chatGPT.OnGotAns.AddListener(TTS);
        baiduTTS.OnGotSpeech.AddListener(SpeechToVideo);
        remoteFace.OnGotVideo.AddListener(PlayVideo);
    }

    IEnumerator WaitForEndOfVideo()
    {
        yield return new WaitWhile(() => vPlayer.isPlaying);
        isProcessing = false;
    }

    private string OptimizeString(string str)
    {
        return str.Replace("\n", " ").Replace('.', ' ');
    }

    #region CallBacks
    private void UserInput(string text)
    {
        if (isProcessing)
        {
            Debug.LogError("Task is running, do not input");
            return;
        }else if(text==string.Empty)
        {
            Debug.LogWarning("ChatGPT got a question of empty string!");
            return;
        }
        isProcessing = true;
        chatGPT.StartProcess(text);
        Debug.LogFormat("InputText: {0}", text);
    }

    private void TTS(string str)
    {
        string opStr = OptimizeString(str);
        if(opStr==string.Empty)
        {
            Debug.LogWarning("TTS got input of empty string!");
            return;
        }
#if UNITY_EDITOR
        File.AppendAllText("./Assets/Logs/chatgpt.txt", "\n---------------\nold:" + str + "\nnew:" + opStr);
#endif
        baiduTTS.TextToSpeech(opStr);
    }

    private void SpeechToVideo(byte[] audio)
    {
        remoteFace.GetFaceData(audio);
    }

    private void PlayVideo(byte[] video)
    {
        File.WriteAllBytes(targetVideoName, video);
        vPlayer.url = targetVideoName;
        videoCnt++;
        vPlayer.Play();
        StartCoroutine(WaitForEndOfVideo());
    }
    #endregion
}
