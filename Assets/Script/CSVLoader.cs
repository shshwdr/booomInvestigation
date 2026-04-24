using System.Collections.Generic;
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
public class CSVLoader : Singleton<CSVLoader>
{
    public Dictionary<string, MapInfo> mapInfoMap = new Dictionary<string, MapInfo>();
    public Dictionary<string, NpcInfo> npcInfoMap = new Dictionary<string, NpcInfo>();
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
        childMapInfos.Clear();
        mapNpcInfos.Clear();

        LoadMapInfos();
        LoadNpcInfos();
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
                Debug.LogError("npc " + npcInfo.identifier + " map is empty.");
                continue;
            }

            if (!mapInfoMap.ContainsKey(npcInfo.map))
            {
                Debug.LogError("npc " + npcInfo.identifier + " map not found: " + npcInfo.map);
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
}
