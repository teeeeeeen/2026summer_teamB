using UnityEngine;

public class Floater : MonoBehaviour
{
    [Header("ふよふよの設定")]
    public float floatSpeed = 2f;    // 揺れる速さ
    public float floatHeight = 0.5f; // 揺れる幅（高さ）

    private Vector3 startPos;

    void Start()
    {
        // ゲーム開始時の位置を基準点として記憶します
        startPos = transform.position;
    }

    void Update()
    {
        // 時間経過(Time.time)とSin波を使って、基準点からのY座標のズレを計算します
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        
        // 現在のX, Z座標（他のスクリプトで移動している可能性を考慮）を維持しつつ、Y座標を適用します
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}