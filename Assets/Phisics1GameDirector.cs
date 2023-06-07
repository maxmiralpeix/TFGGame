using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Phisics1GameDirector : GameDirector
{
    public GameObject[] TrackedObjects;
    public GameObject PJ;
    RequestSolver rs;
    Dictionary<string, PJController> pj_controllers = new Dictionary<string, PJController>();
    GameObject ControlledPJ; bool controling;

    void Start()
    {
        base.BaseStart();
    }

    // Update is called once per frame
    void Update()
    {
        if(ControlledPJ != null && !controling)
        {
            controling = true;
            StartCoroutine(ListenToPJInput());
        }

        base.BaseUpdate();
    }

    public override void AddClient(string client, NetMode mode)
    {
        var pj = Instantiate(PJ, Vector3.zero, Quaternion.identity);
        pj.name = client + ";PJ";
        List<GameObject> nl = new List<GameObject>(TrackedObjects);
        nl.Add(pj);
        TrackedObjects = nl.ToArray();
        PJController controler = pj.GetComponent<PJController>();
        switch (mode)
        {
            case NetMode.Host:
                pj_controllers.Add(client, controler);
                break;
            default:
                Destroy(controler);
                if (client.Equals(LANClient.ConnectionID + ":" + LANClient.PublicName))
                {
                    ControlledPJ = pj;
                }
                break;
        }

        base.AddClient(client, mode);
    }

    public override void PrepareClient(NetworkData NetData)
    {
        Thread connect = null;
        PhisicsGameState.current = new PhisicsGameState(TrackedObjects);
        RemoveRB();
        rs = new CRSPhisics1();
        connect = new Thread(new ThreadStart(
            () => NetData.Net.SetUpAsGest(NetData.server_reference, rs)));
        connect.Start();
        base.PrepareClient(NetData);
    }
    public override void PrepareHost(NetworkData NetData)
    {
        Thread connect = null;
        PhisicsGameState.current = new PhisicsGameState(TrackedObjects);
        rs = new HRSPhisics1();
        connect = new Thread(new ThreadStart(() => NetData.Net.SetUpAsHost(rs)));      
        connect.Start();
        base.PrepareHost(NetData);
    }

    private void RemoveRB()
    {
        foreach (GameObject obj in TrackedObjects)
        {
            Rigidbody rb = null;
            if (obj.TryGetComponent<Rigidbody>(out rb))
            {
                Destroy(rb);
            }
        }
    }

    public override GameState GameState(NetworkData NetData)
    {
        return PhisicsGameState.current;
    }

    public override void SetGameState(GameState gameState)
    {
        PhisicsGameState.current = gameState as PhisicsGameState;
    }

    public override void SetGameState(NetworkData NetData)
    {
        PhisicsGameState.current = new PhisicsGameState(TrackedObjects);
    }

    public override void ApplyGameState(NetworkData NetData)
    {
        PhisicsGameState.current.applyState(TrackedObjects);
    }

    public override void RecoverGameState(string data)
    {
        PhisicsGameState.current = new PhisicsGameState(data);
    }

    public override void ProcessControlInput(string player, string input)
    {
        string[] vectors = input.Split("_");
        pj_controllers[player].RecieveInput(PhisicsGameState.current.StringToVector(vectors[0]), 
            PhisicsGameState.current.StringToVector(vectors[1]));
    }

    IEnumerator ListenToPJInput()
    {

        while (controling)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                ProcessStatus ps = new ProcessStatus();
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                string input = PhisicsGameState.current.VectorToString(ray.origin) + 
                    "_" + PhisicsGameState.current.VectorToString(ray.direction);

                string req = "CONTROL|" + LANClient.ConnectionID + LANClient.PublicName;

                Thread t = new Thread(new ThreadStart(
                    () => NetUtils.NetUpdater(NetworkManager.shared, req + "|" + input , ps)));
                t.Start();
                yield return new WaitUntil(() => ps.terminated);
            }
            else
            {
                yield return null;
            }
        
        }   
    }

    private void OnApplicationQuit()
    {
        controling = false;
    }
}


public class HRSPhisics1 : RequestSolver
{
    public string GenerateResponse(string request)
    {
        Debug.Log("Host recieved: " + request);
        string[] parts = request.Split("|");

        if (parts[0].Equals("CONTROL"))
        {
            GameDirector.currentGame.ProcessControlInput(parts[1], parts[2]);
        }

        return "OK";
    }
}

public class CRSPhisics1 : RequestSolver
{
    public string GenerateResponse(string request)
    {
        //Debug.Log("Client recieved: " + request);
        string[] parts = request.Split("=");
        if (parts[0].Equals("GSTATE"))
        {
            GameDirector.currentGame.RecoverGameState(parts[1]);
        }

        return "OK";
    }
}
