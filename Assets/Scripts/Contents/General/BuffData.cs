using System.Collections.Generic;

using UnityEngine;

public static class BuffData
{
    public class BuffDataStruct
    {
        public int ID;
        public string BuffClass;
        public EBuffTag BuffTag;
        public EBuffType BuffType;
        public bool bOneTimeBuff;
        public float BuffTime;
        public float BuffTimeDiff;
        public int MaxLayer;
        public bool bTimeRefresh;
        public bool bRemoveAll;

        public BuffDataStruct(int ID, string BuffClass, EBuffTag BuffTag, EBuffType BuffType, bool bOneTimeBuff, float BuffTime, float BuffTimeDiff, int MaxLayer, bool bTimeRefresh, bool bRemoveAll)
        {
            this.ID = ID;
            this.BuffClass = BuffClass;
            this.BuffTag = BuffTag;
            this.BuffType = BuffType;
            this.bOneTimeBuff = bOneTimeBuff;
            this.BuffTime = BuffTime;
            this.BuffTimeDiff = BuffTimeDiff;
            this.MaxLayer = MaxLayer;
            this.bTimeRefresh = bTimeRefresh;
            this.bRemoveAll = bRemoveAll;
        }
    }
    public static Dictionary<int, BuffDataStruct> data = new Dictionary<int, BuffDataStruct>
    {
        {0, new BuffDataStruct(0, "SampleBuff", EBuffTag.SAMPLE_BUFF, EBuffType.NEUTRAL, false, 5f, 1f, 3, true, false)},
    };

    public static BuffDataStruct GetData(int id)
    {
        if (data.TryGetValue(id, out BuffDataStruct result))
        {
            return result;
        }
        else
        {
            return null;
        }
    }
}
