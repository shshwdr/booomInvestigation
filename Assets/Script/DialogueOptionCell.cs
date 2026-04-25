using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueOptionCell : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private GameObject selectedGameobject;

    private object generalButtonComponent;
    private MethodInfo setInteractiveMethod;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponentInChildren<Button>(true);
        }

        if (tmpText == null)
        {
            tmpText = GetComponentInChildren<TMP_Text>(true);
        }

        ResolveGeneralButton();
        SetSelected(false);
    }

    public void SetOption(string content, Action onClick, bool interactable)
    {
        if (tmpText != null)
        {
            tmpText.text = content;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }
        }

        SetSelected(false);
        SetInteractable(interactable);
    }

    public void SetSelected(bool selected)
    {
        if (selectedGameobject != null)
        {
            selectedGameobject.SetActive(selected);
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (generalButtonComponent != null && setInteractiveMethod != null)
        {
            setInteractiveMethod.Invoke(generalButtonComponent, new object[] { interactable });
            return;
        }

        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    private void ResolveGeneralButton()
    {
        var components = GetComponents<Component>();
        foreach (var component in components)
        {
            if (component == null)
            {
                continue;
            }

            var type = component.GetType();
            if (type.Name != "GeneralButton")
            {
                continue;
            }

            var method = type.GetMethod("setInteractive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                continue;
            }

            generalButtonComponent = component;
            setInteractiveMethod = method;
            return;
        }
    }
}
