using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("生成するプレハブ")]
    public GameObject badObstaclePrefab;  // 接触してはいけないもの
    public GameObject goodObstaclePrefab; // 接触しなきゃいけないもの

    [Header("生成の設定")]
    public float spawnInterval = 1.5f; // 生成する間隔（秒）
    public float spawnXRange = 4f;     // 軸のランダムな生成範囲（上下）
    public float spawnZ = 15f;         // 生成するX座標（画面の右外）
    
    [Range(0f, 1f)]
    public float goodItemSpawnRate = 0.3f; // 「接触しなきゃいけないもの」が出る確率（0.3 = 30%）

    private float timer;

    void Update()
    {
        // タイマーを更新
        timer += Time.deltaTime;

        // 設定した間隔が過ぎたら生成
        if (timer >= spawnInterval)
        {
            Spawn();
            timer = 0f;
        }
    }

    void Spawn()
    {
        // 確率に基づいてどちらを生成するか決める
        GameObject prefabToSpawn = (Random.value < goodItemSpawnRate) ? goodObstaclePrefab : badObstaclePrefab;

        // 生成位置を決定（Xは固定、Yは上下ランダム、Zは0）
        Vector3 spawnPos = new Vector3(Random.Range(-spawnXRange, spawnXRange), -1f, spawnZ);

        // オブジェクトを生成
        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }
}