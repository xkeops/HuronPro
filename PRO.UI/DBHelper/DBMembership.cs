using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using PRO.UI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data;

namespace PRO.UI.DBHelper
{
    public class DBMembership
    {
        private static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["sqlConn"].ConnectionString;
        
        public static List<MembershipModel> getActiveMemberships()
        {
            List<MembershipModel> lstModels = new List<MembershipModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "select * from Membership where IsActive=1";
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        MembershipModel model = new MembershipModel();
                        model.MembershipID = reader["MembershipID"].ToString();
                        model.Title = reader["Title"].ToString();
                        model.Description = reader["Description"].ToString();
                        model.RoleName = reader["RoleName"].ToString();

                        var dblPrice = reader["Price"].ToString();
                        double dblMPrice = 0;
                        double.TryParse(dblPrice, out dblMPrice);
                        model.Price = dblMPrice;
                        lstModels.Add(model);
                    }

                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                }
            }
            return lstModels;
        }

        public static MembershipModel getMembershipById(int mID)
        {
            MembershipModel model = new MembershipModel();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "select * from Membership where MembershipID=@MembershipId";
                    command.Parameters.AddWithValue("@MembershipId", mID);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        model.MembershipID = mID.ToString();
                        model.Title = reader["Title"].ToString();
                        model.Description = reader["Description"].ToString();
                        model.RoleName = reader["RoleName"].ToString();

                        var dblPrice = reader["Price"].ToString();
                        double dblMPrice = 0;
                        double.TryParse(dblPrice, out dblMPrice);
                        model.Price = dblMPrice;

                    }

                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                }
            }
            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static void BufferRegisterContact(ref MembershipContactInfoModel model)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "BufferRegisterSave";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@email", model.Email);
                    command.Parameters.AddWithValue("@firstname", model.FirstName);
                    command.Parameters.AddWithValue("@lastname", model.LastName);
                    command.Parameters.AddWithValue("@city", model.City);
                    command.Parameters.AddWithValue("@MembershipId", model.MembershipId);

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {

                        model.MembershipId = int.Parse( reader["MembershipId"].ToString());
                        model.MembershipTitle = reader["MembershipTitle"].ToString();
                        model.MembershipDescription = reader["MembershipDescription"].ToString();
                        model.RoleName = reader["RoleName"].ToString();
                        var dblPrice = reader["Price"].ToString();
                        double dblMPrice = 0;
                        double.TryParse(dblPrice, out dblMPrice);
                        model.Price = dblMPrice;

                        model.Email = reader["Email"].ToString();
                        model.FirstName = reader["FirstName"].ToString();
                        model.LastName = reader["LastName"].ToString();
                        model.City = reader["City"].ToString();
                        model.TokenID = reader["TokenID"].ToString();
                    }


                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();

                    //return model.TokenID;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        public static void BufferTransfer(MembershipContactInfoModel model)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "BufferTransfer";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Email", model.Email);

                    command.ExecuteNonQuery();

                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                }
            }
        }

        public static void UpdateTransactionToken(MembershipContactInfoModel model)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "update BufferRegister set TransactionToken = @TransactionToken where Email = @Email";
                    command.Parameters.AddWithValue("@Email", model.Email);
                    command.Parameters.AddWithValue("@TransactionToken", model.TransactionToken);
                    command.ExecuteNonQuery();

                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                }
            }
        }

        public static MembershipContactInfoModel GetModelByTransactionToken(string TransactionToken)
        {
            MembershipContactInfoModel model = new MembershipContactInfoModel();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "select b.*, m.Title MembershipTitle, m.[Description] MembershipDescription, m.RoleName, m.Price from BufferRegister b left join Membership m on m.MembershipID = b.MembershipId where b.TransactionToken = @TransactionToken";
                    command.Parameters.AddWithValue("@TransactionToken", TransactionToken);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        model.TokenID = reader["TokenID"].ToString();
                        model.Email = reader["Email"].ToString();
                        model.FirstName = reader["FirstName"].ToString();
                        model.LastName = reader["LastName"].ToString();
                        model.City = reader["City"].ToString();
                        model.TransactionToken = TransactionToken;

                        model.MembershipId = int.Parse(reader["MembershipId"].ToString());
                        model.MembershipTitle = reader["MembershipTitle"].ToString();
                        model.MembershipDescription = reader["MembershipDescription"].ToString();
                        model.RoleName = reader["RoleName"].ToString();
                        var dblPrice = reader["Price"].ToString();
                        double dblMPrice = 0;
                        double.TryParse(dblPrice, out dblMPrice);
                        model.Price = dblMPrice;

                    }

                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                }
            }
            return model;
        }

        public static MembershipContactInfoModel GetModelByEmail(string Email)
        {
            MembershipContactInfoModel model = new MembershipContactInfoModel();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "select b.*, m.Title MembershipTitle, m.[Description] MembershipDescription, m.RoleName, m.Price from BufferRegister b left join Membership m on m.MembershipID = b.MembershipId where b.Email = @Email";
                    command.Parameters.AddWithValue("@Email", Email);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        model.TokenID = reader["TokenID"].ToString();
                        model.Email = reader["Email"].ToString();
                        model.FirstName = reader["FirstName"].ToString();
                        model.LastName = reader["LastName"].ToString();
                        model.City = reader["City"].ToString();

                        model.MembershipId = int.Parse(reader["MembershipId"].ToString());
                        model.MembershipTitle = reader["MembershipTitle"].ToString();
                        model.MembershipDescription = reader["MembershipDescription"].ToString();
                        model.RoleName = reader["RoleName"].ToString();
                        var dblPrice = reader["Price"].ToString();
                        double dblMPrice = 0;
                        double.TryParse(dblPrice, out dblMPrice);
                        model.Price = dblMPrice;

                    }

                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                }
            }
            return model;
        }

        /// <summary>
        /// last call when receiving the payment confirmation
        /// when payment returns from the gateway
        /// if session saved before sending the payment is the same with the payment token then transaction ok
        /// go extract information from the database for that payment token 
        /// </summary>
        /// <param name="TransactionToken"></param>
        /// <returns></returns>
        public static MembershipContactInfoModel CheckTransactionTokenReturn(string TransactionToken)
        {
            MembershipContactInfoModel model = new MembershipContactInfoModel();

            //if (Session["TransactionToken"] !=null && Session["TransactionToken"].ToString().Equals(TransactionToken))
            //{
            //    model = GetModelByTransactionToken(TransactionToken);
            //}

            model = GetModelByTransactionToken(TransactionToken);

            return model;
        }

        /// <summary>
        /// update Buffer table with transaction information
        /// </summary>
        /// <param name="model"></param>
        public static void ValidateTransaction(MembershipContactInfoModel model) {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "update BufferRegister set PayerID = @PayerID, TransactionNumber=@TransactionNumber where Email = @Email";
                    command.Parameters.AddWithValue("@Email", model.Email);
                    command.Parameters.AddWithValue("@PayerID", model.PayerID);
                    command.Parameters.AddWithValue("@TransactionNumber", model.TransactionNumber);
                    command.ExecuteNonQuery();

                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool CreateMember(MembershipContactInfoModel model)
        {
            AuthDbContext context = new AuthDbContext();

            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            //Here we create a Admin super user who will maintain the website                  
            var user = new ApplicationUser();
            user.Email = model.Email;
            user.UserName = user.Email;
            string userPWD = "xyz";
            var chkUser = UserManager.Create(user, userPWD);

            //if (!roleManager.RoleExists(model.RoleName))

            //Add default User to Role Admin   
            if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, model.RoleName);
                }

            return true;
        }

        public static string PriceFormat(double myNumber)
        {
            var s = string.Format("{0:0.00}", myNumber);

            if (s.EndsWith("00"))
            {
                return ((int)myNumber).ToString();
            }
            else
            {
                return s;
            }
        }

    }
}