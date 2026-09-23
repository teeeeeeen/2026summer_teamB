using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class MenuNavigator : MonoBehaviour
{
    [Header("ボタン設定")]
    public GameObject startButton;
    public GameObject howToButton;
    public GameObject quitButton;
    public GameObject arrow;

    [Header("サウンド設定")]
    public AudioClip selectSound;
    public AudioClip decideSound;

    [Header("フェード設定")]
    [Tooltip("フェード用のマスク (RawImage)")]
    public RawImage fadeMask;
    [Tooltip("フェードインにかかる時間")]
    public float fadeDuration = 1.0f;

    private AudioSource audioSource;
    private GameObject currentButton;

    // アナログスティックの連続入力を防ぐための状態保持
    private bool wasStickRight = false;
    private bool wasStickLeft = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // マウスクリック時にも決定音が鳴るように、各ボタンのイベントに直接音を紐づける
        RegisterClickSound(startButton);
        RegisterClickSound(howToButton);
        RegisterClickSound(quitButton);

        currentButton = startButton;
        SelectButton();

        // RawImageが設定されていればフェードイン（不透明→透明）を開始
        if (fadeMask != null)
        {
            StartCoroutine(FadeInCoroutine());
        }
    }

    void Update()
    {
        bool moveRight = false;
        bool moveLeft = false;
        bool submit = false;

        // キーボード入力
        if (Keyboard.current != null)
        {
            moveRight |= Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame;
            moveLeft |= Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame;
            submit |= Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.zKey.wasPressedThisFrame;
        }

        // コントローラー（ゲームパッド）入力
        if (Gamepad.current != null)
        {
            // 十字キー
            moveRight |= Gamepad.current.dpad.right.wasPressedThisFrame;
            moveLeft |= Gamepad.current.dpad.left.wasPressedThisFrame;

            // スティック (0.5以上倒した瞬間のみ判定する処理)
            float stickX = Gamepad.current.leftStick.x.ReadValue();
            bool isStickRight = stickX > 0.5f;
            bool isStickLeft = stickX < -0.5f;

            if (isStickRight && !wasStickRight) moveRight = true;
            if (isStickLeft && !wasStickLeft) moveLeft = true;

            wasStickRight = isStickRight;
            wasStickLeft = isStickLeft;

            // Aボタン(South) または Bボタン(East) で決定
            submit |= Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.buttonEast.wasPressedThisFrame;
        }

        // 右移動処理
        if (moveRight)
        {
            if (currentButton == startButton) currentButton = howToButton;
            else if (currentButton == howToButton) currentButton = quitButton;
            else currentButton = startButton;

            PlaySelectSound();
            SelectButton();
        }

        // 左移動処理
        if (moveLeft)
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
            Button button = currentButton.GetComponent<Button>();
            if (button != null)
            {
                // UIボタンのクリック処理を実行 (RegisterClickSoundで設定した音もここで鳴る)
                button.onClick.Invoke();
            }
        }
    }

    // マウスクリック時に決定音を鳴らすためのリスナー登録
    private void RegisterClickSound(GameObject buttonObj)
    {
        if (buttonObj != null)
        {
            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(PlayDecideSound);
            }
        }
    }

    void PlaySelectSound()
    {
        if (selectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(selectSound);
        }
    }

    void PlayDecideSound()
    {
        if (decideSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(decideSound);
        }
    }

    void SelectButton()
    {
        // 全ボタンの背景を停止
        startButton.GetComponentInChildren<ScrollingBack>()?.SetSelected(false);
        howToButton.GetComponentInChildren<ScrollingBack>()?.SetSelected(false);
        quitButton.GetComponentInChildren<ScrollingBack>()?.SetSelected(false);

        // 選択中の背景だけ動かす
        currentButton.GetComponentInChildren<ScrollingBack>()?.SetSelected(true);

        // EventSystemの選択を更新
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(currentButton);

        // 矢印を選択中のボタンの子にする
        if (arrow != null)
        {
            arrow.transform.SetParent(currentButton.transform, false);
            RectTransform arrowRect = arrow.GetComponent<RectTransform>();

            arrowRect.anchorMin = new Vector2(0.5f, 1f);
            arrowRect.anchorMax = new Vector2(0.5f, 1f);
            arrowRect.pivot = new Vector2(0.5f, 0f);

            // 矢印の位置
            arrowRect.anchoredPosition = new Vector2(15f, -35f);
            arrow.SetActive(true);
        }
    }

    // RawImageを使用したフェードイン処理
    private IEnumerator FadeInCoroutine()
    {
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
    }
}