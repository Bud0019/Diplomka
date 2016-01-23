using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace AsteriskRoutingSystem
{
    public class Asterisk
    {
        public int id_Asterisk { get; set; }
        public string name_Asterisk { get; set; }
        public string prefix_Asterisk { get; set; }
        public string ip_address { get; set; }
        public string login_AMI { get; set; }
        public string password_AMI { get; set; }
        public string asterisk_owner { get; set; }
        public int tls_enabled { get; set; }
        public string tls_certDestination { get; set; }
    }

    public class TransferedUser
    {
        public string transferedUser { get; set; }
        public string originalContext { get; set; }
        public string originalAsterisk { get; set; }
        public string currentAsterisk { get; set; }
    }

    public class AsteriskAccessLayer
    {
        private string CS = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;

        public int insertNewUniqueASterisk(Asterisk asterisk)
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

        public int updateAsterisk(Asterisk asterisk)
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

        public void insertTransferedUser(TransferedUser user)
        {
            using (SqlConnection connection = new SqlConnection(CS))
            {
                SqlCommand insertTransferedUserCmd = new SqlCommand("insert into transferedUser(transferedUser, originalContext, originalAsterisk, currentAsterisk)" +
                    "values(@transferedUser, @originalContext, @originalAsterisk, @currentAsterisk)", connection);
                insertTransferedUserCmd.Parameters.AddWithValue("@transferedUser", user.transferedUser);
                insertTransferedUserCmd.Parameters.AddWithValue("@originalContext", user.originalContext);
                insertTransferedUserCmd.Parameters.AddWithValue("@originalAsterisk", user.originalAsterisk);
                insertTransferedUserCmd.Parameters.AddWithValue("@currentAsterisk", user.currentAsterisk);
                SqlDataAdapter sda = new SqlDataAdapter(insertTransferedUserCmd);
                connection.Open();
                insertTransferedUserCmd.ExecuteNonQuery();
            };
        }

        public void updateTransferedUser(string user, string newCurrentAsterisk)
        {
            using (SqlConnection connection = new SqlConnection(CS))
            {
                SqlCommand updateTransferedUserCmd = new SqlCommand("update transferedUser set currentAsterisk = @newCurrentAsterisk where transferedUser = @user", connection);
                updateTransferedUserCmd.Parameters.AddWithValue("@newCurrentAsterisk", newCurrentAsterisk);
                updateTransferedUserCmd.Parameters.AddWithValue("@user", user);
                SqlDataAdapter sda = new SqlDataAdapter(updateTransferedUserCmd);
                connection.Open();
                updateTransferedUserCmd.ExecuteNonQuery();
            };
        }

        public List<TransferedUser> selectTransferedUser(string user)
        {
            List<TransferedUser> list = new List<TransferedUser>();
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(CS))
            {
                SqlCommand selectTransferedUserCmd = new SqlCommand("select * from transferedUser where transferedUser = @user", connection);
                selectTransferedUserCmd.Parameters.AddWithValue("@user", user);
                SqlDataAdapter sda = new SqlDataAdapter(selectTransferedUserCmd);
                connection.Open();
                sda.Fill(ds);
                selectTransferedUserCmd.ExecuteNonQuery();
            };
            DataTable dt = ds.Tables[0];
            foreach (DataRow item in ds.Tables[0].Rows)
            {
                TransferedUser transferedUser = new TransferedUser();
                transferedUser.transferedUser = item["transferedUser"].ToString();
                transferedUser.originalContext = item["originalContext"].ToString();
                transferedUser.originalAsterisk = item["originalAsterisk"].ToString();
                transferedUser.currentAsterisk = item["currentAsterisk"].ToString();
                list.Add(transferedUser);
            }
            return list;
        }

        public void deleteTransferedUser(string user)
        {
            using (SqlConnection connection = new SqlConnection(CS))
            {
                SqlCommand deleteTransferedUserCmd = new SqlCommand("delete from transferedUser where transferedUser = @user", connection);
                deleteTransferedUserCmd.Parameters.AddWithValue("@user", user);
                SqlDataAdapter sda = new SqlDataAdapter(deleteTransferedUserCmd);
                connection.Open();
                deleteTransferedUserCmd.ExecuteNonQuery();
            };
        }


        public List<Asterisk> getAsterisksInList(string userName)
        {
            List<Asterisk> list = new List<Asterisk>();
            DataSet ds = SelectAsterisksByUser(userName);
            DataTable dt = ds.Tables[0];

            foreach (DataRow item in ds.Tables[0].Rows)
            {
                Asterisk asterisk = new Asterisk();
                asterisk.id_Asterisk = int.Parse(item["id_Asterisk"].ToString());
                asterisk.name_Asterisk = item["name_Asterisk"].ToString();
                asterisk.ip_address = item["ip_address"].ToString();
                asterisk.prefix_Asterisk = item["prefix_Asterisk"].ToString();
                asterisk.login_AMI = item["login_AMI"].ToString();
                asterisk.password_AMI = item["password_AMI"].ToString();
                asterisk.tls_enabled = int.Parse(item["tls_enabled"].ToString());
                asterisk.password_AMI = item["tls_certDestination"].ToString();
                list.Add(asterisk);
            }
            return list;
        }          
    }
}