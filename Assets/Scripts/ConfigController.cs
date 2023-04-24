using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using LitJson;
using System.Text;

public struct ImageConfig
{
    //语速 0-9 5为中语速
    public string spd;
    //音调 0-9 5为中语调
    public string pit;
    //音量 0-9 5为中音量
    public string vol;
    //发音 0-4 发音人选择, 0为女声，1为男声，3为情感合成-度逍遥，4为情感合成-度丫丫，默认为普通女声
    public string per;

    public ImageConfig(string _spd,string _pit,string _vol,string _per)
    {
        spd = _spd;
        pit = _pit;
        vol = _vol;
        per = _per;
    }
}

public class ConfigController : MonoBehaviour
{
    const string IMAGE_PATH = "./Assets/Images";
    const string CONFIG_PATH = IMAGE_PATH + "/Configs";
    List<byte[]> images = new List<byte[]>();
    List<string> iNames = new List<string>();
    bool firstLoad = true;
    ImageConfig DefaultImageConfig;
    int curIndex = 0;
    RawImage SelectImage;
    Texture2D texture;

    [HideInInspector]
    public byte[] CurImage;
    [HideInInspector]
    public ImageConfig CurImageConfig;
    private void Awake()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Config");
        if(gameObjects.Length > 1 )
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        texture = new Texture2D(200, 200);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void ReadConfig(string path, ref  ImageConfig config)
    {
        byte[] cdata = File.ReadAllBytes(path);
        string sdata = Encoding.UTF8.GetString(cdata);
        JsonData jdata = JsonMapper.ToObject(sdata);
        config = new ImageConfig((string)jdata["spd"], (string)jdata["pit"],
        (string)jdata["vol"], (string)jdata["per"]);
    }

    void WriteConfig(string path, ImageConfig config)
    {
        File.WriteAllText(path, JsonMapper.ToJson(config));
    }

    void OnSceneLoaded(Scene scene,LoadSceneMode mode)
    {
        if(scene.name == "ImageSelect" && firstLoad)
        {
            LoadImages();
            firstLoad = false;
        }else
        {
            // TODO
        }
    }

    void LoadImages()
    {
        DirectoryInfo imageDir = new DirectoryInfo(IMAGE_PATH);
        foreach (FileInfo file in imageDir.GetFiles("*.png"))
        {
            images.Add(File.ReadAllBytes(file.FullName));
            iNames.Add(file.Name);
            // Debug.LogFormat("Load Image File: {0}", file.Name);
        }
        ReadConfig(CONFIG_PATH + "/default.json",ref DefaultImageConfig);
    }

    void RefreshImage()
    {
        CurImage = images[curIndex];
        if(!texture.LoadImage(CurImage))
        {
            Debug.LogError("Image load failed:bytes to texture!");
        }
    }

    public void ChangeIndex(bool isLeft)
    {
        curIndex += (isLeft ? -1 : 1);
        if(curIndex >= images.Count)
            curIndex = 0;
        else if(curIndex < 0)
            curIndex = images.Count - 1;
        RefreshImage();
    }

    public void SetSelectImage(RawImage image)
    {
        SelectImage = image;
        SelectImage.texture = texture;
        RefreshImage();
    }

    public void OnSelectOver()
    {
        string prefix = iNames[curIndex].Split('.')[0];
        string filePath = CONFIG_PATH + "/" + prefix + ".json";
        if(File.Exists(filePath))
        {
            ReadConfig(filePath,ref CurImageConfig);
        }else
        {
            WriteConfig(filePath, DefaultImageConfig);
            CurImageConfig = DefaultImageConfig;
        }
        SceneManager.LoadScene("Main");
    }

    public ImageConfig GetDefaultImageConfig()
    {
        return DefaultImageConfig;
    }

    public void SaveTTSConfig(ImageConfig cfg)
    {
        string prefix = iNames[curIndex].Split('.')[0];
        string filePath = CONFIG_PATH + "/" + prefix + ".json";
        WriteConfig(filePath, cfg);
    }
}
