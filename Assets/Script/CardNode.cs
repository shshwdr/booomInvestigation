using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardNode : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public enum CardNodeContext
    {
        Brain,
        CardListPanel
    }

    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private CanvasGroup canvasGroup;

    private RectTransform rectTransform;
    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector2 originalAnchoredPosition;
    private Canvas rootCanvas;
    private string cardIdentifier;
    private CardNodeContext context;
    private GameObject redDot;

    public string CardIdentifier => cardIdentifier;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        rootCanvas = GetComponentInParent<Canvas>();
        redDot = GeneralButtonRedDotUtil.ResolveRedDot(gameObject);
    }

    private void OnEnable()
    {
        if (!CardManager.hasInitialized())
        {
            return;
        }

        CardManager.Instance.OnCardUnreadStateChanged += RefreshRedDot;
        RefreshRedDot();
    }

    private void OnDisable()
    {
        if (!CardManager.hasInitialized())
        {
            return;
        }

        CardManager.Instance.OnCardUnreadStateChanged -= RefreshRedDot;
    }

    public void Setup(CardInfo cardInfo, CardNodeContext cardContext)
    {
        cardIdentifier = cardInfo == null ? string.Empty : cardInfo.identifier;
        context = cardContext;

        if (tmpText != null)
        {
            tmpText.text = cardInfo == null ? string.Empty : cardInfo.name;
        }

        RefreshRedDot();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        MarkAsRead();

        if (eventData != null && eventData.button == PointerEventData.InputButton.Right)
        {
            ShowCardDetail();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rectTransform == null)
        {
            return;
        }

        originalParent = rectTransform.parent;
        originalSiblingIndex = rectTransform.GetSiblingIndex();
        originalAnchoredPosition = rectTransform.anchoredPosition;

        if (rootCanvas != null)
        {
            rectTransform.SetParent(rootCanvas.transform, true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchoredPosition += eventData.delta / (rootCanvas == null ? 1f : rootCanvas.scaleFactor);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        if (context == CardNodeContext.Brain)
        {
            var targetCard = FindDropTargetCard(eventData);
            if (targetCard != null && targetCard != this)
            {
                var merged = BrainController.Instance.TryMergeCard(this, targetCard);
                if (merged)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }
        else if (context == CardNodeContext.CardListPanel)
        {
            var npc = FindDropTargetNpc(eventData);
            if (npc != null)
            {
                CardListPanel.Instance.TryUseCardOnNpc(cardIdentifier, npc);
            }
        }

        ReturnToOriginalParent();
    }

    public void MarkAsRead()
    {
        if (string.IsNullOrEmpty(cardIdentifier))
        {
            return;
        }

        CardManager.Instance.MarkCardRead(cardIdentifier);
        RefreshRedDot();
    }

    private void RefreshRedDot()
    {
        if (redDot == null || !CardManager.hasInitialized())
        {
            return;
        }

        redDot.SetActive(!string.IsNullOrEmpty(cardIdentifier) && CardManager.Instance.IsCardUnread(cardIdentifier));
    }

    private void ShowCardDetail()
    {
        if (!CardDetailPanel.hasInitialized())
        {
            return;
        }

        CardInfo cardInfo;
        if (!CardManager.Instance.TryGetCardInfo(cardIdentifier, out cardInfo))
        {
            return;
        }

        CardDetailPanel.Instance.Show(cardInfo);
    }

    private void ReturnToOriginalParent()
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.SetParent(originalParent, false);
        rectTransform.SetSiblingIndex(originalSiblingIndex);
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }

    private static T FindComponentInParents<T>(GameObject go) where T : Component
    {
        if (go == null)
        {
            return null;
        }

        var current = go.transform;
        while (current != null)
        {
            var component = current.GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            current = current.parent;
        }

        return null;
    }

    private CardNode FindDropTargetCard(PointerEventData eventData)
    {
        if (eventData != null && eventData.hovered != null)
        {
            for (int i = eventData.hovered.Count - 1; i >= 0; i--)
            {
                var card = FindComponentInParents<CardNode>(eventData.hovered[i]);
                if (card != null && card != this)
                {
                    return card;
                }
            }
        }

        var fallback = FindComponentInParents<CardNode>(eventData == null ? null : eventData.pointerEnter);
        if (fallback != null && fallback != this)
        {
            return fallback;
        }

        return FindDropTargetCardByScreenPoint(eventData);
    }

    private NPCController FindDropTargetNpc(PointerEventData eventData)
    {
        if (eventData != null && eventData.hovered != null)
        {
            for (int i = eventData.hovered.Count - 1; i >= 0; i--)
            {
                var npc = FindComponentInParents<NPCController>(eventData.hovered[i]);
                if (npc != null)
                {
                    return npc;
                }
            }
        }

        var fallback = FindComponentInParents<NPCController>(eventData == null ? null : eventData.pointerEnter);
        if (fallback != null)
        {
            return fallback;
        }

        return FindDropTargetNpcByScreenPoint(eventData);
    }

    private CardNode FindDropTargetCardByScreenPoint(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return null;
        }

        var camera = eventData.pressEventCamera != null ? eventData.pressEventCamera : eventData.enterEventCamera;
        var allCards = FindObjectsByType<CardNode>(FindObjectsSortMode.None);
        for (int i = 0; i < allCards.Length; i++)
        {
            var card = allCards[i];
            if (card == null || card == this || !card.isActiveAndEnabled)
            {
                continue;
            }

            var rect = card.transform as RectTransform;
            if (rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, eventData.position, camera))
            {
                return card;
            }
        }

        return null;
    }

    private NPCController FindDropTargetNpcByScreenPoint(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return null;
        }

        var camera = eventData.pressEventCamera != null ? eventData.pressEventCamera : eventData.enterEventCamera;
        var npcs = FindObjectsByType<NPCController>(FindObjectsSortMode.None);
        for (int i = 0; i < npcs.Length; i++)
        {
            var npc = npcs[i];
            if (npc == null || !npc.isActiveAndEnabled)
            {
                continue;
            }

            var rect = npc.transform as RectTransform;
            if (rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, eventData.position, camera))
            {
                return npc;
            }
        }

        return null;
    }
}
