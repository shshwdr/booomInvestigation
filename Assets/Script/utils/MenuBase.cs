using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 菜单基类：所有弹出菜单的基类
/// </summary>
public abstract class MenuBase : MonoBehaviour
{
    [Header("Menu Base")]
    [SerializeField] protected GameObject mainMenu;
    [SerializeField] protected Button closeButton;
    
    protected virtual void Awake()
    {
        // 隐藏主菜单
        if (mainMenu != null)
        {
            mainMenu.SetActive(false);
        }
        
        // 设置transform为完全stretch并居中
        SetupRectTransform();
        
        // 绑定关闭按钮
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }
    }
    
    /// <summary>
    /// 设置RectTransform为完全stretch并居中
    /// </summary>
    private void SetupRectTransform()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 设置锚点为stretch
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            
            // 设置偏移为0，使其完全填充父容器
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // 确保居中
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
    
    /// <summary>
    /// 显示菜单
    /// </summary>
    public virtual void Show()
    {
        if (mainMenu != null)
        {
            mainMenu.SetActive(true);
        }
    }
    
    /// <summary>
    /// 隐藏菜单
    /// </summary>
    public virtual void Hide()
    {
        if (mainMenu != null)
        {
            mainMenu.SetActive(false);
        }
    }
    
    /// <summary>
    /// 切换菜单显示状态
    /// </summary>
    public void Toggle()
    {
        if (mainMenu != null)
        {
            if (mainMenu.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
    }
}

