using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("生成するプレハブ")]
    public GameObject badObstaclePrefab;  // 接触してはいけないもの
    public GameObject goodObstaclePrefab; // 接触しなきゃいけないもの

    [Header("生成の設定")]
    public float spawnInterval = 1.5f; // 生成する間隔（秒）
    
    [Header("レーンの設定（X座標）")]
    public float[] lanes = { -12f, -6f, 0f, 6f, 12f }; // 生成される5つのレーン

    [Header("生成位置の設定")]
    public float spawnZ = 30f;         // 生成するZ座標（奥）
    public float spawnY = 0.5f;        // 生成するY座標（高さ）
    
    [Range(0f, 1f)]
    public float goodItemSpawnRate = 0.3f; // 良いアイテムが出る確率（0.3 = 30%）

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            Spawn();
            timer = 0f;
        }
    }

    void Spawn()
    {
        GameObject prefabToSpawn = (Random.value < goodItemSpawnRate) ? goodObstaclePrefab : badObstaclePrefab;

        // レーンの配列の中からランダムに1つのX座標を選ぶ
        float randomX = lanes[Random.Range(0, lanes.Length)];

        // 生成位置を決定（Xは選ばれたレーン、Zは固定の奥）
        Vector3 spawnPos = new Vector3(randomX, spawnY, spawnZ);

        // プレハブの元の回転角度（傾き）をそのまま適用して生成
        Instantiate(prefabToSpawn, spawnPos, prefabToSpawn.transform.rotation);
    }
}