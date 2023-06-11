using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class LANClient
{
    public string ip, hostName;
    public bool connecting, connected;
    public Socket socket_sender, socket_reciver;
    RequestSolver rs;
    public string client_identifier;

    public static string ConnectionID;
    public static string PublicName = "DefaultName";

    public LANClient(string reference, RequestSolver rs, ServerReference referenceType = ServerReference.IP)
    {
        if(referenceType == ServerReference.IP)
        {
            StartClient("", reference);
        }
        else
        {
            StartClient(reference, "");
        }

        this.rs = rs;
    }

    public void StartClient(string name, string ip)
    {
        Thread connect_thread = new Thread(new ThreadStart(() => ConnectToServer(name, ip)));
        connecting = true;
        connect_thread.Start();
    }

    public string SendMessage(string message)
    {
        string r = "";
        if(socket_sender != null)
        {
            r = LANUtils.SendData(message + ":" + client_identifier, socket_sender);
        }
        return r;
    }


    void ConnectToServer(string hostName, string ipAdrr)
    {
        Debug.Log(hostName.Equals("LandFish") + " " + hostName.Length.ToString() + " " + hostName);
        try
        {

            IPAddress ip =(ipAdrr.Length > 1)? 
                LANUtils.ConvertFromStringToIpAddress(ipAdrr) :
                Dns.GetHostAddresses(hostName)[0];
            IPHostEntry HostDnsEntry = Dns.GetHostEntry(ip);
            IPEndPoint endpoint = new IPEndPoint(HostDnsEntry.AddressList[0], NetworkManager.port);
            Socket client = new Socket(SocketType.Stream, ProtocolType.Tcp);
            client.Connect(endpoint);
            string partial_connection_info = LANUtils.SendData("Connection Request", client);
            this.ip = client.LocalEndPoint.ToString();
            socket_sender = client;

            int port = int.Parse(partial_connection_info);

            Socket client_recv = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint rcv_endpoint = new IPEndPoint(HostDnsEntry.AddressList[0], port);
            client_recv.Connect(rcv_endpoint);
            ConnectionID = (client_recv.LocalEndPoint as IPEndPoint).Address.ToString();
            client_identifier = LANUtils.SendData("Connection Completed;" + PublicName + ";" + ConnectionID, client_recv); 
            socket_reciver = client_recv;

            Thread lstnr = new Thread(new ThreadStart(() => ListenServer(client_recv)));
            lstnr.Start();

            connected = true;
            connecting = false;
      
        }
        catch (SocketException error)
        {
            Debug.Log(error);
            connecting = false;
        }
        catch
        {
            Debug.Log("Invalid Server code");
            connecting = false;
        }
    }

    void ListenServer(Socket server)
    {
        while (server != null)
        {
            try
            {
                LANUtils.ReciveData(server, rs);
            }
            catch (SocketException e) 
            {
                Debug.Log(e); 
            }
        }
        Debug.Log("Disconnected");
    }
}

public enum ServerReference
{
    HostName, IP
}
