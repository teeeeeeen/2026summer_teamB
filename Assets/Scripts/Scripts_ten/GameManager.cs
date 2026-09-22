using UnityEngine;
using UnityEngine.UI; // Imageを操作するために必要
using UnityEngine.SceneManagement;
using TMPro; // TextMeshProを使うために必要

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI設定")]
    public GameObject gameOverUI;
    public GameObject gameClearUI; // 新設：クリアUI
    public TextMeshProUGUI timerText; // 新設：タイマー用のテキスト窓
    public Image hpGaugeImage; // 新設：HPゲージ用のImage

    [Header("ゲーム設定")]
    public float remainingTime = 180f; // 3分（180秒）
    public int maxHp = 5; // 新設：最大HP

    private int currentHp; // 現在のHP
    private bool isGameActive = true;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // ゲーム開始時にHPを最大値に初期化し、ゲージを更新
        currentHp = maxHp;
        UpdateHpUI();
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

    // 新設：ダメージを受ける処理
    public void TakeDamage(int amount = 1)
    {
        if (!isGameActive) return;

        currentHp -= amount;

        // HPが0以下になったらゲームオーバー
        if (currentHp <= 0)
        {
            currentHp = 0;
            UpdateHpUI();
            TriggerGameOver();
        }
        else
        {
            UpdateHpUI();
        }
    }

    // 新設：HPを回復する処理（形だけ作成）
    public void Heal(int amount = 1)
    {
        if (!isGameActive) return;

        currentHp += amount;

        // 最大HPを超えないように制限
        if (currentHp > maxHp)
        {
            currentHp = maxHp;
        }

        UpdateHpUI();
    }

    // 新設：HPゲージの表示を更新する処理
    private void UpdateHpUI()
    {
        if (hpGaugeImage != null)
        {
            // currentHp / maxHp で 0.0 ~ 1.0 の割合を計算し、FillAmountに適用
            hpGaugeImage.fillAmount = (float)currentHp / maxHp;
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