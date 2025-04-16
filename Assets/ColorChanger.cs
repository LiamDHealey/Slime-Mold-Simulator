using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

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
        
        colorOne = new Vector4(float.Parse(redOne.GetComponent<TMP_InputField>().text) / 255, 
                               float.Parse(greenOne.GetComponent<TMP_InputField>().text) / 255, 
                               float.Parse(blueOne.GetComponent<TMP_InputField>().text) / 255, 1);
        
        colorTwo = new Vector4(float.Parse(redTwo.GetComponent<TMP_InputField>().text) / 255, 
                               float.Parse(greenTwo.GetComponent<TMP_InputField>().text) / 255, 
                               float.Parse(blueTwo.GetComponent<TMP_InputField>().text) / 255, 1);
        
        mat.SetVector("_ColorOne", colorOne);
        mat.SetVector("_ColorTwo", colorTwo);
    }

}
