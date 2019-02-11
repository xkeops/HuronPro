using PRO.Members.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace PRO.Members.Helpers
{
    public class DB
    {
        public BufferRegister GetBufferRegisterByPayToken(string token, AppDbContext ctx)
        {
            BufferRegister model = new BufferRegister();

            using (SqlConnection connection = ctx.Database.Connection as SqlConnection)
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "select b.*, m.Title MembershipTitle, m.[Description] MembershipDescription, m.RoleName, m.Price, g.GatewayID, g.GatewayName from BufferRegister b left join Membership m on m.MembershipID = b.MembershipId left join Gateways g on g.IsDefault = 1 where b.TransactionToken = @TransactionToken";
                    command.Parameters.AddWithValue("@TransactionToken", token);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        model.TokenID =(Guid) reader["TokenID"];
                        //contact
                        Contact modelContact = new Contact();
                        modelContact.Email = reader["Email"].ToString();
                        modelContact.FirstName = reader["FirstName"].ToString();
                        modelContact.LastName = reader["LastName"].ToString();
                        modelContact.City = reader["City"].ToString();
                        model.BufferContact = modelContact;

                        Membership modelMmb = new Membership();
                        modelMmb.MembershipID = int.Parse(reader["MembershipId"].ToString());
                        modelMmb.Title = reader["MembershipTitle"].ToString();
                        modelMmb.Description = reader["MembershipDescription"].ToString();
                        modelMmb.RoleName = reader["RoleName"].ToString();
                        var dblPrice = reader["Price"].ToString();
                        decimal dblMPrice = 0;
                        decimal.TryParse(dblPrice, out dblMPrice);
                        modelMmb.Price = dblMPrice;
                        model.BufferMembership = modelMmb;

                        PayTransaction modelTrans = new PayTransaction();
                        modelTrans.Amount = dblMPrice;
                        modelTrans.PayerID = Tools.SafeGetString(reader,"RoleName");
                        modelTrans.TransactionNumber = Tools.SafeGetString(reader,"TransactionNumber");
                        modelTrans.TransactionToken = Tools.SafeGetString(reader,"TransactionToken");
                        model.BufferTransaction = modelTrans;

                        Gateway modelGateway = new Gateway();
                        modelGateway.GatewayId = reader["GatewayID"] as int? ?? default(int);
                        modelGateway.GatewayName = Tools.SafeGetString(reader,"GatewayName");
                        model.BufferGateway = modelGateway;

                    }

                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                }
            }
            return model;
        }




    }
}