﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

public class TCPConnector
{

    private Socket clientSocket;
    private const int PORT = 5038;

    public bool connect(string ipAddress)
    {
        try
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), PORT);
            clientSocket.Connect(serverEndPoint);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public void disconnect()
    {
        clientSocket.Disconnect(false);
    }

    private bool sendRequest(string message)
    {
        clientSocket.Send(Encoding.ASCII.GetBytes(message));       
        int bytesRead = 0;

        do
        {
            byte[] buffer = new byte[clientSocket.SendBufferSize];
            bytesRead = clientSocket.Receive(buffer);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            if (Regex.Match(response, "Response: Success", RegexOptions.IgnoreCase).Success)
            {
                return true;
            }
            else if (Regex.Match(response, "Response: Error", RegexOptions.IgnoreCase).Success)
            {
                return false;
            }

        } while (bytesRead != 0);
        return false;
    }

    public bool login(string userName, string password)
    {
        string str_login = String.Format("Action: Login\r\nUsername: {0}\r\nSecret: {1}\r\nActionID: 1\r\n\r\n", userName, password);
        return sendRequest(str_login);
    }

    public void logout()
    {
        clientSocket.Send(Encoding.ASCII.GetBytes("Action: Logoff"));
    }

    public bool addTrunk(string trunkName, string hostIP)
    {
        string str_addTrunk = String.Format("Action: UpdateConfig\r\nReload: no\r\nsrcfilename: sip.conf\r\ndstfilename: sip.conf\r\n" +
            "Action-000000: newcat\r\nCat-000000: {0}\r\nAction-000001: append\r\nCat-000001: {0}\r\nVar-000001: type\r\nValue-000001: peer\r\n" +
            "Action-000002: append\r\nCat-000002: {0}\r\nVar-000002: host\r\nValue-000002: {1}\r\n" +
            "Action-000003: append\r\nCat-000003: {0}\r\nVar-000003: context\r\nValue-000003: remote\r\n\r\n", trunkName, hostIP);
        return sendRequest(str_addTrunk);
    }

    public void reloadModules()
    {
        string str_reloadSip = String.Format("Action: Reload\r\nModule: chan_sip\r\n\r\n");
        string str_reloadDialplan = String.Format("Action: Reload\r\nModule: pbx_config\r\n\r\n");
        sendRequest(str_reloadSip);
        sendRequest(str_reloadDialplan);
    }

    public bool deleteTrunk(string trunkName)
    {
        string str_delTrunk = String.Format("Action: UpdateConfig\r\nReload: no\r\nsrcfilename: sip.conf\r\ndstfilename: sip.conf\r\n" +
            "Action-000000: delcat\r\nCat-000000: {0}\r\n\r\n", trunkName);
        return sendRequest(str_delTrunk);
    }

    public bool updateTrunk(string oldTrunkName, string newTrunkName, string hostIP, string contextName)
    {
        string str_updateTrunk = String.Format("Action: UpdateConfig\r\nReload: yes\r\nsrcfilename: sip.conf\r\ndstfilename: sip.conf\r\n" +
            "Action-000000: renamecat\r\nCat-000000: {0}\r\nValue-000000: {1}\r\nAction-000001: update\r\nCat-000001: {1}\r\nVar-000001: host\r\nValue-000001: {2}\r\n" +
            "Action-000002: update\r\nCat-000002: {1}\r\nVar-000002: context\r\nValue-000002: {3}\r\n\r\n", oldTrunkName, newTrunkName, hostIP, contextName);
        return sendRequest(str_updateTrunk);
    }
}