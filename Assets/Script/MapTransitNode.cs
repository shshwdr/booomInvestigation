using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapTransitNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;

    private string mapName;
    private Action onClick;

    public void Setup(MapInfo info, Action onClickAction)
    {
        mapName = info.name;
        onClick = onClickAction;
        if (nameText != null)
        {
            nameText.text = string.Empty;
        }
    }

    public void SetSprite(Sprite sprite)
    {
        if (iconImage != null)
        {
            iconImage.sprite = sprite;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (nameText != null)
        {
            nameText.text = mapName;
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
        onClick?.Invoke();
    }
}
