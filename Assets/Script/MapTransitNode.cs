using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapTransitNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image iconImage;

    private string mapName;
    private Action onClick;
    private Action<string> onHoverChanged;

    public void Setup(MapInfo info, Action onClickAction, Action<string> onHoverChangedAction)
    {
        mapName = info.name;
        onClick = onClickAction;
        onHoverChanged = onHoverChangedAction;
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
        onHoverChanged?.Invoke(mapName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverChanged?.Invoke(string.Empty);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}
