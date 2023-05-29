using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

public class LANUtils : MonoBehaviour
{
    public static string ReciveData(Socket through, RequestSolver rs = null)
    {
        string request = "";
        while (true)
        {
            // Receive message.
            var buffer = new byte[1_024];
            var received = through.Receive(buffer, SocketFlags.None);
            request = Encoding.UTF8.GetString(buffer, 0, received);

            var eom = "<|EOM|>";
            if (request.IndexOf(eom) > -1 /* is end of message */)
            {
                /*Debug.Log(
                    $"Socket server received message: \"{request.Replace(eom, "")}\"");*/

                var ackMessage = (rs != null)? rs.GenerateResponse(request.Replace(eom, "")) + "<|ACK|>" : "<|ACK|>";
                var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                through.SendAsync(echoBytes, 0);
                /*Debug.Log(
                    $"Socket server sent acknowledgment: \"{ackMessage}\"");
                */
                break;
            }
        }
        return request;
    }

    public static string SendData(string data, Socket client)
    {
        string r = "";
        while (true)
        {
            // Send message.
            var message = data + "<|EOM|>";
            var messageBytes = Encoding.UTF8.GetBytes(message);
            _ = client.SendAsync(messageBytes, SocketFlags.None);
           //Debug.Log($"Socket client sent message: \"{message}\"");

            // Receive ack.
            var buffer = new byte[1_024];
            int received = client.Receive(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);
            if (response.Contains("<|ACK|>"))
            {
                r = response.Replace("<|ACK|>", "");
               /* Debug.Log(
                    $"Socket client received acknowledgment: \"{r}\"");      */         
                break;
            }

        }
        return r;
    }

    public static bool SocketConnected(Socket s)
    {
        bool part1 = s.Poll(1000, SelectMode.SelectRead);
        bool part2 = (s.Available == 0);
        if (part1 && part2)
            return false;
        else
            return true;
    }

    public static string ConvertFromIpAddressToString(IPAddress ipAddress)
    {
        string encoded = ipAddress.ToString();

        return encoded;
    }

    public static IPAddress ConvertFromStringToIpAddress(string ipAddress)
    {
        string ipAdrr = ipAddress;
        Debug.Log("Reconverted string: " + ipAdrr);
        try
        {
            return IPAddress.Parse(ipAdrr);
        }
        catch
        {
            return null;
        }
    }

    public static string InfoDataToString(Dictionary<string, string> data)
    {
        string info = "";
        foreach(string key in data.Keys)
        {
            info += key + ":" + data[key] + ", ";
        }
        return info;
    }

    public static string EncodeIP(string ip)
    {
        return ip;
    }
    public static string DecodeIP(string ip)
    {
        return ip;
    }

}
