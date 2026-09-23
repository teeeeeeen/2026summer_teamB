using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MainMenuController : MonoBehaviour
{
    [Header("パネル設定")]
    public GameObject mainMenuPanel;
    public GameObject howToPlayPanel;

    [Header("ボタン設定")]
    public GameObject startButton;
    public GameObject howToButton;
    public GameObject quitButton;
    public GameObject arrow;

    [Header("シーン遷移・フェード設定")]
    public RawImage fadeMask;
    [Tooltip("フェードにかかる時間")]
    public float fadeDuration = 1.0f;
    [Tooltip("フェード完了からシーン遷移までの待機時間（秒）")]
    public float transitionDelay = 1.0f; // 追加：待機時間
    [Tooltip("スタート時に遷移するシーン名")]
    public string nextSceneName = "Main";

    [Header("サウンド設定")]
    public AudioClip selectSound;
    public AudioClip decideSound;
    private AudioSource audioSource;

    private GameObject currentButton;
    private bool isTransitioning = false;
    
    // スティックの入力状態保持
    private bool wasStickNext = false;
    private bool wasStickPrev = false;

    // パネル開閉のクールタイム用
    private float howToOpenTime = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        SetupButton(startButton, OnStartClicked);
        SetupButton(howToButton, OnHowToClicked);
        SetupButton(quitButton, OnQuitClicked);

        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        
        currentButton = startButton;
        SelectButton();

        if (fadeMask != null)
        {
            StartCoroutine(FadeInCoroutine());
        }
    }

    private void SetupButton(GameObject btnObj, UnityEngine.Events.UnityAction action)
    {
        if (btnObj != null)
        {
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(action);
            }
        }
    }

    void Update()
    {
        if (isTransitioning) return;

        // 遊び方パネルが開いている時の処理
        if (howToPlayPanel != null && howToPlayPanel.activeSelf)
        {
            // 開いた瞬間に「決定ボタン」の判定を拾って即閉じしてしまうのを防ぐため、0.2秒のクールタイムを設ける
            if (Time.unscaledTime - howToOpenTime > 0.2f)
            {
                bool cancel = false;
                if (Gamepad.current != null) cancel |= Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame;
                if (Keyboard.current != null) cancel |= Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.backspaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.zKey.wasPressedThisFrame;

                if (cancel) CloseHowTo();
            }
            return; 
        }

        bool moveNext = false; // 右・下
        bool movePrev = false; // 左・上
        bool submit = false;

        // キーボード入力
        if (Keyboard.current != null)
        {
            moveNext |= Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame;
            movePrev |= Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame;
            submit |= Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.zKey.wasPressedThisFrame;
        }

        // コントローラー入力
        if (Gamepad.current != null)
        {
            moveNext |= Gamepad.current.dpad.right.wasPressedThisFrame || Gamepad.current.dpad.down.wasPressedThisFrame;
            movePrev |= Gamepad.current.dpad.left.wasPressedThisFrame || Gamepad.current.dpad.up.wasPressedThisFrame;

            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            bool isStickNext = stick.x > 0.5f || stick.y < -0.5f;
            bool isStickPrev = stick.x < -0.5f || stick.y > 0.5f;

            if (isStickNext && !wasStickNext) moveNext = true;
            if (isStickPrev && !wasStickPrev) movePrev = true;

            wasStickNext = isStickNext;
            wasStickPrev = isStickPrev;

            submit |= Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.buttonEast.wasPressedThisFrame;
        }

        // 移動処理
        if (moveNext)
        {
            if (currentButton == startButton) currentButton = howToButton;
            else if (currentButton == howToButton) currentButton = quitButton;
            else currentButton = startButton;

            PlaySelectSound();
            SelectButton();
        }
        else if (movePrev)
        {
            if (currentButton == startButton) currentButton = quitButton;
            else if (currentButton == howToButton) currentButton = startButton;
            else currentButton = howToButton;

            PlaySelectSound();
            SelectButton();
        }

        // 決定処理
        if (submit)
        {
            Button btn = currentButton.GetComponent<Button>();
            if (btn != null) btn.onClick.Invoke();
        }
    }

    public void OnStartClicked()
    {
        if (isTransitioning) return;
        PlayDecideSound();
        StartCoroutine(FadeOutAndLoad());
    }

    public void OnHowToClicked()
    {
        if (isTransitioning) return;
        PlayDecideSound();
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        
        howToOpenTime = Time.unscaledTime;
    }

    public void CloseHowTo()
    {
        PlayDecideSound();
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void OnQuitClicked()
    {
        if (isTransitioning) return;
        PlayDecideSound();
        isTransitioning = true;
        
        Debug.Log("ゲーム終了");
        Application.Quit();
    }

    private void SelectButton()
    {
        startButton.GetComponentInChildren<ScrollingBack>()?.SetSelected(false);
        howToButton.GetComponentInChildren<ScrollingBack>()?.SetSelected(false);
        quitButton.GetComponentInChildren<ScrollingBack>()?.SetSelected(false);

        currentButton.GetComponentInChildren<ScrollingBack>()?.SetSelected(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(currentButton);

        if (arrow != null)
        {
            arrow.transform.SetParent(currentButton.transform, false);
            RectTransform arrowRect = arrow.GetComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(0.5f, 1f);
            arrowRect.anchorMax = new Vector2(0.5f, 1f);
            arrowRect.pivot = new Vector2(0.5f, 0f);
            arrowRect.anchoredPosition = new Vector2(15f, -35f);
            arrow.SetActive(true);
        }
    }

    private void PlaySelectSound()
    {
        if (selectSound != null && audioSource != null) audioSource.PlayOneShot(selectSound);
    }

    private void PlayDecideSound()
    {
        if (decideSound != null && audioSource != null) audioSource.PlayOneShot(decideSound);
    }

    private IEnumerator FadeInCoroutine()
    {
        isTransitioning = true;
        fadeMask.gameObject.SetActive(true);
        Color color = fadeMask.color;
        color.a = 1f;
        fadeMask.color = color;

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, time / fadeDuration);
            fadeMask.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeMask.color = color;
        fadeMask.gameObject.SetActive(false);
        isTransitioning = false; 
    }

    private IEnumerator FadeOutAndLoad()
    {
        isTransitioning = true;
        fadeMask.gameObject.SetActive(true);
        Color color = fadeMask.color;
        color.a = 0f;
        fadeMask.color = color;

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, time / fadeDuration);
            fadeMask.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeMask.color = color;

        // 追加：フェード完了後、指定した秒数（デフォルト1秒）待機する
        yield return new WaitForSeconds(transitionDelay);

        SceneManager.LoadScene(nextSceneName);
    }
}