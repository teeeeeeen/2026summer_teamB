using UnityEngine;
using UnityEngine.UI;

public class RainbowEffect : MonoBehaviour
{
    public Image targetImage;
    public float speed = 1.0f;
    
    // 透明度を0〜1の間でスライダー調整できるようにする
    [Range(0f, 1f)] public float alpha = 0.5f; 

    void Update()
    {
        if (targetImage != null)
        {
            // 時間経過で色をループさせる
            float hue = Mathf.Repeat(Time.time * speed, 1f);
            Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
            
            // 設定した透明度を合体させる
            rainbowColor.a = alpha; 
            
            targetImage.color = rainbowColor;
        }
    }
}
