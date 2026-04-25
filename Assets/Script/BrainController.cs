using UnityEngine;
using UnityEngine.UI;

public class BrainController : Singleton<BrainController>
{
    [SerializeField] private Button closeButton;
    [SerializeField] private RectTransform tokenParent;
    [SerializeField] private RectTransform cardParent;
    [SerializeField] private TokenNode tokenNodePrefab;
    [SerializeField] private CardNode cardNodePrefab;

    protected override void Awake()
    {
        base.Awake();

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        CardManager.Instance.OnCardsChanged += OnCardsChanged;
    }

    private void OnDestroy()
    {
        if (CardManager.hasInitialized())
        {
            CardManager.Instance.OnCardsChanged -= OnCardsChanged;
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
        RefreshTokens();
        RefreshCards();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void RefreshTokens()
    {
        if (tokenParent == null || tokenNodePrefab == null)
        {
            return;
        }

        ClearChildren(tokenParent);
        var tokens = TokenManager.Instance.GetOwnedTokens();
        foreach (var token in tokens)
        {
            var node = Instantiate(tokenNodePrefab, tokenParent);
            node.Setup(token);
        }
    }

    private void RefreshCards()
    {
        if (cardParent == null || cardNodePrefab == null)
        {
            return;
        }

        ClearChildren(cardParent);
        var cards = CardManager.Instance.GetOwnedCards();
        foreach (var card in cards)
        {
            var node = Instantiate(cardNodePrefab, cardParent);
            node.Setup(card, CardNode.CardNodeContext.Brain);
        }
    }

    private void OnCardsChanged()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        RefreshCards();
    }

    public bool TryMergeCard(CardNode sourceCard, CardNode targetCard)
    {
        if (sourceCard == null || targetCard == null || sourceCard == targetCard)
        {
            return false;
        }

        string mergedCardId;
        if (!CardManager.Instance.TryMergeCards(sourceCard.CardIdentifier, targetCard.CardIdentifier, out mergedCardId))
        {
            return false;
        }

        if (CardManager.Instance.AddCard(mergedCardId))
        {
            RefreshCards();
            return true;
        }

        return false;
    }

    private static void ClearChildren(RectTransform root)
    {
        if (root == null)
        {
            return;
        }

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }
}
