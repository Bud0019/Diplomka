using System.Collections.Generic;
using System.Linq;
using AsterNET.Manager;
using AsterNET.Manager.Action;
using AsterNET.Manager.Response;
using System.Web;

public abstract class AMIManager
{
    private const int PORT = 5038;
    private const string SIP_CONFIG = "sip.conf";
    private const string EXTENSIONS_CONFIG = "extensions.conf";

    protected ManagerConnection managerConnection;
    protected ManagerResponse managerResponse;
    protected UpdateConfigAction updateSipConfig;
    protected UpdateConfigAction updateExtensionsConfig;
    protected static bool rollbackState = false; 
     
       
    //constructor
    public AMIManager()
    {       
    }

    protected void reloadModules()
    {
        updateSipConfig = new UpdateConfigAction(SIP_CONFIG, SIP_CONFIG, true);
        updateExtensionsConfig = new UpdateConfigAction(EXTENSIONS_CONFIG, EXTENSIONS_CONFIG, true);
        managerResponse = managerConnection.SendAction(updateSipConfig);
        managerResponse = managerConnection.SendAction(updateExtensionsConfig);
    }

    protected void login(string ipAddress, string amiLogin, string amiPassword)
    {
        managerConnection = new ManagerConnection(ipAddress, PORT, amiLogin, amiPassword);
        managerConnection.Login(15000);         
    }

    protected void logoff()
    {
        managerConnection.Logoff();
    }

    protected void addTrunk(List<Asterisks> asteriskList)
    {
        updateSipConfig = new UpdateConfigAction(SIP_CONFIG, SIP_CONFIG);
        foreach (Asterisks asterisk in asteriskList)
        {
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, asterisk.name_Asterisk);
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "host", asterisk.ip_address);
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "type", "peer");
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "context", "remote");   
        }
        managerResponse = managerConnection.SendAction(updateSipConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void addTrunk(string trunkName, string hostIP, int currentTlsEnabled, int otherTlsEnabled)
    {
        updateSipConfig = new UpdateConfigAction(SIP_CONFIG, SIP_CONFIG);
        updateSipConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, trunkName);
        updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, trunkName, "host", hostIP);
        updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, trunkName, "type", "peer");
        updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, trunkName, "context", "remote");
        if (currentTlsEnabled == 1 && otherTlsEnabled == 1)
        {
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, trunkName, "transport", "tls");
        }
        managerResponse = managerConnection.SendAction(updateSipConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void deleteTrunk(string trunkName)
    {
        updateSipConfig = new UpdateConfigAction(SIP_CONFIG, SIP_CONFIG);
        updateSipConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, trunkName);
        managerResponse = managerConnection.SendAction(updateSipConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void deleteTrunk(List<Asterisks> asteriskList)
    {
        updateSipConfig = new UpdateConfigAction(SIP_CONFIG, SIP_CONFIG);
        foreach (Asterisks asterisk in asteriskList)
        {
              updateSipConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, asterisk.name_Asterisk);
        }       
        managerResponse = managerConnection.SendAction(updateSipConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void updateTrunk(string oldTrunkName, string newTrunkName, string newHostIP, string oldHostIP)
    {
        updateSipConfig = new UpdateConfigAction(SIP_CONFIG, SIP_CONFIG);
        updateSipConfig.AddCommand(UpdateConfigAction.ACTION_RENAMECAT, oldTrunkName, null, newTrunkName);
        updateSipConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, newTrunkName, "host", newHostIP, oldHostIP);       
        managerResponse = managerConnection.SendAction(updateSipConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void addTLS(int tlsEnabled, string certDestination, List<Asterisks> asteriskList)
    {
        if (tlsEnabled == 1)
        {
            updateSipConfig = new UpdateConfigAction(SIP_CONFIG, SIP_CONFIG, true);
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "globals", "tlsenable", "tls");
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "globals", "tlscertfile", certDestination);
            foreach(Asterisks asterisk in asteriskList)
            {
                if(asterisk.tls_enabled == 1)
                    updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "transport", "tls");
            }
            managerResponse = managerConnection.SendAction(updateSipConfig);
            if (!managerResponse.IsSuccess() && !rollbackState)
            {
                throw new ManagerException(managerResponse.Message);
            }
        }
    }

    protected void deleteTLS(int tlsEnable, string certDestination)
    {
        updateSipConfig = new UpdateConfigAction(SIP_CONFIG, SIP_CONFIG);
        if (tlsEnable == 1)
        {
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, "globals", "tlsenable", "tls", "tls");
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, "globals", "tlscertfile", certDestination, certDestination);
            managerResponse = managerConnection.SendAction(updateSipConfig);
            if (!managerResponse.IsSuccess() && !rollbackState)
            {
                throw new ManagerException(managerResponse.Message);
            }
        }      
    }

    protected void updateTLS(int newTLSstatus, string newCertDestination, string oldCertDestination, int oldTLSstatus, List<Asterisks> asteriskList)
    {
        updateSipConfig = new UpdateConfigAction(SIP_CONFIG, SIP_CONFIG);
        if (newTLSstatus == 1 && oldTLSstatus == 0)
        {
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "globals", "tlsenable", "tls");
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "globals", "tlscertfile", oldCertDestination);
            foreach (Asterisks asterisk in asteriskList)
                {    
                     if(asterisk.tls_enabled == 1)               
                        updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "transport", "tls");
                }                  
        }
        else if (oldTLSstatus == 1 && newTLSstatus == 0)
        {
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, "globals", "tlsenable", "tls", "tls");
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, "globals", "tlscertfile", oldCertDestination, oldCertDestination);           
                foreach (Asterisks asterisk in asteriskList)
                {
                    if (asterisk.tls_enabled == 1)
                        updateSipConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, asterisk.name_Asterisk, "transport", "tls", "tls");
                }        
        }
        else if (oldTLSstatus == 1 && newTLSstatus == 1)
        {
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, "globals", "tlscertfile", newCertDestination, oldCertDestination);
        }
        managerResponse = managerConnection.SendAction(updateSipConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void updateTLS(Asterisks currentAsterisk, Asterisks updatedAsterisk, Asterisks originalAsterisk)
    {
        updateSipConfig = new UpdateConfigAction(SIP_CONFIG, SIP_CONFIG);
        if(currentAsterisk.tls_enabled == 1 && updatedAsterisk.tls_enabled == 0 && originalAsterisk.tls_enabled == 1)
        {
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, updatedAsterisk.name_Asterisk, "transport", "tls", "tls");
        }
        else if(currentAsterisk.tls_enabled == 1 && updatedAsterisk.tls_enabled == 1 && originalAsterisk.tls_enabled == 0)
        {
            updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, updatedAsterisk.name_Asterisk, "transport", "tls", "tls");
        }
        managerResponse = managerConnection.SendAction(updateSipConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void addPrefixContext(List<Asterisks> asteriskList)
    {
        updateExtensionsConfig = new UpdateConfigAction(EXTENSIONS_CONFIG, EXTENSIONS_CONFIG);
        foreach (Asterisks asterisk in asteriskList)
        {
            string createdPrefix = Utils.createPrefix(asterisk.prefix_Asterisk);
            updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, asterisk.name_Asterisk);
            updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "exten", "_" + createdPrefix + ",1,NoOp()");
            updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "exten", "_" + createdPrefix + ",n,Dial(SIP/${EXTEN}@" + asterisk.name_Asterisk + ")");
            updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, asterisk.name_Asterisk, "exten", "_" + createdPrefix + ",n,HangUp()");
        }
        managerResponse = managerConnection.SendAction(updateExtensionsConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void addInclude(string trunkName)
    {
        updateExtensionsConfig = new UpdateConfigAction(EXTENSIONS_CONFIG, EXTENSIONS_CONFIG);
        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "remote", "include", trunkName);
        managerResponse = managerConnection.SendAction(updateExtensionsConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void addInclude(List<Asterisks> asteriskList)
    {
        updateExtensionsConfig = new UpdateConfigAction(EXTENSIONS_CONFIG, EXTENSIONS_CONFIG);
        foreach(Asterisks asterisk in asteriskList)
        {
            updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "remote", "include", asterisk.name_Asterisk);
        }       
        managerResponse = managerConnection.SendAction(updateExtensionsConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void addPrefixContext(string contextName, string prefix)
    {
        string createdPrefix = Utils.createPrefix(prefix);
        updateExtensionsConfig = new UpdateConfigAction(EXTENSIONS_CONFIG, EXTENSIONS_CONFIG);
        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, contextName);
        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, contextName, "exten", "_" + createdPrefix + ",1,NoOp()");
        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, contextName, "exten", "_" + createdPrefix + ",n,Dial(SIP/${EXTEN}@" + contextName + ")");
        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, contextName, "exten", "_" + createdPrefix + ",n,HangUp()");       
        managerResponse = managerConnection.SendAction(updateExtensionsConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void createInitialContexts(List<Asterisks> asteriskList)
    {      
        managerResponse = managerConnection.SendAction(new GetConfigAction(EXTENSIONS_CONFIG));
        updateExtensionsConfig = new UpdateConfigAction(EXTENSIONS_CONFIG, EXTENSIONS_CONFIG);
        if (managerResponse.IsSuccess())
        {
            GetConfigResponse responseConfig = (GetConfigResponse)managerResponse;         
            updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, "remote");        
            foreach (int key in responseConfig.Categories.Keys)
            {
                string extensionsCategory = responseConfig.Categories[key];
                updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "remote", "include", extensionsCategory);

                if (!asteriskList.Contains(asteriskList.Find(asterisk => asterisk.name_Asterisk.Equals(extensionsCategory))))
                {
                    updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, extensionsCategory, "include", "remote");
                }
            }
            managerResponse = managerConnection.SendAction(updateExtensionsConfig);
            if (!managerResponse.IsSuccess() && !rollbackState)
            {
                throw new ManagerException(managerResponse.Message);
            }
        }
        else
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void addToRemoteDialPlans(List<Asterisks> asteriskList)
    {
        managerResponse = managerConnection.SendAction(new GetConfigAction(EXTENSIONS_CONFIG));
        updateExtensionsConfig = new UpdateConfigAction(EXTENSIONS_CONFIG, EXTENSIONS_CONFIG);
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
                        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, "remote", "include", extensionsCategory);
                    }
                    if (!asteriskList.Contains(asteriskList.Find(asterisk => asterisk.name_Asterisk.Equals(extensionsCategory))))
                    {
                        if (!responseConfig.Lines(key).ContainsValue("include=remote"))
                        {
                            updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, extensionsCategory, "include", "remote");
                        }
                    }
                }
            }
            managerResponse = managerConnection.SendAction(updateExtensionsConfig);
            if (!managerResponse.IsSuccess() && !rollbackState)
            {
                throw new ManagerException(managerResponse.Message);
            }
        }
        else
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void deleteAllRemoteContexts(List<Asterisks> asteriskList)
    {
        updateExtensionsConfig = new UpdateConfigAction(EXTENSIONS_CONFIG, EXTENSIONS_CONFIG);
        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, "remote");    
        managerResponse = managerConnection.SendAction(new GetConfigAction(EXTENSIONS_CONFIG));       
        if (managerResponse.IsSuccess())
        {
            GetConfigResponse responseConfig = (GetConfigResponse)managerResponse;
            foreach (int key in responseConfig.Categories.Keys)
            {                
                string extensionsCategory = responseConfig.Categories[key];
                if (extensionsCategory.Equals("remote"))
                    continue;
                if (asteriskList.Contains(asteriskList.Find(asterisk => asterisk.name_Asterisk.Equals(extensionsCategory))))
                {
                    updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, extensionsCategory);
                }
                else
                {
                    updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, extensionsCategory, "include", "remote", "remote");
                }
            }
            managerResponse = managerConnection.SendAction(updateExtensionsConfig);
            if (!managerResponse.IsSuccess() && !rollbackState)
            {
                throw new ManagerException(managerResponse.Message);
            }
        }
        else
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void deleteOneContext(string deletedContext)
    {
        updateExtensionsConfig = new UpdateConfigAction(EXTENSIONS_CONFIG, EXTENSIONS_CONFIG);
        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, deletedContext);
        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, "remote", "include", deletedContext, deletedContext);
        managerResponse = managerConnection.SendAction(updateExtensionsConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void deleteContexts(List<Asterisks> asteriskList)
    {
        updateExtensionsConfig = new UpdateConfigAction(EXTENSIONS_CONFIG, EXTENSIONS_CONFIG);
        foreach (Asterisks asterisk in asteriskList)
        {
            updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, asterisk.name_Asterisk);
        }
        managerResponse = managerConnection.SendAction(updateExtensionsConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void updateDialPlans(string oldContextName, string newContextName, string newPrefix)
    {
        string createdPrefix = Utils.createPrefix(newPrefix);
        updateExtensionsConfig = new UpdateConfigAction(EXTENSIONS_CONFIG, EXTENSIONS_CONFIG);
        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, "remote", "include", newContextName, oldContextName);
        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, oldContextName);
        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, newContextName);
        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, newContextName, "exten", "_" + createdPrefix + ",1,NoOp()");
        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, newContextName, "exten", "_" + createdPrefix + ",n,Dial(SIP/${EXTEN}@" + newContextName + ")");
        updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, newContextName, "exten", "_" + createdPrefix + ",n,HangUp()");
        managerResponse = managerConnection.SendAction(updateExtensionsConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void relocateUser(string userNumber, List<string> userDetailList, out string originalContext)
    {
        originalContext = null;
        updateSipConfig = new UpdateConfigAction(SIP_CONFIG, SIP_CONFIG);
        updateSipConfig.AddCommand(UpdateConfigAction.ACTION_NEWCAT, userNumber);
        foreach (string item in userDetailList)
        {
            string[] items = item.Split('=');
            if (item.StartsWith("context"))
            {
                originalContext = items[1];
                updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, userNumber, "context", "remote");
            }
            else
            {
                updateSipConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, userNumber, items[0], items[1]);
            }
        }
        managerResponse = managerConnection.SendAction(updateSipConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void sendUpdateDialPlanRequest(UpdateMessages message, TransferedUser transferedUser, string newCurrentAsterisk)
    {
        updateExtensionsConfig = new UpdateConfigAction(EXTENSIONS_CONFIG, EXTENSIONS_CONFIG, true);
        switch (message)
        {
            case UpdateMessages.addToTrunkContextInOriginal:
                updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, transferedUser.original_asterisk, "exten", transferedUser.name_user + ",1,Dial(SIP/${EXTEN})");
                break;
            case UpdateMessages.addToOriginalContext:
                updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, transferedUser.original_context, "exten", transferedUser.name_user + ",1,Dial(SIP/${EXTEN}@" + transferedUser.current_asterisk + ")");
                break;
            case UpdateMessages.addToOthersAsteriskDialPlans:
                updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_APPEND, transferedUser.original_asterisk, "exten", transferedUser.name_user + ",1,Dial(SIP/${EXTEN}@" + transferedUser.current_asterisk + ")");
                break;
            case UpdateMessages.deleteInOriginalContext:
                updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, transferedUser.original_context, "exten", transferedUser.name_user + ",1,Dial(SIP/${EXTEN}@" + transferedUser.current_asterisk + ")", transferedUser.name_user + ",1,Dial(SIP/${EXTEN}@" + transferedUser.current_asterisk + ")");
                break;
            case UpdateMessages.deleteFromSourceAsteriskDialPlan:
                updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, transferedUser.original_asterisk, "exten", transferedUser.name_user + ",1,Dial(SIP/${EXTEN})", transferedUser.name_user + ",1,Dial(SIP/${EXTEN})");
                break;
            case UpdateMessages.deleteFromRestAsteriskDialPlan:
                updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_DELETE, transferedUser.original_asterisk, "exten", transferedUser.name_user + ",1,Dial(SIP/${EXTEN}@" + transferedUser.current_asterisk + ")", transferedUser.name_user + ",1,Dial(SIP/${EXTEN}@" + transferedUser.current_asterisk + ")");
                break;
            case UpdateMessages.updateDialPlanInDestinationAsterisk:
                updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, transferedUser.original_asterisk, "exten", transferedUser.name_user + ",1,Dial(SIP/${EXTEN})", transferedUser.name_user + ",1,Dial(SIP/${EXTEN}@" + transferedUser.current_asterisk + ")");
                break;
            case UpdateMessages.updateInCurrentAsteriskDialPlan:
                updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, transferedUser.original_asterisk, "exten", transferedUser.name_user + ",1,Dial(SIP/${EXTEN}@" + transferedUser.current_asterisk + ")", transferedUser.name_user + ",1,Dial(SIP/${EXTEN})");
                break;
            case UpdateMessages.updateInOriginalAsteriskDialPlan:
                updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, transferedUser.original_context, "exten", transferedUser.name_user + ",1,Dial(SIP/${EXTEN}@" + newCurrentAsterisk + ")", transferedUser.name_user + ",1,Dial(SIP/${EXTEN}@" + transferedUser.current_asterisk + ")");
                break;
            case UpdateMessages.updateInRestAsteriskDialPlan:
                updateExtensionsConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, transferedUser.original_asterisk, "exten", transferedUser.name_user + ",1,Dial(SIP/${EXTEN}@" + newCurrentAsterisk + ")", transferedUser.name_user + ",1,Dial(SIP/${EXTEN}@" + transferedUser.current_asterisk + ")");
                break;
            default:
                break;
        }
        managerResponse = managerConnection.SendAction(updateExtensionsConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void deleteFromOriginal(string userName)
    {
        updateSipConfig = new UpdateConfigAction(SIP_CONFIG, SIP_CONFIG, true);
        updateSipConfig.AddCommand(UpdateConfigAction.ACTION_DELCAT, userName);
        managerResponse = managerConnection.SendAction(updateSipConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected void returnOriginalContext(string userName, string originalContext)
    {
        updateSipConfig = new UpdateConfigAction(SIP_CONFIG, SIP_CONFIG, true);
        updateSipConfig.AddCommand(UpdateConfigAction.ACTION_UPDATE, userName, "context", originalContext, "remote");
        managerResponse = managerConnection.SendAction(updateSipConfig);
        if (!managerResponse.IsSuccess() && !rollbackState)
        {
            throw new ManagerException(managerResponse.Message);
        }
    }

    protected List<string> getUserDetail(string prefix)
    {
        List<string> usersDetailList = new List<string>();
        managerResponse = managerConnection.SendAction(new GetConfigAction(SIP_CONFIG));
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
        {
            throw new ManagerException(managerResponse.Message);
        }           
    }

    protected List<string> getUsersByAsterisk(string asteriskName)
    {
        List<string> usersByAsteriskList = new List<string>();
        managerResponse = managerConnection.SendAction(new GetConfigAction(SIP_CONFIG));
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
            throw new ManagerException(managerResponse.Message);
    }
}

