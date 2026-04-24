using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TokenNode : MonoBehaviour
{
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private Button button;

    private string tokenIdentifier;

    private void Awake()
    {
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
    }

    private void OnClickToken()
    {
        if (string.IsNullOrEmpty(tokenIdentifier))
        {
            return;
        }

        DialogueController.Instance.OpenTokenDialogue(tokenIdentifier);
    }
}
