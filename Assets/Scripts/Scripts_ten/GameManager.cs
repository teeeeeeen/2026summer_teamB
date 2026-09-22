using UnityEngine;
using UnityEngine.UI; // Imageを操作するために必要
using UnityEngine.SceneManagement;
using TMPro; // TextMeshProを使うために必要

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI設定")]
    public GameObject gameOverUI;
    public GameObject gameClearUI; // クリアUI
    public TextMeshProUGUI timerText; // タイマー用のテキスト窓
    public Image hpImage; // HP表示用のImageコンポーネント
    public AudioSource DamageSound; // ダメージを受けたときの効果音

    [Header("HPスプライト設定")]
    [Tooltip("インデックス0にHP0の画像、インデックス6にHP6の画像になるよう、計7枚セットしてください。")]
    public Sprite[] hpSprites; // 状態ごとのスプライト配列

    [Header("ゲーム設定")]
    public float remainingTime = 180f; // 3分（180秒）
    public int maxHp = 6; // 最大HPを6に変更

    private int currentHp; // 現在のHP
    private bool isGameActive = true;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // ゲーム開始時にHPを最大値に初期化し、画像を更新
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

    // ダメージを受ける処理
    public void TakeDamage(int amount = 1)
    {
        if (!isGameActive) return;
        AudioSource.PlayClipAtPoint(DamageSound.clip, transform.position); // ダメージ音を再生

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

    // HPを回復する処理
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

    // HP画像の表示を更新する処理
    private void UpdateHpUI()
    {
        if (hpImage != null && hpSprites != null && hpSprites.Length > 0)
        {
            // 現在のHPが配列の範囲外にならないよう安全対策をしてからスプライトを適用
            int spriteIndex = Mathf.Clamp(currentHp, 0, hpSprites.Length - 1);
            hpImage.sprite = hpSprites[spriteIndex];
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

    // 3分耐えきったときに呼ばれる関数
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