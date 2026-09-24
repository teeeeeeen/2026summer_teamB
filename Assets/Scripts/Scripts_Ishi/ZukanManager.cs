using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic; // 【追加】Listを使うために必要

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
    public TextMeshProUGUI ingredientsText;
    [Header("特殊演出")]
    public Image overlayIconImage;

    // 【追加】生成したボタンの情報を保持しておくリスト
    private List<ZukanButtonNode> buttonNodes = new List<ZukanButtonNode>();

    private void Start()
    {
        GenerateZukanList();
    }

    // 【追加】図鑑パネルがオン（表示）になるたびに呼ばれる処理
    private void OnEnable()
    {
        RefreshZukanData();
    }

    private void GenerateZukanList()
    {
        GameObject firstButton = null;
        buttonNodes.Clear(); // リストを初期化

        foreach (MisoSoupData data in allMisoSoups)
        {
            // SoupManagerで保存したアンロック状況を読み込む
            data.isUnlocked = PlayerPrefs.GetInt("UnlockedSoup_" + data.soupName, 0) == 1;

            GameObject newButton = Instantiate(buttonPrefab, contentPanel);
            
            ZukanButtonNode node = newButton.GetComponent<ZukanButtonNode>();
            node.Setup(data);
            
            buttonNodes.Add(node); // 生成したボタンをリストに追加

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

    // 【追加】すでに生成されているボタンのアンロック状況だけを最新に更新する処理
    private void RefreshZukanData()
    {
        // Startが呼ばれる前（ゲーム開始時の初回起動）はスキップする
        if (buttonNodes.Count == 0) return;

        for (int i = 0; i < allMisoSoups.Length; i++)
        {
            MisoSoupData data = allMisoSoups[i];
            
            // 最新のセーブデータでフラグを上書き
            data.isUnlocked = PlayerPrefs.GetInt("UnlockedSoup_" + data.soupName, 0) == 1;
            
            if (i < buttonNodes.Count)
            {
                // 各ボタンの表示（？？？やアイコンの色など）を更新
                buttonNodes[i].Setup(data);
            }
        }

        // 図鑑を開き直したときは、一番上のボタンを自動的に選択し直す
        if (buttonNodes.Count > 0)
        {
            StartCoroutine(SelectFirstButtonRoutine(buttonNodes[0].gameObject));
        }
    }

    private IEnumerator SelectFirstButtonRoutine(GameObject targetButton)
    {
        yield return null; 
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(targetButton);
    }
public void UpdateDetailView(MisoSoupData data)
    {
        if (ingredientsText != null) ingredientsText.text = "【材料】\n" + data.ingredients; 

        if (data.isUnlocked)
        {
            nameText.text = data.soupName;
            iconImage.sprite = data.soupIcon;
            iconImage.color = Color.white; 
            descriptionText.text = data.description;

            // ★変更：オーバーレイ画像にも同じ絵をセットし、超最強ならONにする
            if (overlayIconImage != null)
            {
                overlayIconImage.sprite = data.soupIcon;
                overlayIconImage.gameObject.SetActive(data.isSuperLegendary);
            }
        }
        else
        {
            nameText.text = "？？？";
            iconImage.sprite = data.soupIcon;
            iconImage.color = Color.black; 
            descriptionText.text = "まだ発見していません。";

            // ★変更：未開放時はオーバーレイを隠す
            if (overlayIconImage != null)
            {
                overlayIconImage.gameObject.SetActive(false);
            }
        }
    }
}