using UnityEngine;
using static UnityEngine.Mathf;

public static class FunctionLibrary
{
    public delegate Vector3 Function (float u, float v, float t);

    public enum FunctionName { Wave, MultiWave, Ripple, Sphere, DonutKindOf, Onion, SpaceStation, TwistedOnion, Torus, TwistingTorus };

    public static Function[] functions = { Wave, MultiWave, Ripple, Sphere, DonutKindOf, Onion, SpaceStation, TwistedOnion, Torus, TwistingTorus };

    public static int FunctionCount
    { 
        get => functions.Length;
    }

    public static Function GetFunction(FunctionName name)
    {
        return functions[(int) name];
    }

    public static FunctionName GetNextFunctionName(FunctionName name)
    {
        return ((int)name < functions.Length - 1) ? name + 1 : FunctionName.Wave;
    }

    public static FunctionName GetRandomFunctionName(FunctionName name)
    {
        var choice = (FunctionName) Random.Range(1, functions.Length);
        return (choice == name) ? 0 : choice;
    }

    public static Vector3 Morph(float u, float v, float t, Function from, Function to, float progress)
    {
        return Vector3.LerpUnclamped(from(u, v, t), to(u, v, t), SmoothStep(0f, 1f, progress));
    }

    public static Vector3 Wave(float u, float v, float t)
    {
        Vector3 vec;
        vec.x = u;
        vec.y = Sin(PI * (u + v + t));
        vec.z = v;
        return vec;
    }

    public static Vector3 MultiWave(float u, float v, float t)
    {
        Vector3 vec;
        vec.x = u;
        vec.z = v;

        float y = Sin(PI * (u + 0.5f * t));
        y += 0.5f * Sin(2f * PI * (u + v + t));
        y += Sin(PI * (u + v + 0.25f * t));
        vec.y = y;
        
        return vec;
    }

    public static Vector3 Ripple(float u, float v, float t)
    {
        Vector3 vec;
        vec.x = u;
        vec.z = v;

        float y = Sqrt(u * u + v * v);
        vec.y = Sin(PI * (4f * y - t)) / (1f + 10f * y);;
        
        return vec;
    }

    public static Vector3 Sphere(float u, float v, float t)
    {
        float r = (1 + Sin(PI * t)) * 0.5f;
        float s = r * Cos(PI * 0.5f * v);
        Vector3 vec;
        vec.x = Sin(PI * u) * s;
        vec.y = Sin(PI * 0.5f * v) * r;
        vec.z = Cos(PI * u) * s;
        return vec;
    }

    public static Vector3 DonutKindOf(float u, float v, float t) 
    {
        float r = (1 + Sin(PI * t)) * 0.5f;
        float s = r * Cos(PI * 0.5f * v);
        Vector3 vec;
        vec.x = Sin(PI * u) * s;
        vec.y = Sin(PI * 0.5f * v) * s;
        vec.z = Cos(PI * u) * s;
        return vec;
    }

    public static Vector3 Onion(float u, float v, float t)
    {
        float r = 0.9f + 0.1f * Sin(6f * PI * u);
        float s = r * Cos(PI * 0.5f * v);
        Vector3 vec;
        vec.x = Sin(PI * u) * s;
        vec.y = Sin(PI * 0.5f * v) * r;
        vec.z = Cos(PI * u) * s;
        return vec;
    }

    public static Vector3 SpaceStation(float u, float v, float t)
    {
        float r = 0.9f + 0.1f * Sin(12f * PI * v);
        float s = r * Cos(PI * 0.5f * v);
        Vector3 vec;
        vec.x = Sin(PI * u) * s;
        vec.y = Sin(PI * 0.5f * v) * r;
        vec.z = Cos(PI * u) * s;
        return vec;
    }

    public static Vector3 TwistedOnion(float u, float v, float t) 
    {
        float r = 0.9f + 0.1f * Sin(PI * (6f * u + 4f * v + t));
        float s = r * Cos(PI * 0.5f * v);
        Vector3 vec;
        vec.x = Sin(PI * u) * s;
        vec.y = Sin(PI * 0.5f * v) * r;
        vec.z = Cos(PI * u) * s;
        return vec;
    }

    public static Vector3 Torus(float u, float v, float t)
    {
        float r1 = 0.75f;
		float r2 = 0.25f;
        float s = r1 + r2 * Cos(PI * v);
        Vector3 vec;
        vec.x = Sin(PI * u) * s;
        vec.y = Sin(PI * v) * r2;
        vec.z = Cos(PI * u) * s;
        return vec;
    }

    public static Vector3 TwistingTorus(float u, float v, float t)
    {
        float r1 = 0.75f + 0.1f * Sin(6f * u + 0.5f * t);
		float r2 = 0.25f + 0.05f * Sin(PI * (8f * u + 4f * v + 2f * t));
        float s = r1 + r2 * Cos(PI * v);
        Vector3 vec;
        vec.x = Sin(PI * u) * s;
        vec.y = Sin(PI * v) * r2;
        vec.z = Cos(PI * u) * s;
        return vec;
    }
}
