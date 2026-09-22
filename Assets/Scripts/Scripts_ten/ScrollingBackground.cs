using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ScrollingBackground : MonoBehaviour
{
    [Header("スクロール速度")]
    public Vector2 scrollSpeed = new Vector2(0.1f, 0.1f);

    private RawImage rawImage;
    private Rect uvRect;
    private bool isSelected = false;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        uvRect = rawImage.uvRect;
    }

    void Update()
    {
        // 選択中じゃなければ停止
        if (!isSelected)
            return;

        // 選択中だけ斜めにスクロール
        uvRect.position += scrollSpeed * Time.unscaledDeltaTime;

        // RawImageに反映
        rawImage.uvRect = uvRect;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }
}