using UnityEngine;
using TMPro;

public class FrameRateCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI display;
    [SerializeField, Range(0.1f, 2f)] float sampleDuration = 1f; 

    public enum DisplayMode { FPS , MS };
    [SerializeField] DisplayMode displayMode = DisplayMode.FPS;

    int frames;

    float duration;
    float bestDuration = float.MaxValue;
    float worstDuration = 0f;

    private void Update()
    {
        float frameDuration = Time.unscaledDeltaTime;
        ++frames;
        duration += frameDuration;

        if (frameDuration < bestDuration) bestDuration = frameDuration;
        if (frameDuration > worstDuration) worstDuration = frameDuration;

        if (duration >= sampleDuration) 
        {
            if (displayMode == DisplayMode.FPS)
            {
                display.SetText("FPS\n{0:0}\n{1:0}\n{2:0}", frames / duration, 1f / bestDuration, 1f / worstDuration);
            }
            else
            {
                display.SetText("FPS\n{0:2}\n{1:2}\n{2:2}", 1000 * duration / frames, 1000 * bestDuration, 1000 * worstDuration);
            }
            frames = 0;
            duration = 0f;
            bestDuration = float.MaxValue;
            worstDuration = 0f;
        }
    }
}
