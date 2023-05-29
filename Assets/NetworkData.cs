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
        
    }

    public IEnumerator Transmit()
    {
        yield return new WaitUntil(() => Net.isConnected());
        yield return new WaitForSeconds(2f);
        Debug.Log(net_mode.ToString() + " connected.");

        OnLine = true;
        while (OnLine)
        {
            ProcessStatus ps = new ProcessStatus();
            if (net_mode == NetMode.Host)
            {
                Thread t = new Thread(new ThreadStart(
                    () => NetUtils.NetUpdater(Net, "GSTATE=" + GameState.current.toData(), ps)));
                t.Start();
            }
            else
            {
                
            }
            yield return new WaitUntil(() => ps.terminated);
            yield return new WaitForSeconds(0.02f);
            GameState.current.applyState(NetworkData.shared.TrackedObjects);
        }
        
    }

    private void OnApplicationQuit()
    {
        OnLine = false;
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