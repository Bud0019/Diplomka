using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

/// <summary>
/// Summary description for TransferedUserAccessLayer
/// </summary>
    public class TransferedUserAccessLayer
    {

        private string CS = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;

        public void insertTransferedUser(TransferedUser transferedUser)
        {
            using (SqlConnection connection = new SqlConnection(CS))
            {
                SqlCommand insertTransferedUserCmd = new SqlCommand("insert into transferedUser(name_user, original_context, original_asterisk, current_asterisk)" +
                    "values(@name_user, @original_context, @original_asterisk, @current_asterisk)", connection);
                insertTransferedUserCmd.Parameters.AddWithValue("@name_user", transferedUser.name_user);
                insertTransferedUserCmd.Parameters.AddWithValue("@original_context", transferedUser.original_context);
                insertTransferedUserCmd.Parameters.AddWithValue("@original_asterisk", transferedUser.original_asterisk);
                insertTransferedUserCmd.Parameters.AddWithValue("@current_asterisk", transferedUser.current_asterisk);
                SqlDataAdapter sda = new SqlDataAdapter(insertTransferedUserCmd);
                connection.Open();
                insertTransferedUserCmd.ExecuteNonQuery();
            };
        }

        public void updateTransferedUser(string name_user, string newCurrentAsterisk)
        {
            using (SqlConnection connection = new SqlConnection(CS))
            {
                SqlCommand updateTransferedUserCmd = new SqlCommand("update transferedUser set current_asterisk = @newCurrentAsterisk where name_user = @name_user", connection);
                updateTransferedUserCmd.Parameters.AddWithValue("@newCurrentAsterisk", newCurrentAsterisk);
                updateTransferedUserCmd.Parameters.AddWithValue("@name_user",name_user);
                SqlDataAdapter sda = new SqlDataAdapter(updateTransferedUserCmd);
                connection.Open();
                updateTransferedUserCmd.ExecuteNonQuery();
            };
        }

        public TransferedUser selectTransferedUser(string name_user)
        {
            TransferedUser transferedUser = new TransferedUser();
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(CS))
            {
                SqlCommand selectTransferedUserCmd = new SqlCommand("select * from transferedUser where name_user = @name_user", connection);
                selectTransferedUserCmd.Parameters.AddWithValue("@name_user", name_user);
                SqlDataAdapter sda = new SqlDataAdapter(selectTransferedUserCmd);
                connection.Open();
                sda.Fill(ds);
                selectTransferedUserCmd.ExecuteNonQuery();
            };
            DataTable dt = ds.Tables[0];
            foreach (DataRow item in ds.Tables[0].Rows)
            {               
                transferedUser.name_user = item["name_user"].ToString();
                transferedUser.original_context = item["original_context"].ToString();
                transferedUser.original_asterisk = item["original_asterisk"].ToString();
                transferedUser.current_asterisk = item["current_asterisk"].ToString();
                return transferedUser;
            }
            return null;
        }

        public void deleteTransferedUser(string name_user)
        {
            using (SqlConnection connection = new SqlConnection(CS))
            {
                SqlCommand deleteTransferedUserCmd = new SqlCommand("delete from transferedUser where name_user = @name_user", connection);
                deleteTransferedUserCmd.Parameters.AddWithValue("@name_user", name_user);
                SqlDataAdapter sda = new SqlDataAdapter(deleteTransferedUserCmd);
                connection.Open();
                deleteTransferedUserCmd.ExecuteNonQuery();
            };
        }
    }