using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    [SerializeField] private Button brainButton;
    [SerializeField] private BrainController brainController;
    private GameObject brainRedDot;

    private void Awake()
    {
        if (brainButton != null)
        {
            brainButton.onClick.RemoveAllListeners();
            brainButton.onClick.AddListener(OpenBrainController);
            brainRedDot = GeneralButtonRedDotUtil.ResolveRedDot(brainButton.gameObject);
        }

        TokenManager.Instance.OnTokenUnreadStateChanged += RefreshBrainRedDot;
        CardManager.Instance.OnCardUnreadStateChanged += RefreshBrainRedDot;
    }

    private void Start()
    {
        RefreshBrainRedDot();
    }

    private void OnDestroy()
    {
        if (TokenManager.hasInitialized())
        {
            TokenManager.Instance.OnTokenUnreadStateChanged -= RefreshBrainRedDot;
        }

        if (CardManager.hasInitialized())
        {
            CardManager.Instance.OnCardUnreadStateChanged -= RefreshBrainRedDot;
        }
    }

    private void OpenBrainController()
    {
        if (brainController != null)
        {
            brainController.Open();
        }
    }

    private void RefreshBrainRedDot()
    {
        if (brainRedDot == null)
        {
            return;
        }

        brainRedDot.SetActive(TokenManager.Instance.HasUnreadToken() || CardManager.Instance.HasUnreadCard());
    }
}
