using System.Collections.Generic;
using System.Linq;
using Sinbad;
using UnityEngine;

public class MapInfo
{
    public string identifier;
    public string name;
    public List<float> pos = new List<float>();
    public string parent;
}

public class NpcInfo
{
    public string identifier;
    public string name;
    public List<float> pos = new List<float>();
    public string map;
}

public class TokenInfo
{
    public string identifier;
    public string name;
    public bool isStart;
}

public class CardInfo
{
    public string identifier;
    public string name;
    public bool isStart;
    public bool isKey;
}

public class CardMergeInfo
{
    public string card1;
    public string card2;
    public string result;
}

public class DialogueInfo
{
    public string identifier;
    public string text;
    public string speaker;
    public List<string> next = new List<string>();
    public List<string> options = new List<string>();
    public Dictionary<string, string> reward = new Dictionary<string, string>();
}
public class CSVLoader : Singleton<CSVLoader>
{
    public Dictionary<string, MapInfo> mapInfoMap = new Dictionary<string, MapInfo>();
    public Dictionary<string, NpcInfo> npcInfoMap = new Dictionary<string, NpcInfo>();
    public Dictionary<string, TokenInfo> tokenInfoMap = new Dictionary<string, TokenInfo>();
    public Dictionary<string, CardInfo> cardInfoMap = new Dictionary<string, CardInfo>();
    public List<CardMergeInfo> cardMergeInfos = new List<CardMergeInfo>();
    public Dictionary<string, Dictionary<string, DialogueInfo>> dialogueInfosByFile = new Dictionary<string, Dictionary<string, DialogueInfo>>();
    public Dictionary<string, List<string>> dialogueOrderByFile = new Dictionary<string, List<string>>();
    public Dictionary<string, List<MapInfo>> childMapInfos = new Dictionary<string, List<MapInfo>>();
    public Dictionary<string, List<NpcInfo>> mapNpcInfos = new Dictionary<string, List<NpcInfo>>();

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        LoadAllCsv();
    }

    private void LoadAllCsv()
    {
        mapInfoMap.Clear();
        npcInfoMap.Clear();
        tokenInfoMap.Clear();
        cardInfoMap.Clear();
        cardMergeInfos.Clear();
        dialogueInfosByFile.Clear();
        dialogueOrderByFile.Clear();
        childMapInfos.Clear();
        mapNpcInfos.Clear();

        LoadMapInfos();
        LoadNpcInfos();
        LoadTokenInfos();
        LoadCardInfos();
        LoadCardMergeInfos();
        LoadDialogueInfos();
        ValidateData();
        BuildLookupTables();
    }

    private void LoadMapInfos()
    {
        var infos = CsvUtil.LoadObjects<MapInfo>("map");
        foreach (var info in infos)
        {
            mapInfoMap[info.identifier] = info;
        }
    }

    private void LoadNpcInfos()
    {
        var infos = CsvUtil.LoadObjects<NpcInfo>("npc");
        foreach (var info in infos)
        {
            npcInfoMap[info.identifier] = info;
        }
    }

    private void LoadTokenInfos()
    {
        var infos = CsvUtil.LoadObjects<TokenInfo>("token");
        foreach (var info in infos)
        {
            tokenInfoMap[info.identifier] = info;
        }
    }

    private void LoadCardInfos()
    {
        var infos = CsvUtil.LoadObjects<CardInfo>("card");
        foreach (var info in infos)
        {
            cardInfoMap[info.identifier] = info;
        }
    }

    private void LoadCardMergeInfos()
    {
        cardMergeInfos = CsvUtil.LoadObjects<CardMergeInfo>("cardMerge");
    }

    private void LoadDialogueInfos()
    {
        var files = Resources.LoadAll<TextAsset>("csv/dialogue");
        foreach (var file in files)
        {
            var fileName = file.name;
            var infos = CsvUtil.LoadObjects<DialogueInfo>("dialogue/" + fileName);
            var dialogueMap = new Dictionary<string, DialogueInfo>();
            var dialogueOrder = new List<string>();
            foreach (var info in infos)
            {
                if (string.IsNullOrEmpty(info.identifier))
                {
                    continue;
                }

                info.next = SanitizeList(info.next);
                info.options = SanitizeList(info.options);
                info.reward = SanitizeDictionary(info.reward);

                dialogueMap[info.identifier] = info;
                dialogueOrder.Add(info.identifier);
            }

            dialogueInfosByFile[fileName] = dialogueMap;
            dialogueOrderByFile[fileName] = dialogueOrder;
        }
    }

    private void ValidateData()
    {
        foreach (var mapInfo in mapInfoMap.Values)
        {
            if (!IsValidPos(mapInfo.pos))
            {
                Debug.LogError("map " + mapInfo.identifier + " has invalid pos, require exactly 2 floats.");
            }

            if (!string.IsNullOrEmpty(mapInfo.parent) && !mapInfoMap.ContainsKey(mapInfo.parent))
            {
                Debug.LogError("map " + mapInfo.identifier + " parent not found: " + mapInfo.parent);
            }
        }

        foreach (var npcInfo in npcInfoMap.Values)
        {
            if (!IsValidPos(npcInfo.pos))
            {
                Debug.LogError("npc " + npcInfo.identifier + " has invalid pos, require exactly 2 floats.");
            }

            if (string.IsNullOrEmpty(npcInfo.map))
            {
               // Debug.LogError("npc " + npcInfo.identifier + " map is empty.");
                continue;
            }

            if (!mapInfoMap.ContainsKey(npcInfo.map))
            {
                Debug.LogError("npc " + npcInfo.identifier + " map not found: " + npcInfo.map);
            }
        }

        foreach (var fileEntry in dialogueInfosByFile)
        {
            var fileName = fileEntry.Key;
            var dialogues = fileEntry.Value;
            foreach (var dialogue in dialogues.Values)
            {
                foreach (var nextId in dialogue.next)
                {
                    if (!dialogues.ContainsKey(nextId))
                    {
                        Debug.LogError("dialogue " + fileName + " next not found: " + nextId);
                    }
                }

                foreach (var reward in dialogue.reward)
                {
                    if (reward.Key == "token" && !tokenInfoMap.ContainsKey(reward.Value))
                    {
                        Debug.LogError("dialogue " + fileName + " reward token not found: " + reward.Value);
                    }

                    if (reward.Key == "card" && !cardInfoMap.ContainsKey(reward.Value))
                    {
                        Debug.LogError("dialogue " + fileName + " reward card not found: " + reward.Value);
                    }
                }
            }
        }
    }

    private void BuildLookupTables()
    {
        foreach (var mapInfo in mapInfoMap.Values)
        {
            var key = string.IsNullOrEmpty(mapInfo.parent) ? string.Empty : mapInfo.parent;
            if (!childMapInfos.ContainsKey(key))
            {
                childMapInfos[key] = new List<MapInfo>();
            }

            childMapInfos[key].Add(mapInfo);
        }

        foreach (var npcInfo in npcInfoMap.Values)
        {
            if (string.IsNullOrEmpty(npcInfo.map))
            {
                continue;
            }

            if (!mapNpcInfos.ContainsKey(npcInfo.map))
            {
                mapNpcInfos[npcInfo.map] = new List<NpcInfo>();
            }

            mapNpcInfos[npcInfo.map].Add(npcInfo);
        }
    }

    public List<MapInfo> GetChildMaps(string parentIdentifier)
    {
        var key = string.IsNullOrEmpty(parentIdentifier) ? string.Empty : parentIdentifier;
        List<MapInfo> list;
        if (childMapInfos.TryGetValue(key, out list))
        {
            return list;
        }

        return new List<MapInfo>();
    }

    public List<NpcInfo> GetNpcsByMap(string mapIdentifier)
    {
        List<NpcInfo> list;
        if (mapNpcInfos.TryGetValue(mapIdentifier, out list))
        {
            return list;
        }

        return new List<NpcInfo>();
    }

    private static bool IsValidPos(List<float> pos)
    {
        return pos != null && pos.Count == 2;
    }

    public bool TryGetDialogueFile(string fileName, out Dictionary<string, DialogueInfo> dialogueMap)
    {
        return dialogueInfosByFile.TryGetValue(fileName, out dialogueMap);
    }

    public string GetFirstDialogueId(string fileName)
    {
        List<string> order;
        if (dialogueOrderByFile.TryGetValue(fileName, out order))
        {
            return order.FirstOrDefault();
        }

        return string.Empty;
    }

    private static List<string> SanitizeList(List<string> source)
    {
        if (source == null)
        {
            return new List<string>();
        }

        var result = new List<string>();
        foreach (var value in source)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            result.Add(value.Trim());
        }

        return result;
    }

    private static Dictionary<string, string> SanitizeDictionary(Dictionary<string, string> source)
    {
        if (source == null)
        {
            return new Dictionary<string, string>();
        }

        return source
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key))
            .ToDictionary(
                pair => pair.Key.Trim(),
                pair => pair.Value == null ? string.Empty : pair.Value.Trim()
            );
    }
}
