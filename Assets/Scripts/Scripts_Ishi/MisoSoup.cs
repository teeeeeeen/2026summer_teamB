using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MisoSoupMaker : MonoBehaviour
{
    public Text displayText;
    public float textSpeed = 0.1f;

    public enum Ingredient
    {
        Tofu, Wakame, Aburaage, Nasu, Nameko, Potato, Negi, Onion
    }

    private List<Ingredient> selectedIngredients = new List<Ingredient>();
    private bool isTyping = false;

    public void OnClickTofu() { AddIngredient(Ingredient.Tofu); }
    public void OnClickWakame() { AddIngredient(Ingredient.Wakame); }
    public void OnClickAburaage() { AddIngredient(Ingredient.Aburaage); }
    public void OnClickNasu() { AddIngredient(Ingredient.Nasu); }
    public void OnClickNameko() { AddIngredient(Ingredient.Nameko); }
    public void OnClickPotato() { AddIngredient(Ingredient.Potato); }
    public void OnClickNegi() { AddIngredient(Ingredient.Negi); }
    public void OnClickOnion() { AddIngredient(Ingredient.Onion); }

    private void AddIngredient(Ingredient ingredient)
    {
        if (isTyping) return;

        if (selectedIngredients.Count >= 8)
        {
            selectedIngredients.Clear();
            displayText.text = "";
        }

        selectedIngredients.Add(ingredient);

        if (selectedIngredients.Count < 8)
        {
            displayText.text = $"具材を投入中... ({selectedIngredients.Count}/8)";
        }
        else
        {
            EvaluateSoup();
        }
    }

    private void EvaluateSoup()
    {
        // 具材のカウント
        Dictionary<Ingredient, int> counts = new Dictionary<Ingredient, int>();
        foreach (Ingredient ing in System.Enum.GetValues(typeof(Ingredient)))
        {
            counts[ing] = 0;
        }
        foreach (var ing in selectedIngredients)
        {
            counts[ing]++;
        }

        // 何「種類」の具材が入っているかをカウント
        int uniqueCount = counts.Count(kv => kv.Value > 0);

        // 1. 判定結果を「数字(ID)」として取得
        int soupId = GetSoupId(counts, uniqueCount);

        // 2. その数字(ID)を見て、対応する「テキスト」を取得
        string resultText = GetSoupText(soupId);

        // 3. テキストを流す
        StartCoroutine(StreamText(resultText));
    }

    // 組み合わせを判定し、対応する数字（ID）を返すメソッド
    private int GetSoupId(Dictionary<Ingredient, int> counts, int uniqueCount)
    {
        if (uniqueCount == 8) return 0; // 具材の重複がない (全種類1個ずつ)

        if (uniqueCount == 1) // 1種類の具材だけを8回
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

        // ▼修正箇所：指定した具材が「すべて」入っており、かつ「それ以外」が入っていないかを判定
        if (IsExactCombo(counts, uniqueCount, Ingredient.Tofu, Ingredient.Wakame, Ingredient.Aburaage)) return 10;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Potato, Ingredient.Onion)) return 11;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Negi, Ingredient.Onion)) return 12;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Aburaage, Ingredient.Nasu)) return 13;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Wakame, Ingredient.Nameko)) return 14;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Tofu, Ingredient.Aburaage)) return 9;
        if (IsExactCombo(counts, uniqueCount, Ingredient.Nasu, Ingredient.Potato, Ingredient.Negi, Ingredient.Onion)) return 15;

        // 上記のどれにも当てはまらない場合（例：豆腐とわかめだけ、など）
        return 16; 
    }

    // 数字（ID）を受け取り、対応するテキストを返す
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

    // 指定された具材が「それぞれ1つ以上入っている」かつ「指定されていない具材は入っていない」ことを判定
    private bool IsExactCombo(Dictionary<Ingredient, int> counts, int uniqueCount, params Ingredient[] required)
    {
        // そもそも入っている具材の「種類数」が、要求されている種類数と違えば弾く
        if (uniqueCount != required.Length) return false;

        // 要求されている具材が「すべて1つ以上」入っているかチェック
        foreach (var req in required)
        {
            if (counts[req] == 0) return false;
        }

        return true;
    }

    private IEnumerator StreamText(string text)
    {
        isTyping = true;
        displayText.text = "";

        foreach (char c in text)
        {
            displayText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }
}