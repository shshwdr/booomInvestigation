using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TokenNode : MonoBehaviour
{
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private Button button;

    private string tokenIdentifier;
    private GameObject redDot;

    private void Awake()
    {
        redDot = GeneralButtonRedDotUtil.ResolveRedDot(gameObject);
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickToken);
        }
    }

    public void Setup(TokenInfo tokenInfo)
    {
        tokenIdentifier = tokenInfo == null ? string.Empty : tokenInfo.identifier;
        if (tmpText != null)
        {
            tmpText.text = tokenInfo == null ? string.Empty : tokenInfo.name;
        }

        if (redDot != null)
        {
            redDot.SetActive(!string.IsNullOrEmpty(tokenIdentifier) && TokenManager.Instance.IsTokenUnread(tokenIdentifier));
        }
    }

    private void OnClickToken()
    {
        if (string.IsNullOrEmpty(tokenIdentifier))
        {
            return;
        }

        TokenManager.Instance.MarkTokenRead(tokenIdentifier);
        if (redDot != null)
        {
            redDot.SetActive(false);
        }

        DialogueController.Instance.OpenTokenDialogue(tokenIdentifier);
    }
}
