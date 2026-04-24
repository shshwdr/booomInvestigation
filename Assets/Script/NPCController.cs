using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NPCController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image npcImage;

    private NpcInfo npcInfo;
    private Action<string> onHoverChanged;

    public void Setup(NpcInfo info, Action<string> onHoverChangedAction)
    {
        npcInfo = info;
        onHoverChanged = onHoverChangedAction;
        LoadSprite();
    }

    private void LoadSprite()
    {
        if (npcImage == null || npcInfo == null)
        {
            return;
        }

        var sprite = Resources.Load<Sprite>("npc/" + npcInfo.identifier);
        if (sprite == null)
        {
            Debug.LogWarning("NPC sprite not found: Resources/npc/" + npcInfo.identifier);
            return;
        }

        npcImage.sprite = sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (npcInfo != null)
        {
            onHoverChanged?.Invoke(npcInfo.name);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverChanged?.Invoke(string.Empty);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (npcInfo != null)
        {
            Debug.Log("Enter dialog with npc: " + npcInfo.identifier);
        }
    }
}
