using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for RollbackManager
/// </summary>

public sealed class RollbackManager : AMIManager
{
    private AsteriskAccessLayer asteriskAccessLayer;
    public static RollbackManager rollbackManagerInstance
    {
        get
        {
            string singleton = "rollbackManagerSingleton";
            if (HttpContext.Current.Session[singleton] == null)
                HttpContext.Current.Session[singleton] = new RollbackManager();
            return (RollbackManager)HttpContext.Current.Session[singleton];
        }
    }

    private RollbackManager()
    {
        asteriskAccessLayer = new AsteriskAccessLayer();
    }

    public void rollbackAddAsterisk(string errorMethod, Asterisks asterisk, Asterisks createdAsterisk, List<Asterisks> rollbackList, List<Asterisks> asteriskList)
    {
        try
        {
            if (rollbackList.Count > 0)
            {
                if (errorMethod.Equals("addContext"))
                {
                    login(asterisk.ip_address, asterisk.login_AMI, Utils.DecryptAMIPassword(asterisk.password_AMI));
                    deleteTrunk(createdAsterisk.name_Asterisk);
                    logoff();
                }
                if (errorMethod.Equals("checkContexts"))
                {
                    login(asterisk.ip_address, asterisk.login_AMI, Utils.DecryptAMIPassword(asterisk.password_AMI));
                    deleteTrunk(createdAsterisk.name_Asterisk);
                    deleteContext(createdAsterisk.name_Asterisk);
                    logoff();
                }
                foreach (Asterisks rollbackAsterisk in rollbackList)
                {
                    login(rollbackAsterisk.ip_address, rollbackAsterisk.login_AMI, Utils.DecryptAMIPassword(rollbackAsterisk.password_AMI));
                    deleteTrunk(createdAsterisk.name_Asterisk);
                    deleteContext(createdAsterisk.name_Asterisk);
                    logoff();
                }
                login(createdAsterisk.ip_address, createdAsterisk.login_AMI, Utils.DecryptAMIPassword(createdAsterisk.password_AMI));
                deleteInitialContexts(asteriskList);
                deleteTLS(createdAsterisk.tls_enabled, createdAsterisk.tls_certDestination);
                deleteTrunk(asteriskList);
                asteriskAccessLayer.deleteAsteriskByName(createdAsterisk.name_Asterisk);
                logoff();
            }
            else
            {
                if (errorMethod.Equals("addTLS"))
                {
                    login(createdAsterisk.ip_address, createdAsterisk.login_AMI, Utils.DecryptAMIPassword(createdAsterisk.password_AMI));
                    deleteTrunk(asteriskList);
                    logoff();
                }
                if (errorMethod.Equals("addContext"))
                {
                    login(createdAsterisk.ip_address, createdAsterisk.login_AMI, Utils.DecryptAMIPassword(createdAsterisk.password_AMI));
                    deleteTrunk(asteriskList);
                    deleteTLS(createdAsterisk.tls_enabled, createdAsterisk.tls_certDestination);
                    logoff();
                }
                if (errorMethod.Equals("createInitialContexts"))
                {
                    login(createdAsterisk.ip_address, createdAsterisk.login_AMI, Utils.DecryptAMIPassword(createdAsterisk.password_AMI));
                    deleteTrunk(asteriskList);
                    deleteTLS(createdAsterisk.tls_enabled, createdAsterisk.tls_certDestination);
                    deleteInitialContexts(asteriskList);
                    logoff();
                }
                asteriskAccessLayer.deleteAsteriskByName(createdAsterisk.name_Asterisk);
            }
        }
        catch (Exception e)
        {

        }
    }

    public void rollbackUpdateAsterisk(string errorMethod, Asterisks currentAsterisk, Asterisks updatedAsterisk, Asterisks originalAsterisk, List<Asterisks> rollbackList, List<Asterisks> asteriskList)
    {
        try
        {
            if (rollbackList.Count > 0)
            {
                if (errorMethod.Equals("updateTLS"))
                {
                    login(currentAsterisk.ip_address, currentAsterisk.login_AMI, Utils.DecryptAMIPassword(currentAsterisk.password_AMI));
                    updateTrunk(updatedAsterisk.name_Asterisk, originalAsterisk.name_Asterisk, originalAsterisk.ip_address, updatedAsterisk.ip_address);
                    logoff();
                }
                if (errorMethod.Equals("updateContext"))
                {
                    login(currentAsterisk.ip_address, currentAsterisk.login_AMI, Utils.DecryptAMIPassword(currentAsterisk.password_AMI));
                    updateTrunk(updatedAsterisk.name_Asterisk, originalAsterisk.name_Asterisk, originalAsterisk.ip_address, updatedAsterisk.ip_address);
                    updateTLS(originalAsterisk, currentAsterisk, updatedAsterisk);
                    logoff();
                }
                foreach (Asterisks asterisk in rollbackList)
                {
                    if (asterisk.Equals(updatedAsterisk))
                        continue;
                    login(asterisk.ip_address, asterisk.login_AMI, Utils.DecryptAMIPassword(asterisk.password_AMI));
                    updateTrunk(updatedAsterisk.name_Asterisk, originalAsterisk.name_Asterisk, originalAsterisk.ip_address, updatedAsterisk.ip_address);
                    updateTLS(updatedAsterisk, asterisk, originalAsterisk);
                    updateContext(updatedAsterisk.name_Asterisk, originalAsterisk.name_Asterisk, originalAsterisk.prefix_Asterisk);
                    reloadModules();
                    logoff();
                }
                login(updatedAsterisk.ip_address, updatedAsterisk.login_AMI, Utils.DecryptAMIPassword(updatedAsterisk.password_AMI));
                updateTLS(originalAsterisk.tls_enabled, originalAsterisk.tls_certDestination, updatedAsterisk.tls_certDestination, updatedAsterisk.tls_enabled, asteriskList);
                reloadModules();
                logoff();
                asteriskAccessLayer.updateAsterisk(originalAsterisk);
            }
            else
            {
                login(currentAsterisk.ip_address, currentAsterisk.login_AMI, Utils.DecryptAMIPassword(currentAsterisk.password_AMI));
                updateTLS(originalAsterisk.tls_enabled, originalAsterisk.tls_certDestination, currentAsterisk.tls_certDestination, currentAsterisk.tls_enabled, asteriskList);                
                reloadModules();
                logoff();
                asteriskAccessLayer.updateAsterisk(originalAsterisk);
            }
        }
        catch (Exception e)
        {

        }
    }

    public void rollbackDeleteAsterisk(string errorMethod, Asterisks deletedAsterisk, Asterisks currentAsterisk, List<Asterisks> rollbackList, List<Asterisks> asteriskList)
    {
        try
        {
            if (rollbackList.Count > 0)
            {
                if (errorMethod.Equals("deleteOneContext"))
                {
                    login(currentAsterisk.ip_address, currentAsterisk.login_AMI, Utils.DecryptAMIPassword(currentAsterisk.password_AMI));
                    addTrunk(deletedAsterisk.name_Asterisk, deletedAsterisk.ip_address, deletedAsterisk.tls_enabled, currentAsterisk.tls_enabled);
                    logoff();
                }
                foreach (Asterisks asterisk in rollbackList)
                {
                    login(asterisk.ip_address, asterisk.login_AMI, Utils.DecryptAMIPassword(asterisk.password_AMI));
                    addTrunk(deletedAsterisk.name_Asterisk, deletedAsterisk.ip_address, deletedAsterisk.tls_enabled, currentAsterisk.tls_enabled);
                    addContext(deletedAsterisk.name_Asterisk, deletedAsterisk.prefix_Asterisk);
                    addInclude(deletedAsterisk.name_Asterisk);
                    reloadModules();
                    logoff();
                }
                login(deletedAsterisk.ip_address, deletedAsterisk.login_AMI, Utils.DecryptAMIPassword(deletedAsterisk.password_AMI));
                addTLS(deletedAsterisk.tls_enabled, deletedAsterisk.tls_certDestination, asteriskList);
                addTrunk(asteriskList);
                createInitialContexts(asteriskList);
                addContext(asteriskList);
                addInclude(asteriskList);
                reloadModules();
                logoff();
            }
            else
            {
                if (errorMethod.Equals("deleteTrunk"))
                {
                    login(deletedAsterisk.ip_address, deletedAsterisk.login_AMI, Utils.DecryptAMIPassword(deletedAsterisk.password_AMI));
                    addTLS(deletedAsterisk.tls_enabled, deletedAsterisk.tls_certDestination, asteriskList);
                    reloadModules();
                    logoff();
                }
                if (errorMethod.Equals("deleteAllRemoteContexts"))
                {
                    login(deletedAsterisk.ip_address, deletedAsterisk.login_AMI, Utils.DecryptAMIPassword(deletedAsterisk.password_AMI));
                    addTLS(deletedAsterisk.tls_enabled, deletedAsterisk.tls_certDestination, asteriskList);
                    addTrunk(asteriskList);
                    reloadModules();
                    logoff();
                }
            }
        }
        catch (Exception e)
        {

        }
    }
}