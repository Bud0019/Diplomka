using System;
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
            byte[] buffer = new byte[clientSocket.ReceiveBufferSize];
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

    public bool updateTrunk(string oldTrunkName, string newTrunkName, string hostIP)
    {
        string str_updateTrunk = String.Format("Action: UpdateConfig\r\nReload: no\r\nsrcfilename: sip.conf\r\ndstfilename: sip.conf\r\n" +
            "Action-000000: renamecat\r\nCat-000000: {0}\r\nValue-000000: {1}\r\nAction-000001: update\r\nCat-000001: {1}\r\nVar-000001: host\r\nValue-000001: {2}\r\n\r\n",
            oldTrunkName, newTrunkName, hostIP);
        return sendRequest(str_updateTrunk);
    }

    public bool addPrefix(string contextName, string prefix)
    {        
        string str_addToDialPlan = String.Format("Action: UpdateConfig\r\nReload: no\r\nsrcfilename: extensions.conf\r\ndstfilename: extensions.conf\r\n" +
                    "Action-000000: newcat\r\nCat-000000: {0}\r\nAction-000001: append\r\nCat-000001: {0}\r\nVar-000001: exten\r\n"+
                    "Value-000001:>_{1},1,NoOp()\r\nAction-000002: append\r\nCat-000002: {0}\r\nVar-000002: exten\r\n" +
                    "Value-000002:>_{1},n,Dial(SIP/${{EXTEN}}@{0})\r\nAction-000003: append\r\nCat-000003: {0}\r\nVar-000003: exten\r\n" +
                    "Value-000003:>_{1},n,HangUp()\r\n\r\n", contextName, createPrefix(prefix));
        return sendRequest(str_addToDialPlan);
    }
    //preprobit na Bool
    public void createInitialContexts(List<string> asteriskNamesList)
    {
        List<string> dialPlanContextsList = getDialPlanContexts();
        string str_addRemoteContext = "Action: UpdateConfig\r\nReload: no\r\nsrcfilename: extensions.conf\r\ndstfilename: extensions.conf\r\n" +
                    "Action-000000: newcat\r\nCat-000000: remote\r\n\r\n";
        sendRequest(str_addRemoteContext);

        foreach (string context in dialPlanContextsList)
        {
            string pureContextName = context.Substring(8, (context.IndexOf("\r")-8));
            string str_addToRemoteContext = String.Format("Action: UpdateConfig\r\nReload: no\r\nsrcfilename: extensions.conf\r\ndstfilename: extensions.conf\r\n" +
                "Action-000000: append\r\nCat-000000: remote\r\nVar-000000: include\r\nValue-000000:>{0}\r\n\r\n", pureContextName);
            sendRequest(str_addToRemoteContext);
            if (!asteriskNamesList.Contains(pureContextName))
            {
                string str_addToInternalContexts = String.Format("Action: UpdateConfig\r\nReload: no\r\nsrcfilename: extensions.conf\r\ndstfilename: extensions.conf\r\n" +
                "Action-000000: append\r\nCat-000000: {0}\r\nVar-000000: include\r\nValue-000000:>remote\r\n\r\n", pureContextName);
                sendRequest(str_addToInternalContexts);
            }
        }
    }
    //preprobit na Bool
    public void addToRemoteDialPlans(List<string> asteriskNamesList)
    {
        List<string> dialPlanContextsList = getDialPlanContexts();
        string remoteIncludes = null;
        foreach (string context in dialPlanContextsList)
        {
            if (Regex.Match(context, ": remote", RegexOptions.IgnoreCase).Success)
            {
                remoteIncludes = context;
                break;
            }
        }
        foreach (string context in dialPlanContextsList)
        {
            string pureContextName = context.Substring(8, (context.IndexOf("\r") - 8));
            if (!pureContextName.Equals("remote"))
            {
                if (!Regex.Match(remoteIncludes, "include=" + pureContextName, RegexOptions.IgnoreCase).Success)
                {
                    string str_addToRemoteContext = String.Format("Action: UpdateConfig\r\nReload: no\r\nsrcfilename: extensions.conf\r\ndstfilename: extensions.conf\r\n" +
                   "Action-000000: append\r\nCat-000000: remote\r\nVar-000000: include\r\nValue-000000:>{0}\r\n\r\n", pureContextName);
                    sendRequest(str_addToRemoteContext);
                }
                if (!asteriskNamesList.Contains(pureContextName))
                {
                    if (!Regex.Match(context, "include=remote", RegexOptions.IgnoreCase).Success)
                    {
                        string str_addToInternalContexts = String.Format("Action: UpdateConfig\r\nReload: no\r\nsrcfilename: extensions.conf\r\ndstfilename: extensions.conf\r\n" +
                            "Action-000000: append\r\nCat-000000: {0}\r\nVar-000000: include\r\nValue-000000:>remote\r\n\r\n", pureContextName);
                        sendRequest(str_addToInternalContexts);
                    }
                }
            }
        }
    }
    //preprobit na Bool
    public void deleteAllRemoteContexts(List<string>asteriskNamesList)
    {        
        string str_deleteRemoteContext = "Action: UpdateConfig\r\nReload: no\r\nsrcfilename: extensions.conf\r\ndstfilename: extensions.conf\r\n" +
                    "Action-000000: delcat\r\nCat-000000: remote\r\n\r\n";
        sendRequest(str_deleteRemoteContext);
        List<string> dialPlanContextsList = getDialPlanContexts();
        
        foreach (string context in dialPlanContextsList)
        {
            string pureContextName = context.Substring(8, (context.IndexOf("\r") - 8));
            if (asteriskNamesList.Contains(pureContextName))
            {
                string str_deleteAsterisksContexts = String.Format("Action: UpdateConfig\r\nReload: no\r\nsrcfilename: extensions.conf\r\ndstfilename: extensions.conf\r\n" +
                    "Action-000000: delcat\r\nCat-000000: {0}\r\n\r\n", pureContextName);
                sendRequest(str_deleteAsterisksContexts);
            }
            else
            {                
                string str_deleteIncludeRemote = String.Format("Action: UpdateConfig\r\nReload: no\r\nsrcfilename: extensions.conf\r\ndstfilename: extensions.conf\r\n" +
                    "Action-000000: delete\r\nCat-000000: {0}\r\nVar-000000: include\r\nValue-000000: remote\r\nMatch-000000: remote\r\n\r\n", pureContextName);
                sendRequest(str_deleteIncludeRemote);
            }
        }
    }
    //preprobit na Bool
    public void deleteOneContext(string deletedContext)
    {
        string str_deleteRemovedContext = String.Format("Action: UpdateConfig\r\nReload: no\r\nsrcfilename: extensions.conf\r\ndstfilename: extensions.conf\r\n" +
                    "Action-000000: delcat\r\nCat-000000: {0}\r\nAction-000001: delete\r\nCat-000001: remote\r\nVar-000001: include\r\nValue-000001: {0}\r\n"+
                    "Match-000001: {0}\r\n\r\n", deletedContext);
        sendRequest(str_deleteRemovedContext);
    }

    public void updateDialPlans(string oldContextName, string newContextName, string newPrefix)
    {
        string str_updateRemoteContext = String.Format("Action: UpdateConfig\r\nReload: no\r\nsrcfilename: extensions.conf\r\ndstfilename: extensions.conf\r\n" +
                    "Action-000000: update\r\nCat-000000: remote\r\nVar-000000: include\r\nValue-000000: {1}\r\n" +
                    "Match-000000: {0}\r\nAction-000001: delCat\r\nCat-000001: {0}\r\nAction-000002: newcat\r\nCat-000002: {1}\r\nAction-000003: append\r\n"+
                    "Cat-000003: {1}\r\nVar-000003: exten\r\nValue-000003:>_{2},1,NoOp()\r\nAction-000004: append\r\nCat-000004: {1}\r\nVar-000004: exten\r\n" +
                    "Value-000004:>_{2},n,Dial(SIP/${{EXTEN}}@{1})\r\nAction-000005: append\r\nCat-000005: {1}\r\nVar-000005: exten\r\n" +
                    "Value-000005:>_{2},n,HangUp()\r\n\r\n", oldContextName, newContextName, createPrefix(newPrefix));
        sendRequest(str_updateRemoteContext);
    }
    
    private string createPrefix(string prefix)
    {
        string tmpStr = "XXXXXXXXX";
        return prefix + tmpStr.Substring(prefix.Length);                       
    }
    //skusit prerobit na lepsie riesenie, aby to nekrachlo pri prekroceni 65kb prijatych dat!!
    private List<string> getDialPlanContexts()
    {
        List<string> tmpDialPlanContextsList = new List<string>();
        List<string> finalDialPlanContextsList = new List<string>();
        string str_getDialPlanContexts = "Action: GetConfig\r\nSynopsis: Retrieve configuration\r\nPrivilege: config,all\r\nDescription: test\r\n"+
            "Variables: \r\nFilename: extensions.conf\r\n\r\n";
        clientSocket.Send(Encoding.ASCII.GetBytes(str_getDialPlanContexts));
        int bytesRead = 0;
        string[] subStrings;
        Thread.Sleep(200);
        do
        {
            byte[] buffer = new byte[clientSocket.ReceiveBufferSize];
            bytesRead = clientSocket.Receive(buffer);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            response = response.Substring(response.IndexOf("Response"));
            if (Regex.Match(response, "Response: Success", RegexOptions.IgnoreCase).Success)
            {                              
                subStrings = response.Split(new string[] { "Category-" }, StringSplitOptions.None);
                tmpDialPlanContextsList = subStrings.OfType<string>().ToList();
                tmpDialPlanContextsList.RemoveAt(0);              
                foreach(string context in tmpDialPlanContextsList)
                {
                   finalDialPlanContextsList.Add(context);                  
                }
                return finalDialPlanContextsList;
                
            }
            else if (Regex.Match(response, "Response: Error", RegexOptions.IgnoreCase).Success)
            {
                return finalDialPlanContextsList;
            }
        } while (bytesRead != 0);
        return finalDialPlanContextsList;
    }   
}


