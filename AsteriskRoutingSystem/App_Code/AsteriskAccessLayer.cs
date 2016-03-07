using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Data;

/// <summary>
/// Summary description for AsteriskAccessLayer
/// </summary>
public class AsteriskAccessLayer
{

    private string CS = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;

    public int insertNewUniqueASterisk(Asterisks asterisk)
    {
        using (SqlConnection connection = new SqlConnection(CS))
        {
            SqlCommand insertCmd = new SqlCommand("insertUniqueAsterisk", connection);
            insertCmd.CommandType = System.Data.CommandType.StoredProcedure;
            insertCmd.Parameters.AddWithValue("@name_Asterisk", asterisk.name_Asterisk);
            insertCmd.Parameters.AddWithValue("@prefix_Asterisk", asterisk.prefix_Asterisk);
            insertCmd.Parameters.AddWithValue("@ip_address", asterisk.ip_address);
            insertCmd.Parameters.AddWithValue("@login_AMI", asterisk.login_AMI);
            insertCmd.Parameters.AddWithValue("@password_AMI", asterisk.password_AMI);
            insertCmd.Parameters.AddWithValue("@asterisk_owner", asterisk.asterisk_owner);
            insertCmd.Parameters.AddWithValue("@tls_enabled", asterisk.tls_enabled);
            insertCmd.Parameters.AddWithValue("@tls_certDestination", asterisk.tls_certDestination);
            connection.Open();
            int returnCode = (int)insertCmd.ExecuteScalar();
            return returnCode;
        };
    }

    public int updateAsterisk(Asterisks asterisk)
    {
        using (SqlConnection connection = new SqlConnection(CS))
        {
            SqlCommand updateCmd = new SqlCommand("updateAsterisk", connection);
            updateCmd.CommandType = CommandType.StoredProcedure;
            updateCmd.Parameters.AddWithValue("@id_Asterisk", asterisk.id_Asterisk);
            updateCmd.Parameters.AddWithValue("@name_Asterisk", asterisk.name_Asterisk);
            updateCmd.Parameters.AddWithValue("@prefix_Asterisk", asterisk.prefix_Asterisk);
            updateCmd.Parameters.AddWithValue("@ip_address", asterisk.ip_address);
            updateCmd.Parameters.AddWithValue("@login_AMI", asterisk.login_AMI);
            updateCmd.Parameters.AddWithValue("@password_AMI", asterisk.password_AMI);
            updateCmd.Parameters.AddWithValue("@tls_enabled", asterisk.tls_enabled);
            updateCmd.Parameters.AddWithValue("@tls_certDestination", asterisk.tls_certDestination);
            connection.Open();
            string returnCode = (string)updateCmd.ExecuteScalar();

            if (returnCode.Contains(asterisk.name_Asterisk))
                return 1;
            else if (returnCode.Contains(asterisk.ip_address))
                return 2;
            else if (returnCode.Contains(asterisk.prefix_Asterisk))
                return 3;
            else
                return -1;
        };
    }

    public DataSet SelectAsterisksByUser(string userName)
    {
        using (SqlConnection connection = new SqlConnection(CS))
        {
            SqlCommand selectAsteriskByUserCmd = new SqlCommand("select * from Asterisks where asterisk_owner = (select UserId from dbo.aspnet_Users where UserName = @userName)", connection);
            selectAsteriskByUserCmd.Parameters.AddWithValue("@userName", userName);
            SqlDataAdapter sda = new SqlDataAdapter(selectAsteriskByUserCmd);
            DataSet ds = new DataSet();
            connection.Open();
            sda.Fill(ds);
            selectAsteriskByUserCmd.ExecuteNonQuery();
            return ds;
        };
    }

    public Asterisks SelectAsterisksByName(string asteriskName)
    {
        using (SqlConnection connection = new SqlConnection(CS))
        {
            SqlCommand selectAsteriskByUserCmd = new SqlCommand("select * from Asterisks where name_Asterisk = @asteriskName", connection);
            selectAsteriskByUserCmd.Parameters.AddWithValue("@asteriskName", asteriskName);
            SqlDataAdapter sda = new SqlDataAdapter(selectAsteriskByUserCmd);
            DataSet ds = new DataSet();
            connection.Open();
            sda.Fill(ds);
            selectAsteriskByUserCmd.ExecuteNonQuery();
            DataTable dt = ds.Tables[0];
            Asterisks asterisk = new Asterisks();
            foreach (DataRow item in ds.Tables[0].Rows)
            {               
                asterisk.id_Asterisk = int.Parse(item["id_Asterisk"].ToString());
                asterisk.name_Asterisk = item["name_Asterisk"].ToString();
                asterisk.ip_address = item["ip_address"].ToString();
                asterisk.prefix_Asterisk = item["prefix_Asterisk"].ToString();
                asterisk.login_AMI = item["login_AMI"].ToString();
                asterisk.password_AMI = item["password_AMI"].ToString();
                asterisk.tls_enabled = int.Parse(item["tls_enabled"].ToString());
                asterisk.tls_certDestination = item["tls_certDestination"].ToString();
            }
            return asterisk;
        };
    }

    public void deleteAsterisk(int id_Asterisk)
    {
        using (SqlConnection connection = new SqlConnection(CS))
        {
            SqlCommand deleteAsteriskCmd = new SqlCommand("delete from Asterisks where id_Asterisk = @id_Asterisk", connection);
            deleteAsteriskCmd.Parameters.AddWithValue("@id_Asterisk", id_Asterisk);
            SqlDataAdapter sda = new SqlDataAdapter(deleteAsteriskCmd);
            connection.Open();
            deleteAsteriskCmd.ExecuteNonQuery();
        };
    }

    public void deleteAsteriskByName(string name_Asterisk)
    {
        using (SqlConnection connection = new SqlConnection(CS))
        {
            SqlCommand deleteAsteriskCmd = new SqlCommand("delete from Asterisks where name_Asterisk = @name_Asterisk", connection);
            deleteAsteriskCmd.Parameters.AddWithValue("@name_Asterisk", name_Asterisk);
            SqlDataAdapter sda = new SqlDataAdapter(deleteAsteriskCmd);
            connection.Open();
            deleteAsteriskCmd.ExecuteNonQuery();
        };
    }
    public List<Asterisks> getAsterisksInList(string userName)
    {
        List<Asterisks> list = new List<Asterisks>();
        DataSet ds = SelectAsterisksByUser(userName);
        DataTable dt = ds.Tables[0];

        foreach (DataRow item in ds.Tables[0].Rows)
        {
            Asterisks asterisk = new Asterisks();
            asterisk.id_Asterisk = int.Parse(item["id_Asterisk"].ToString());
            asterisk.name_Asterisk = item["name_Asterisk"].ToString();
            asterisk.ip_address = item["ip_address"].ToString();
            asterisk.prefix_Asterisk = item["prefix_Asterisk"].ToString();
            asterisk.login_AMI = item["login_AMI"].ToString();
            asterisk.password_AMI = item["password_AMI"].ToString();
            asterisk.tls_enabled = int.Parse(item["tls_enabled"].ToString());
            asterisk.tls_certDestination = item["tls_certDestination"].ToString();
            list.Add(asterisk);
        }
        return list;
    }
}
