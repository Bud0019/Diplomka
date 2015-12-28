using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AsterNET.Manager;
using AsterNET.Manager.Action;
using AsterNET.Manager.Response;
using AsterNET.FastAGI;
using AsterNET.Manager.Event;
using AsterNET.FastAGI.MappingStrategies;
using System.Security.Cryptography;

public class TCPConnector
{

    private Socket clientSocket;
    private const int PORT = 5038;

    private ManagerConnection managerConnection;
    private ManagerResponse managerResponse;


    public void reloadModules()
    {
        UpdateConfigAction reloadSipConf = new UpdateConfigAction("sip.conf", "sip.conf", true);
        UpdateConfigAction reloadExtensionsConf = new UpdateConfigAction("extensions.conf", "extensions.conf", true);
        managerResponse = managerConnection.SendAction(reloadSipConf);
        managerResponse = managerConnection.SendAction(reloadExtensionsConf);
    }

    public bool addTrunk(string trunkName, string hostIP)
    {
        UpdateConfigAction addTrunkUpdateConfig = new UpdateConfigAction("sip.conf", "sip.conf", false);
        addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, trunkName);
        addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, trunkName, "host", hostIP);
        addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, trunkName, "type", "peer");
        addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, trunkName, "secret", "AsteriskRoutingSystem");
        addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, trunkName, "context", "remote");
        managerResponse = managerConnection.SendAction(addTrunkUpdateConfig);
        if (managerResponse.IsSuccess())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool deleteTrunk(string trunkName)
    {
        UpdateConfigAction deleteTrunkUpdateConfig = new UpdateConfigAction("sip.conf", "sip.conf", false);
        deleteTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, trunkName);
        managerResponse = managerConnection.SendAction(deleteTrunkUpdateConfig);
        if (managerResponse.IsSuccess())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool updateTrunk(string oldTrunkName, string newTrunkName, string hostIP)
    {
        UpdateConfigAction updateTrunkUpdateConfig = new UpdateConfigAction("sip.conf", "sip.conf", false);
        updateTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_RENAMECAT, oldTrunkName,null,newTrunkName);
        updateTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, newTrunkName, "host", hostIP);
        managerResponse = managerConnection.SendAction(updateTrunkUpdateConfig);
        if (managerResponse.IsSuccess())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool addPrefix(string contextName, string prefix)
    {
        string createdPrefix = createPrefix(prefix);
        UpdateConfigAction addPrefixUpdateConfig = new UpdateConfigAction("extensions.conf", "extensions.conf", false);
        addPrefixUpdateConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, contextName);
        addPrefixUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, contextName, "exten", "_" + createdPrefix + ",1,NoOp()");
        addPrefixUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, contextName, "exten", "_" + createdPrefix + ",n,Dial(SIP/${{EXTEN}}@" + contextName + ")");
        addPrefixUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, contextName, "exten", "_" + createdPrefix + ",n,HangUp()");
        managerResponse = managerConnection.SendAction(addPrefixUpdateConfig);
        if (managerResponse.IsSuccess())
        {
            return true;
        }
        else
        {
            return false;
        }
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
            string pureContextName = context.Substring(8, (context.IndexOf("\r") - 8));
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

    public bool createInitialContexts(List<string> asteriskNameList)
    {

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
    public void deleteAllRemoteContexts(List<string> asteriskNamesList)
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
                    "Action-000000: delcat\r\nCat-000000: {0}\r\nAction-000001: delete\r\nCat-000001: remote\r\nVar-000001: include\r\nValue-000001: {0}\r\n" +
                    "Match-000001: {0}\r\n\r\n", deletedContext);
        sendRequest(str_deleteRemovedContext);
    }

    public void updateDialPlans(string oldContextName, string newContextName, string newPrefix)
    {
        string str_updateRemoteContext = String.Format("Action: UpdateConfig\r\nReload: no\r\nsrcfilename: extensions.conf\r\ndstfilename: extensions.conf\r\n" +
                    "Action-000000: update\r\nCat-000000: remote\r\nVar-000000: include\r\nValue-000000: {1}\r\n" +
                    "Match-000000: {0}\r\nAction-000001: delCat\r\nCat-000001: {0}\r\nAction-000002: newcat\r\nCat-000002: {1}\r\nAction-000003: append\r\n" +
                    "Cat-000003: {1}\r\nVar-000003: exten\r\nValue-000003:>_{2},1,NoOp()\r\nAction-000004: append\r\nCat-000004: {1}\r\nVar-000004: exten\r\n" +
                    "Value-000004:>_{2},n,Dial(SIP/${{EXTEN}}@{1})\r\nAction-000005: append\r\nCat-000005: {1}\r\nVar-000005: exten\r\n" +
                    "Value-000005:>_{2},n,HangUp()\r\n\r\n", oldContextName, newContextName, createPrefix(newPrefix));
        sendRequest(str_updateRemoteContext);
    }

    public void ping()
    {
        string str_ping = "Action: ping\r\n\r\n";
        sendRequest(str_ping);
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
        string str_getDialPlanContexts = "Action: GetConfig\r\nSynopsis: Retrieve configuration\r\nPrivilege: config,all\r\nDescription: test\r\n" +
            "Variables: \r\nFilename: extensions.conf\r\n\r\n";
        //test();
        clientSocket.Send(Encoding.ASCII.GetBytes(str_getDialPlanContexts), Encoding.ASCII.GetByteCount(str_getDialPlanContexts), SocketFlags.None);
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
                foreach (string context in tmpDialPlanContextsList)
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

    public bool login(string ipAddress, string amiLogin, string amiPassword)
    {
        managerConnection = new ManagerConnection(ipAddress, PORT, amiLogin, amiPassword);
        managerConnection.Login(30000);
        if (managerConnection.IsConnected())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void logoff()
    {
        managerConnection.Logoff();
    }



    public void test() {
        ManagerConnection mc = new ManagerConnection("158.196.244.214", 5038, "asterisk214", "asterisk214");
        mc.Login();
        if (mc.IsConnected()) {    
        ManagerResponse mr = mc.SendAction(new GetConfigAction("extensions.conf"));
            if (mr.IsSuccess())
            {
                GetConfigResponse responseConfig = (GetConfigResponse)mr;
                foreach (int key in responseConfig.Categories.Keys)
                {
                    Console.WriteLine(string.Format("{0}:{1}", key, responseConfig.Categories[key]));
                    foreach (int keyLine in responseConfig.Lines(key).Keys)
                    {
                        Console.WriteLine(string.Format("\t{0}:{1}", keyLine, responseConfig.Lines(key)[keyLine]));
                    }
                }
            }
            else
                Console.WriteLine(mr);
        }
        UpdateConfigAction up = new UpdateConfigAction("sip.conf", "sip.conf", false);
        up.AddCommand(UpdateConfigAction.ACTION_NEWCAT, "asternet");
        up.AddCommand(UpdateConfigAction.ACTION_APPEND, "asternet", "host", "158.196.244.214");
        up.AddCommand(UpdateConfigAction.ACTION_APPEND, "asternet", "type", "peer");
        up.AddCommand(UpdateConfigAction.ACTION_APPEND, "asternet", "context", "remote");
        ManagerResponse mr1 = mc.SendAction(up);
        if (mr1.IsSuccess())
        {
            Console.WriteLine(mr1);
        }
        mc.Logoff();
    }
}


