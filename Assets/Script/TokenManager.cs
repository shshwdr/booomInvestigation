using System.Collections.Generic;
using UnityEngine;

public class TokenManager : Singleton<TokenManager>
{
    public event System.Action OnTokensChanged;
    public event System.Action OnTokenUnreadStateChanged;

    private readonly HashSet<string> ownedTokenIds = new HashSet<string>();
    private readonly HashSet<string> readTokenIds = new HashSet<string>();

    public void Init()
    {
        ownedTokenIds.Clear();
        readTokenIds.Clear();
        foreach (var tokenInfo in CSVLoader.Instance.tokenInfoMap.Values)
        {
            if (tokenInfo != null && tokenInfo.isStart && !string.IsNullOrEmpty(tokenInfo.identifier))
            {
                ownedTokenIds.Add(tokenInfo.identifier);
            }
        }

        NotifyTokenChanged();
    }

    public bool HasToken(string identifier)
    {
        return !string.IsNullOrEmpty(identifier) && ownedTokenIds.Contains(identifier);
    }

    public bool AddToken(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return false;
        }

        if (!CSVLoader.Instance.tokenInfoMap.ContainsKey(identifier))
        {
            Debug.LogWarning("Try add unknown token: " + identifier);
            return false;
        }

        var added = ownedTokenIds.Add(identifier);
        if (added)
        {
            NotifyTokenChanged();
        }

        return added;
    }

    public bool MarkTokenRead(string identifier)
    {
        if (string.IsNullOrEmpty(identifier) || !ownedTokenIds.Contains(identifier))
        {
            return false;
        }

        var changed = readTokenIds.Add(identifier);
        if (changed)
        {
            OnTokenUnreadStateChanged?.Invoke();
        }

        return changed;
    }

    public bool IsTokenUnread(string identifier)
    {
        return !string.IsNullOrEmpty(identifier) && ownedTokenIds.Contains(identifier) && !readTokenIds.Contains(identifier);
    }

    public bool HasUnreadToken()
    {
        foreach (var tokenId in ownedTokenIds)
        {
            if (!readTokenIds.Contains(tokenId))
            {
                return true;
            }
        }

        return false;
    }

    public List<TokenInfo> GetOwnedTokens()
    {
        var result = new List<TokenInfo>();
        foreach (var identifier in ownedTokenIds)
        {
            TokenInfo info;
            if (CSVLoader.Instance.tokenInfoMap.TryGetValue(identifier, out info))
            {
                result.Add(info);
            }
        }

        return result;
    }

    private void NotifyTokenChanged()
    {
        OnTokensChanged?.Invoke();
        OnTokenUnreadStateChanged?.Invoke();
    }
}
