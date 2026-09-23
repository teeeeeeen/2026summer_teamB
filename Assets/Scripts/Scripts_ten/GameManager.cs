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
    public GameObject gameClearUI; // クリアUI（時間経過でのクリアは廃止しましたが、他からの呼び出し用に残しています）
    public TextMeshProUGUI timerText; // タイマー用のテキスト窓
    public Image hpImage; // HP表示用のImageコンポーネント
    
    [Header("オーディオ設定")]
    public AudioSource bgmSource;   // BGM用のAudioSource
    public AudioSource DamageSound; // ダメージを受けたときの効果音
    public AudioSource Alart;       // HPが1になったときの警告音
    public float pinchBgmPitch = 1.2f; // HPが2以下の時のBGMピッチ
    
    [Header("HPスプライト設定")]
    [Tooltip("インデックス0にHP0の画像、インデックス6にHP6の画像になるよう、計7枚セットしてください。")]
    public Sprite[] hpSprites; // 状態ごとのスプライト配列

    [Header("無敵・点滅設定")]
    public float invincibleDuration = 1.5f; // 無敵になる時間（秒）
    public float blinkInterval = 0.1f;      // 点滅の切り替え間隔（秒）
    [Tooltip("点滅させたいバイクの親オブジェクトをアタッチしてください（全ての子パーツが点滅します）")]
    public GameObject playerObject;         // プレイヤーの親オブジェクト
    
    private Renderer[] playerRenderers;     // バイクを構成する全パーツのRendererを格納する配列

    [Header("ゲーム設定")]
    public int maxHp = 6; // 最大HP
    
    [Header("難易度上昇設定")]
    public float timeToDoubleSpeed = 120f;  // 倍速になるまでの秒数（120秒＝2分）

    // スポナー側で取得して出現間隔を早めるために使う変数
    public float currentSpeedMultiplier { get; private set; } = 1f;

    // 【追加】現在の経過時間を記録する変数（外部のスクリプトからも参照可能）
    public float elapsedTime { get; private set; } = 0f;

    private int currentHp; // 現在のHP
    private bool isGameActive = true;
    private bool isInvincible = false; // 無敵状態かどうかを判定するフラグ

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // ゲーム開始時にHPを最大値に初期化
        currentHp = maxHp;
        elapsedTime = 0f; // 経過時間をリセット
        UpdateHpUI();
        UpdateBgmPitch(); // 初期化時にBGMのピッチを通常にセット

        // プレイヤーオブジェクトがセットされていれば、その中にある全てのRenderer（見た目のパーツ）を取得する
        if (playerObject != null)
        {
            playerRenderers = playerObject.GetComponentsInChildren<Renderer>();
        }
    }

    void Update()
    {
        // ゲームが進行中（クリアでもゲームオーバーでもない）のときだけ時間を加算する
        if (isGameActive)
        {
            // 【変更】時間を加算する
            elapsedTime += Time.deltaTime;

            // 経過時間から難易度の倍率を計算（なだらかな一次関数のグラフ）
            currentSpeedMultiplier = 1f + (elapsedTime / timeToDoubleSpeed);

            // タイマー表示を更新（少数点を切り捨てて整数の秒数にする）
            if (timerText != null)
            {
                timerText.text = "TIME: " + Mathf.FloorToInt(elapsedTime).ToString();
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

        // HPが変動したのでBGMのピッチを更新する
        UpdateBgmPitch();

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

        // HPが変動したのでBGMのピッチを更新する
        UpdateBgmPitch();

        // 無敵（点滅）中でなければ、通常通りUIを更新する
        if (!isInvincible)
        {
            UpdateHpUI();
        }
    }

    // HPに応じてBGMのピッチを変更する処理
    private void UpdateBgmPitch()
    {
        if (bgmSource != null && isGameActive)
        {
            if (currentHp <= 0) return; // 死亡時はゲームオーバー用のスロー演出に任せるため無視

            if (currentHp <= 2)
            {
                bgmSource.pitch = pinchBgmPitch; // ピンチ時は少し高くする
            }
            else
            {
                bgmSource.pitch = 1.0f; // 通常時
            }
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

    // 通常ダメージ時の点滅と無敵時間を管理するコルーチン
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

            // バイクを構成するすべてのパーツの表示/非表示を切り替えて点滅させる
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
        
        // バイクの全パーツを確実に表示状態に戻す
        if (playerRenderers != null)
        {
            foreach (Renderer r in playerRenderers)
            {
                if (r != null) r.enabled = true;
            }
        }

        isInvincible = false; // 無敵オフ
    }

    // ゲームオーバーになったときに呼ばれる関数
    public void TriggerGameOver()
    {
        if (!isGameActive) return;
        isGameActive = false; // これ以降ダメージや時間経過を停止

        // 即座に停止するのではなく、ゲームオーバー用のコルーチンを開始する
        StartCoroutine(GameOverRoutine());
    }

    // ゲームオーバー時のスローモーション＆点滅＆フェードイン演出
    private IEnumerator GameOverRoutine()
    {
        float duration = 2.0f; // スローダウンにかける時間（2秒）
        float timer = 0f;
        bool toggle = false;
        float blinkTimer = 0f;

        // 死んだ瞬間のピッチを取得（ピンチ中なら高い状態からゆっくり落ちる）
        float startPitch = bgmSource != null ? bgmSource.pitch : 1f;

        // 2秒かけて徐々に動きを止める
        while (timer < duration)
        {
            // timeScaleが下がっていくため、現実時間（unscaledDeltaTime）を使って計測する
            timer += Time.unscaledDeltaTime;
            blinkTimer += Time.unscaledDeltaTime;

            float progress = timer / duration;

            // 時間の進み方を1から0へ徐々に下げる
            Time.timeScale = Mathf.Lerp(1f, 0f, progress);

            // BGMも一緒にゆっくり止まっていく（ピッチダウン）演出
            if (bgmSource != null)
            {
                bgmSource.pitch = Mathf.Lerp(startPitch, 0f, progress);
            }

            // プレイヤーとHPUIを点滅させる（表示・非表示の切り替え）
            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                toggle = !toggle;

                if (hpImage != null) hpImage.enabled = toggle;
                
                if (playerRenderers != null)
                {
                    foreach (Renderer r in playerRenderers)
                    {
                        if (r != null) r.enabled = toggle;
                    }
                }
            }

            yield return null;
        }

        // 完全に時間を止める
        Time.timeScale = 0f;

        // 点滅で消えたままにならないよう、最後は非表示状態で確定させておく
        if (hpImage != null) hpImage.enabled = false;
        if (playerRenderers != null)
        {
            foreach (Renderer r in playerRenderers)
            {
                if (r != null) r.enabled = false;
            }
        }

        // ゲームオーバーUIのフェードイン処理
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            
            // CanvasGroupがない場合は自動で追加する
            CanvasGroup cg = gameOverUI.GetComponent<CanvasGroup>();
            if (cg == null) cg = gameOverUI.AddComponent<CanvasGroup>();
            
            cg.alpha = 0f; // 最初は透明
            float fadeTimer = 0f;
            float fadeDuration = 1.0f; // フェードインにかける時間（1秒）

            while (fadeTimer < fadeDuration)
            {
                fadeTimer += Time.unscaledDeltaTime;
                cg.alpha = fadeTimer / fadeDuration;
                yield return null;
            }
            
            cg.alpha = 1f; // 完全に表示
        }
    }

    // ゲームクリア時に呼ばれる関数（特定のスコア到達などで別のスクリプトから呼べるよう残しています）
    public void TriggerGameClear()
    {
        if (!isGameActive) return;
        isGameActive = false;

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