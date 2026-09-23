using UnityEngine;

public class DecoSpawner : MonoBehaviour
{
    [Header("生成する飾りのプレハブ")]
    public GameObject[] decoPrefabs; // 複数の飾りを登録可能

    [Header("生成の設定")]
    public float spawnInterval = 0.5f; // 飾りを生成する間隔
    public float spawnZ = 30f;         // 生成するZ座標（奥）
    public float spawnY = 0f;          // 生成する高さ
            
    [Header("道の両脇に生成するためのX座標")]
    public float leftX = -8f;  // 左側の位置
    public float rightX = 8f;  // 右側の位置

    private float timer;

    void Start()
    {
        // 初期化処理が必要な場合はここに追加
        SpawnDecorations();
    }

    void Update()
    {
        timer += Time.deltaTime;
        float currentMultiplier = GameManager.instance != null ? GameManager.instance.currentSpeedMultiplier : 1f;
        float currentInterval = spawnInterval / currentMultiplier;

        if (timer >= currentInterval)
        {
            SpawnDecorations();
            timer = 0f;
        }
    }

    void SpawnDecorations()
    {
        if (decoPrefabs == null || decoPrefabs.Length == 0) return;

        // 登録したプレハブの中からランダムに1つ選ぶ
        GameObject prefab = decoPrefabs[Random.Range(0, decoPrefabs.Length)];

        // 左側に生成
        Vector3 leftPos = new Vector3(leftX, spawnY, spawnZ);
        Instantiate(prefab, leftPos, Quaternion.identity);

        // 右側に生成
        Vector3 rightPos = new Vector3(rightX, spawnY, spawnZ);
        Instantiate(prefab, rightPos, Quaternion.identity);
    }
}