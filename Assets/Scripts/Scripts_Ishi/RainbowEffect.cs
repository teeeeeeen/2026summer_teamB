using UnityEngine;
using UnityEngine.UI;

public class RainbowEffect : MonoBehaviour
{
    public Image targetImage;
    public float speed = 1.0f;
    [Range(0f, 1f)] public float alpha = 0.5f; 

    void Update()
    {
        if (targetImage != null)
        {
            // ★変更：Time.time ではなく Time.unscaledTime（現実の時間）を使う
            float hue = Mathf.Repeat(Time.unscaledTime * speed, 1f);
            Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
            
            rainbowColor.a = alpha; 
            targetImage.color = rainbowColor;
        }
    }
}