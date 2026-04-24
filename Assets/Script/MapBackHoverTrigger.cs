using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapBackHoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string parentName;
    private Action<string> onHoverChanged;
    private Action onHoverExit;

    public void Setup(string parentMapName, Action onExit, Action<string> onHover)
    {
        parentName = parentMapName;
        onHoverExit = onExit;
        onHoverChanged = onHover;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverChanged?.Invoke(parentName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverExit?.Invoke();
    }
}
