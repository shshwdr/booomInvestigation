using System;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : Singleton<CardManager>
{
    public event Action OnCardsChanged;
    public event Action OnCardUnreadStateChanged;

    private readonly HashSet<string> ownedCardIds = new HashSet<string>();
    private readonly HashSet<string> readCardIds = new HashSet<string>();
    private readonly Dictionary<string, string> mergeMap = new Dictionary<string, string>();

    public void Init()
    {
        ownedCardIds.Clear();
        readCardIds.Clear();
        mergeMap.Clear();

        foreach (var pair in CSVLoader.Instance.cardInfoMap)
        {
            var info = pair.Value;
            if (info != null && info.isStart && !string.IsNullOrEmpty(info.identifier))
            {
                ownedCardIds.Add(info.identifier);
            }
        }

        foreach (var mergeInfo in CSVLoader.Instance.cardMergeInfos)
        {
            if (mergeInfo == null
                || string.IsNullOrEmpty(mergeInfo.card1)
                || string.IsNullOrEmpty(mergeInfo.card2)
                || string.IsNullOrEmpty(mergeInfo.result))
            {
                continue;
            }

            mergeMap[BuildMergeKey(mergeInfo.card1, mergeInfo.card2)] = mergeInfo.result;
        }

        NotifyCardChanged();
    }

    public bool AddCard(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return false;
        }

        if (!CSVLoader.Instance.cardInfoMap.ContainsKey(identifier))
        {
            Debug.LogWarning("Try add unknown card: " + identifier);
            return false;
        }

        var added = ownedCardIds.Add(identifier);
        if (added)
        {
            NotifyCardChanged();
        }

        return added;
    }

    public bool HasCard(string identifier)
    {
        return !string.IsNullOrEmpty(identifier) && ownedCardIds.Contains(identifier);
    }

    public bool MarkCardRead(string identifier)
    {
        if (string.IsNullOrEmpty(identifier) || !ownedCardIds.Contains(identifier))
        {
            return false;
        }

        var changed = readCardIds.Add(identifier);
        if (changed)
        {
            OnCardUnreadStateChanged?.Invoke();
        }

        return changed;
    }

    public bool IsCardUnread(string identifier)
    {
        return !string.IsNullOrEmpty(identifier) && ownedCardIds.Contains(identifier) && !readCardIds.Contains(identifier);
    }

    public bool HasUnreadCard()
    {
        foreach (var cardId in ownedCardIds)
        {
            if (!readCardIds.Contains(cardId))
            {
                return true;
            }
        }

        return false;
    }

    public List<CardInfo> GetOwnedCards()
    {
        var result = new List<CardInfo>();
        foreach (var identifier in ownedCardIds)
        {
            CardInfo info;
            if (CSVLoader.Instance.cardInfoMap.TryGetValue(identifier, out info))
            {
                result.Add(info);
            }
        }

        return result;
    }

    public bool TryMergeCards(string cardA, string cardB, out string resultCardId)
    {
        resultCardId = string.Empty;
        if (!HasCard(cardA) || !HasCard(cardB))
        {
            return false;
        }

        return mergeMap.TryGetValue(BuildMergeKey(cardA, cardB), out resultCardId);
    }

    private static string BuildMergeKey(string cardA, string cardB)
    {
        return string.CompareOrdinal(cardA, cardB) <= 0 ? cardA + "|" + cardB : cardB + "|" + cardA;
    }

    private void NotifyCardChanged()
    {
        OnCardsChanged?.Invoke();
        OnCardUnreadStateChanged?.Invoke();
    }
}
