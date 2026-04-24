using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    [SerializeField] private Button brainButton;
    [SerializeField] private BrainController brainController;

    private void Awake()
    {
        if (brainButton != null)
        {
            brainButton.onClick.RemoveAllListeners();
            brainButton.onClick.AddListener(OpenBrainController);
        }
    }

    private void OpenBrainController()
    {
        if (brainController != null)
        {
            brainController.Open();
        }
    }
}
