using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine.SceneManagement;

public class ZukanManager2 : MonoBehaviour
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
    
    [Header("サウンド設定")]
    public AudioSource audioSource;
    public AudioClip selectSE;
    public AudioClip decideSE;

    // ボタンのGameObjectと図鑑データを紐づけて管理する辞書
    private Dictionary<GameObject, MisoSoupData> buttonDataMap = new Dictionary<GameObject, MisoSoupData>();
    private List<ZukanButtonNode> buttonNodes = new List<ZukanButtonNode>();

    // 現在選択されているボタンを記録（カーソル移動検知用）
    private GameObject lastSelectedButton;
    private bool isInitialized = false; 

    public void PlaySelectSound()
    {
        if (audioSource != null && selectSE != null)
        {
            audioSource.PlayOneShot(selectSE);
        }
    }

    public void GoToTitleScene()
    {
        StartCoroutine(TransitionToTitle());
    }

    private IEnumerator TransitionToTitle()
    {
        if (audioSource != null && decideSE != null)
        {
            audioSource.PlayOneShot(decideSE);
        }
        
        yield return new WaitForSecondsRealtime(0.7f); 
        SceneManager.LoadScene("suika_test"); 
    }

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(null);
        lastSelectedButton = null;

        if (!isInitialized)
        {
            GenerateZukanList();
            isInitialized = true;
        }
        else
        {
            RefreshZukanData();
        }
    }

    private void Update()
    {
        // ボタンが1つも無い、またはEventSystemが無い場合は処理しない
        if (EventSystem.current == null || buttonNodes.Count == 0) return;

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        // ① フォーカスが完全に消滅した場合、または「非表示になったポーズUIのボタン(Openなど)」を掴んだままの場合
        if (currentSelected == null || !currentSelected.activeInHierarchy)
        {
            // 前回選択していたものが有効ならそれ、無ければ強制的に一番上のボタンをターゲットにする
            GameObject target = (lastSelectedButton != null && lastSelectedButton.activeInHierarchy) ? lastSelectedButton : buttonNodes[0].gameObject;
            
            EventSystem.current.SetSelectedGameObject(target);
            currentSelected = target;

            // 視覚的な選択状態（ハイライトなど）を確実にする
            Button btn = target.GetComponent<Button>();
            if (btn != null) btn.Select();
        }

        // ② 選択対象が切り替わった時（初回起動時も含む）の処理
        if (currentSelected != lastSelectedButton)
        {
            if (buttonDataMap.ContainsKey(currentSelected))
            {
                // 右側の詳細画面にデータを反映
                UpdateDetailView(buttonDataMap[currentSelected]);
                
                // 初回表示時（lastSelectedButtonがnullの時）は音を鳴らさず、手動でカーソルを動かした時のみ鳴らす
                if (lastSelectedButton != null)
                {
                    PlaySelectSound();
                }
                
                lastSelectedButton = currentSelected;
            }
            else
            {
                // スクロールバーなど、図鑑リスト以外のUIが選ばれた場合はそのまま記録する
                lastSelectedButton = currentSelected;
            }
        }
    }

    private void GenerateZukanList()
    {
        buttonNodes.Clear(); 
        buttonDataMap.Clear();

        foreach (MisoSoupData data in allMisoSoups)
        {
            data.isUnlocked = PlayerPrefs.GetInt("UnlockedSoup_" + data.soupName, 0) == 1;

            GameObject newButton = Instantiate(buttonPrefab, contentPanel);
            
            ZukanButtonNode node = newButton.GetComponent<ZukanButtonNode>();
            if (node != null)
            {
                node.Setup(data);
                buttonNodes.Add(node);
                buttonDataMap.Add(newButton, data); // GameObjectとデータを紐づけて登録
            }
        }
    }

    private void RefreshZukanData()
    {
        if (buttonNodes.Count == 0) return;

        for (int i = 0; i < allMisoSoups.Length; i++)
        {
            MisoSoupData data = allMisoSoups[i];
            data.isUnlocked = PlayerPrefs.GetInt("UnlockedSoup_" + data.soupName, 0) == 1;
            
            if (i < buttonNodes.Count)
            {
                buttonNodes[i].Setup(data);
            }
        }
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
            descriptionText.text = "まだ作っていません...";

            if (overlayIconImage != null)
            {
                overlayIconImage.gameObject.SetActive(false);
            }
        }
    }
}