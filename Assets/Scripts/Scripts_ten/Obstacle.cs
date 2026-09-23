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
    public AudioSource catchSound;        // 回収時の効果音
    public AudioSource pochanSound;       // ぽちゃん音の効果音

    private bool isFlying = false;
    private float flightTimer = 0f;
    private Vector3 startPos;
    
    // 【追加】音の制御と遅延破棄用のフラグ
    private bool hasPlayedPochan = false; 
    private bool isReachedTarget = false; 

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
        // ターゲットに到達した後は、音が鳴り終わって削除されるのを待つだけなので何もしない
        if (isReachedTarget) return;

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

        // 【修正1】まだ鳴らしていなければ1回だけ鳴らす（メッチャ鳴る問題の解決）
        if (t >= 0.9f && !hasPlayedPochan)
        {
            if (pochanSound != null)
            {
                pochanSound.Play();
            }
            hasPlayedPochan = true; // フラグを立てて2回目以降を防ぐ
        }

        if (t >= 1.0f)
        {
            t = 1.0f; // 最後の位置をぴったり合わせる
            isReachedTarget = true; // Updateの処理を止めるフラグを立てる

            // 【修正2】すぐにDestroyせず、見た目を消して音が鳴り終わるのを待つ（音が途切れる問題の解決）
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            // オーディオクリップの長さを取得（設定されていない場合は余裕を持って2秒待つ）
            float delay = 2f;
            if (pochanSound != null && pochanSound.clip != null)
            {
                delay = pochanSound.clip.length;
            }
            
            // 指定した秒数（delay）が経過した後にオブジェクトを完全に削除する
            Destroy(gameObject, delay);
        }

        // 開始位置と目標位置の間を線形補間（XZ平面での移動）
        Vector3 currentPos = Vector3.Lerp(startPos, targetTransform.position, t);
        
        // サイン波を使ってY軸にジャンプを加える（放物線の計算）
        currentPos.y += Mathf.Sin(t * Mathf.PI) * flightHeight;

        transform.position = currentPos;

        // Y軸を中心に一定速度で回転
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
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
            
            if (catchSound != null)
            {
                catchSound.Play();
            }
        }
        else
        {
            Debug.Log("ダメージ！");
            GameManager.instance.TakeDamage();
            Destroy(gameObject); // ダメージ時はGameManager側で音を鳴らしているので即消しでOK
        }
    }
}