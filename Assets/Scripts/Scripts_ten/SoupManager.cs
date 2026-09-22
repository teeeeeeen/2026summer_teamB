using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SoupManager : MonoBehaviour
{
    public static SoupManager instance;

    [Header("UI設定：テキスト")]
    public Text displayText;           // 投入中や完成品の名前を表示するテキスト
    public float textSpeed = 0.1f;     // 文字が流れるスピード

    [Header("UI設定：アイコン（8個）")]
    public Image[] ingredientIcons; 

    [Header("UI設定：完成品演出")]
    public Image finalSoupImage;    
    [Tooltip("0〜16のIDに対応する17枚のスプライトをセットしてください")]
    public Sprite[] finalSoupSprites; 
    public float showDuration = 3.0f;  // 表示しておく秒数

    public enum Ingredient
    {
        Tofu, Wakame, Aburaage, Nasu, Nameko, Potato, Negi, Onion, Unknown
    }

    private int currentCount = 0;
    private List<Ingredient> currentIngredients = new List<Ingredient>();
    
    private Coroutine currentAnimationRoutine = null;
    private Coroutine currentTextRoutine = null;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        ResetIcons();
        if (finalSoupImage != null)
        {
            finalSoupImage.gameObject.SetActive(false);
        }
        if (displayText != null)
        {
            displayText.text = "";
        }
    }

    // 障害物から呼ばれる処理
    public void CatchIngredient(string ingredientName, Sprite ingredientSprite)
    {
        Ingredient ing = GetIngredientFromName(ingredientName);
        if (ing == Ingredient.Unknown)
        {
            Debug.LogWarning($"未定義の具材名です: {ingredientName}");
            return;
        }

        // アイコンを灯す
        if (currentCount < ingredientIcons.Length && ingredientIcons[currentCount] != null)
        {
            ingredientIcons[currentCount].sprite = ingredientSprite;
            ingredientIcons[currentCount].color = new Color(1, 1, 1, 1); 
        }

        currentIngredients.Add(ing);
        currentCount++;

        if (currentCount < 8)
        {
            // 投入中テキストの更新（テキストアニメーション中でなければ表示）
            if (currentTextRoutine != null) StopCoroutine(currentTextRoutine);
            if (displayText != null) displayText.text = $"具材を投入中... ({currentCount}/8)";
        }
        else
        {
            // 8個集まった時の処理
            int soupId = EvaluateSoupId(currentIngredients);
            string resultText = GetSoupText(soupId);

            // テキスト流し開始
            if (currentTextRoutine != null) StopCoroutine(currentTextRoutine);
            currentTextRoutine = StartCoroutine(StreamText(resultText));

            // 画像アニメーション開始
            if (currentAnimationRoutine != null) StopCoroutine(currentAnimationRoutine);
            currentAnimationRoutine = StartCoroutine(ShowFinalSoupRoutine(soupId));

            // 次の収集ができるように即座にリセット
            ResetIcons();
        }
    }

    private void ResetIcons()
    {
        currentCount = 0;
        currentIngredients.Clear();
        foreach (var icon in ingredientIcons)
        {
            if (icon != null) icon.color = new Color(1, 1, 1, 0); 
        }
    }

    private Ingredient GetIngredientFromName(string rawName)
    {
        switch (rawName)
        {
            case "Tofu": case "豆腐": return Ingredient.Tofu;
            case "Wakame": case "わかめ": return Ingredient.Wakame;
            case "Aburaage": case "油揚げ": return Ingredient.Aburaage;
            case "Nasu": case "ナス": case "なす": return Ingredient.Nasu;
            case "Nameko": case "なめこ": return Ingredient.Nameko;
            case "Potato": case "じゃがいも": case "芋": return Ingredient.Potato;
            case "Negi": case "ネギ": case "ねぎ": return Ingredient.Negi;
            case "Onion": case "玉ねぎ": case "タマネギ": return Ingredient.Onion;
            default: return Ingredient.Unknown;
        }
    }

    private int EvaluateSoupId(List<Ingredient> ingredients)
    {
        Dictionary<Ingredient, int> counts = new Dictionary<Ingredient, int>();
        foreach (Ingredient ing in System.Enum.GetValues(typeof(Ingredient)))
        {
            if (ing != Ingredient.Unknown) counts[ing] = 0;
        }

        foreach (var ing in ingredients)
        {
            counts[ing]++;
        }

        int uniqueCount = counts.Count(kv => kv.Value > 0);

        if (uniqueCount == 8) return 0; // 具材の重複がない

        if (uniqueCount == 1)
        {
            if (counts[Ingredient.Tofu] == 8) return 1;
            if (counts[Ingredient.Wakame] == 8) return 2;
            if (counts[Ingredient.Aburaage] == 8) return 3;
            if (counts[Ingredient.Nasu] == 8) return 4;
            if (counts[Ingredient.Nameko] == 8) return 5;
            if (counts[Ingredient.Potato] == 8) return 6;
            if (counts[Ingredient.Negi] == 8) return 7;
            if (counts[Ingredient.Onion] == 8) return 8;
        }

        if (IsExactCombo(counts, uniqueCount, Ingredient.Tofu, Ingredient.Wakame, Ingredient.Aburaage)) return 10;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Potato, Ingredient.Onion)) return 11;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Negi, Ingredient.Onion)) return 12;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Aburaage, Ingredient.Nasu)) return 13;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Wakame, Ingredient.Nameko)) return 14;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Tofu, Ingredient.Aburaage)) return 9;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Nasu, Ingredient.Potato, Ingredient.Negi, Ingredient.Onion)) return 15;

        return 16;
    }

    private bool IsExactCombo(Dictionary<Ingredient, int> counts, int uniqueCount, params Ingredient[] required)
    {
        if (uniqueCount != required.Length) return false;
        foreach (var req in required)
        {
            if (counts[req] == 0) return false;
        }
        return true;
    }

    private string GetSoupText(int soupId)
    {
        switch (soupId)
        {
            case 0: return "超最強の味噌汁";
            case 1: return "湯豆腐";
            case 2: return "わかめスープ";
            case 3: return "きつねみそしる";
            case 4: return "超トロトロなすみそしる";
            case 5: return "なめこの餡かけみそしる";
            case 6: return "芋煮";
            case 7: return "ねぎだく薬味みそしる";
            case 8: return "和風オニオンスープ";
            case 9: return "畑の肉みそしる";
            case 10: return "実家のような安心感のみそしる";
            case 11: return "ほぼポトフ";
            case 12: return "Ｗネギみそしる";
            case 13: return "旨味スポンジみそしる";
            case 14: return "超ネバトロみそしる";
            case 15: return "畑の恵み野菜みそしる";
            case 16: return "具だくさんみそしる";
            default: return "プログラム壊れちゃった...";
        }
    }

    // 完成品の名前を流すコルーチン
    private IEnumerator StreamText(string text)
    {
        if (displayText == null) yield break;
        
        displayText.text = "";
        foreach (char c in text)
        {
            displayText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    // 完成品を拡縮アニメーションさせるコルーチン
    private IEnumerator ShowFinalSoupRoutine(int soupId)
    {
        if (finalSoupImage == null || finalSoupSprites == null || finalSoupSprites.Length == 0) yield break;

        int spriteIndex = Mathf.Clamp(soupId, 0, finalSoupSprites.Length - 1);
        finalSoupImage.sprite = finalSoupSprites[spriteIndex];
        
        finalSoupImage.gameObject.SetActive(true);
        finalSoupImage.enabled = true;
        finalSoupImage.transform.localScale = Vector3.one;

        float elapsed = 0f;
        
        while (elapsed < showDuration)
        {
            elapsed += Time.deltaTime;
            
            // 0 -> 1 -> 0 を1秒ごとに繰り返して拡縮
            float scale = Mathf.Lerp(1.0f, 1.1f, Mathf.PingPong(elapsed, 1.0f));
            finalSoupImage.transform.localScale = new Vector3(scale, scale, 1f);
            
            yield return null;
        }

        finalSoupImage.transform.localScale = Vector3.one;
        finalSoupImage.gameObject.SetActive(false);
    }
}