using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace PRO.UI.Models
{
    public class MembershipContactInfoModel
    {
        public string TokenID { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(250, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [Display(Name = "FirstName")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(250, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [Display(Name = "LastName")]
        public string LastName { get; set; }

        [Required]
        [StringLength(250, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
        [Display(Name = "City")]
        public string City { get; set; }

        //membership
        public int MembershipId { get; set; }
        public double Price { get; set; }
        public string MembershipTitle { get; set; }
        public string MembershipDescription { get; set; }
        public string RoleName { get; set; }

        //payment
        public string TransactionToken { get; set; }
        //paypal
        public string PayerID { get; set; }
        //paypal
        public string TransactionNumber { get; set; }


        //gateway
        public int GatewayId { get; set; }
        public string GatewayName{ get; set; }

        public bool isContactValid() {
            if (string.IsNullOrEmpty(City) || string.IsNullOrEmpty(FirstName) ||
                string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(Email))
                return false;
            else
                return true;
        }

   }

    public class MembershipModel {
        public string MembershipID { get; set; }
        public string Title { get; set; }

        public string Description{ get; set; }

        public double Price { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
    }

    //public class MembershipContactInfoOperations
    //{
    //    public string connectionString = "Initial Catalog=TSTDB;Data Source=danlaptop;Password=xyz;Persist Security Info=True;User ID=usrone;";
    //    public void Create(MembershipContactInfoModel model)
    //    {
    //        using (SqlConnection connection = new SqlConnection(connectionString))
    //        {
    //            if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
    //            using (SqlCommand command = connection.CreateCommand())
    //            {
    //                command.CommandText = "insert into BufferContacts (Email, FirstName, LastName, City) values(@email,@firstname,@lastname,@city)";
    //                command.Parameters.AddWithValue("@email", model.Email);
    //                command.Parameters.AddWithValue("@firstname", model.FirstName);
    //                command.Parameters.AddWithValue("@lastname", model.LastName);
    //                command.Parameters.AddWithValue("@city", model.City);

    //                int newid = (int)command.ExecuteScalar();

    //                if (connection.State == System.Data.ConnectionState.Open)
    //                    connection.Close();

    //            }
    //        }
    //    }

    //    public MembershipContactInfoModel Find(int id)
    //    {
    //        var model = new MembershipContactInfoModel();
    //        using (SqlConnection connection = new SqlConnection(connectionString))
    //        {
    //            if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
    //            using (SqlCommand command = connection.CreateCommand())
    //            {
    //                command.CommandText = "select * from BufferContacts where ProductId=@Id";
    //                command.Parameters.AddWithValue("@Id", id);
    //                SqlDataReader reader = command.ExecuteReader();
    //                while (reader.Read())
    //                {
    //                    //model.ProductID = id;
    //                    model.Email  = reader["ProductName"].ToString();
    //                    model.FirstName= reader["ProductDescription"].ToString();
    //                    double amt = 0;
    //                    double.TryParse(reader["ProductPrice"].ToString(), out amt);
    //                    //model.ProductPrice = amt;
    //                }
    //            }
    //        }
    //        return model;
    //    }

    //    public void Delete(MembershipContactInfoModel model)
    //    {
    //        using (SqlConnection connection = new SqlConnection(connectionString))
    //        {
    //            if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
    //            using (SqlCommand command = connection.CreateCommand())
    //            {
    //                command.CommandText = "delete from BufferContacts where ProductId=@Id";
    //                command.Parameters.AddWithValue("@Id", model.Email);
    //                command.ExecuteNonQuery();
    //            }
    //        }
    //    }

    //    public void Delete(int id)
    //    {
    //        using (SqlConnection connection = new SqlConnection(connectionString))
    //        {
    //            if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
    //            using (SqlCommand command = connection.CreateCommand())
    //            {
    //                command.CommandText = "delete from BufferContacts where ProductId=@Id";
    //                command.Parameters.AddWithValue("@Id", id);
    //                command.ExecuteNonQuery();
    //            }
    //        }
    //    }

    //    public void Edit(MembershipContactInfoModel model)
    //    {
    //        using (SqlConnection connection = new SqlConnection(connectionString))
    //        {
    //            if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
    //            using (SqlCommand command = connection.CreateCommand())
    //            {
    //                command.CommandText = "Update BufferContacts set ProductName=@ProductName, ProductDescription=@ProductDescription,ProductPrice=@ProductPrice where ProductId=@Id";
    //                //command.Parameters.AddWithValue("@ProductName", model.ProductName);
    //                //command.Parameters.AddWithValue("@ProductDescription", model.ProductDescription);
    //                //command.Parameters.AddWithValue("@ProductPrice", model.ProductPrice);
    //                //command.Parameters.AddWithValue("@Id", model.ProductID);
    //                command.ExecuteNonQuery();
    //            }
    //        }
    //    }

    //    public List<MembershipContactInfoModel> LoadProducts()
    //    {
    //        var models = new List<MembershipContactInfoModel>();
    //        using (SqlConnection connection = new SqlConnection(connectionString))
    //        {
    //            if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }
    //            using (SqlCommand command = connection.CreateCommand())
    //            {
    //                command.CommandText = "select * from BufferContacts";
    //                SqlDataReader reader = command.ExecuteReader();
    //                while (reader.Read())
    //                {
    //                    MembershipContactInfoModel model = new MembershipContactInfoModel();
    //                    int prodID = 0;
    //                    int.TryParse(reader["ProductId"].ToString(), out prodID);
    //                    //model.ProductID = prodID;
    //                    model.Email  = reader["ProductName"].ToString();
    //                    model.FirstName = reader["ProductDescription"].ToString();
    //                    double amt = 0;
    //                    double.TryParse(reader["ProductPrice"].ToString(), out amt);
    //                    //model.ProductPrice = amt;
    //                    models.Add(model);
    //                }
    //            }

    //        }
    //        return models;
    //    }


    //}

}