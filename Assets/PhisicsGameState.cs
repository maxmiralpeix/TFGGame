using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PhisicsGameState : GameState
{
    public static PhisicsGameState current;
    public Dictionary<string, ObjectData> object_states;

    public PhisicsGameState(GameObject[] objects)
    {
        object_states = new Dictionary<string, ObjectData>();
        foreach(GameObject obj in objects)
        {
            object_states.Add(obj.name, new ObjectDataPhisics(obj));
        }
    }
    public PhisicsGameState(string data)
    {
        object_states = new Dictionary<string, ObjectData>();
        string[] objects = data.Split("|");
        foreach(string object_ in objects)
        {
            if(object_.Length > 1)
            {
                string[] splited = object_.Split("~");
                object_states.Add(splited[0], new ObjectDataPhisics(splited[1]));
            }           
        }
    }

    public string toData()
    {
        string s = "";
        foreach(string key in object_states.Keys)
        {
            s += key + "~" + object_states[key].toData() + "|";           
        }

        return s;
    }

    public void applyState(GameObject[] objects)
    {
        bool instantiate = (objects.Length < object_states.Count);
        List<string> unused = (instantiate) ? new List<string>(object_states.Keys) : null;
        foreach (GameObject obj in objects)
        {
            object_states[obj.name].applyDatta(obj);
            if (instantiate) unused.Remove(obj.name);
        }
        if (instantiate)
        {
            foreach (string newobj in unused)
            {
                InstantiateNewObject(newobj);
            }
        }
    }

    void InstantiateNewObject(string name)
    {
        string object_type = name.Split(";")[1];

        switch(object_type)
        {           
            case "PJ":
                GameDirector.currentGame.AddClient(name, NetMode.Client);
            break;
            default:
                
            break;
        }
    }

    public string VectorToString(Vector3 v)
    {
        return GameState.vectorToString(v);
    }

    public Vector3 StringToVector(string v)
    {
        return GameState.StringToVector(v);
    }
}

public class ObjectDataPhisics : ObjectData
{   
    Vector3 pos; Vector3 vel; Vector3 rot; Vector3 ang_vel;  

    public ObjectDataPhisics(GameObject obj)
    {
        pos = obj.transform.position;
        rot = obj.transform.rotation.eulerAngles;

        Rigidbody rb = null;
        if(obj.TryGetComponent<Rigidbody>(out rb))
        {
            vel = rb.velocity;
            ang_vel = rb.angularVelocity;
        } else { vel = Vector3.zero; ang_vel = vel; }
    }

    public ObjectDataPhisics(string data)
    {
        string[] info = data.Split('_');

        pos = GameState.StringToVector(info[0]);
              
        vel = GameState.StringToVector(info[1]);
              
        rot = GameState.StringToVector(info[2]);

        ang_vel = GameState.StringToVector(info[3]);
    }

    public string toData()
    {
        string s = GameState.vectorToString(pos) + "_";
       
        s += GameState.vectorToString(vel) + "_"; ;

        s += GameState.vectorToString(rot) + "_"; ;

        s += GameState.vectorToString(ang_vel) + "_"; ;
        return s;
    }

    public void applyDatta(GameObject obj)
    {
        obj.transform.position = pos;
        obj.transform.rotation = Quaternion.Euler(rot);

        FakeRigidBody frb = null;
        if (obj.TryGetComponent<FakeRigidBody>(out frb))
        {
            frb.velocity = vel;
            frb.angularVelocity = ang_vel;
        }
    }
}
