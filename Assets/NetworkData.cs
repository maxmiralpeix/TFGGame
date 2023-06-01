using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class NetworkData : MonoBehaviour
{
    public NetworkManager Net;
    public static NetworkData shared;
    public NetMode net_mode;
    public string server_reference;
    RequestSolver rs;
    public GameObject[] TrackedObjects;

    public static bool OnLine;
    public string[] connections;

    public void Awake()
    {
        shared = this;
    }

    void Start()
    {       
        Thread connect = null;
        GameState.current = new GameState(TrackedObjects);
        switch (net_mode)
        {           
            case NetMode.Host:
                rs = new HRS();
                connect = new Thread(new ThreadStart(() => Net.SetUpAsHost(rs)));
                break;
            default:
                RemoveRB();
                rs = new CRS();
                connect = new Thread(new ThreadStart(
                    () => Net.SetUpAsGest(server_reference, rs)));
                break;
        }
        connect.Start();
        StartCoroutine(Transmit());
    }

    // Update is called once per frame
    void Update()
    {
        if(net_mode == NetMode.Host &&
            Net.server != null &&
            Net.server.clients != null &&
            Net.server.clients.Count > 0)
        {
            List<string> concts = new List<string>();
            foreach(ClientConnection cl in Net.server.clients)
            {
                concts.Add(cl.identifier);
            }
            connections = concts.ToArray();
        }
    }

    public IEnumerator Transmit()
    {
        yield return new WaitUntil(() => Net.isConnected());
        if(net_mode == NetMode.Host)
        {
            Debug.Log("Public Adress: " + Net.GetEncodedServerIP());
            Debug.Log("Translated to: " + NetworkManager.DecodeIP(Net.GetEncodedServerIP()));
        }
        yield return new WaitForSeconds(2f);
        Debug.Log(net_mode.ToString() + " connected.");

        OnLine = true;
        while (OnLine)
        {
            ProcessStatus ps = new ProcessStatus();
            if (net_mode == NetMode.Host)
            {
                GameState.current = new GameState(TrackedObjects);
                Thread t = new Thread(new ThreadStart(
                    () => NetUtils.NetUpdater(Net, "GSTATE=" + GameState.current.toData(), ps)));
                t.Start();
            }
            else
            {
                GameState.current.applyState(TrackedObjects);
                ps.terminated = true;
            }
            yield return new WaitUntil(() => ps.terminated);
            //yield return new WaitForSeconds(0.05f);
            
        }
        
    }

    private void OnApplicationQuit()
    {
        OnLine = false;
    }

    private void RemoveRB()
    {
        foreach(GameObject obj in TrackedObjects)
        {
            Rigidbody rb = null;
            if (obj.TryGetComponent<Rigidbody>(out rb))
            {
                Destroy(rb);
            }
        }
    }
}

public enum NetMode
{
    Host, Client
}

public class HRS : RequestSolver
{
    public string GenerateResponse(string request)
    {
        Debug.Log("Host recieved: " + request);
        return "OK";
    }
}

public class CRS : RequestSolver
{
    public string GenerateResponse(string request)
    {
        Debug.Log("Client recieved: " + request);
        string[] parts = request.Split("=");
        if (parts[0].Equals("GSTATE"))
        {
            GameState.current = new GameState(parts[1]);
        }
        
        return "OK";
    }
}
