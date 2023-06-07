using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public interface GameState
{
    const string FLOATFORMAT = "{0:0.###}";

    public string toData();

    public void applyState(GameObject[] objects);

    public static string vectorToString(Vector3 v)
    {
        return string.Format(FLOATFORMAT, v.x).Replace(',', '.') + ","
            + string.Format(FLOATFORMAT,  v.y).Replace(',', '.') + ","
            + string.Format(FLOATFORMAT,  v.z).Replace(',', '.');
    }

    public static Vector3 StringToVector(string v)
    {
        string[] sv = v.Split(',');
        return new Vector3(float.Parse(
            sv[0], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(sv[1], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(sv[2], CultureInfo.InvariantCulture.NumberFormat));
    }
}

public interface ObjectData
{
    public string toData();

    public void applyDatta(GameObject obj);
}
