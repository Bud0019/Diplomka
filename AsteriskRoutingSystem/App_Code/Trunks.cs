using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AsteriskRoutingSystem
{

    public class Trunks
    {
        public int id_trunk { get; set; }
        public string trunk_name { get; set; }
        public string host_ip { get; set; }
        public string context_name { get; set; }
        public int id_Asterisk { get; set; }
    }

    public class TrunksAccessLayer
    {
        private string CS = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;

        public DataSet SelectTrunksByAsterisk(int id_Asterisk)
        {
            using (SqlConnection connection = new SqlConnection(CS))
            {
                SqlCommand selectTrunkByAsteriskrCmd = new SqlCommand("select * from Trunks where id_Asterisk = @id_Asterisk", connection);
                selectTrunkByAsteriskrCmd.Parameters.AddWithValue("@id_Asterisk", id_Asterisk);
                SqlDataAdapter sda = new SqlDataAdapter(selectTrunkByAsteriskrCmd);
                DataSet ds = new DataSet();
                connection.Open();
                sda.Fill(ds);
                selectTrunkByAsteriskrCmd.ExecuteNonQuery();
                return ds;
            };
        }

        public bool insertUniqueTrunkByAsterisk(Trunks trunk)
        {
            using (SqlConnection connection = new SqlConnection(CS))
            {
                SqlCommand insertCmd = new SqlCommand("insertUniqueTrunkByAsterisk", connection);
                insertCmd.CommandType = CommandType.StoredProcedure;
                insertCmd.Parameters.AddWithValue("@trunk_name", trunk.trunk_name);
                insertCmd.Parameters.AddWithValue("@host_ip", trunk.host_ip);
                insertCmd.Parameters.AddWithValue("@context_name", trunk.context_name);
                insertCmd.Parameters.AddWithValue("@id_Asterisk", trunk.id_Asterisk);
                connection.Open();
                int returnCode = (int)insertCmd.ExecuteScalar();
                return returnCode == 1;
            };
        }

        public void deleteTrunk(int id_trunk)
        {
            using (SqlConnection connection = new SqlConnection(CS))
            {
                SqlCommand deleteCmd = new SqlCommand("delete from Trunks where id_trunk = @id_trunk", connection);

                deleteCmd.Parameters.AddWithValue("@id_trunk", id_trunk);
                connection.Open();
                deleteCmd.ExecuteNonQuery();
            };
        }

        public bool updateTrunk(Trunks trunk)
        {
            using (SqlConnection connection = new SqlConnection(CS))
            {
                SqlCommand updateCmd = new SqlCommand("updateTrunk", connection);
                updateCmd.CommandType = CommandType.StoredProcedure;
                updateCmd.Parameters.AddWithValue("@trunk_name", trunk.trunk_name);
                updateCmd.Parameters.AddWithValue("@host_ip", trunk.host_ip);
                updateCmd.Parameters.AddWithValue("@context_name", trunk.context_name);
                updateCmd.Parameters.AddWithValue("@id_trunk", trunk.id_trunk);
                updateCmd.Parameters.AddWithValue("@id_Asterisk", trunk.id_Asterisk);
                connection.Open();
                int returnCode = (int)updateCmd.ExecuteScalar();
                return returnCode == 1;
            };
        }
    }
}