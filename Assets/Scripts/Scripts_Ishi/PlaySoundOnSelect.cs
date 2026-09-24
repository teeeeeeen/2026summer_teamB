using UnityEngine;
using UnityEngine.EventSystems; 
public class PlaySoundOnSelect : MonoBehaviour, ISelectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        ZukanManager manager = FindFirstObjectByType<ZukanManager>();
        if (manager != null)
        {
            manager.PlaySelectSound();
        }
    }
}