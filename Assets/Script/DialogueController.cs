using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueController : Singleton<DialogueController>, IPointerClickHandler
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private RectTransform dialogueRoot;
    [SerializeField] private ScrollRect dialogueScrollRect;
    [SerializeField] private DialogueCell dialogueCellPrefab;
    [SerializeField] private DialogueOptionCell dialogueOptionCellPrefab;
    [SerializeField] private Button endDialogueButton;

    private Dictionary<string, DialogueInfo> currentDialogueMap;
    private string currentDialogueId;
    private readonly HashSet<string> rewardedDialogueIds = new HashSet<string>();
    private readonly List<DialogueOptionCell> currentOptionCells = new List<DialogueOptionCell>();
    private bool waitingForOptionSelection;

    private void Start()
    {
        if (endDialogueButton != null)
        {
            endDialogueButton.gameObject.SetActive(false);
            endDialogueButton.onClick.RemoveAllListeners();
            endDialogueButton.onClick.AddListener(CloseDialogue);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void OpenNpcDialogue(string npcIdentifier)
    {
        OpenDialogueFile(npcIdentifier);
    }

    public void OpenDialogueFileByName(string fileName)
    {
        OpenDialogueFile(fileName);
    }

    public void OpenTokenDialogue(string tokenIdentifier)
    {
        OpenDialogueFile("token_" + tokenIdentifier);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (waitingForOptionSelection)
        {
            return;
        }

        AdvanceDialogue();
    }

    private void OpenDialogueFile(string fileName)
    {
        Dictionary<string, DialogueInfo> dialogueMap;
        if (!CSVLoader.Instance.TryGetDialogueFile(fileName, out dialogueMap))
        {
            Debug.LogWarning("Dialogue file not found: " + fileName);
            return;
        }

        var firstId = CSVLoader.Instance.GetFirstDialogueId(fileName);
        if (string.IsNullOrEmpty(firstId))
        {
            Debug.LogWarning("Dialogue file is empty: " + fileName);
            return;
        }

        currentDialogueMap = dialogueMap;
        currentDialogueId = firstId;
        rewardedDialogueIds.Clear();
        currentOptionCells.Clear();
        waitingForOptionSelection = false;
        ClearChildren(dialogueRoot);

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (endDialogueButton != null)
        {
            endDialogueButton.gameObject.SetActive(false);
        }

        ShowCurrentDialogue();
    }

    private void AdvanceDialogue()
    {
        if (waitingForOptionSelection)
        {
            return;
        }

        if (currentDialogueMap == null || string.IsNullOrEmpty(currentDialogueId))
        {
            return;
        }

        DialogueInfo currentInfo;
        if (!currentDialogueMap.TryGetValue(currentDialogueId, out currentInfo))
        {
            return;
        }

        if (currentInfo.next != null && currentInfo.next.Count > 0)
        {
            var nextId = currentInfo.next[0];
            if (!currentDialogueMap.ContainsKey(nextId))
            {
                Debug.LogWarning("Next dialogue id not found: " + nextId);
                ShowEndButton();
                return;
            }

            currentDialogueId = nextId;
            ShowCurrentDialogue();
            return;
        }

        if (currentInfo.options != null && currentInfo.options.Count > 0)
        {
            ShowOptionCells(currentInfo.options);
            return;
        }

        ShowEndButton();
    }

    private void ShowCurrentDialogue()
    {
        if (dialogueCellPrefab == null || dialogueRoot == null)
        {
            return;
        }

        DialogueInfo info;
        if (!currentDialogueMap.TryGetValue(currentDialogueId, out info))
        {
            return;
        }

        var cell = Instantiate(dialogueCellPrefab, dialogueRoot);
        cell.SetContent(info.text);
        ClearCurrentOptionCells();
        waitingForOptionSelection = false;
        if (endDialogueButton != null)
        {
            endDialogueButton.gameObject.SetActive(false);
        }
        ApplyReward(info);
        ScrollToBottom();
    }

    private void ShowOptionCells(List<string> optionIds)
    {
        ClearCurrentOptionCells();
        waitingForOptionSelection = false;
        if (endDialogueButton != null)
        {
            endDialogueButton.gameObject.SetActive(false);
        }

        if (dialogueOptionCellPrefab == null || dialogueRoot == null)
        {
            Debug.LogWarning("Dialogue option cell prefab missing.");
            ShowEndButton();
            return;
        }

        var validOptions = new List<DialogueInfo>();
        foreach (var optionId in optionIds)
        {
            DialogueInfo optionInfo;
            if (!currentDialogueMap.TryGetValue(optionId, out optionInfo))
            {
                Debug.LogWarning("Option dialogue id not found: " + optionId);
                continue;
            }

            validOptions.Add(optionInfo);
        }

        if (validOptions.Count == 0)
        {
            ShowEndButton();
            return;
        }

        waitingForOptionSelection = true;
        foreach (var optionInfo in validOptions)
        {
            var optionCell = Instantiate(dialogueOptionCellPrefab, dialogueRoot);
            var interactable = IsRequirementSatisfied(optionInfo.requirement);
            optionCell.SetOption(optionInfo.text, () => OnOptionSelected(optionCell, optionInfo), interactable);
            currentOptionCells.Add(optionCell);
        }

        ScrollToBottom();
    }

    private void OnOptionSelected(DialogueOptionCell selectedCell, DialogueInfo selectedOption)
    {
        if (!waitingForOptionSelection || selectedOption == null)
        {
            return;
        }

        waitingForOptionSelection = false;
        foreach (var optionCell in currentOptionCells)
        {
            if (optionCell == null)
            {
                continue;
            }

            var isSelected = optionCell == selectedCell;
            optionCell.SetSelected(isSelected);
            optionCell.SetInteractable(false);
        }

        currentDialogueId = selectedOption.identifier;
        ShowCurrentDialogue();
    }

    private bool IsRequirementSatisfied(List<string> requirements)
    {
        if (requirements == null || requirements.Count == 0)
        {
            return true;
        }

        foreach (var requirement in requirements)
        {
            if (string.IsNullOrEmpty(requirement))
            {
                return false;
            }

            var splitIndex = requirement.IndexOf('_');
            if (splitIndex <= 0 || splitIndex >= requirement.Length - 1)
            {
                return false;
            }

            var prefix = requirement.Substring(0, splitIndex);
            var identifier = requirement.Substring(splitIndex + 1);
            if (prefix == "card")
            {
                if (!CardManager.Instance.HasCard(identifier))
                {
                    return false;
                }

                continue;
            }

            if (prefix == "token")
            {
                if (!TokenManager.Instance.HasToken(identifier))
                {
                    return false;
                }

                continue;
            }

            return false;
        }

        return true;
    }

    private void ApplyReward(DialogueInfo info)
    {
        if (info == null || string.IsNullOrEmpty(info.identifier))
        {
            return;
        }

        if (rewardedDialogueIds.Contains(info.identifier))
        {
            return;
        }

        rewardedDialogueIds.Add(info.identifier);
        if (info.reward == null || info.reward.Count == 0)
        {
            return;
        }

        foreach (var reward in info.reward)
        {
            if (reward.Key == "token")
            {
                TokenManager.Instance.AddToken(reward.Value);
            }
            else if (reward.Key == "card")
            {
                CardManager.Instance.AddCard(reward.Value);
            }
        }
    }

    private void ScrollToBottom()
    {
        if (dialogueScrollRect == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        dialogueScrollRect.verticalNormalizedPosition = 0f;
    }

    private void ShowEndButton()
    {
        if (endDialogueButton != null)
        {
            endDialogueButton.gameObject.SetActive(true);
        }
    }

    private void CloseDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        currentDialogueMap = null;
        currentDialogueId = string.Empty;
        rewardedDialogueIds.Clear();
        ClearCurrentOptionCells();
        waitingForOptionSelection = false;
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

    private void ClearCurrentOptionCells()
    {
        currentOptionCells.Clear();
    }
}
