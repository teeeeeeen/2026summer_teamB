using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 10f;       // 奥から手前へ進む速度
    public float destroyZ = -25f;   // これより手前に来たら削除するZ座標

    [Header("種類の設定")]
    [Tooltip("チェックを入れると「接触しなきゃいけないもの」になります。")]
    public bool isMustCatch = false;

    [Header("具材情報（スクリプトが自動設定）")]
    public SpriteRenderer spriteRenderer; // プレハブのSpriteRendererをアタッチしてください
    public string ingredientName = "";
    [HideInInspector] public Transform targetTransform;

    [Header("回収時の演出設定")]
    public float flightDuration = 0.8f;  // 目標に到達するまでの秒数
    public float flightHeight = 3.0f;    // 放物線の高さ（Y軸のジャンプ力）
    public float rotationSpeed = 1080f;  // 飛行中のY軸回転速度

    private bool isFlying = false;
    private float flightTimer = 0f;
    private Vector3 startPos;

    // Spawnerから呼ばれて具材のデータをセットする関数
    public void SetIngredient(string name, Sprite sprite)
    {
        ingredientName = name;
        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    void Update()
    {
        // 回収されて飛んでいる間の処理
        if (isFlying)
        {
            FlyToTarget();
            return; 
        }

        // 通常の移動：Space.Worldを指定してワールド空間のZ軸マイナス方向へ進ませる
        transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);

        // 画面手前を通り過ぎた時の処理
        if (transform.position.z < destroyZ)
        {
            if (isMustCatch)
            {
                // GameManager.instance.TriggerGameOver();
            }
            Destroy(gameObject);
        }
    }

    // ターゲットへ放物線を描いて飛んでいく処理
    private void FlyToTarget()
    {
        if (targetTransform == null)
        {
            Destroy(gameObject); 
            return;
        }

        flightTimer += Time.deltaTime;
        float t = flightTimer / flightDuration;

        if (t >= 1.0f)
        {
            Destroy(gameObject); 
            return;
        }

        // 開始位置と目標位置の間を線形補間（XZ平面での移動）
        Vector3 currentPos = Vector3.Lerp(startPos, targetTransform.position, t);
        
        // サイン波を使ってY軸にジャンプを加える（放物線の計算）
        currentPos.y += Mathf.Sin(t * Mathf.PI) * flightHeight;

        transform.position = currentPos;

        // Y軸を中心に一定速度で回転
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HandleCollision();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HandleCollision();
        }
    }

    private void HandleCollision()
    {
        if (isFlying) return; 

        if (isMustCatch)
        {
            Debug.Log($"{ingredientName}を獲得！");

            // マネージャーへ具材の名前とスプライトを渡す
            if (SoupManager.instance != null)
            {
                SoupManager.instance.CatchIngredient(ingredientName, spriteRenderer.sprite);
            }

            // 当たり判定を即座に無効化
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            // 飛行演出へ移行
            isFlying = true;
            startPos = transform.position;
            flightTimer = 0f;
        }
        else
        {
            Debug.Log("ダメージ！");
            GameManager.instance.TakeDamage();
            Destroy(gameObject);
        }
    }
}