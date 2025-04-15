using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ComputeController : MonoBehaviour
{
    [Range(1, 100)]
    public int itterations = 1;

    [BoxGroup("Agents")]
    [TitleGroup("Agents/Spawning")]
    public int numAgents = 100;

    [TitleGroup("Agents/Spawning")]
    [Range(0,1)]
    public float normalizedSpawnRadius = 0.25f;

    [TitleGroup("Agents/Movement")]
    public float agentSpeed = 1.0f;

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
        diffusionTexture = new(texture);
    }

    private void Start()
    {
        ResetSimulation();
    }

    private void OnDestroy()
    {
        agentBuffer?.Dispose();
        sensorBuffer?.Dispose();
    }

    public void FixedUpdate()
    {
        if (!Application.isPlaying)
            return;

        if (!sensors.SequenceEqual(lastSensors))
        {
            sensorBuffer?.Dispose();
            sensorBuffer = new ComputeBuffer(sensors.Length, Sensor.sizeOf);
            sensorBuffer.SetData(sensors);
            agentShader.SetBuffer(agentKernel, "Sensors", sensorBuffer);
            agentShader.SetInt("NumSensors", sensors.Length);
            lastSensors = sensors.ToArray();
        }

        for (int i = 0; i < itterations; i++)
        {
            agentShader.SetFloat("AgentSpeed", agentSpeed);
            agentShader.SetFloat("Time", Time.time);
            agentShader.Dispatch(agentKernel, agentBuffer.count / 64, 1, 1);

            diffusionShader.SetTexture(diffusionKernel, "Input", texture);
            diffusionShader.SetTexture(diffusionKernel, "Result", diffusionTexture);
            diffusionShader.SetFloat("DecaySpeed", decaySpeed);
            diffusionShader.SetFloat("DecayPercent", decayPercent);
            diffusionShader.Dispatch(diffusionKernel, texture.width / 4, texture.height / 4, texture.volumeDepth / 4);
            Graphics.CopyTexture(diffusionTexture, texture);
        }

    }

    [GUIColor(0.5f, 1f, 0.5f)]
    [Button]
    public void ResetSimulation()
    {
        diffusionKernel = diffusionShader.FindKernel("SimulateDiffusion");
        agentKernel = agentShader.FindKernel("SimulateAgents");

        texture.Release();
        texture.Create();
        agentShader.SetTexture(agentKernel, "Result", texture);
        agentShader.SetFloats("MaxBounds", new float[] { texture.width, texture.height, 0, 0 });


        Agent[] data = new Agent[numAgents];
        for (int i = 0; i < numAgents; i++)
        {
            Vector3 direction = Vector3.Scale(Random.insideUnitSphere, new Vector3(1,0,0));
            Vector3 position = direction * normalizedSpawnRadius * texture.height / 2f + new Vector3(texture.width / 2f, texture.height / 2f, texture.volumeDepth / 2f);
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, direction.normalized);
            float yaw = rotation.eulerAngles.z * Mathf.Deg2Rad;
            float pitch = rotation.eulerAngles.x * Mathf.Deg2Rad;
            data[i] = new Agent(position, yaw, pitch);
        }

        agentBuffer?.Dispose();
        agentBuffer = new ComputeBuffer(data.Length, Agent.sizeOf);
        agentBuffer.SetData(data);
        agentShader.SetBuffer(agentKernel, "Agents", agentBuffer);

        Debug.Log("Dispatched!");
    }

}