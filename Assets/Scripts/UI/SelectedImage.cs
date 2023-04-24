using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectedImage : MonoBehaviour
{
    ConfigController ccontroller;
    RawImage image;

    private void Awake()
    {
        image = GetComponent<RawImage>();
    }

    private void Start()
    {
        ccontroller = GameObject.FindWithTag("Config").GetComponent<ConfigController>();
        ccontroller.SetSelectImage(image);
    }
}
