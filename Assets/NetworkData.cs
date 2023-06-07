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

    public static bool OnLine;
    public string[] connections;

    public void Awake()
    {
        shared = this;
    }

    void Start()
    {               
        switch (net_mode)
        {           
            case NetMode.Host:
                GameDirector.currentGame.PrepareHost(this);                
                break;
            default:
                GameDirector.currentGame.PrepareClient(this);              
                break;
        }
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
            if(Net.server.clients.Count > connections.Length)
            {
                string id = Net.server.clients[Net.server.clients.Count - 1].identifier;
                GameDirector.currentGame.AddClient(id, NetMode.Host);
            }
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
                GameDirector.currentGame.SetGameState(this);
                Thread t = new Thread(new ThreadStart(
                    () => NetUtils.NetUpdater(Net, "GSTATE=" + GameDirector.currentGame.GameState(this).toData(), ps)));
                t.Start();
            }
            else
            {
                GameDirector.currentGame.ApplyGameState(this);
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
}

public enum NetMode
{
    Host, Client
}