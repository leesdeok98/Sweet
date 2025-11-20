using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

public class UISound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Sound Settings")]
    public AudioManager.Sfx hoverSound = AudioManager.Sfx.ButtonHover_SFX;
    public AudioManager.Sfx clickSound = AudioManager.Sfx.ButtonClick_SFX;

    private Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (btn != null && !btn.interactable) return;

        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(hoverSound);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (btn != null && !btn.interactable) return;

        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(clickSound);
    }
}