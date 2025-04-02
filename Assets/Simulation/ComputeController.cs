using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RawImage))]
public class ComputeController : MonoBehaviour
{
    [BoxGroup("Agents")]
    [TitleGroup("Agents/Spawning")]
    public int numAgents = 100;


    [TitleGroup("Agents/Spawning")]
    [Range(0,1)]
    public float normalizedSpawnRadius = 0.25f;


    [TitleGroup("Agents/Movement")]
    public float agentSpeed = 1.0f;


    [FoldoutGroup("References", false)]
    public ComputeShader shader;


    [FoldoutGroup("References")]
    public RenderTexture texture;

    private ComputeBuffer buffer;
    private int kernelHandle;

    private void Awake()
    {
        GetComponent<RawImage>().texture = texture;
    }

    [GUIColor(0.5f,1f,0.5f)]
    [Button]
    public void ResetSimulation()
    {
        kernelHandle = shader.FindKernel("CSMain");

        texture.Release();
        texture.Create();
        shader.SetTexture(kernelHandle, "Result", texture);
        shader.SetFloats("MaxBounds", new float[] { texture.width, texture.height, 0, 0 });


        Agent[] data = new Agent[numAgents];
        for (int i = 0; i < numAgents; i++)
        {
            Vector2 position = Random.insideUnitCircle * normalizedSpawnRadius * texture.height / 2f + new Vector2(texture.width / 2f, texture.height / 2f);
            Vector2 forward = Random.insideUnitCircle.normalized;
            data[i] = new Agent(position, forward);
        }
        
        buffer?.Release();
        buffer = new ComputeBuffer(data.Length, Agent.sizeOf);
        buffer.SetData(data);
        shader.SetBuffer(kernelHandle, "Agents", buffer);

        shader.Dispatch(kernelHandle, buffer.count / 32, 1, 1);

        Debug.Log("Dispatched!");
    }

    private void Start()
    {
        ResetSimulation();
    }

    public void Update()
    {
        if (!Application.isPlaying)
            return;

        shader.SetFloat("AgentSpeed", agentSpeed * Time.deltaTime);
        shader.SetFloat("Time", agentSpeed * Time.time);
        shader.Dispatch(kernelHandle, buffer.count / 32, 1, 1);
    }

}