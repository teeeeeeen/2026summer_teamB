using UnityEngine;
using System.Collections.Generic; // Listを使用するために必要

// 具材のデータをインスペクターで設定できるようにするためのクラス
[System.Serializable]
public class IngredientData
{
    public string ingredientName;
    public Sprite sprite;
}

public class Spawner : MonoBehaviour
{
    [Header("生成するプレハブ")]
    public GameObject badObstaclePrefab;  // 接触してはいけないもの
    public GameObject goodObstaclePrefab; // 接触しなきゃいけないもの

    [Header("具材の設定")]
    public IngredientData[] ingredients; // ここに8種類の具材（名前とスプライト）を登録する

    [Header("演出の設定")]
    public Transform collectionTarget; // 「接触していいやつ」が飛んでいく目標となるGameObject

    [Header("生成の設定")]
    public float spawnInterval = 1.5f; // 生成する間隔（秒）
    
    [Header("レーンの設定（X座標）")]
    public float[] lanes = { -12f, -6f, 0f, 6f, 12f }; // 生成される5つのレーン

    [Header("生成数と確率（重み）の設定")]
    [Tooltip("インデックス0が「0個」の確率、インデックス4が「4個」の確率になります。")]
    public float[] spawnCountWeights = { 10f, 30f, 30f, 20f, 10f }; // デフォルト設定（0個:10%, 1個:30%, 2個:30%, 3個:20%, 4個:10%）

    [Header("生成位置の設定")]
    public float spawnZ = 30f;         // 生成するZ座標（奥）
    public float spawnY = 0.5f;        // 生成するY座標（高さ）
    
    [Range(0f, 1f)]
    public float goodItemSpawnRate = 0.3f; // 良いアイテムが出る確率（0.3 = 30%）

    private float timer;

    void Start()
    {
        // 初期化処理が必要な場合はここに追加
        SpawnMultiple();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 【修正】GameManagerから現在の倍率を取得し、間隔を短く（速く）する
        float currentMultiplier = GameManager.instance != null ? GameManager.instance.currentSpeedMultiplier : 1f;
        float currentInterval = spawnInterval / currentMultiplier;

        if (timer >= currentInterval)
        {
            SpawnMultiple();
            timer = 0f;
        }
    }

    void SpawnMultiple()
    {
        // 1. 今回生成する個数を抽選で決める
        int spawnCount = GetRandomSpawnCount();

        if (spawnCount <= 0) return; // 0個の場合は何も出さずに終了

        // 2. 同じレーンに重ならないよう、利用可能なレーンの番号をリストに入れてシャッフルする
        List<int> availableLanes = new List<int>();
        for (int i = 0; i < lanes.Length; i++)
        {
            availableLanes.Add(i);
        }

        // リストのシャッフル処理
        for (int i = 0; i < availableLanes.Count; i++)
        {
            int temp = availableLanes[i];
            int randomIndex = Random.Range(i, availableLanes.Count);
            availableLanes[i] = availableLanes[randomIndex];
            availableLanes[randomIndex] = temp;
        }

        // 3. 抽選された個数分だけ、シャッフルしたレーンに生成する
        // （安全対策：レーンの最大数を超えて生成しないようにする）
        int actualSpawnCount = Mathf.Min(spawnCount, lanes.Length);

        for (int i = 0; i < actualSpawnCount; i++)
        {
            int laneIndex = availableLanes[i];
            SpawnSingle(lanes[laneIndex]);
        }
    }

    // 重み付け抽選を行って 0〜4 の生成数を返す関数
    int GetRandomSpawnCount()
    {
        if (spawnCountWeights == null || spawnCountWeights.Length == 0) return 1;

        float totalWeight = 0f;
        foreach (float weight in spawnCountWeights)
        {
            totalWeight += weight;
        }

        float randomVal = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < spawnCountWeights.Length; i++)
        {
            currentWeight += spawnCountWeights[i];
            if (randomVal <= currentWeight)
            {
                return i; // 配列のインデックス（0〜4）がそのまま生成数になる
            }
        }

        return 0; // 万が一ここに来たら0個
    }

    // 1個のオブジェクトを指定したX座標に生成する処理
    void SpawnSingle(float xPos)
    {
        GameObject prefabToSpawn = (Random.value < goodItemSpawnRate) ? goodObstaclePrefab : badObstaclePrefab;

        Vector3 spawnPos = new Vector3(xPos, spawnY, spawnZ);

        // プレハブの元の回転角度（傾き）をそのまま適用して生成
        GameObject spawned = Instantiate(prefabToSpawn, spawnPos, prefabToSpawn.transform.rotation);

        // 具材のランダム設定とターゲットの受け渡し
        Obstacle obstacle = spawned.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            obstacle.targetTransform = collectionTarget;

            if (ingredients != null && ingredients.Length > 0)
            {
                // 登録された具材の中からランダムに1つ選ぶ
                int randIndex = Random.Range(0, ingredients.Length);
                obstacle.SetIngredient(ingredients[randIndex].ingredientName, ingredients[randIndex].sprite);
            }
        }
    }
}