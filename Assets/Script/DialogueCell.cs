using TMPro;
using UnityEngine;

public class DialogueCell : MonoBehaviour
{
    [SerializeField] private TMP_Text tmpText;

    public void SetContent(string content)
    {
        if (tmpText != null)
        {
            tmpText.text = content;
        }
    }
}
