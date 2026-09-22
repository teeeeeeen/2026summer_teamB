using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.EventSystems; 
using UnityEngine.InputSystem; 

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
    public Image fadeMask;
    [Tooltip("フェードにかかる時間（秒）")]
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

    private int currentPage = 0;
    private bool isTyping = false;
    private bool isFinished = false; // 終了処理中かどうかの判定フラグ
    private Coroutine typingCoroutine;

    void Start()
    {
        // フェード用のマスクが設定されていれば、開始時は透明にしておく
        if (fadeMask != null)
        {
            Color c = fadeMask.color;
            c.a = 0f;
            fadeMask.color = c;
            fadeMask.gameObject.SetActive(false);
        }

        if (pages.Length > 0)
        {
            ShowPage(currentPage);
        }
    }

    void Update()
    {
        if (isFinished) return; // フェード中（終了処理中）は入力を受け付けない

        bool isClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool isEnterPressed = Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);

        if (isClicked || isEnterPressed)
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        if (isTyping)
        {
            // 文字送り中のクリック：即座に全文表示
            StopCoroutine(typingCoroutine);
            prologueText.text = pages[currentPage].text;
            isTyping = false;

            // 全表示した瞬間に「次へ」UIをオンにする
            if (nextPromptUI != null) nextPromptUI.SetActive(true);
        }
        else
        {
            // 表示完了後のクリック：次のページへ
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

        // 新しいページに入ったら「次へ」UIは一旦隠す
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
        
        // 文字送りが自然に終わった時に「次へ」UIをオンにする
        if (nextPromptUI != null) nextPromptUI.SetActive(true);
    }

    public void Skip()
    {
        if (isFinished || currentPage >= skipTargetIndex) return;

        currentPage = skipTargetIndex;
        ShowPage(currentPage);
    }

    void EndPrologue()
    {
        isFinished = true; // 入力をブロック
        
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
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f); // アルファ値を1（不透明）に

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
        Debug.Log("フェード完了。本編へ移行します。");
        // ここに SceneManager.LoadScene("MainScene"); などのシーン遷移処理を追加してください
    }
}