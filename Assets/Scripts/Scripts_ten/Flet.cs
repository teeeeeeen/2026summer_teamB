using UnityEngine;

public class Flet : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 10f;       // 奥から手前へ進む速度
    public float destroyZ = -25f;   // これより手前に来たら削除するZ座標

    [Tooltip("チェックを入れると「接触しなきゃいけないもの」になります。")]
    public bool isMustCatch = false;
    private Vector3 startPos;
    
    void Update()
    {
  
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
        if (isMustCatch)
        {

            // 当たり判定を即座に無効化
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

        }
        else
        {
            Debug.Log("ダメージ！");
            GameManager.instance.TakeDamage();
        }
    }
}