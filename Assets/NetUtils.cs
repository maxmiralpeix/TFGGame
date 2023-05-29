using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetUtils 
{
    public static void NetUpdater(NetworkManager net, string request, ProcessStatus status)
    {
        string response;
        response = net.SendRequest(request);
        status.result = response;
        status.successfull = true;
        status.terminated = true;
    }
}

public class ProcessStatus
{
    public bool terminated;
    public bool successfull;
    public string result;

    public void Reset()
    {
        terminated = false;
        successfull = false;
    }
}
