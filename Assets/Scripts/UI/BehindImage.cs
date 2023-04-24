using UnityEngine;
using UnityEngine.UI;

public class BehindImage : MonoBehaviour
{
    RawImage rawImage;
    Texture2D texture;
    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        texture = new Texture2D(200, 200);
        rawImage.texture = texture;
        ConfigController ccontroller = GameObject.FindWithTag("Config").GetComponent<ConfigController>();
        texture.LoadImage(ccontroller.CurImage);
    }
}
