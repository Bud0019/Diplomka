using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;


    public class Asterisks
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
