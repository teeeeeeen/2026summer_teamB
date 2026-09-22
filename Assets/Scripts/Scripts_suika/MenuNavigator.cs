using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuNavigator : MonoBehaviour
{
    public GameObject startButton;
    public GameObject howToButton;
    public GameObject quitButton;
    public GameObject arrow;

    private GameObject currentButton;

    void Start()
    {
        // 最初はStartボタンを選択
        currentButton = startButton;

        SelectButton();
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        // Dキー → 右へ
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            if (currentButton == startButton)
            {
                currentButton = howToButton;
            }
            else if (currentButton == howToButton)
            {
                currentButton = quitButton;
            }
            else
            {
                currentButton = startButton;
            }

            SelectButton();
        }

        // Aキー → 左へ
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            if (currentButton == quitButton)
            {
                currentButton = howToButton;
            }
            else if (currentButton == howToButton)
            {
                currentButton = startButton;
            }
            else
            {
                currentButton = quitButton;
            }

            SelectButton();
        }

        // Enter / Space / Z → 決定
        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.zKey.wasPressedThisFrame)
        {
            Button button = currentButton.GetComponent<Button>();

            if (button != null)
            {
                button.onClick.Invoke();
            }
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
        arrow.transform.SetParent(currentButton.transform, false);

        RectTransform arrowRect = arrow.GetComponent<RectTransform>();

        arrowRect.anchorMin = new Vector2(0.5f, 1f);
        arrowRect.anchorMax = new Vector2(0.5f, 1f);
        arrowRect.pivot = new Vector2(0.5f, 0f);

        // 矢印の位置
        arrowRect.anchoredPosition = new Vector2(15f, -20f);

        arrow.SetActive(true);
    }
}