using UnityEngine;
using UnityEngine.UI; // Imageを操作するために必要
using UnityEngine.SceneManagement;
using TMPro; // TextMeshProを使うために必要
using System.Collections; // コルーチン（IEnumerator）を使うために必要

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI設定")]
    public GameObject gameOverUI;
    public GameObject gameClearUI; // クリアUI
    public TextMeshProUGUI timerText; // タイマー用のテキスト窓
    public Image hpImage; // HP表示用のImageコンポーネント
    public AudioSource DamageSound; // ダメージを受けたときの効果音
    public AudioSource Alart; // HPが1になったときの警告音
    
    [Header("HPスプライト設定")]
    [Tooltip("インデックス0にHP0の画像、インデックス6にHP6の画像になるよう、計7枚セットしてください。")]
    public Sprite[] hpSprites; // 状態ごとのスプライト配列

    [Header("無敵・点滅設定")]
    public float invincibleDuration = 1.5f; // 無敵になる時間（秒）
    public float blinkInterval = 0.1f;      // 点滅の切り替え間隔（秒）
    [Tooltip("点滅させたいバイクの親オブジェクトをアタッチしてください（全ての子パーツが点滅します）")]
    public GameObject playerObject;         // 【変更】プレイヤーの親オブジェクト
    
    private Renderer[] playerRenderers;     // 【追加】バイクを構成する全パーツのRendererを格納する配列

    [Header("ゲーム設定")]
    public float remainingTime = 180f; // 3分（180秒）
    public int maxHp = 6; // 最大HPを6に変更

    private int currentHp; // 現在のHP
    private bool isGameActive = true;
    private bool isInvincible = false; // 無敵状態かどうかを判定するフラグ

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // ゲーム開始時にHPを最大値に初期化し、画像を更新
        currentHp = maxHp;
        UpdateHpUI();

        // 【追加】プレイヤーオブジェクトがセットされていれば、その中にある全てのRenderer（見た目のパーツ）を取得する
        if (playerObject != null)
        {
            playerRenderers = playerObject.GetComponentsInChildren<Renderer>();
        }
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
        // ゲームがアクティブでない、または【無敵状態】ならダメージ処理をしない
        if (!isGameActive || isInvincible) return;
        
        if (DamageSound != null)
        {
            DamageSound.Play(); 
        }

        // ダメージを受ける前のHPを記憶しておく
        int previousHp = currentHp;
        
        currentHp -= amount;

        if (currentHp == 1)
        {
            if (Alart != null)
            {
                Alart.Play();
            }
        }

        // HPが0以下になったらゲームオーバー
        if (currentHp <= 0)
        {
            currentHp = 0;
            UpdateHpUI();
            TriggerGameOver();
        }
        else
        {
            // まだ生きている場合は、点滅＆無敵処理を開始する
            StartCoroutine(DamageBlinkRoutine(previousHp));
        }
    }

    // 点滅と無敵時間を管理するコルーチン
    private IEnumerator DamageBlinkRoutine(int previousHp)
    {
        isInvincible = true; // 無敵オン
        float timer = 0f;
        bool toggle = false;

        // ダメージ前のHPの配列インデックス（範囲外エラー防止）
        int prevIndex = Mathf.Clamp(previousHp, 0, hpSprites.Length - 1);

        while (timer < invincibleDuration)
        {
            toggle = !toggle; // trueとfalseを反転
            
            // UIのHP画像を「ダメージ前」と「ダメージ後(現在)」で交互に切り替え
            if (hpImage != null && hpSprites.Length > 0)
            {
                int currentIndex = Mathf.Clamp(currentHp, 0, hpSprites.Length - 1);
                hpImage.sprite = toggle ? hpSprites[prevIndex] : hpSprites[currentIndex];
            }

            // 【変更】バイクを構成するすべてのパーツの表示/非表示を切り替えて点滅させる
            if (playerRenderers != null)
            {
                foreach (Renderer r in playerRenderers)
                {
                    if (r != null) r.enabled = toggle;
                }
            }

            // blinkInterval（0.1秒など）だけ待機
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // 無敵時間が終わったら、正しい状態に戻す
        UpdateHpUI(); // 現在のHP画像を確実に表示
        
        // 【変更】バイクの全パーツを確実に表示状態に戻す
        if (playerRenderers != null)
        {
            foreach (Renderer r in playerRenderers)
            {
                if (r != null) r.enabled = true;
            }
        }

        isInvincible = false; // 無敵オフ
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

        // 無敵（点滅）中でなければ、通常通りUIを更新する
        // 点滅中なら、コルーチンの終了時に正しいHPに更新されます
        if (!isInvincible)
        {
            UpdateHpUI();
        }
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