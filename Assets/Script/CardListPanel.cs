using UnityEngine;

public class CardListPanel : Singleton<CardListPanel>
{
    [SerializeField] private RectTransform cardRoot;
    [SerializeField] private CardNode cardNodePrefab;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        CardManager.Instance.OnCardsChanged += RefreshCards;
    }

    private void OnDisable()
    {
        if (CardManager.hasInitialized())
        {
            CardManager.Instance.OnCardsChanged -= RefreshCards;
        }
    }

    private void Start()
    {
        RefreshCards();
    }

    private void RefreshCards()
    {
        if (cardRoot == null || cardNodePrefab == null)
        {
            return;
        }

        ClearChildren(cardRoot);
        var cards = CardManager.Instance.GetOwnedCards();
        foreach (var card in cards)
        {
            var node = Instantiate(cardNodePrefab, cardRoot);
            node.Setup(card, CardNode.CardNodeContext.CardListPanel);
        }
    }

    public void TryUseCardOnNpc(string cardIdentifier, NPCController npcController)
    {
        if (string.IsNullOrEmpty(cardIdentifier) || npcController == null)
        {
            return;
        }

        var npcId = npcController.GetIdentifier();
        if (string.IsNullOrEmpty(npcId))
        {
            return;
        }

        var cardDialogue = npcId + "_" + cardIdentifier;
        if (CSVLoader.Instance.TryGetDialogueFile(cardDialogue, out _))
        {
            DialogueController.Instance.OpenDialogueFileByName(cardDialogue);
            return;
        }

        var defaultDialogue = npcId + "_default";
        if (CSVLoader.Instance.TryGetDialogueFile(defaultDialogue, out _))
        {
            DialogueController.Instance.OpenDialogueFileByName(defaultDialogue);
            return;
        }

        Debug.LogError("No dialogue found for card use. npc=" + npcId + ", card=" + cardIdentifier);
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
