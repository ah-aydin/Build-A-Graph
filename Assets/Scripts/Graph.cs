using System.Collections;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField] Transform pointPrefab;
    [SerializeField, Range(10, 100)] int resolution = 10;

    private Transform[] points;

    private void Awake()
    {
        points = new Transform[resolution];

        float step = 2f / resolution;
        var position = Vector3.zero;
        var pointScale = Vector3.one * step;

        for (int i = 0; i < points.Length; ++i)
        {
            Transform point = Instantiate(pointPrefab);
            points[i] = point;
         
            point.SetParent(transform);
            position.x  = (i + 0.5f) * step - 1f; // Subtract -1 to put the cubes in range [-1, 1]
            point.localPosition = position;
            point.localScale = pointScale;
        }
    }

    private float time;

    private void Update()
    {
        time = Time.time;
        for (int i = 0; i < points.Length; ++i)
        {
            Transform point = points[i];
            Vector3 position = point.localPosition;
            position.y = computeFunction(position.x);
            point.localPosition = position;
        }
    }

    private float computeFunction(float x) {
        // Manupulate the return value of the function to your hearts content
        // return x * x * x;
        return Mathf.Sin(Mathf.PI * (x + time));
    }
}
