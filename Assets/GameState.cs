using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class GameState 
{
    public static GameState current;
    public Dictionary<string, ObjectData> object_states;

    public GameState(GameObject[] objects)
    {
        object_states = new Dictionary<string, ObjectData>();
        foreach(GameObject obj in objects)
        {
            object_states.Add(obj.name, new ObjectData(obj));
        }
    }
    public GameState(string data)
    {
        object_states = new Dictionary<string, ObjectData>();
        string[] objects = data.Split("|");
        foreach(string object_ in objects)
        {
            if(object_.Length > 1)
            {
                string[] splited = object_.Split(":");
                object_states.Add(splited[0], new ObjectData(splited[1]));
            }           
        }
    }

    public string toData()
    {
        string s = "";
        foreach(string key in object_states.Keys)
        {
            s += key + ":" + object_states[key].toData() + "|";           
        }

        return s;
    }

    public void applyState(GameObject[] objects)
    {
        foreach (GameObject obj in objects)
        {
            object_states[obj.name].applyDatta(obj);
        }
    }
}

public class ObjectData
{   
    Vector3 pos;
    const string FLOATFORMAT = "{0:0.###}";

    public ObjectData(GameObject obj)
    {
        pos = obj.transform.position;
    }

    public ObjectData(string data)
    {
        string[] info = data.Split('_');

        string[] spos = info[0].Split(',');
        pos = new Vector3(float.Parse(
            spos[0], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(spos[1], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(spos[2], CultureInfo.InvariantCulture.NumberFormat));
    }

    public string toData()
    {
        string s = string.Format(FLOATFORMAT, pos.x).Replace(',', '.') + "," 
            + string.Format(FLOATFORMAT, pos.y).Replace(',', '.') + "," 
            + string.Format(FLOATFORMAT, pos.z).Replace(',', '.') + "_";
        return s;
    }

    public void applyDatta(GameObject obj)
    {
        obj.transform.position = pos;
    }
}
