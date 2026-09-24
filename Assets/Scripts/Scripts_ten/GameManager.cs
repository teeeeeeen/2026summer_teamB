using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem; 
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI設定")]
    public GameObject gameOverUI;
    public GameObject gameClearUI; 
    public TextMeshProUGUI timerText; // タイマー用のテキスト窓
    public Image hpImage; // HP表示用のImageコンポーネント
    
    [Header("ポーズ（一時停止）設定")]
    public GameObject pauseUI; // ポーズ画面のパネル
    public GameObject zukanPanel; // 図鑑画面のパネル
    public string titleSceneName = "TitleScene"; // タイトルシーンの名前
    
    public bool isPaused { get; private set; } = false; // ポーズ状態の判定フラグ
    private bool isZukanOpen = false; // 図鑑が開いているかどうかのフラグ

    [Header("ポーズメニューのボタン設定")]
    public GameObject resumeButton; // 「ゲームに戻る」ボタン
    public GameObject zukanButton;  // 「図鑑を開く」ボタン
    public GameObject titleButton;  // 「タイトルへ」ボタン
    public GameObject arrow;        // 選択中を示す矢印オブジェクト
    private GameObject currentPauseButton; // 現在選択されているボタン

    [Header("シーン遷移・フェード設定")]
    public RawImage fadeMask;           // 画面全体を覆う黒いRawImage
    public float fadeDuration = 1.0f;   // フェードにかかる時間（秒）
    public float transitionDelay = 2.0f;// 完全にフェードインした後に待機する時間（秒）
    private bool isTransitioning = false; // 遷移中かどうかのフラグ

    // スティックの入力状態保持用
    private bool wasStickNext = false;
    private bool wasStickPrev = false;
    
    [Header("オーディオ設定")]
    public AudioSource bgmSource;     // BGM用のAudioSource
    public AudioSource uiAudioSource; // UI操作音（カーソルや決定音）用の専用AudioSource
    public AudioSource DamageSound;   // ダメージを受けたときの効果音
    public AudioSource Alart;         // HPが1になったときの警告音
    public float pinchBgmPitch = 1.2f; // HPが2以下の時のBGMピッチ
    public AudioClip selectSound;     // カーソル移動音
    public AudioClip decideSound;     // 決定音
    
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

    public float currentSpeedMultiplier { get; private set; } = 1f;
    public float elapsedTime { get; private set; } = 0f;

    private int currentHp; // 現在のHP
    private bool isGameActive = true;
    private bool isInvincible = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        currentHp = maxHp;
        elapsedTime = 0f;
        UpdateHpUI();
        UpdateBgmPitch();

        if (pauseUI != null) pauseUI.SetActive(false);
        if (zukanPanel != null) zukanPanel.SetActive(false); 
        if (arrow != null) arrow.SetActive(false);
        
        if (fadeMask != null)
        {
            StartCoroutine(FadeInRoutine());
        }

        if (playerObject != null)
        {
            playerRenderers = playerObject.GetComponentsInChildren<Renderer>();
        }

        if (uiAudioSource != null)
        {
            uiAudioSource.ignoreListenerPause = true;
        }

        SetupButton(resumeButton, ResumeGame);
        SetupButton(zukanButton, OpenZukan);
        SetupButton(titleButton, GoToTitle);
    }

    private void SetupButton(GameObject btnObj, UnityEngine.Events.UnityAction action)
    {
        if (btnObj != null)
        {
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners(); 
                btn.onClick.AddListener(action);

                Navigation nav = btn.navigation;
                nav.mode = Navigation.Mode.None;
                btn.navigation = nav;
            }
        }
    }

    private IEnumerator FadeInRoutine()
    {
        isTransitioning = true; 
        Time.timeScale = 0f; // 【修正】フェードイン中は完全に時間を止めて、裏で敵が動くのを防ぐ
        
        fadeMask.gameObject.SetActive(true);
        Color color = fadeMask.color;
        color.a = 1f; 
        fadeMask.color = color;

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(1f, 0f, time / fadeDuration);
            fadeMask.color = color;
            yield return null;
        }
        
        color.a = 0f;
        fadeMask.color = color;
        fadeMask.gameObject.SetActive(false);
        
        isTransitioning = false; 
        
        // ポーズ中でなければ時間を再開
        if (!isPaused) 
        {
            Time.timeScale = 1f;
        }
    }

    void Update()
    {
        if (!isGameActive) return;

        // 遷移中（フェードイン・アウト中）は入力を一切受け付けず、念のため時間も止めておく
        if (isTransitioning) 
        {
            Time.timeScale = 0f;
            return;
        }

        bool isEscape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool isNumpadPlus = Keyboard.current != null && Keyboard.current.numpadPlusKey.wasPressedThisFrame;
        bool isNumpadMinus = Keyboard.current != null && Keyboard.current.numpadMinusKey.wasPressedThisFrame;
        bool isStandardMinus = Keyboard.current != null && Keyboard.current.minusKey.wasPressedThisFrame;
        
        bool isGamepadPlus = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
        bool isGamepadMinus = Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame;

        bool togglePauseInput = isEscape || isNumpadPlus || isNumpadMinus || isStandardMinus || isGamepadPlus || isGamepadMinus;

        if (togglePauseInput)
        {
            if (isZukanOpen)
            {
                CloseZukan();
            }
            else
            {
                TogglePause();
            }
        }

        if (isPaused)
        {
            Time.timeScale = 0f; // 【修正】ポーズ中は強制的に時間を止める（他の要因での時間進行を完全にシャットアウト）

            if (isZukanOpen)
            {
                bool cancelZukan = false;
                if (Gamepad.current != null) cancelZukan |= Gamepad.current.buttonSouth.wasPressedThisFrame;
                if (Keyboard.current != null) cancelZukan |= Keyboard.current.backspaceKey.wasPressedThisFrame; 
                
                if (cancelZukan) CloseZukan();
            }
            else
            {
                bool cancelPause = false;
                if (Gamepad.current != null) cancelPause |= Gamepad.current.buttonSouth.wasPressedThisFrame;
                if (Keyboard.current != null) cancelPause |= Keyboard.current.backspaceKey.wasPressedThisFrame;

                if (cancelPause)
                {
                    PlayDecideSound();
                    ResumeGame();
                }
                else
                {
                    HandlePauseMenuInput();
                }
            }
        }
        else
        {
            Time.timeScale = 1f; // 【修正】通常時は確実に時間を進める

            elapsedTime += Time.deltaTime;
            currentSpeedMultiplier = 1f + (elapsedTime / timeToDoubleSpeed);

            if (timerText != null)
            {
                timerText.text = "TIME: " + Mathf.FloorToInt(elapsedTime).ToString();
            }
        }
    }

    public void TogglePause()
    {
        // 【追加】遷移中（フェードイン中など）はUIボタンから呼ばれても強制ブロックする
        if (!isGameActive || isTransitioning) return;

        PlayDecideSound();
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pauseUI != null) pauseUI.SetActive(true);
            if (bgmSource != null) bgmSource.Pause();

            currentPauseButton = resumeButton;
            SelectPauseButton();
        }
        else
        {
            Time.timeScale = 1f;
            if (pauseUI != null) pauseUI.SetActive(false);
            if (zukanPanel != null) zukanPanel.SetActive(false); 
            isZukanOpen = false;
            if (bgmSource != null) bgmSource.UnPause();
        }
    }

    private void HandlePauseMenuInput()
    {
        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;
        if (selectedObj != null && selectedObj != currentPauseButton)
        {
            if (selectedObj == resumeButton || selectedObj == zukanButton || selectedObj == titleButton)
            {
                currentPauseButton = selectedObj;
                UpdateArrowPosition();
            }
        }
        else if (selectedObj == null && currentPauseButton != null)
        {
            EventSystem.current.SetSelectedGameObject(currentPauseButton);
        }

        bool moveNext = false; 
        bool movePrev = false; 
        bool submit = false;   

        if (Keyboard.current != null)
        {
            moveNext |= Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame;
            movePrev |= Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame;
            submit |= Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame;
        }

        if (Gamepad.current != null)
        {
            moveNext |= Gamepad.current.dpad.right.wasPressedThisFrame;
            movePrev |= Gamepad.current.dpad.left.wasPressedThisFrame;

            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            bool isStickNext = stick.x > 0.5f;
            bool isStickPrev = stick.x < -0.5f;

            if (isStickNext && !wasStickNext) moveNext = true;
            if (isStickPrev && !wasStickPrev) movePrev = true;

            wasStickNext = isStickNext;
            wasStickPrev = isStickPrev;

            submit |= Gamepad.current.buttonEast.wasPressedThisFrame;
        }

        if (moveNext)
        {
            if (currentPauseButton == resumeButton) currentPauseButton = zukanButton;
            else if (currentPauseButton == zukanButton) currentPauseButton = titleButton;
            else currentPauseButton = resumeButton;

            PlaySelectSound();
            SelectPauseButton();
        }
        else if (movePrev)
        {
            if (currentPauseButton == resumeButton) currentPauseButton = titleButton;
            else if (currentPauseButton == zukanButton) currentPauseButton = resumeButton;
            else currentPauseButton = zukanButton;

            PlaySelectSound();
            SelectPauseButton();
        }

        if (submit && currentPauseButton != null)
        {
            Button btn = currentPauseButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.Invoke();
            }
        }
    }

    private void SelectPauseButton()
    {
        if (currentPauseButton == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(currentPauseButton);
        UpdateArrowPosition();
    }

    private void UpdateArrowPosition()
    {
        if (arrow != null && currentPauseButton != null)
        {
            arrow.SetActive(true);
            arrow.transform.SetParent(currentPauseButton.transform, false);
            
            RectTransform arrowRect = arrow.GetComponent<RectTransform>();
            if (arrowRect != null)
            {
                arrowRect.anchorMin = new Vector2(0.5f, 1f);
                arrowRect.anchorMax = new Vector2(0.5f, 1f);
                arrowRect.pivot = new Vector2(0.5f, 0f);
                arrowRect.anchoredPosition = new Vector2(0f, 15f); 
            }
        }
    }

    private void PlaySelectSound()
    {
        if (selectSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(selectSound);
        }
    }

    private void PlayDecideSound()
    {
        if (decideSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(decideSound);
        }
    }

    public void ResumeGame()
    {
        if (isPaused) TogglePause();
    }

    public void OpenZukan()
    {
        if (zukanPanel != null)
        {
            PlayDecideSound(); 
            zukanPanel.SetActive(true);
            if (pauseUI != null) pauseUI.SetActive(false);
            isZukanOpen = true;
        }
    }

    public void CloseZukan()
    {
        if (zukanPanel != null)
        {
            PlayDecideSound(); 
            zukanPanel.SetActive(false);
            if (pauseUI != null) pauseUI.SetActive(true);
            isZukanOpen = false;
            
            currentPauseButton = zukanButton;
            SelectPauseButton();
        }
    }

    public void GoToTitle()
    {
        if (isTransitioning) return;
        PlayDecideSound(); 
        StartCoroutine(FadeAndLoadScene(titleSceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        isTransitioning = true;
        Time.timeScale = 0f; // 【修正】フェードアウト中も時間を完全に止める
        
        if (fadeMask != null)
        {
            fadeMask.gameObject.SetActive(true);
            Color color = fadeMask.color;
            color.a = 0f;
            fadeMask.color = color;

            float time = 0f;
            while (time < fadeDuration)
            {
                time += Time.unscaledDeltaTime; 
                color.a = Mathf.Lerp(0f, 1f, time / fadeDuration);
                fadeMask.color = color;
                yield return null;
            }
            
            color.a = 1f;
            fadeMask.color = color;
            
            yield return new WaitForSecondsRealtime(transitionDelay);
        }

        Time.timeScale = 1f;
        AudioListener.pause = false; 
        SceneManager.LoadScene(sceneName);
    }

    // ==========================================
    // 既存のダメージ・回復・ゲームオーバー処理
    // ==========================================
    public void TakeDamage(int amount = 1)
    {
        if (!isGameActive || isInvincible) return;
        
        if (DamageSound != null) DamageSound.Play(); 

        int previousHp = currentHp;
        currentHp -= amount;

        if (currentHp == 1 && Alart != null) Alart.Play();

        UpdateBgmPitch();

        if (currentHp <= 0)
        {
            currentHp = 0;
            UpdateHpUI();
            TriggerGameOver();
        }
        else
        {
            StartCoroutine(DamageBlinkRoutine(previousHp));
        }
    }

    public void Heal(int amount = 1)
    {
        if (!isGameActive) return;

        currentHp += amount;
        if (currentHp > maxHp) currentHp = maxHp;

        UpdateBgmPitch();
        if (!isInvincible) UpdateHpUI();
    }

    private void UpdateBgmPitch()
    {
        if (bgmSource != null && isGameActive)
        {
            if (currentHp <= 0) return; 

            if (currentHp <= 2) bgmSource.pitch = pinchBgmPitch; 
            else bgmSource.pitch = 1.0f; 
        }
    }

    private void UpdateHpUI()
    {
        if (hpImage != null && hpSprites != null && hpSprites.Length > 0)
        {
            int spriteIndex = Mathf.Clamp(currentHp, 0, hpSprites.Length - 1);
            hpImage.sprite = hpSprites[spriteIndex];
        }
    }

    private IEnumerator DamageBlinkRoutine(int previousHp)
    {
        isInvincible = true; 
        float timer = 0f;
        bool toggle = false;

        int prevIndex = Mathf.Clamp(previousHp, 0, hpSprites.Length - 1);

        while (timer < invincibleDuration)
        {
            toggle = !toggle; 
            
            if (hpImage != null && hpSprites.Length > 0)
            {
                int currentIndex = Mathf.Clamp(currentHp, 0, hpSprites.Length - 1);
                hpImage.sprite = toggle ? hpSprites[prevIndex] : hpSprites[currentIndex];
            }

            if (playerRenderers != null)
            {
                foreach (Renderer r in playerRenderers) if (r != null) r.enabled = toggle;
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        UpdateHpUI(); 
        
        if (playerRenderers != null)
        {
            foreach (Renderer r in playerRenderers) if (r != null) r.enabled = true;
        }

        isInvincible = false; 
    }

    public void TriggerGameOver()
    {
        if (!isGameActive) return;
        isGameActive = false; 
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        float duration = 2.0f; 
        float timer = 0f;
        bool toggle = false;
        float blinkTimer = 0f;

        float startPitch = bgmSource != null ? bgmSource.pitch : 1f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            blinkTimer += Time.unscaledDeltaTime;
            float progress = timer / duration;

            Time.timeScale = Mathf.Lerp(1f, 0f, progress);

            if (bgmSource != null) bgmSource.pitch = Mathf.Lerp(startPitch, 0f, progress);

            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                toggle = !toggle;

                if (hpImage != null) hpImage.enabled = toggle;
                
                if (playerRenderers != null)
                {
                    foreach (Renderer r in playerRenderers) if (r != null) r.enabled = toggle;
                }
            }
            yield return null;
        }

        Time.timeScale = 0f;

        if (hpImage != null) hpImage.enabled = false;
        if (playerRenderers != null)
        {
            foreach (Renderer r in playerRenderers) if (r != null) r.enabled = false;
        }

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            
            CanvasGroup cg = gameOverUI.GetComponent<CanvasGroup>();
            if (cg == null) cg = gameOverUI.AddComponent<CanvasGroup>();
            
            cg.alpha = 0f; 
            float fadeTimer = 0f;
            float fadeDuration = 1.0f; 

            while (fadeTimer < fadeDuration)
            {
                fadeTimer += Time.unscaledDeltaTime;
                cg.alpha = fadeTimer / fadeDuration;
                yield return null;
            }
            cg.alpha = 1f; 
        }
    }

    public void TriggerGameClear()
    {
        if (!isGameActive) return;
        isGameActive = false;
        Time.timeScale = 0f; 
        if (gameClearUI != null) gameClearUI.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}