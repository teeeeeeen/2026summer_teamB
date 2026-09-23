using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class AutoScroller : MonoBehaviour
{
    private ScrollRect scrollRect;
    private RectTransform contentPanel;
    private GameObject lastSelected;

    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        contentPanel = scrollRect.content;
    }

    void Update()
    {
        // EventSystemが現在選択しているオブジェクトを取得
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        // 選択対象が変わった、かつそれがこのScroll View内にある場合のみスクロール処理を行う
        if (selected != null && selected != lastSelected && selected.transform.IsChildOf(contentPanel))
        {
            lastSelected = selected;
            SnapTo(selected.GetComponent<RectTransform>());
        }
    }

    private void SnapTo(RectTransform target)
    {
        // Content(親)とTarget(選択されたボタン)のローカル位置を計算
        Canvas.ForceUpdateCanvases(); // レイアウトを最新状態に強制更新

        Vector2 viewportLocalPosition = scrollRect.viewport.localPosition;
        Vector2 targetLocalPosition = target.localPosition;

        // 縦方向のスクロール位置を計算
        // ScrollRectのnormalizedPosition (0〜1) ではなく、contentのanchoredPositionを直接操作します
        Vector2 newPosition = contentPanel.anchoredPosition;

        // ターゲットの位置がビューポートの中央にくるように計算
        // （上端/下端に合わせることも可能ですが、中央に持ってくるのが最も自然です）
        newPosition.y = -targetLocalPosition.y - (scrollRect.viewport.rect.height / 2f) + (target.rect.height / 2f);

        // Contentの高さとViewportの高さから、スクロール可能な範囲を制限する
        float maxY = contentPanel.rect.height - scrollRect.viewport.rect.height;
        // ScrollViewの中身がViewportより小さい場合はスクロールさせない
        if (maxY < 0) maxY = 0; 
        
        newPosition.y = Mathf.Clamp(newPosition.y, 0, maxY);

        contentPanel.anchoredPosition = newPosition;
    }
}