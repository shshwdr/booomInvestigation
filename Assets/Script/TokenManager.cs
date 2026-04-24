using System.Collections.Generic;
using UnityEngine;

public class TokenManager : Singleton<TokenManager>
{
    private readonly HashSet<string> ownedTokenIds = new HashSet<string>();

    public void Init()
    {
        ownedTokenIds.Clear();
        foreach (var tokenInfo in CSVLoader.Instance.tokenInfoMap.Values)
        {
            if (tokenInfo != null && tokenInfo.isStart && !string.IsNullOrEmpty(tokenInfo.identifier))
            {
                ownedTokenIds.Add(tokenInfo.identifier);
            }
        }
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

        return ownedTokenIds.Add(identifier);
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
}
