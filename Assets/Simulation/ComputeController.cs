using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RawImage), typeof(AspectRatioFitter))]
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

    [TitleGroup("Agents/Movement")]
    public float forwardTurnSpeed = 1.0f;

    [TitleGroup("Agents/Movement")]
    public Sensor[] sensors;



    [BoxGroup("Diffusion")]
    [MinValue(0f)]
    public float decaySpeed = 0.001f;

    [Range(0f, 1f)]
    public float decayPercent= 0.5f;



    [FoldoutGroup("References", false)]
    public ComputeShader agentShader;

    [FoldoutGroup("References")]
    public ComputeShader diffusionShader;

    [FoldoutGroup("References")]
    public RenderTexture texture;




    private ComputeBuffer agentBuffer;
    private ComputeBuffer sensorBuffer;
    private Sensor[] lastSensors = new Sensor[] { };
    private RenderTexture diffusionTexture;
    private int agentKernel;
    private int diffusionKernel;

    private void Awake()
    {
        GetComponent<RawImage>().texture = texture;
        diffusionTexture = new(texture);
    }

    private void Start()
    {
        ResetSimulation();
    }

    public void FixedUpdate()
    {
        if (!Application.isPlaying)
            return;

        if (!sensors.SequenceEqual(lastSensors))
        {
            sensorBuffer?.Release();
            sensorBuffer = new ComputeBuffer(sensors.Length, Sensor.sizeOf);
            sensorBuffer.SetData(sensors);
            agentShader.SetBuffer(agentKernel, "Sensors", sensorBuffer);
            agentShader.SetInt("NumSensors", sensors.Length);
            agentShader.SetFloat("ForwardTurnSpeed", forwardTurnSpeed);
            lastSensors = sensors;
        }

        agentShader.SetFloat("AgentSpeed", agentSpeed);
        agentShader.SetFloat("Time", Time.time);
        agentShader.Dispatch(agentKernel, agentBuffer.count / 32, 1, 1);

        diffusionShader.SetTexture(diffusionKernel, "Input", texture);
        diffusionShader.SetTexture(diffusionKernel, "Result", diffusionTexture);
        diffusionShader.SetFloat("DecaySpeed", decaySpeed);
        diffusionShader.SetFloat("DecayPercent", decayPercent);
        diffusionShader.Dispatch(diffusionKernel, texture.width / 8, texture.height / 8, 1);
        Graphics.CopyTexture(diffusionTexture, texture);

    }

    [GUIColor(0.5f, 1f, 0.5f)]
    [Button]
    public void ResetSimulation()
    {
        GetComponent<AspectRatioFitter>().aspectRatio = texture.width / texture.height;

        diffusionKernel = diffusionShader.FindKernel("SimulateDiffusion");
        agentKernel = agentShader.FindKernel("SimulateAgents");

        texture.Release();
        texture.Create();
        agentShader.SetTexture(agentKernel, "Result", texture);
        agentShader.SetFloats("MaxBounds", new float[] { texture.width, texture.height, 0, 0 });


        Agent[] data = new Agent[numAgents];
        for (int i = 0; i < numAgents; i++)
        {
            Vector2 direction = Random.insideUnitCircle;
            Vector2 position = direction * normalizedSpawnRadius * texture.height / 2f + new Vector2(texture.width / 2f, texture.height / 2f);
            float angle = Mathf.Atan2(-direction.y, -direction.x);
            data[i] = new Agent(position, angle);
        }

        agentBuffer?.Release();
        agentBuffer = new ComputeBuffer(data.Length, Agent.sizeOf);
        agentBuffer.SetData(data);
        agentShader.SetBuffer(agentKernel, "Agents", agentBuffer);


        agentShader.Dispatch(agentKernel, agentBuffer.count / 32, 1, 1);

        Debug.Log("Dispatched!");
    }

}