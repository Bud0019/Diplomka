using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for TransferUserManager
/// </summary>
public sealed class TransferUserManager : AMIManager
{
    private static Asterisks selectedAsterisk = null;
    private AsteriskAccessLayer asteriskAccessLayer;
    private TransferedUserAccessLayer transferedUserAccessLayer;
    private TransferedUser transferedUser;

    public static TransferUserManager TransferUserManagerInstance
    {
        get
        {
            string singleton = "TransferUserManagerSingleton";
            if (HttpContext.Current.Session[singleton] == null)
                HttpContext.Current.Session[singleton] = new TransferUserManager();
            return (TransferUserManager)HttpContext.Current.Session[singleton];
        }
    }
    private TransferUserManager()
    {
        asteriskAccessLayer = new AsteriskAccessLayer();
        transferedUserAccessLayer = new TransferedUserAccessLayer();
    }

    public List<string> loadUsersInAsterisk(string asteriskName, out string errorMessage)
    {
        errorMessage = string.Empty;
        selectedAsterisk = asteriskAccessLayer.SelectAsterisksByName(asteriskName);
        List<string> usersList = new List<string>();
        try
        {
            login(selectedAsterisk.ip_address, selectedAsterisk.login_AMI, Utils.DecryptAMIPassword(selectedAsterisk.password_AMI));
            usersList = getUsersByAsterisk(selectedAsterisk.name_Asterisk);
            logoff();
        }
        catch (AsterNET.Manager.AuthenticationFailedException afe) { errorMessage = "Načítanie užívateľov zlyhalo!"; }
        catch (AsterNET.Manager.TimeoutException to) { errorMessage = "Načítanie užívateľov zlyhalo!"; }
        catch (AsterNET.Manager.ManagerException me) { errorMessage = "Načítanie užívateľov zlyhalo!"; }
        return usersList;
    }

    public List<string> loadUserDetailList(string selectedUser, out string errorMessage)
    {
        errorMessage = string.Empty;
        List<string> userDetailList = new List<string>();
        try
        {
            login(selectedAsterisk.ip_address, selectedAsterisk.login_AMI, Utils.DecryptAMIPassword(selectedAsterisk.password_AMI));
            userDetailList = getUserDetail(selectedUser);
            logoff();
        }
        catch (AsterNET.Manager.AuthenticationFailedException afe) { errorMessage = "Načítanie detailu zlyhalo!"; }
        catch (AsterNET.Manager.TimeoutException to) { errorMessage = "Načítanie detailu zlyhalo!"; }
        catch (AsterNET.Manager.ManagerException me) { errorMessage = "Načítanie detailu zlyhalo!"; }
        return userDetailList;
    }

    private void transferUser(string asteriskFrom, string asteriskTo, string userName, List<string> userDetailList)
    {
        Asterisks destinationAsterisk = asteriskAccessLayer.SelectAsterisksByName(asteriskTo);
        try
        {
            string originalContext;
            transferedUser = new TransferedUser();
            login(destinationAsterisk.ip_address, destinationAsterisk.login_AMI, Utils.DecryptAMIPassword(destinationAsterisk.password_AMI));
            relocateUser(userName, userDetailList, out originalContext);
            transferedUser.name_user = userName;
            transferedUser.original_context = originalContext;
            transferedUser.original_asterisk = asteriskFrom;
            transferedUser.current_asterisk = asteriskTo;
        }
        catch (AsterNET.Manager.AuthenticationFailedException afe) { }
        catch (AsterNET.Manager.TimeoutException to) { }
        catch (AsterNET.Manager.ManagerException me) { }
    }

    private void transferFromHomeAsterisk(string ownerName, string asteriskFrom, string asteriskTo)
    {
        transferedUserAccessLayer.insertTransferedUser(transferedUser);
        List<Asterisks> asteriskList = asteriskAccessLayer.getAsterisksInList(ownerName);
        asteriskList.Remove(asteriskList.Find(asterisk => asterisk.name_Asterisk.Equals(asteriskFrom)));
        asteriskList.Remove(asteriskList.Find(asterisk => asterisk.name_Asterisk.Equals(asteriskTo)));
        try
        {
            sendUpdateDialPlanRequest(UpdateMessages.addToTrunkContextInOriginal, transferedUser, null);
            logoff();
            login(selectedAsterisk.ip_address, selectedAsterisk.login_AMI, Utils.DecryptAMIPassword(selectedAsterisk.password_AMI));
            sendUpdateDialPlanRequest(UpdateMessages.addToOriginalContext, transferedUser, null);
            deleteFromOriginal(transferedUser.name_user);
            logoff();
            foreach (Asterisks asterisk in asteriskList)
            {
                login(asterisk.ip_address, asterisk.login_AMI, Utils.DecryptAMIPassword(asterisk.password_AMI));
                sendUpdateDialPlanRequest(UpdateMessages.addToOthersAsteriskDialPlans, transferedUser, null);
                logoff();
            }

        }
        catch (AsterNET.Manager.AuthenticationFailedException afe) { }
        catch (AsterNET.Manager.TimeoutException to) { }
        catch (AsterNET.Manager.ManagerException me) { }
    }

    private void transferToHomeASterisk(string userName, string ownerName, string asteriskFrom, string asteriskTo)
    {
        TransferedUser transferedUserFromDB = transferedUserAccessLayer.selectTransferedUser(userName);
        List<Asterisks> asteriskList = asteriskAccessLayer.getAsterisksInList(ownerName);
        asteriskList.Remove(asteriskList.Find(asterisk => asterisk.name_Asterisk.Equals(asteriskFrom)));
        asteriskList.Remove(asteriskList.Find(asterisk => asterisk.name_Asterisk.Equals(asteriskTo)));

        try
        {
            sendUpdateDialPlanRequest(UpdateMessages.deleteInOriginalContext, transferedUserFromDB, null);
            returnOriginalContext(transferedUserFromDB.name_user, transferedUserFromDB.original_context);
            logoff();
            login(selectedAsterisk.ip_address, selectedAsterisk.login_AMI, Utils.DecryptAMIPassword(selectedAsterisk.password_AMI));
            sendUpdateDialPlanRequest(UpdateMessages.deleteFromSourceAsteriskDialPlan, transferedUserFromDB, null);
            deleteFromOriginal(transferedUserFromDB.name_user);
            logoff();

            foreach (Asterisks asterisk in asteriskList)
            {
                login(asterisk.ip_address, asterisk.login_AMI, Utils.DecryptAMIPassword(asterisk.password_AMI));
                sendUpdateDialPlanRequest(UpdateMessages.deleteFromRestAsteriskDialPlan, transferedUserFromDB, null);
                logoff();
            }
            transferedUserAccessLayer.deleteTransferedUser(transferedUserFromDB.name_user);
        }
        catch (AsterNET.Manager.AuthenticationFailedException afe) { }
        catch (AsterNET.Manager.TimeoutException to) { }
        catch (AsterNET.Manager.ManagerException me) { }
    }

    private void transferBetweenAsterisks(string userName, string ownerName, string asteriskFrom, string asteriskTo)
    {
        TransferedUser transferedUserFromDB = transferedUserAccessLayer.selectTransferedUser(userName);
        List<Asterisks> asteriskList = asteriskAccessLayer.getAsterisksInList(ownerName);
        asteriskList.Remove(asteriskList.Find(asterisk => asterisk.name_Asterisk.Equals(asteriskFrom)));
        asteriskList.Remove(asteriskList.Find(asterisk => asterisk.name_Asterisk.Equals(asteriskTo)));
        try
        {
            sendUpdateDialPlanRequest(UpdateMessages.updateDialPlanInDestinationAsterisk, transferedUserFromDB, null);
            logoff();
            login(selectedAsterisk.ip_address, selectedAsterisk.login_AMI, Utils.DecryptAMIPassword(selectedAsterisk.password_AMI));
            string tmpCurrentAsterisk = transferedUserFromDB.current_asterisk;
            transferedUserFromDB.current_asterisk = asteriskTo;
            sendUpdateDialPlanRequest(UpdateMessages.updateInCurrentAsteriskDialPlan, transferedUserFromDB, null);
            transferedUserFromDB.current_asterisk = tmpCurrentAsterisk;
            deleteFromOriginal(transferedUserFromDB.name_user);
            logoff();
            foreach (Asterisks asterisk in asteriskList)
            {
                login(asterisk.ip_address, asterisk.login_AMI, Utils.DecryptAMIPassword(asterisk.password_AMI));
                if (asterisk.name_Asterisk.Equals(transferedUserFromDB.original_asterisk))
                {
                    sendUpdateDialPlanRequest(UpdateMessages.updateInOriginalAsteriskDialPlan, transferedUserFromDB, asteriskTo);
                }
                else
                {
                    sendUpdateDialPlanRequest(UpdateMessages.updateInRestAsteriskDialPlan, transferedUserFromDB, asteriskTo);
                }
            }
            transferedUserAccessLayer.updateTransferedUser(transferedUserFromDB.name_user, asteriskTo);
        }
        catch (AsterNET.Manager.AuthenticationFailedException afe) { }
        catch (AsterNET.Manager.TimeoutException to) { }
        catch (AsterNET.Manager.ManagerException me) { }
    }

    public List<string> loadUsersInList(string asteriskName)
    {
        Asterisks asterisk = asteriskAccessLayer.SelectAsterisksByName(asteriskName);
        login(asterisk.ip_address, asterisk.login_AMI, Utils.DecryptAMIPassword(asterisk.password_AMI));
        List<string> usersList = getUsersByAsterisk(asterisk.name_Asterisk);
        logoff();
        return usersList;
    }

    public string transfer(string asteriskFrom, string asteriskTo, string ownerName, string userName, List<string> userDetailList)
    {
        try
        {
            transferUser(asteriskFrom, asteriskTo, userName, userDetailList);
            if (transferedUserAccessLayer.selectTransferedUser(userName) == null)
            {
                transferFromHomeAsterisk(ownerName, asteriskFrom, asteriskTo);
                return "Presun prebehol v poriadku!";
            }
            else if (transferedUserAccessLayer.selectTransferedUser(userName).original_asterisk.Equals(asteriskTo))
            {
                transferToHomeASterisk(userName, ownerName, asteriskFrom, asteriskTo);
                return "Presun prebehol v poriadku!";
            }
            else
            {
                transferBetweenAsterisks(userName, ownerName, asteriskFrom, asteriskTo);
                return "Presun prebehol v poriadku!";
            }
        }
        catch (AsterNET.Manager.AuthenticationFailedException afe) { return "Presun zlyhal!"; }
        catch (AsterNET.Manager.TimeoutException to) { return "Presun zlyhal!"; }
        catch (AsterNET.Manager.ManagerException me) { return "Presun zlyhal!"; }
    }   
}
        