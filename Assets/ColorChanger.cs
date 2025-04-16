using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorChanger : MonoBehaviour
{
    private Material mat;
    
    [TabGroup("Color 1")]
    [SerializeField]
    GameObject redOne;
    
    [TabGroup("Color 1")]
    [SerializeField]
    GameObject greenOne;
    
    [TabGroup("Color 1")]
    [SerializeField]
    GameObject blueOne;
    
    [TabGroup("Color 2")]
    [SerializeField]
    GameObject redTwo;
    
    [TabGroup("Color 2")]
    [SerializeField]
    GameObject greenTwo;
    
    [TabGroup("Color 2")]
    [SerializeField]
    GameObject blueTwo;
    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        Vector4 colorOne;
        Vector4 colorTwo;
        
        colorOne = new Vector4(redOne.GetComponentInChildren<Slider>().value, 
                               greenOne.GetComponentInChildren<Slider>().value, 
                               blueOne.GetComponentInChildren<Slider>().value, 1);
        
        colorTwo = new Vector4(redTwo.GetComponentInChildren<Slider>().value, 
                               greenTwo.GetComponentInChildren<Slider>().value, 
                               blueTwo.GetComponentInChildren<Slider>().value, 1);
        
        mat.SetVector("_ColorOne", colorOne);
        mat.SetVector("_ColorTwo", colorTwo);
    }

}
