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
    public GameObject nextPromptUI; // 新設：「次へ」を表示するUI

    [Header("サウンド設定")]
    public AudioSource audioSource;      
    public AudioClip typeSound;          

    [Header("プロローグ進行設定")]
    public float typeSpeed = 0.05f;      
    
    public int skipTargetIndex = 2;      
    public ProloguePage[] pages;         

    private int currentPage = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        if (pages.Length > 0)
        {
            ShowPage(currentPage);
        }
    }

    void Update()
    {

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
        if (currentPage >= skipTargetIndex) return;

        currentPage = skipTargetIndex;
        ShowPage(currentPage);
    }

    void EndPrologue()
    {
        Debug.Log("プロローグ終了。本編へ移行します。");
        // 終了時に「次へ」UIを消す
        if (nextPromptUI != null) nextPromptUI.SetActive(false);
    }
}