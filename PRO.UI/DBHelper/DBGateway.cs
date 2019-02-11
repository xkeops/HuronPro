using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PRO.UI.Models;
using System.Data.SqlClient;

namespace PRO.UI.DBHelper
{
    public class DBGateway
    {
        private static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["sqlConn"].ConnectionString;

        public static Gateway getDefaultGateway()
        {
            Gateway model = new Gateway();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "select * from Gateways where IsDefault=1";
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        model.GatewayId = int.Parse(reader["GatewayId"].ToString());
                        model.GatewayName = reader["GatewayName"].ToString();
                    }

                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                }
            }
            return model;
        }

    }
}