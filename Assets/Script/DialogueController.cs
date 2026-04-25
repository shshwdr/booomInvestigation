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
            DialogueInfo nextInfo;
            if (!TryResolveNextDialogue(currentInfo.next, out nextInfo))
            {
                ShowEndButton();
                return;
            }

            currentDialogueId = nextInfo.identifier;
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
            string prefix;
            string identifier;
            if (!TryParseTypedIdentifier(requirement, out prefix, out identifier))
            {
                return false;
            }

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
            string prefix;
            string identifier;
            if (!TryParseTypedIdentifier(reward, out prefix, out identifier))
            {
                Debug.LogWarning("Invalid reward format: " + reward);
                continue;
            }

            if (prefix == "token")
            {
                TokenManager.Instance.AddToken(identifier);
            }
            else if (prefix == "card")
            {
                CardManager.Instance.AddCard(identifier);
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

    private bool TryResolveNextDialogue(List<string> nextIds, out DialogueInfo resolvedInfo)
    {
        resolvedInfo = null;
        if (nextIds == null || nextIds.Count == 0 || currentDialogueMap == null)
        {
            return false;
        }

        foreach (var nextId in nextIds)
        {
            DialogueInfo candidate;
            if (!currentDialogueMap.TryGetValue(nextId, out candidate))
            {
                Debug.LogWarning("Next dialogue id not found: " + nextId);
                continue;
            }

            if (!IsRequirementSatisfied(candidate.requirement))
            {
                continue;
            }

            resolvedInfo = candidate;
            return true;
        }

        return false;
    }

    private static bool TryParseTypedIdentifier(string value, out string prefix, out string identifier)
    {
        prefix = string.Empty;
        identifier = string.Empty;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        var splitIndex = value.IndexOf(':');
        if (splitIndex <= 0 || splitIndex >= value.Length - 1)
        {
            return false;
        }

        prefix = value.Substring(0, splitIndex).Trim();
        identifier = value.Substring(splitIndex + 1).Trim();
        return !string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(identifier);
    }
}
