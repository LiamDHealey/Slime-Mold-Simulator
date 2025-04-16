using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;

[ExecuteAlways]
public class ComputeController : MonoBehaviour
{
    [field: SerializeField, TitleGroup("Agents/Spawning")]
    public bool filled { get; set; } = true;

    [field: SerializeField, TitleGroup("Agents/Spawning")]
    public bool pointInward { get; set; } = true;

    [field: SerializeField, TitleGroup("Agents/Spawning")]
    public bool RandomYaw { get; set; } = false;

    [field: SerializeField, TitleGroup("Agents/Spawning")]
    public bool RandomPitch { get; set; } = true;

    [Range(1, 10)]
    public int itterations = 1;

    [BoxGroup("Agents")]
    [TitleGroup("Agents/Spawning")]
    public int numAgents = 100;
    public string stringNumAgents { set => numAgents = Max(int.Parse(value), 100); }

    [field: TitleGroup("Agents/Spawning"), SerializeField]
    [field: Range(0, 1)]
    public float normalizedSpawnRadius { get; set; } = 0.25f;

    [field: TitleGroup("Agents/Movement"), SerializeField]
    public float agentSpeed { get; set; } = 0.2f;

    public float agentTurnSpeed
    {
        get => _agentTurnSpeed;
        set { _agentTurnSpeed = value; UpdateSensorAngles(); }
    }
    private float _agentTurnSpeed = 0.2f;

    public bool yawSensors
    {
        get => _yawSensors;
        set { _yawSensors = value; UpdateSensorAngles(); UpdateIndecisionSensorAngles(); }
    }
    private bool _yawSensors = true;
    
    public bool pitchSensors 
    { 
        get => _pitchSensors; 
        set { _pitchSensors = value; UpdateSensorAngles(); UpdateIndecisionSensorAngles(); } 
    }
    private bool _pitchSensors = true;

    private void UpdateSensorAngles()
    {
        sensors[1].yaw = yawSensors ? agentTurnSpeed : 0;
        sensors[2].yaw = -(yawSensors ? agentTurnSpeed : 0);
        sensors[3].pitch = pitchSensors ? agentTurnSpeed : 0;
        sensors[4].pitch = -(pitchSensors ? agentTurnSpeed : 0);
    }

    private float _indecisionTurnSpeed = 1f;
    public float indecisionTurnSpeed
    {
        get => _indecisionTurnSpeed;
        set { _indecisionTurnSpeed = value; UpdateIndecisionSensorAngles(); }
    }
    private void UpdateIndecisionSensorAngles()
    {
        sensors[0].yawTurnSpeed = yawSensors ? indecisionTurnSpeed : 0;
        sensors[0].pitchTurnSpeed = pitchSensors ? indecisionTurnSpeed : 0;
    }

    public float sensorDistance 
    { 
        set
        {
            for (int i = 0; i < sensors.Length; i++)
            {
                sensors[i].distance = value;
            }
        }
    }


    public float sensorAngle
    {
        set
        {
            sensors[1].yaw = value;
            sensors[2].yaw = -value;
            sensors[3].pitch = value;
            sensors[4].pitch = -value;
        }
    }


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
        agentShader.SetFloats("MaxBounds", new float[] { texture.width, texture.height, texture.volumeDepth, 0 });


        Agent[] data = new Agent[numAgents];
        for (int i = 0; i < numAgents; i++)
        {
            float yaw = RandomYaw ? Random.Range(-PI, PI) : 0;
            float pitch =  RandomPitch ? Random.Range(-PI, PI) : 0;
            Vector3 direction = new(Cos(yaw) * Cos(pitch), Sin(yaw) * Cos(pitch), Sin(pitch));
            if (filled)
                direction *= Pow(Random.value, 1f / 3);
            if (pointInward)
                direction *= -1;
            Vector3 position = direction * normalizedSpawnRadius * texture.height / 2f + new Vector3(texture.width / 2f, texture.height / 2f, texture.volumeDepth / 2f);
            data[i] = new Agent(position, yaw, pitch);
        }

        agentBuffer?.Dispose();
        agentBuffer = new ComputeBuffer(data.Length, Agent.sizeOf);
        agentBuffer.SetData(data);
        agentShader.SetBuffer(agentKernel, "Agents", agentBuffer);

        Debug.Log("Dispatched!");
    }

}