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
    public TextMeshProUGUI ingredientsText;
    [Header("特殊演出")]
    public GameObject specialEffect;

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

    public void UpdateDetailView(MisoSoupData data)
    {
        if (ingredientsText != null)
        {
            ingredientsText.text = "【材料】\n" + data.ingredients; 
        }

        if (data.isUnlocked)
        {
            nameText.text = data.soupName;
            iconImage.sprite = data.soupIcon;
            iconImage.color = Color.white; 
            descriptionText.text = data.description;

            // フラグがONならエフェクトを表示、OFFなら隠す
            if (specialEffect != null)
            {
                specialEffect.SetActive(data.isSuperLegendary);
            }
        }
        else
        {
            nameText.text = "？？？";
            iconImage.sprite = data.soupIcon;
            iconImage.color = Color.black; 
            descriptionText.text = "まだ発見していません。";

            // 未開放の時は光らせない
            if (specialEffect != null)
            {
                specialEffect.SetActive(false);
            }
        }
    }
}