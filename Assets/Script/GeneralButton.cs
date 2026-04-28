using UnityEngine;
using UnityEngine.UI;

public class GeneralButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject redDot;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    // Keep this method name for compatibility with existing reflection calls.
    public void setInteractive(bool interactive)
    {
        if (button != null)
        {
            button.interactable = interactive;
        }
    }

    public void SetRedDotVisible(bool visible)
    {
        if (redDot != null)
        {
            redDot.SetActive(visible);
        }
    }

    public void ShowRedDot()
    {
        SetRedDotVisible(true);
    }

    public void HideRedDot()
    {
        SetRedDotVisible(false);
    }
}
