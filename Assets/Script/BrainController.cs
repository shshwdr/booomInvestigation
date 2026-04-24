using UnityEngine;
using UnityEngine.UI;

public class BrainController : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private RectTransform tokenParent;
    [SerializeField] private RectTransform cardParent;
    [SerializeField] private TokenNode tokenNodePrefab;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
        RefreshTokens();
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
