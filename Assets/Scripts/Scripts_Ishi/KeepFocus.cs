using UnityEngine;
using UnityEngine.EventSystems;

public class KeepFocus : MonoBehaviour
{
    private GameObject lastSelected;

    void Update()
    {
        if (EventSystem.current == null) return;

        // 現在選択されているUIオブジェクトを取得
        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current != null)
        {
            // 何かを選択中なら、それを「最後に選択したもの」として記憶しておく
            lastSelected = current;
        }
        else if (lastSelected != null)
        {
            // もしクリック等で何も選択されていない状態（null）になってしまったら、
            // 記憶しておいた最後のボタンに、強制的にフォーカスを戻す！
            EventSystem.current.SetSelectedGameObject(lastSelected);
        }
    }
}