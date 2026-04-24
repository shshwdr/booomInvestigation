using System.Collections;
using System.Collections.Generic;
using Sinbad;
using UnityEngine;

public class MapInfo
{
    public string identifier;
}
public class CSVLoader : Singleton<CSVLoader>
{
    // 修改为按state分组的字典结构：外层key是state，内层key是shape identifier
    public Dictionary<string, MapInfo> mapInfoMap = new Dictionary<string, MapInfo>();
    // Start is called before the first frame update
    void Start()
    {
        
        var mapInfos = CsvUtil.LoadObjects<MapInfo>("map");
        foreach (var info in mapInfos)
        {
            mapInfoMap[info.identifier] = info;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
