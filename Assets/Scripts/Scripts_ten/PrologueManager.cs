using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.EventSystems; 
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; 

[System.Serializable]
public class ProloguePage
{
    [TextArea(3, 10)]
    public string text;
    
    public Sprite backgroundImage;
}

public class PrologueManager : MonoBehaviour
{
    [Header("UIの設定")]
    public TextMeshProUGUI prologueText; 
    public Image backgroundImage;        
    public GameObject skipButton;        
    [Tooltip("テキスト表示完了後に表示する「次へ」のUIオブジェクト")]
    public GameObject nextPromptUI;

    [Header("フェード設定")]
    [Tooltip("画面を覆うフェード用のImage")]
    public RawImage fadeMask;
    [Tooltip("開始時のフェードインにかかる時間（秒）")]
    public float fadeInDuration = 2.0f; // 追加：フェードイン時間
    [Tooltip("終了時のフェードアウトにかかる時間（秒）")]
    public float fadeDuration = 1.0f;

    [Header("サウンド設定")]
    public AudioSource audioSource;      
    public AudioClip typeSound;
    public AudioClip GuiterSound;
    public AudioClip BikeSound;          

    [Header("プロローグ進行設定")]
    public float typeSpeed = 0.05f;      
    
    public int skipTargetIndex = 2;      
    public ProloguePage[] pages;         

    [Header("シーン遷移設定")]
    [Tooltip("プロローグ終了後に移動するシーンの名前")]
    public string nextSceneName;
    [Tooltip("フェード完了からシーン遷移までの待ち時間（秒）")]
    public float waitBeforeTransition = 4.0f; 

    private int currentPage = 0;
    private bool isTyping = false;
    private bool isFinished = false; 
    private bool isInputEnabled = false; // 追加：序盤の入力ロック用フラグ
    private Coroutine typingCoroutine;

    void Start()
    {
        // 初期状態の表示リセット
        prologueText.text = "";
        if (nextPromptUI != null) nextPromptUI.SetActive(false);
        
        if (pages.Length > 0)
        {
            // フェードイン中に背景が空にならないよう、最初の背景画像をあらかじめセット
            if (pages[0].backgroundImage != null && backgroundImage != null)
            {
                backgroundImage.sprite = pages[0].backgroundImage;
            }
        }

        if (fadeMask != null)
        {
            // フェードイン開始
            Color c = fadeMask.color;
            c.a = 1f;
            fadeMask.color = c;
            fadeMask.gameObject.SetActive(true);
            
            StartCoroutine(FadeInCoroutine());
        }
        else
        {
            // マスクがない場合のフォールバック
            isInputEnabled = true;
            if (pages.Length > 0)
            {
                ShowPage(currentPage);
            }
        }
    }

    void Update()
    {
        // フェード完了前（開始時）、または終了処理中は入力を受け付けない
        if (isFinished || !isInputEnabled) return; 

        // ページ送りの入力判定（クリック、Space、Enterキー）
        bool isClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool isKeyboardNext = Keyboard.current != null && (
            Keyboard.current.enterKey.wasPressedThisFrame || 
            Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame);
        
        // ページ送りの入力判定（コントローラーのA/Bボタン。機種による配置違いをカバーするために両方判定）
        bool isGamepadNext = Gamepad.current != null && (
            Gamepad.current.buttonSouth.wasPressedThisFrame || 
            Gamepad.current.buttonEast.wasPressedThisFrame);

        if (isClicked || isKeyboardNext || isGamepadNext)
        {
            HandleInput();
        }

        // スキップの入力判定（コントローラーの+/-ボタン。Start=+/Menu, Select=-/Viewに該当）
        bool isGamepadSkip = Gamepad.current != null && (
            Gamepad.current.startButton.wasPressedThisFrame || 
            Gamepad.current.selectButton.wasPressedThisFrame);

        if (isGamepadSkip)
        {
            Skip();
        }
    }

    // 追加：フェードイン処理
    IEnumerator FadeInCoroutine()
    {
        float timer = 0f;
        bool hasStartedTyping = false;
        
        // フェードが完了する0.5秒前の時間を計算 (最低でも0秒以上にする)
        float startTypingTime = Mathf.Max(0f, fadeInDuration - 0.5f);
        Color c = fadeMask.color;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, timer / fadeInDuration);
            fadeMask.color = c;

            // 指定の時間を迎えたら、フェード中であってもテキスト表示と入力受付を開始する
            if (!hasStartedTyping && timer >= startTypingTime)
            {
                hasStartedTyping = true;
                isInputEnabled = true; // 入力ロック解除
                if (pages.Length > 0)
                {
                    ShowPage(currentPage);
                }
            }

            yield return null;
        }

        c.a = 0f;
        fadeMask.color = c;
        fadeMask.gameObject.SetActive(false);

        // 万が一フェードイン時間が短すぎて呼ばれなかった場合の保険
        if (!hasStartedTyping)
        {
            isInputEnabled = true;
            if (pages.Length > 0)
            {
                ShowPage(currentPage);
            }
        }
    }

    void HandleInput()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            prologueText.text = pages[currentPage].text;
            isTyping = false;

            if (nextPromptUI != null) nextPromptUI.SetActive(true);
        }
        else
        {
            currentPage++;
            if (currentPage < pages.Length)
            {
                ShowPage(currentPage);
            }
            else
            {
                audioSource.PlayOneShot(GuiterSound);
                audioSource.PlayOneShot(BikeSound);
                EndPrologue();
            }
        }
    }

    void ShowPage(int index)
    {
        if (skipButton != null)
        {
            skipButton.SetActive(index < skipTargetIndex);
        }

        if (nextPromptUI != null) nextPromptUI.SetActive(false);

        if (pages[index].backgroundImage != null && backgroundImage != null)
        {
            backgroundImage.sprite = pages[index].backgroundImage;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(pages[index].text));
    }

    IEnumerator TypeText(string textToType)
    {
        isTyping = true;
        prologueText.text = "";

        foreach (char c in textToType)
        {
            prologueText.text += c;

            if (audioSource != null && typeSound != null)
            {
                audioSource.PlayOneShot(typeSound);
            }

            yield return new WaitForSecondsRealtime(typeSpeed);
        }

        isTyping = false;
        
        if (nextPromptUI != null) nextPromptUI.SetActive(true);
    }

    public void Skip()
    {
        if (isFinished || currentPage >= skipTargetIndex || !isInputEnabled) return;

        currentPage = skipTargetIndex;
        ShowPage(currentPage);
    }

    void EndPrologue()
    {
        isFinished = true; 
        
        if (nextPromptUI != null) nextPromptUI.SetActive(false);

        if (fadeMask != null)
        {
            StartCoroutine(FadeOutMask());
        }
        else
        {
            CompletePrologue();
        }
    }

    IEnumerator FadeOutMask()
    {
        fadeMask.gameObject.SetActive(true);
        float timer = 0f;
        Color startColor = fadeMask.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f); 

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeMask.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            yield return null;
        }

        fadeMask.color = endColor;
        CompletePrologue();
    }

    void CompletePrologue()
    {
        StartCoroutine(WaitAndLoadScene());
    }

    IEnumerator WaitAndLoadScene()
    {
        yield return new WaitForSeconds(waitBeforeTransition);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("遷移先のシーン名がインスペクターで設定されていません。");
        }
    }
}