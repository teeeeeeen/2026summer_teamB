using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    [Header("生成するプレハブ")]
    public GameObject prefabToSpawn;

    [Header("生成間隔（秒）")]
    public float spawnInterval = 1.0f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        
        // GameManagerから現在の倍率を取得し、間隔を短く（速く）する
        float currentMultiplier = GameManager.instance != null ? GameManager.instance.currentSpeedMultiplier : 1f;
        float currentInterval = spawnInterval / currentMultiplier;

        if (timer >= currentInterval)
        {
            if (prefabToSpawn != null)
            {
                // スポナー自身の位置（transform.position）と回転で生成する
                Instantiate(prefabToSpawn, transform.position, transform.rotation);
            }
            
            timer = 0f;
        }
    }
}