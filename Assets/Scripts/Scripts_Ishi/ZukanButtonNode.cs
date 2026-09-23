using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ZukanButtonNode : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [HideInInspector] public MisoSoupData myData; 
    private ZukanManager manager;
    public GameObject cursorFrame;

    [Header("ボタン自体の見た目")]
    public Image buttonIconImage; 
    public TextMeshProUGUI buttonNameText; 

    private void Awake()
    {
        manager = FindObjectOfType<ZukanManager>();
        if (cursorFrame != null) cursorFrame.SetActive(false);
    }

    public void Setup(MisoSoupData data)
    {
        myData = data;

        // ★アイコン画像は常に通常の画像を表示（黒塗りにしない）
        if (buttonIconImage != null)
        {
            buttonIconImage.sprite = myData.soupIcon;
            buttonIconImage.color = Color.white;
        }

        // ★名前の表示：未開放なら「？？？」、解放済みなら本名
        if (buttonNameText != null)
        {
            if (myData.isUnlocked)
            {
                buttonNameText.text = myData.soupName;
            }
            else
            {
                buttonNameText.text = "？？？";
            }
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (manager != null && myData != null)
        {
            manager.UpdateDetailView(myData);
        }
        if (cursorFrame != null) cursorFrame.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (cursorFrame != null) cursorFrame.SetActive(false);
    }
}