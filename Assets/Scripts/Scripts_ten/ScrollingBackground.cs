using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ScrollingBackground : MonoBehaviour
{
    [Header("スクロール速度")]
    [Tooltip("斜めに動かす場合はXとYの両方に値を入れてください（例：X=0.1, Y=0.1）")]
    private Vector2 scrollSpeed = new Vector2(-0.3f, -0.3f);

    private RawImage rawImage;
    private Rect uvRect;

    void Start()
    {
        // RawImageコンポーネントを取得
        rawImage = GetComponent<RawImage>();
        // 現在のUV矩形を取得
        uvRect = rawImage.uvRect;
    }

    void Update()
    {
        // Time.deltaTime から Time.unscaledDeltaTime に変更
        // これにより時間が停止していても背景がスクロールし続けます
        uvRect.position += scrollSpeed * Time.unscaledDeltaTime;
        // 更新したUVをRawImageに適用
        rawImage.uvRect = uvRect;
    }
}