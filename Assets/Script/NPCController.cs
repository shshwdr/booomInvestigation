using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NPCController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image npcImage;
    [SerializeField] private Text nameText;

    private NpcInfo npcInfo;

    public void Setup(NpcInfo info)
    {
        npcInfo = info;
        if (nameText != null)
        {
            nameText.text = string.Empty;
        }
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
        if (npcInfo != null && nameText != null)
        {
            nameText.text = npcInfo.name;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (nameText != null)
        {
            nameText.text = string.Empty;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (npcInfo != null)
        {
            DialogueController.Instance.OpenNpcDialogue(npcInfo.identifier);
        }
    }
}
