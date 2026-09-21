using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshProを使うために必要

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI設定")]
    public GameObject gameOverUI;
    public GameObject gameClearUI; // 新設：クリアUI
    public TextMeshProUGUI timerText; // 新設：タイマー用のテキスト窓

    [Header("ゲーム設定")]
    public float remainingTime = 180f; // 3分（180秒）

    private bool isGameActive = true;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Update()
    {
        // ゲームが進行中（クリアでもゲームオーバーでもない）のときだけ時間を減らす
        if (isGameActive)
        {
            // 残り時間を減らす
            remainingTime -= Time.deltaTime;

            // タイマー表示を更新（少数点を切り捨てて整数の秒数にする）
            if (timerText != null)
            {
                timerText.text = "TIME: " + Mathf.CeilToInt(remainingTime).ToString();
            }

            // 0秒になったらクリア処理
            if (remainingTime <= 0f)
            {
                TriggerGameClear();
            }
        }
    }

    // ゲームオーバーになったときに呼ばれる関数
    public void TriggerGameOver()
    {
        if (!isGameActive) return;
        isGameActive = false;

        Time.timeScale = 0f; // 時間を止める
        if (gameOverUI != null) gameOverUI.SetActive(true);
    }

    // 新設：3分耐えきったときに呼ばれる関数
    public void TriggerGameClear()
    {
        if (!isGameActive) return;
        isGameActive = false;

        remainingTime = 0f; // マイナス表示にならないよう固定
        if (timerText != null) timerText.text = "TIME: 0";

        Time.timeScale = 0f; // 時間を止める（障害物などの動きが止まる）
        if (gameClearUI != null) gameClearUI.SetActive(true);
    }

    // ボタン用：もう一度遊ぶ関数（クリア・ゲームオーバー共通で使えます）
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}