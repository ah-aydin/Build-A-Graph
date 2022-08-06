using System.Collections;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField] Transform pointPrefab;
    [SerializeField, Range(10, 100)] int resolution = 10;
    [SerializeField] FunctionLibrary.FunctionName function;

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
}
