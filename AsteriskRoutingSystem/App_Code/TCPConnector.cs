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

    private const int PORT = 5038;

    private ManagerConnection managerConnection;
    private ManagerResponse managerResponse;


    public bool login(string ipAddress, string amiLogin, string amiPassword)
    {
        managerConnection = new ManagerConnection(ipAddress, PORT, amiLogin, amiPassword);
        managerConnection.Login(15000);
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
  
    public bool createInitialContexts(List<string> asteriskNameList)
    {        
        managerResponse = managerConnection.SendAction(new GetConfigAction("extensions.conf"));
        UpdateConfigAction addToExtensionsUpdateConfig = new UpdateConfigAction("extensions.conf", "extensions.conf", false);
        if (managerResponse.IsSuccess())
        {
            GetConfigResponse responseConfig = (GetConfigResponse)managerResponse;
            UpdateConfigAction createRemoteUpdateConfig = new UpdateConfigAction("extensions.conf", "extensions.conf", false);
            createRemoteUpdateConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, "remote");
            managerResponse = managerConnection.SendAction(createRemoteUpdateConfig);
            if (!managerResponse.IsSuccess())
            {
                return false;
            }
            foreach (int key in responseConfig.Categories.Keys)
            {
                string extensionsCategory = responseConfig.Categories[key];                
                addToExtensionsUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "remote", "include", extensionsCategory);
                
                if (!asteriskNameList.Contains(extensionsCategory))
                {                   
                    addToExtensionsUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, extensionsCategory, "include", "remote");                   
                }
            }
            managerResponse = managerConnection.SendAction(addToExtensionsUpdateConfig);
            if (!managerResponse.IsSuccess())
            {
                return false;
            }            
            return true;            
        }
        else
            return false;
    }

    public bool addToRemoteDialPlans(List<string> asteriskNamesList)
    {
        managerResponse = managerConnection.SendAction(new GetConfigAction("extensions.conf"));
        UpdateConfigAction addToRemoteDialPlansUpdateConfig = new UpdateConfigAction("extensions.conf", "extensions.conf", false);
        if (managerResponse.IsSuccess())
        {
            GetConfigResponse responseConfig = (GetConfigResponse)managerResponse;
            int remoteIndex = responseConfig.Categories.Values.ToList().IndexOf("remote");
            foreach (int key in responseConfig.Categories.Keys)
            {
                string extensionsCategory = responseConfig.Categories[key];
                if (!extensionsCategory.Equals("remote"))
                {
                    if (!responseConfig.Lines(remoteIndex).ContainsValue("include=" + extensionsCategory))
                    {
                        addToRemoteDialPlansUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "remote", "include", extensionsCategory);
                    }
                    if (!asteriskNamesList.Contains(extensionsCategory))
                    {
                        if(!responseConfig.Lines(key).ContainsValue("include=remote"))
                        {
                            addToRemoteDialPlansUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, extensionsCategory, "include", "remote");
                        }
                    }
                }              
            }
            managerResponse = managerConnection.SendAction(addToRemoteDialPlansUpdateConfig);
            if (!managerResponse.IsSuccess())
            {
                return false;
            }
            return true;
        }
        else
            return false;
    }

    public bool deleteAllRemoteContexts(List<string> asteriskNamesList)
    {
        UpdateConfigAction deleteRemoteContextUpdateConfig = new UpdateConfigAction("extensions.conf", "extensions.conf", false);
        deleteRemoteContextUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, "remote");
        managerResponse = managerConnection.SendAction(deleteRemoteContextUpdateConfig);
        if (!managerResponse.IsSuccess())
        {
            return false;
        }

        managerResponse = managerConnection.SendAction(new GetConfigAction("extensions.conf"));
        UpdateConfigAction deleteAsteriskUpdateConfig = new UpdateConfigAction("extensions.conf", "extensions.conf", false);
        if (managerResponse.IsSuccess())
        {
            GetConfigResponse responseConfig = (GetConfigResponse)managerResponse;
            foreach (int key in responseConfig.Categories.Keys)
            {
                string extensionsCategory = responseConfig.Categories[key];
                if (asteriskNamesList.Contains(extensionsCategory))
                {
                    deleteAsteriskUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, extensionsCategory);
                }
                else
                {
                    deleteAsteriskUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, extensionsCategory, "include", "remote", "remote");
                }
            }
            managerResponse = managerConnection.SendAction(deleteAsteriskUpdateConfig);
            if (!managerResponse.IsSuccess())
            {
                return false;
            }
            return true;
        }
        else
            return false;
    }

    public bool deleteOneContext(string deletedContext)
    {
        UpdateConfigAction deleteOneContextUpdateConfig = new UpdateConfigAction("extensions.conf", "extensions.conf", false);
        deleteOneContextUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, deletedContext);
        deleteOneContextUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, "remote", "include", deletedContext, deletedContext);
        managerResponse = managerConnection.SendAction(deleteOneContextUpdateConfig);
        if (!managerResponse.IsSuccess())
        {
            return false;
        }
        return true;
    }

    public bool updateDialPlans(string oldContextName, string newContextName, string newPrefix)
    {
        string createdPrefix = createPrefix(newPrefix);
        UpdateConfigAction updateDialPlansUpdateConfig = new UpdateConfigAction("extensions.conf", "extensions.conf", false);
        updateDialPlansUpdateConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, "remote", "include", newContextName, oldContextName);
        updateDialPlansUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, oldContextName);
        updateDialPlansUpdateConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, newContextName);
        updateDialPlansUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, newContextName, "exten", "_" + createdPrefix + ",1,NoOp()");
        updateDialPlansUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, newContextName, "exten", "_" + createdPrefix + ",n,Dial(SIP/${{EXTEN}}@" + newContextName + ")");
        updateDialPlansUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, newContextName, "exten", "_" + createdPrefix + ",n,HangUp()");
        managerResponse = managerConnection.SendAction(updateDialPlansUpdateConfig);
        if (!managerResponse.IsSuccess())
        {
            return false;
        }
        return true;
    }

    public List<string> getUsersByAsterisk(string asteriskName)
    {
        List<string> usersByAsteriskList = new List<string>();
        managerResponse = managerConnection.SendAction(new GetConfigAction("sip.conf"));
        if (managerResponse.IsSuccess())
        {
            GetConfigResponse responseConfig = (GetConfigResponse)managerResponse;
            foreach (int key in responseConfig.Categories.Keys)
            {
                string sipCategory = responseConfig.Categories[key];
                if (sipCategory.Length == 9 && sipCategory.All(char.IsDigit))
                {
                    usersByAsteriskList.Add(sipCategory);
                }                            
            }
            return usersByAsteriskList;         
        }
        else
            return usersByAsteriskList;
    }

    private string createPrefix(string prefix)
    {
        string tmpStr = "XXXXXXXXX";
        return prefix + tmpStr.Substring(prefix.Length);
    }
}


