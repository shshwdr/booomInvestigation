using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform mapNodeRoot;
    [SerializeField] private RectTransform npcRoot;
    [SerializeField] private Image mapImage;
    [SerializeField] private Button backButton;
    [SerializeField] private Text hoverNameText;

    [Header("Prefabs")]
    [SerializeField] private MapTransitNode mapTransitNodePrefab;
    [SerializeField] private NPCController npcPrefab;

    private string currentMapIdentifier;
    private BigMapController owner;

    private void Awake()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnClickBack);
        }
    }

    public void Initialize(BigMapController bigMapController)
    {
        owner = bigMapController;
    }

    public void ShowMap(string mapIdentifier)
    {
        currentMapIdentifier = mapIdentifier;
        gameObject.SetActive(true);

        ClearChildren(mapNodeRoot);
        ClearChildren(npcRoot);
        UpdateMapImage();
        UpdateBackButtonState();
        ShowHoverName(string.Empty);
        SpawnChildMaps();
        SpawnNpcs();
    }

    public void HideSelf()
    {
        gameObject.SetActive(false);
    }

    public void ShowHoverName(string text)
    {
        if (hoverNameText != null)
        {
            hoverNameText.text = text;
        }
    }

    private void UpdateMapImage()
    {
        if (mapImage == null)
        {
            return;
        }

        var sprite = Resources.Load<Sprite>("map/" + currentMapIdentifier);
        if (sprite == null)
        {
            Debug.LogWarning("Map sprite not found: Resources/map/" + currentMapIdentifier);
            return;
        }

        mapImage.sprite = sprite;
    }

    private void UpdateBackButtonState()
    {
        if (backButton == null)
        {
            return;
        }

        var mapInfo = CSVLoader.Instance.mapInfoMap[currentMapIdentifier];
        backButton.gameObject.SetActive(true);

        var backHover = backButton.GetComponent<MapBackHoverTrigger>();
        if (backHover == null)
        {
            backHover = backButton.gameObject.AddComponent<MapBackHoverTrigger>();
        }

        var parentName = string.Empty;
        if (!string.IsNullOrEmpty(mapInfo.parent) && CSVLoader.Instance.mapInfoMap.ContainsKey(mapInfo.parent))
        {
            parentName = CSVLoader.Instance.mapInfoMap[mapInfo.parent].name;
        }

        backHover.Setup(
            parentName,
            () => ShowHoverName(string.Empty),
            ShowHoverName
        );
    }

    private void SpawnChildMaps()
    {
        if (mapTransitNodePrefab == null || mapNodeRoot == null)
        {
            return;
        }

        var children = CSVLoader.Instance.GetChildMaps(currentMapIdentifier);
        foreach (var child in children)
        {
            var node = Instantiate(mapTransitNodePrefab, mapNodeRoot);
            node.Setup(child, () => ShowMap(child.identifier), ShowHoverName);
            SetPositionByPercent(node.GetComponent<RectTransform>(), child.pos, mapNodeRoot);
        }
    }

    private void SpawnNpcs()
    {
        if (npcPrefab == null || npcRoot == null)
        {
            return;
        }

        var npcs = CSVLoader.Instance.GetNpcsByMap(currentMapIdentifier);
        foreach (var npc in npcs)
        {
            var node = Instantiate(npcPrefab, npcRoot);
            node.Setup(npc, ShowHoverName);
            SetPositionByPercent(node.GetComponent<RectTransform>(), npc.pos, npcRoot);
        }
    }

    private void OnClickBack()
    {
        var mapInfo = CSVLoader.Instance.mapInfoMap[currentMapIdentifier];
        if (string.IsNullOrEmpty(mapInfo.parent))
        {
            HideSelf();
            return;
        }

        ShowMap(mapInfo.parent);
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

    private static void SetPositionByPercent(RectTransform target, System.Collections.Generic.List<float> pos, RectTransform parent)
    {
        if (target == null || parent == null || pos == null || pos.Count != 2)
        {
            return;
        }

        var x = parent.rect.width * pos[0];
        var y = parent.rect.height * pos[1];
        target.anchorMin = new Vector2(0f, 0f);
        target.anchorMax = new Vector2(0f, 0f);
        target.pivot = new Vector2(0.5f, 0.5f);
        target.anchoredPosition = new Vector2(x, y);
    }
}
