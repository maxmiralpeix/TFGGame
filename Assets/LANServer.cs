using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Net;
using System.Net.Sockets;

public class LANServer
{
    public bool accepting_connections = false;
    public List<ClientConnection> clients = new List<ClientConnection>();
    public List<Thread> listeners = new List<Thread>();
    public string ip, hostName;
    public bool online = true;
    int client_count;
    RequestSolver rs;

    public LANServer(RequestSolver rs)
    {
        this.rs = rs;
        StartServer();
    }

    public void StartServer()
    {
        Thread open_thread = new Thread(new ThreadStart(() => AcceptClients()));
        open_thread.Start();
    }

    private void OnApplicationQuit()
    {
        accepting_connections = false;
        CloseConnections();
    }

    void AcceptClients()
    {
        accepting_connections = true;

        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];//IPAddress.Parse("192.168.1.62");

        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, NetworkManager.port); // PORT?
        

        while (accepting_connections)
        {
            Socket listener = null;
            Socket sender_ = null;
            try
            {
                listener = new Socket(SocketType.Stream, ProtocolType.Tcp);              
                listener.Bind(ipEndPoint);
                listener.Listen(100);
                ip = LANUtils.ConvertFromIpAddressToString(ipEndPoint.Address);
                hostName = Dns.GetHostName();
                var reciever = listener.Accept();
                int port = NetworkManager.port + ((clients.Count + 1) *16);
                var data = LANUtils.ReciveData(reciever, new ConnectionRequestSolver(port));
                if (LANUtils.SocketConnected(reciever) && data.Contains("Connection Request"))
                {                    
                    sender_ = new Socket(SocketType.Stream, ProtocolType.Tcp);                   
                    sender_.Bind(new IPEndPoint(ipAddress, port));
                    sender_.Listen(100);
                    var sender = sender_.Accept();
                    client_count++;
                    data = LANUtils.ReciveData(sender, new ConnectionRequestSolver(port, client_count));
                    int id = client_count;
                    if (LANUtils.SocketConnected(sender) && data.Contains("Connection Completed"))
                    {
                        var client_con = new ClientConnection(sender, reciever, id.ToString());
                        clients.Add(client_con);

                        Thread lstnr = new Thread(new ThreadStart(() => ListenClient(reciever, client_con)));
                        listeners.Add(lstnr);
                        lstnr.Start();
                    }                   
                }                     
                listener.Close();
            }
            catch (SocketException error)
            {
                Debug.Log(error.ToString());
                online = false;
                if(listener != null) { listener.Close(); }
                if(sender_ != null) { sender_.Close(); }
            }
            catch
            {
                Debug.Log("Unknown error");
                online = false;
            }
        }
    }

    public Dictionary<string, string>[] ConnectionData()
    {
        Dictionary<string, string>[] dataList = new Dictionary<string, string>[clients.Count];

        for (int i = 0; i < clients.Count; i++)
        {
            var con = clients[i];
            Dictionary<string, string> data = new Dictionary<string, string>() {
                { "IP" , con.reciever.RemoteEndPoint.ToString()},
                { "Status" , (con.reciever.Connected)? "Connected": "Disconnected"}
            };
            dataList[i] = data;
        }

        return dataList;
    }

    public void CloseConnections()
    {
        foreach(ClientConnection connection in clients)
        {
            connection.reciever.Close();
            connection.sender.Close();
        }
        clients.Clear();
    }

    void ListenClient(Socket client, ClientConnection client_con)
    {
        Debug.Log("Client " + client.RemoteEndPoint.ToString() + " Connected");

        while (client != null)
        {
            LANUtils.ReciveData(client, rs);
            /*try
            {
                LANUtils.ReciveData(client, rs);
            }
            catch { break; }*/
        }
        Debug.Log("Client disconnected");
    }

    public string SendMessage(string message, ClientConnection client)
    {
        string r = "";
        if (client.sender != null)
        {
            r = LANUtils.SendData(message, client.sender);
        }
        return r;
    }
}

public class ClientConnection
{
    public string identifier;
    public Socket sender;
    public Socket reciever;

    public ClientConnection(Socket s, Socket r, string identifier = null)
    {
        sender = s;
        reciever = r;
        if (identifier != null) this.identifier = identifier;
        else
        {
            identifier = s.RemoteEndPoint.ToString();
            Debug.Log("Seted client identifier to: " + identifier);
        }
    }
}

public class ConnectionRequestSolver : RequestSolver
{
    int port, identifier;

    public ConnectionRequestSolver(int port, int identifier = 0)
    {
        this.port = port; this.identifier = identifier;
    }
    public string GenerateResponse(string request)
    {
        if(request.Contains("Connection Request"))
        {
            return port.ToString();
        }
        else if (request.Contains("Connection Completed"))
        {
            return identifier.ToString();
        }
        return "Unexpected request";
    }
}
