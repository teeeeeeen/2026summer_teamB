using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("スケール設定")]
    public float hoverScale = 1.1f; // カーソルが乗った時のサイズ（1.1倍）
    public float pressScale = 0.9f; // 押した時のサイズ（0.9倍）
    public float bounceSpeed = 15f; // アニメーションの素早さ

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovering = false;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // Time.deltaTime から Time.unscaledDeltaTime に変更
        // これにより時間が停止していてもUIのアニメーションが動くようになります
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * bounceSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = originalScale * pressScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = isHovering ? (originalScale * hoverScale) : originalScale;
    }

    void OnDisable()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;
        isHovering = false;
    }
}