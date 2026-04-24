using UnityEngine;
using UnityEngine.UI;

public class BigMapController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform rootNodeContainer;
    [SerializeField] private Text hoverNameText;
    [SerializeField] private MapController mapController;

    [Header("Prefabs")]
    [SerializeField] private MapTransitNode mapTransitNodePrefab;

    private void Start()
    {
        BuildRootMapNodes();
        if (mapController != null)
        {
            mapController.Initialize(this);
            mapController.HideSelf();
        }
    }

    private void BuildRootMapNodes()
    {
        if (rootNodeContainer == null || mapTransitNodePrefab == null)
        {
            return;
        }

        ClearChildren(rootNodeContainer);
        var roots = CSVLoader.Instance.GetChildMaps(string.Empty);
        foreach (var mapInfo in roots)
        {
            var node = Instantiate(mapTransitNodePrefab, rootNodeContainer);
            node.Setup(mapInfo, () => OpenMap(mapInfo.identifier), ShowHoverName);
            SetPositionByPercent(node.GetComponent<RectTransform>(), mapInfo.pos, rootNodeContainer);
        }
    }

    private void OpenMap(string mapIdentifier)
    {
        if (mapController == null)
        {
            return;
        }

        mapController.ShowMap(mapIdentifier);
    }

    public void ShowHoverName(string mapName)
    {
        if (hoverNameText != null)
        {
            hoverNameText.text = mapName;
        }
    }

    private static void ClearChildren(RectTransform root)
    {
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
