using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Obstacle : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 10f;       // 右から左へ進む速度
    public float destroyZ = -15f;   // これより左に来たら削除するX座標

    [Header("種類の設定")]
    [Tooltip("チェックを入れると「接触しなきゃいけないもの」になります。")]
    public bool isMustCatch = false; 

    [Header("見た目の設定")]
    [Tooltip("ランダムに選ばれるスプライトのリスト")]
    public Sprite[] sprites;
    
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // SpriteRendererを取得
        spriteRenderer = GetComponent<SpriteRenderer>();

        // リストにスプライトが登録されていれば、ランダムに1つ適用する
        if (sprites != null && sprites.Length > 0)
        {
            int randomIndex = Random.Range(0, sprites.Length);
            spriteRenderer.sprite = sprites[randomIndex];
        }

        // スプライトが影を落とす・影を受ける設定を有効化
        spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        spriteRenderer.receiveShadows = true;
    }

    void Update()
    {
        // 右から左（X軸のマイナス方向）に進む
        transform.Translate(Vector3.back * speed * Time.deltaTime);

        // 画面左端を通り過ぎた時の処理
        if (transform.position.z < destroyZ)
        {
            if (isMustCatch)
            {
                // 接触しなきゃいけないものをスルーしてしまったらゲームオーバー
                
            }
            // オブジェクトを削除
            Destroy(gameObject);
        }
    }

    // プレイヤーと接触した時の処理（※ColliderのIsTriggerがオン、もしくは物理衝突時）
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isMustCatch)
            {
                // 接触しなきゃいけないもの → 成功（オブジェクトを消してやり過ごす）
                Destroy(gameObject);
            }
            else
            {
                // 接触してはいけないもの → ゲームオーバー
                GameManager.instance.TriggerGameOver();
            }
        }
    }

    // IsTriggerを使っていない（物理衝突の）場合の保険
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (isMustCatch)
            {
                Destroy(gameObject);
            }
            else
            {
                GameManager.instance.TriggerGameOver();
            }
        }
    }
}