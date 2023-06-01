using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager shared;
    public static int port = 8000;
    public LANClient client;
    public LANServer server;
    public NetworkMode mode = NetworkMode.disconected;
    RequestSolver rs;
    public int test;

    private void Awake()
    {
        shared = this;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (mode)
        {
            case NetworkMode.gest:
                if(!client.connected && !client.connecting) { mode = NetworkMode.disconected; }
                break;
            case NetworkMode.host:
                if (!server.online) { mode = NetworkMode.disconected; }
                break;
            default:
                break;
        }

    }

    public void SetUpAsHost(RequestSolver rs)
    {
        this.rs = rs;
        mode = NetworkMode.host;
        server = new LANServer(rs);           
    }

    public void SetUpAsGest(string reference, RequestSolver rs, ServerReference referenceType = ServerReference.IP)
    {
        mode = NetworkMode.gest;
        client = new LANClient(DecodeIP(reference), rs, referenceType);
    }

    public bool isConnected()
    {
        switch (mode)
        {
            case NetworkMode.gest:
                if(client != null)
                {
                    return (client != null && client.connected);
                }
                else { return false; }
            case NetworkMode.host:
                if (server != null && server.ip != null)
                {
                    return server.online && server.ip.Length > 3;
                }
                else { return false; }  
            default:
                return false;
        }
    }

    public string SendRequest(string message)
    {
        if(mode == NetworkMode.gest && isConnected())
        {
            return client.SendMessage(message);
        }
        if (mode == NetworkMode.host && isConnected())
        {
            List<string> responses = new List<string>();
            foreach(ClientConnection con in server.clients)
            {
                responses.Add(server.SendMessage(message, con));
            }
            return responses.ToString();
        }
        return "";
    }

    public string GetConnectionInfo()
    {
        string repr = "";
        switch (mode)
        {
            case NetworkMode.gest:
                break;
            case NetworkMode.host:
                var dataList = server.ConnectionData();
                foreach(var data in dataList)
                {
                    repr += LANUtils.InfoDataToString(data) + "\n";
                }
                break;
            default:
                break;
        }

        return repr;
    }

    public Dictionary<string, string> GetNetworkStatus()
    {
        if(mode == NetworkMode.disconected)
        {
            var data_ = new Dictionary<string, string>()
            {{"Mode", mode.ToString() }};
            return data_;
        }
        var data = new Dictionary<string, string>()
        {
            {"Mode", mode.ToString() },
            {"IP", (mode == NetworkMode.gest)? client.ip : server.ip},
            {"Name", (mode == NetworkMode.gest)? client.hostName : server.hostName}
        };
        return data;
    }

    public string GetEncodedServerIP()
    {
        Debug.Log("Server ip: " + server.ip);
        return LANUtils.EncodeIP(server.ip);
    }
    public static string DecodeIP(string ip)
    {
        return LANUtils.DecodeIP(ip);
    }


    private void OnApplicationQuit()
    {
        switch (mode)
        {
            case NetworkMode.gest:
                break;
            case NetworkMode.host:
                server.CloseConnections();
                break;
            default:
                break;
        }
    }
}

public enum NetworkMode{
    host, gest, disconected
}

public interface RequestSolver
{
    public string GenerateResponse(string request);
}

public interface NetworkConnection
{
    public void SetUpAsHost(RequestSolver rs);
    public void SetUpAsGest(string reference, RequestSolver rs, ServerReference referenceType = ServerReference.IP);
    public bool isConnected();
    public string SendRequest(string message);

}