using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailPanel : Singleton<CardDetailPanel>
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Button closeButton;

    protected override void Awake()
    {
        base.Awake();

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        Close();
    }

    public void Show(CardInfo cardInfo)
    {
        if (cardInfo == null)
        {
            return;
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }

        if (nameText != null)
        {
            nameText.text = cardInfo.name;
        }

        if (descText != null)
        {
            descText.text = string.IsNullOrEmpty(cardInfo.desc) ? string.Empty : cardInfo.desc;
        }
    }

    public void Close()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
