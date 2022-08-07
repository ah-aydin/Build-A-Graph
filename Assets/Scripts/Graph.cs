using System.Collections;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField] Transform pointPrefab;
    [SerializeField, Range(10, 400)] int resolution = 10;
    [SerializeField] FunctionLibrary.FunctionName function;

    public enum TransitionMode { Sequential, Random };
    [SerializeField] TransitionMode transitionMode = TransitionMode.Sequential;

    [SerializeField, Min(0f)] float functionDuration = 1f;
    [SerializeField, Min(0f)] float transitionDuration = 1f;
    float duration = 0f;
    bool transitioning = false;
    FunctionLibrary.FunctionName transitionFunction;

    private Transform[] points;

    private void Awake()
    {
        points = new Transform[resolution * resolution];

        float step = 2f / resolution;
        var pointScale = Vector3.one * step;

        for (int i = 0; i < points.Length; ++i)
        {
            Transform point = Instantiate(pointPrefab);
            points[i] = point;
            point.SetParent(transform, false);
            point.localScale = pointScale;
        }
    }

    private float time;

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
        if (transitioning) UpdateTransitionFunction();
        else UpdateFunction();
    }

    private void GetNextFunction()
    {
        if (transitionMode == TransitionMode.Sequential)
            function = FunctionLibrary.GetNextFunctionName(function);
        else if (transitionMode == TransitionMode.Random) 
            function = FunctionLibrary.GetRandomFunctionName(function);
    }
    
    private void UpdateFunction()
    {
        FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);
        time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; ++i, ++x)
        {
            if (x == resolution)
            {
                x = 0;
                ++z;
                v = (z + 0.5f) * step - 1f; // Evaluate this only when z changes
            }
            Transform point = points[i];

            float u = (x + 0.5f) * step - 1f;
            point.localPosition = f(u, v, time);;
        }
    }

    private void UpdateTransitionFunction()
    {
        FunctionLibrary.Function from = FunctionLibrary.GetFunction(transitionFunction);
        FunctionLibrary.Function to = FunctionLibrary.GetFunction(function);

        float progress = duration / transitionDuration;
        time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; ++i, ++x)
        {
            if (x == resolution)
            {
                x = 0;
                ++z;
                v = (z + 0.5f) * step - 1f; // Evaluate this only when z changes
            }
            Transform point = points[i];

            float u = (x + 0.5f) * step - 1f;
            point.localPosition = FunctionLibrary.Morph(u, v, time, from, to, progress);
        }
    }   
}
