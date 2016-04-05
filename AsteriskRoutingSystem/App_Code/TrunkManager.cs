using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using AsterNET.Manager;

/// <summary>
/// Summary description for TrunkManager
/// </summary>
/// 
public sealed class TrunkManager : AMIManager
{
    private AsteriskAccessLayer asteriskAccessLayer;
    private StringBuilder textToLog;

    public delegate void addAsteriskErrorHandler(string errorMethod, Asterisks asteriskArg, Asterisks createdAsteriskArg, List<Asterisks> rollbackAsteriskListArg, List<Asterisks> asteriskListArg);
    public delegate void updateAsteriskErrorHandler(string errorMethod, Asterisks asteriskArg, Asterisks updatedAsteriskArg, Asterisks originalAsterisk, List<Asterisks> rollbackAsteriskListArg, List<Asterisks> asteriskListArg);
    public delegate void deleteAsteriskErrorHandler(string errorMethod, Asterisks deletedAsteriskArg, Asterisks currentAsterisks, List<Asterisks> rollbackAsteriskListArg, List<Asterisks> asteriskListArg);

    private addAsteriskErrorHandler addError;
    private updateAsteriskErrorHandler updateError;
    private deleteAsteriskErrorHandler deleteError;

    public event addAsteriskErrorHandler addAsteriskErrorEvent
    {
        add
        {
            if (addError == null || !addError.GetInvocationList().Contains(value))
            {
                addError += value;
            }
        }
        remove
        {
            addError -= value;
        }
    }

    public event updateAsteriskErrorHandler updateAsteriskErrorEvent
    {
        add
        {
            if (updateError == null || !updateError.GetInvocationList().Contains(value))
            {
                updateError += value;
            }
        }
        remove
        {
            updateError -= value;
        }
    }

    public event deleteAsteriskErrorHandler deleteAsteriskErrorEvent
    {
        add
        {
            if (deleteError == null || !deleteError.GetInvocationList().Contains(value))
            {
                deleteError += value;
            }
        }
        remove
        {
            deleteError -= value;
        }
    }

    public static TrunkManager trunkManagerInstance
    {
        get
        {
            string singleton = "trunkManagerSingleton";
            if (HttpContext.Current.Session[singleton] == null)
                HttpContext.Current.Session[singleton] = new TrunkManager();
            return (TrunkManager)HttpContext.Current.Session[singleton];
        }
    }

    private TrunkManager()
    {
        asteriskAccessLayer = new AsteriskAccessLayer();
    }

    public void raiseUpdateTrunkErrorEvent(string errorMethod, Asterisks asteriskArg, Asterisks updatedAsteriskArg, Asterisks originalAsterisk, List<Asterisks> rollbackAsteriskListArg, List<Asterisks> asteriskListArg)
    {
        if (updateError != null)
            updateError(errorMethod, asteriskArg, updatedAsteriskArg, originalAsterisk, rollbackAsteriskListArg, asteriskListArg);
    }

    public void raiseAddTrunkErrorEvent(string errorMethod, Asterisks asteriskArg, Asterisks createdAsteriskArg, List<Asterisks> rollbackAsteriskListArg, List<Asterisks> asteriskListArg)
    {
        if (addError != null)
            addError(errorMethod, asteriskArg, createdAsteriskArg, rollbackAsteriskListArg, asteriskListArg);
    }

    public void raiseDeleteTrunkErrorEvent(string errorMethod, Asterisks deletedAsteriskArg, Asterisks currentAsterisks, List<Asterisks> rollbackAsteriskListArg, List<Asterisks> asteriskListArg)
    {
        if (deleteError != null)
            deleteError(errorMethod, deletedAsteriskArg, currentAsterisks, rollbackAsteriskListArg, asteriskListArg);
    }

    public string createTrunk(Asterisks createdAsterisk)
    {
        Asterisks currentAsterisk = null;
        int returnCode;
        rollbackState = false;
        textToLog = new StringBuilder();
        List<Asterisks> asteriskList = asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString());       
        List<Asterisks> rollbackList = new List<Asterisks>();

        try
        {
            login(createdAsterisk.ip_address, createdAsterisk.login_AMI, Utils.DecryptAMIPassword(createdAsterisk.password_AMI));
            if ((returnCode = asteriskAccessLayer.insertAsterisk(createdAsterisk)) == -1)
            {
            }
            else if (returnCode == 1)
            {
                textToLog.Append("<" + createdAsterisk.name_Asterisk + ">: Názov je zadaný!\n");
                return textToLog.ToString();
            }
            else if (returnCode == 2)
            {
                textToLog.Append("<" + createdAsterisk.name_Asterisk + ">: IP adresa je zadaná!\n");
                return textToLog.ToString();
            }
            else if (returnCode == 3)
            {
                textToLog.Append("<" + createdAsterisk.name_Asterisk + ">: Prefix je zadaný!\n");
                return textToLog.ToString();
            }
            addTrunk(asteriskList);
            addTLS(createdAsterisk.tls_enabled, createdAsterisk.tls_certDestination, asteriskList);
            addContext(asteriskList);
            asteriskList.Add(createdAsterisk);
            createInitialContexts(asteriskList);
            asteriskList.Add(createdAsterisk);
            reloadModules();
            logoff();            
            foreach (Asterisks asterisk in asteriskList)
            {
                currentAsterisk = asterisk;
                if (asterisk.name_Asterisk.Equals(createdAsterisk.name_Asterisk))
                    continue;
                login(asterisk.ip_address, asterisk.login_AMI, Utils.DecryptAMIPassword(asterisk.password_AMI));
                addTrunk(createdAsterisk.name_Asterisk, createdAsterisk.ip_address, createdAsterisk.tls_enabled, asterisk.tls_enabled);
                addContext(createdAsterisk.name_Asterisk, createdAsterisk.prefix_Asterisk);
                addInclude(createdAsterisk.name_Asterisk);
                checkContexts(asteriskList);
                reloadModules();
                logoff();
                rollbackList.Add(asterisk);
            }
            return textToLog.Append("Pridanie Asterisku prebehlo úspešne!\n").ToString();
        }
        catch (AuthenticationFailedException)
        {
            asteriskList.Remove(createdAsterisk);
            rollbackState = true;
            raiseAddTrunkErrorEvent("login", currentAsterisk, createdAsterisk, rollbackList, asteriskList);
            return textToLog.Append("Neplatné meno alebo heslo!\n").ToString();
        }
        catch (AsterNET.Manager.TimeoutException)
        {
            asteriskList.Remove(createdAsterisk);
            rollbackState = true;
            raiseAddTrunkErrorEvent("login", currentAsterisk, createdAsterisk, rollbackList, asteriskList);
            return textToLog.Append("Čas spojenia vypršal!\n").ToString();
        }
        catch (ManagerException managerException)
        {
            asteriskList.Remove(createdAsterisk);
            rollbackState = true;
            raiseAddTrunkErrorEvent(managerException.TargetSite.Name, currentAsterisk, createdAsterisk, rollbackList, asteriskList);
            logoff();
            return textToLog.Append("Nastala chyba v:[" + currentAsterisk.name_Asterisk + "]\n" + managerException.Message + "\n").ToString();
        }
        catch (Exception e)
        {
            rollbackState = true;
            raiseAddTrunkErrorEvent("login", currentAsterisk, createdAsterisk, rollbackList, asteriskList);
            return textToLog.Append("Nastala chyba v:[" + currentAsterisk.name_Asterisk + "]\n" + e.Message + "\n").ToString();
        }
    }

    public string deleteTrunk(int IDDeletedAsterisk, bool rollback)
    {
        rollbackState = rollback;
        List<Asterisks> rollbackList = new List<Asterisks>();
        List<Asterisks> asteriskList = asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString());
        Asterisks deletedAsterisk = asteriskList.Find(selectedAsterisk => selectedAsterisk.id_Asterisk == IDDeletedAsterisk);
        asteriskList.Remove(deletedAsterisk);
        textToLog = new StringBuilder();
        Asterisks currentAsterisk = deletedAsterisk;

        try
        {
            login(deletedAsterisk.ip_address, deletedAsterisk.login_AMI, Utils.DecryptAMIPassword(deletedAsterisk.password_AMI));
            deleteTLS(deletedAsterisk.tls_enabled, deletedAsterisk.tls_certDestination);
            deleteTrunk(asteriskList);
            deleteInitialContexts(asteriskList);
            reloadModules();
            logoff();

            foreach (Asterisks asterisk in asteriskList)
            {
                currentAsterisk = asterisk;
                login(asterisk.ip_address, asterisk.login_AMI, Utils.DecryptAMIPassword(asterisk.password_AMI));
                deleteTrunk(deletedAsterisk.name_Asterisk);
                deleteContext(deletedAsterisk.name_Asterisk);
                reloadModules();
                logoff();
                rollbackList.Add(asterisk);
            }
            asteriskAccessLayer.deleteAsterisk(deletedAsterisk.id_Asterisk);
            return textToLog.Append("Zmazanie Asterisku prebehlo úspešne!\n").ToString();
        }
        catch (AuthenticationFailedException)
        {
            rollbackState = true;
            raiseDeleteTrunkErrorEvent("login", deletedAsterisk, currentAsterisk, rollbackList, asteriskList);
            return textToLog.Append("Neplatné meno alebo heslo!\n").ToString();
        }
        catch (AsterNET.Manager.TimeoutException)
        {
            rollbackState = true;
            raiseDeleteTrunkErrorEvent("login", deletedAsterisk, currentAsterisk, rollbackList, asteriskList);
            return textToLog.Append("Čas spojenia vypršal!\n").ToString();
        }
        catch (ManagerException managerException)
        {
            rollbackState = true;
            raiseDeleteTrunkErrorEvent(managerException.TargetSite.Name, deletedAsterisk, currentAsterisk, rollbackList, asteriskList);
            logoff();
            return textToLog.Append("Nastala chyba v:[" + currentAsterisk.name_Asterisk + "]\n" + managerException.Message + "\n").ToString();
        }
        catch (Exception e)
        {
            rollbackState = true;
            raiseDeleteTrunkErrorEvent("login", deletedAsterisk, currentAsterisk, rollbackList, asteriskList);
            return textToLog.Append("Nastala chyba v:[" + currentAsterisk.name_Asterisk + "]\n" + e.Message + "\n").ToString();
        }
    }

    public string updateTrunk(Asterisks updatedAsterisk)
    {
        int returnCode;
        rollbackState = false;
        Asterisks currentAsterisk = updatedAsterisk;
        List<Asterisks> asteriskList = asteriskAccessLayer.getAsterisksInList(Membership.GetUser().UserName.ToString());
        textToLog = new StringBuilder();
        Asterisks originalAsterisk = asteriskList.Find(selectedAsterisk => selectedAsterisk.id_Asterisk == updatedAsterisk.id_Asterisk);
        asteriskList.Remove(originalAsterisk);
        List<Asterisks> rollbackList = new List<Asterisks>();

        try
        {
            login(updatedAsterisk.ip_address, updatedAsterisk.login_AMI, Utils.DecryptAMIPassword(updatedAsterisk.password_AMI));
            if ((returnCode = asteriskAccessLayer.updateAsterisk(updatedAsterisk)) == -1)
            {

            }
            else if (returnCode == 1)
            {
                textToLog.Append("<" + originalAsterisk.name_Asterisk + ">: Nazov je zadany!\n");
                return textToLog.ToString();
            }
            else if (returnCode == 2)
            {
                textToLog.Append("<" + originalAsterisk.name_Asterisk + ">: IP adresa je zadana!\n");
                return textToLog.ToString();
            }
            else if (returnCode == 3)
            {
                textToLog.Append("<" + originalAsterisk.name_Asterisk + ">: Prefix je zadany!\n");
                return textToLog.ToString();
            }
            updateTLS(updatedAsterisk.tls_enabled, updatedAsterisk.tls_certDestination, originalAsterisk.tls_certDestination, originalAsterisk.tls_enabled, asteriskList);
            updateContext(originalAsterisk.prefix_Asterisk, updatedAsterisk.prefix_Asterisk);
            rollbackList.Add(updatedAsterisk);
            foreach (Asterisks asterisk in asteriskList)
            {
                currentAsterisk = asterisk;
                login(asterisk.ip_address, asterisk.login_AMI, Utils.DecryptAMIPassword(asterisk.password_AMI));
                updateTrunk(originalAsterisk.name_Asterisk, updatedAsterisk.name_Asterisk, updatedAsterisk.ip_address, originalAsterisk.ip_address);
                updateTLS(asterisk, updatedAsterisk, originalAsterisk);
                updateContext(originalAsterisk.name_Asterisk, updatedAsterisk.name_Asterisk, updatedAsterisk.prefix_Asterisk);
                reloadModules();
                logoff();
                rollbackList.Add(asterisk);
            }
            return textToLog.Append("Zmena Asterisku prebehla úspešne!\n").ToString();
        }
        catch (AuthenticationFailedException)
        {
            rollbackState = true;
            raiseUpdateTrunkErrorEvent("login", currentAsterisk, updatedAsterisk, originalAsterisk, rollbackList, asteriskList);
            return textToLog.Append("Neplatné meno alebo heslo!\n").ToString();
        }
        catch (AsterNET.Manager.TimeoutException)
        {
            rollbackState = true;
            raiseUpdateTrunkErrorEvent("login", currentAsterisk, updatedAsterisk, originalAsterisk, rollbackList, asteriskList);
            return textToLog.Append("Čas spojenia vypršal!\n").ToString();
        }
        catch (ManagerException managerException)
        {
            rollbackState = true;
            raiseUpdateTrunkErrorEvent(managerException.TargetSite.Name, currentAsterisk, updatedAsterisk, originalAsterisk, rollbackList, asteriskList);
            logoff();
            return textToLog.Append("Nastala chyba v:[" + currentAsterisk.name_Asterisk + "]\n" + managerException.Message + "\n").ToString();
        }
        catch (Exception e)
        {
            rollbackState = true;
            raiseUpdateTrunkErrorEvent("unknow", currentAsterisk, updatedAsterisk, originalAsterisk, rollbackList, asteriskList);
            return textToLog.Append("Nastala chyba v:[" + currentAsterisk.name_Asterisk + "]\n" + e.Message + "\n").ToString();
        }
    }
}

