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

public class AMIConnector : Utils
{

    private const int PORT = 5038;

    private ManagerConnection managerConnection;
    private ManagerResponse managerResponse;
    public string originalContext;
       
    public enum updateDialPlanMessage
    {
        addToTrunkContextOnOriginal,
        addToOriginalContext,
        addToOthersAsteriskDialPlans,
        deleteInOriginalContext,
        deleteFromSourceAsteriskDialPlan,
        deleteFromRestAsteriskDialPlan,
        updateDialPlanInDestinationAsterisk,
        updateInCurrentAsteriskDialPlan,
        updateInOriginalAsteriskDialPlan,
        updateInRestAsteriskDialPlan
    }
                
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

    public bool addTrunk(List<AsteriskRoutingSystem.Asterisk> asteriskList, int idAdded)
    {
        UpdateConfigAction addTrunkUpdateConfig = new UpdateConfigAction("sip.conf", "sip.conf", false);    
        foreach(AsteriskRoutingSystem.Asterisk asterisk in asteriskList)
            {
            if (asterisk.id_Asterisk != idAdded) { 
                addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, asterisk.name_Asterisk);
                addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "host", asterisk.ip_address);
                addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "type", "peer");
                addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "context", "remote");
                if (asterisk.tls_enabled == 1)
                {
                    addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "transport", "tls");
                }
            }
        }                    
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

    public bool addTrunk(string trunkName, string hostIP, int tlsEnabled)
    {
        UpdateConfigAction addTrunkUpdateConfig = new UpdateConfigAction("sip.conf", "sip.conf", false);
        addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, trunkName);
        addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, trunkName, "host", hostIP);
        addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, trunkName, "type", "peer");
        addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, trunkName, "context", "remote");
        if (tlsEnabled == 1)
        {
            addTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, trunkName, "transport", "tls");
        }
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

    public bool deleteTrunk(List<AsteriskRoutingSystem.Asterisk> asteriskList, int idDeleted, bool tlsEnable, string certDestination)
    {
        UpdateConfigAction deleteTrunkUpdateConfig = new UpdateConfigAction("sip.conf", "sip.conf", false);
        foreach(AsteriskRoutingSystem.Asterisk asterisk in asteriskList) {
            if (asterisk.id_Asterisk != idDeleted) 
                deleteTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, asterisk.name_Asterisk);           
        }
        if (tlsEnable)
        {
            deleteTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, "globals", "tlsenable", "tls", "tls");
            deleteTrunkUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, "globals", "tlscertfile", certDestination, certDestination);
        }
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

    public bool updateTLS(int tlsEnabled, string certDestination, int currentTLSstatus, bool isFirst, List<AsteriskRoutingSystem.Asterisk> asteriskList, string updatedAsterisk)
    {
        UpdateConfigAction updateTLSUpdateConfig = new UpdateConfigAction("sip.conf", "sip.conf", false);
        if (tlsEnabled == 1 && currentTLSstatus == 0)
        {          
            updateTLSUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "globals", "tlsenable", "tls");
            updateTLSUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "globals", "tlscerfile", certDestination);
            if (!isFirst)
            {
                foreach(AsteriskRoutingSystem.Asterisk asterisk in asteriskList)
                {
                    if(!asterisk.name_Asterisk.Equals(updatedAsterisk))
                        updateTLSUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "transport", "tls");
                }
            }
            managerResponse = managerConnection.SendAction(updateTLSUpdateConfig);
            if (managerResponse.IsSuccess())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (currentTLSstatus == 1 && tlsEnabled == 0)
        {      
            updateTLSUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, "globals", "tlsenable", "tls", "tls");
            updateTLSUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, "globals", "tlscerfile", certDestination, certDestination);
            if (!isFirst)
            {
                foreach (AsteriskRoutingSystem.Asterisk asterisk in asteriskList)
                {
                    if (!asterisk.name_Asterisk.Equals(updatedAsterisk))
                        updateTLSUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, asterisk.name_Asterisk, "transport", "tls", "tls");
                }
            }
            managerResponse = managerConnection.SendAction(updateTLSUpdateConfig);
            if (managerResponse.IsSuccess())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    public bool addPrefix(List<AsteriskRoutingSystem.Asterisk> asteriskList, int idAdded)
    {
        UpdateConfigAction addPrefixUpdateConfig = new UpdateConfigAction("extensions.conf", "extensions.conf", false);
        foreach(AsteriskRoutingSystem.Asterisk asterisk in asteriskList) { 
            if(idAdded != asterisk.id_Asterisk) {         
                string createdPrefix = createPrefix(asterisk.prefix_Asterisk);       
                addPrefixUpdateConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, asterisk.name_Asterisk);
                addPrefixUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "exten", "_" + createdPrefix + ",1,NoOp()");
                addPrefixUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "exten", "_" + createdPrefix + ",n,Dial(SIP/${EXTEN}@" + asterisk.name_Asterisk + ")");
                addPrefixUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "exten", "_" + createdPrefix + ",n,HangUp()");
            }
        }
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

    public bool addPrefix(string contextName, string prefix, bool isRollback)
    {
        string createdPrefix = createPrefix(prefix);
        UpdateConfigAction addPrefixUpdateConfig = new UpdateConfigAction("extensions.conf", "extensions.conf", false);
        addPrefixUpdateConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, contextName);
        addPrefixUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, contextName, "exten", "_" + createdPrefix + ",1,NoOp()");
        addPrefixUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, contextName, "exten", "_" + createdPrefix + ",n,Dial(SIP/${EXTEN}@" + contextName + ")");
        addPrefixUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, contextName, "exten", "_" + createdPrefix + ",n,HangUp()");
        if (isRollback)
        {
            addPrefixUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "remote", "include", contextName);
        }
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

    public bool createInitialContexts(List<string> asteriskNameList, int tlsEnabled, string certDestination)
    {          
        if(tlsEnabled == 1)
        {
            UpdateConfigAction addTlsEnableUpdateConfig = new UpdateConfigAction("sip.conf", "sip.conf", true);
            addTlsEnableUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "globals", "tlsenable", "tls");
            addTlsEnableUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "globals", "tlscerfile", certDestination);
            managerResponse = managerConnection.SendAction(addTlsEnableUpdateConfig);
            if (!managerResponse.IsSuccess())
                return false;        
        }  
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

    public bool deleteContexts(List<string> asteriskNameList)
    {
        UpdateConfigAction deleteOneContextUpdateConfig = new UpdateConfigAction("extensions.conf", "extensions.conf", false);
        foreach(string asteriskName in asteriskNameList) { 
            deleteOneContextUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, asteriskName);
        }
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
        updateDialPlansUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, newContextName, "exten", "_" + createdPrefix + ",n,Dial(SIP/${EXTEN}@" + newContextName + ")");
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
    
    public bool relocateUser(string userNumber, List<string>userDetails)
    {
       
        UpdateConfigAction userTransferUpdateConfig = new UpdateConfigAction("sip.conf", "sip.conf", true);
        userTransferUpdateConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, userNumber);    
        foreach (string item in userDetails)
        {
            string[] items = item.Split('=');
            if (item.StartsWith("context"))
            {
                originalContext = items[1];       
                userTransferUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, userNumber, "context", "remote");
            }
            else
            {               
                userTransferUpdateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, userNumber, items[0], items[1]);
            }                    
        }
        managerResponse = managerConnection.SendAction(userTransferUpdateConfig);
        if (!managerResponse.IsSuccess())
        {
            return false;
        }     
        return true;
    }

    public bool sendUpdateDialPlanRequest(updateDialPlanMessage message, string userNumber, string originalAsterisk, string currentAsterisk, string originalContext, string newCurrentAsterisk)
    {
        UpdateConfigAction updateConfig = new UpdateConfigAction("extensions.conf", "extensions.conf", true);
        switch (message)
        {
            case updateDialPlanMessage.addToTrunkContextOnOriginal:
                updateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, originalAsterisk, "exten", userNumber + ",1,Dial(SIP/${EXTEN})");
                break;
            case updateDialPlanMessage.addToOriginalContext:
                updateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, originalContext, "exten", userNumber + ",1,Dial(SIP/${EXTEN}@" + currentAsterisk + ")");
                break;
            case updateDialPlanMessage.addToOthersAsteriskDialPlans:
                updateConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, originalAsterisk, "exten", userNumber+ ",1,Dial(SIP/${EXTEN}@" + currentAsterisk + ")");
                break;
            case updateDialPlanMessage.deleteInOriginalContext:
                updateConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, originalContext, "exten", userNumber + ",1,Dial(SIP/${EXTEN}@" + currentAsterisk + ")", userNumber + ",1,Dial(SIP/${EXTEN}@" + currentAsterisk + ")");
                break;
            case updateDialPlanMessage.deleteFromSourceAsteriskDialPlan:
                updateConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, originalAsterisk, "exten", userNumber + ",1,Dial(SIP/${EXTEN})", userNumber + ",1,Dial(SIP/${EXTEN})");
                break;
            case updateDialPlanMessage.deleteFromRestAsteriskDialPlan:
                updateConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, originalAsterisk, "exten", userNumber + ",1,Dial(SIP/${EXTEN}@" + currentAsterisk + ")", userNumber + ",1,Dial(SIP/${EXTEN}@" + currentAsterisk + ")");
                break;
            case updateDialPlanMessage.updateDialPlanInDestinationAsterisk:
                updateConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, originalAsterisk, "exten", userNumber + ",1,Dial(SIP/${EXTEN})", userNumber + ",1,Dial(SIP/${EXTEN}@" + currentAsterisk + ")");
                break;
            case updateDialPlanMessage.updateInCurrentAsteriskDialPlan:
                updateConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, originalAsterisk, "exten", userNumber + ",1,Dial(SIP/${EXTEN}@" + currentAsterisk + ")", userNumber + ",1,Dial(SIP/${EXTEN})");
                break;
            case updateDialPlanMessage.updateInOriginalAsteriskDialPlan:
                updateConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, originalContext, "exten", userNumber + ",1,Dial(SIP/${EXTEN}@" + newCurrentAsterisk + ")", userNumber + ",1,Dial(SIP/${EXTEN}@" + currentAsterisk + ")");
                break;
            case updateDialPlanMessage.updateInRestAsteriskDialPlan:
                updateConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, originalAsterisk, "exten", userNumber + ",1,Dial(SIP/${EXTEN}@" + newCurrentAsterisk + ")", userNumber + ",1,Dial(SIP/${EXTEN}@" + currentAsterisk + ")");
                break;
            default:
                break;
        }
        managerResponse = managerConnection.SendAction(updateConfig);
        if (!managerResponse.IsSuccess())
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool deleteFromOriginal(string prefix)
    {
        UpdateConfigAction deleteContextUpdateConfig = new UpdateConfigAction("sip.conf", "sip.conf", true);
        deleteContextUpdateConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT,prefix);
        managerResponse = managerConnection.SendAction(deleteContextUpdateConfig);
        if (!managerResponse.IsSuccess())
        {
            return false;
        }
        else
        {
            return true;
        }
    }
     
    public bool returnOriginalContext(string prefix, string originalContext)
    {
        UpdateConfigAction returnOriginalContextUpdateConfig = new UpdateConfigAction("sip.conf", "sip.conf", true);
        returnOriginalContextUpdateConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, prefix, "context", originalContext, "remote");
        managerResponse = managerConnection.SendAction(returnOriginalContextUpdateConfig);
        if (!managerResponse.IsSuccess())
        {
            return false;
        }
        else
        {
            return true;
        }
    }      

    public List<string>getUserDetail(string prefix)
    {
        List<string> usersDetailList = new List<string>();
        managerResponse = managerConnection.SendAction(new GetConfigAction("sip.conf"));
        if (managerResponse.IsSuccess())
        {
            GetConfigResponse responseConfig = (GetConfigResponse)managerResponse;
            foreach (int key in responseConfig.Categories.Keys)
            {
                string sipCategory = responseConfig.Categories[key];
                if (sipCategory.Equals(prefix))
                {
                    foreach (int keyLine in responseConfig.Lines(key).Keys)
                    {
                      usersDetailList.Add(responseConfig.Lines(key)[keyLine]);
                    }
                    break;
                }
            }
            return usersDetailList;
        }
        else
            return usersDetailList;
    }

    private string createPrefix(string prefix)
    {
        string tmpStr = "XXXXXXXXX";
        return prefix + tmpStr.Substring(prefix.Length);
    }
}


