using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ScrollingBack : MonoBehaviour
{
    [Header("スクロール速度")]
    public Vector2 scrollSpeed = new Vector2(0.1f, 0.1f);

    private RawImage rawImage;
    private Rect uvRect;
    private bool isSelected = false;

    void Start()
    {
        rawImage = GetComponent<RawImage>();

        // 背景を繰り返し表示
        uvRect = rawImage.uvRect;
        //rawImage.uvRect = uvRect;
    }

    void Update()
    {
        if (!isSelected)
            return;

        // 背景を斜めに動かす
        uvRect.position += scrollSpeed * Time.unscaledDeltaTime;

        rawImage.uvRect = uvRect;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }
}