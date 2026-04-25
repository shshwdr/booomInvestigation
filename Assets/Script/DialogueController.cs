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
    [SerializeField] private Button endDialogueButton;

    private Dictionary<string, DialogueInfo> currentDialogueMap;
    private string currentDialogueId;
    private readonly HashSet<string> rewardedDialogueIds = new HashSet<string>();

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
        if (currentDialogueMap == null || string.IsNullOrEmpty(currentDialogueId))
        {
            return;
        }

        DialogueInfo currentInfo;
        if (!currentDialogueMap.TryGetValue(currentDialogueId, out currentInfo))
        {
            return;
        }

        if (currentInfo.next != null && currentInfo.next.Count > 0 && !string.IsNullOrEmpty(currentInfo.next[0]))
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
        ApplyReward(info);
        ScrollToBottom();
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
