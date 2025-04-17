using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundColorPicker : MonoBehaviour
{
    public Slider red;
    public Slider green;
    public Slider blue;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponentInParent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        cam.backgroundColor = new Color(red.value, green.value, blue.value);
    }
}
