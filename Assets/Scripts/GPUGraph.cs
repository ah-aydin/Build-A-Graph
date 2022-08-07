using System.Collections;
using UnityEngine;

public class GPUGraph : MonoBehaviour
{
    const int maxResolution = 1000;
    [SerializeField, Range(10, maxResolution)] int resolution = 10;
    [SerializeField] FunctionLibrary.FunctionName function;
    
    public enum TransitionMode { Sequential, Random };
    [SerializeField] TransitionMode transitionMode = TransitionMode.Sequential;

    [SerializeField, Min(0f)] float functionDuration = 1f;
    [SerializeField, Min(0f)] float transitionDuration = 1f;
    float duration = 0f;
    bool transitioning = false;
    FunctionLibrary.FunctionName transitionFunction;

    private Transform[] points;

    private float time;

    [SerializeField] ComputeShader computeShader;
    ComputeBuffer positionsBuffer;

    [SerializeField] Material material;
    [SerializeField] Mesh mesh;

    static readonly int positionsId = Shader.PropertyToID("_Positions");
    static readonly int resolutionId = Shader.PropertyToID("_Resolution");
    static readonly int stepId = Shader.PropertyToID("_Step");
    static readonly int timeId = Shader.PropertyToID("_Time");
    static readonly int transitionProgresssId = Shader.PropertyToID("_TransitionProgress");

    private void OnEnable()
    {
        positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
    }

    private void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }

    private void Update()
    {
        duration += Time.deltaTime;
        if (transitioning)
        {
            if (duration >= transitionDuration)
            {
                duration -= transitionDuration;
                transitioning = false;
            }
        }
        else if (duration >= functionDuration)
        {
            duration -= functionDuration;
            transitioning = true;
            transitionFunction = function;
            GetNextFunction();
        }
        UpdateFunctionOnGPU();
    }

    private void GetNextFunction()
    {
        if (transitionMode == TransitionMode.Sequential)
            function = FunctionLibrary.GetNextFunctionName(function);
        else if (transitionMode == TransitionMode.Random) 
            function = FunctionLibrary.GetRandomFunctionName(function);
    }

    private void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);
        if (transitioning)
        {
            computeShader.SetFloat(transitionProgresssId, duration / transitionDuration);
        }

        var kernelIndex = (int) function + (int) (transitioning ? transitionFunction : function) * FunctionLibrary.FunctionCount;
        computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer);

        int groupCount = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(kernelIndex, groupCount, groupCount, 1);

        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);
        var bounds = new Bounds(Vector3.zero, Vector3.one * 2);
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
    }
}
