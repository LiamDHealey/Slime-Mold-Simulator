using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

[ExecuteAlways]
[RequireComponent(typeof(RawImage))]
public class Controller : MonoBehaviour
{
    public ComputeShader shader;
    public Texture texture;

    public void Dispatch()
    {
        int kernelHandle = shader.FindKernel("CSMain");

        shader.SetTexture(kernelHandle, "Result", texture);
        shader.Dispatch(kernelHandle, texture.width / 8, texture.height / 8, 1);
        
        GetComponent<RawImage>().texture = texture;

        Debug.Log("Dispatched!");
    }

}
[CustomEditor(typeof(Controller))]
class DecalMeshHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Dispatch"))
            (target as Controller).Dispatch();
    }
}