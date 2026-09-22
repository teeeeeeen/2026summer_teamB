using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 10f;       // 奥から手前へ進む速度
    public float destroyZ = -25f;   // これより手前に来たら削除するZ座標

    [Header("種類の設定")]
    [Tooltip("チェックを入れると「接触しなきゃいけないもの」になります。")]
    public bool isMustCatch = false; 

    void Update()
    {
        // Space.Worldを指定して、傾きに関係なくワールド空間のZ軸マイナス方向へ進ませる
        transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);

        // 画面手前を通り過ぎた時の処理
        if (transform.position.z < destroyZ)
        {
            if (isMustCatch)
            {
                // 接触しなきゃいけないものをスルーしてしまったらゲームオーバー
                //GameManager.instance.TriggerGameOver();
            }
            // オブジェクトを削除
            Destroy(gameObject);
        }
    }

    // プレイヤーと接触した時の処理（ColliderのIsTriggerがオン、もしくは物理衝突時）
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
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