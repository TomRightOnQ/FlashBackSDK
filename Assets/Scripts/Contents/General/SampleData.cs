using System.Collections.Generic;

using UnityEngine;

public static class SampleData
{
    public class SampleDataStruct
    {
        public int ID;
        public string Name;
        public string Info;

        public SampleDataStruct(int ID, string Name, string Info)
        {
            this.ID = ID;
            this.Name = Name;
            this.Info = Info;
        }
    }
    public static Dictionary<int, SampleDataStruct> data = new Dictionary<int, SampleDataStruct>
    {
        {1, new SampleDataStruct(1, "SampleName", "SampleInfo")},
    };

    public static SampleDataStruct GetData(int id)
    {
        if (data.TryGetValue(id, out SampleDataStruct result))
        {
            return result;
        }
        else
        {
            return null;
        }
    }
}
