using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ZukanManager : MonoBehaviour
{
    [Header("図鑑のデータと生成設定")]
    public MisoSoupData[] allMisoSoups;
    public GameObject buttonPrefab;
    public Transform contentPanel;

    [Header("右側のUI参照")]
    public TextMeshProUGUI nameText;
    public Image iconImage;
    public TextMeshProUGUI descriptionText;

    private void Start()
    {
        GenerateZukanList();
    }

    private void GenerateZukanList()
    {
        GameObject firstButton = null;

        foreach (MisoSoupData data in allMisoSoups)
        {
            GameObject newButton = Instantiate(buttonPrefab, contentPanel);
            
            ZukanButtonNode node = newButton.GetComponent<ZukanButtonNode>();
            node.Setup(data);

            if (firstButton == null)
            {
                firstButton = newButton;
            }
        }

        if (firstButton != null)
        {
            StartCoroutine(SelectFirstButtonRoutine(firstButton));
        }
    }

    private IEnumerator SelectFirstButtonRoutine(GameObject targetButton)
    {
        yield return null; 
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(targetButton);
    }

    // ★右側の詳細を更新する処理
    public void UpdateDetailView(MisoSoupData data)
    {
        if (data.isUnlocked)
        {
            // 解放済み：綺麗に表示
            nameText.text = data.soupName;
            iconImage.sprite = data.soupIcon;
            iconImage.color = Color.white; // 通常の色
            descriptionText.text = data.description;
        }
        else
        {
            // 未開放：画像は黒塗り、名前は？？？
            nameText.text = "？？？";
            iconImage.sprite = data.soupIcon;
            iconImage.color = Color.black; // 黒く塗りつぶしてシルエットにする
            descriptionText.text = "まだ作っていません...";
        }
    }
}