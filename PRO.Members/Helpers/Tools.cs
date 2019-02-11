using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PRO.Members.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace PRO.Members.Helpers
{
    public class Tools
    {

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
        public static string PriceFormat(decimal myNumber)
        {
            var s = string.Format("{0:0.00}", myNumber);

            return s;    
            //code to return 25 in case 25.00
            //if (s.EndsWith("00"))
            //{
            //    return ((int)myNumber).ToString();
            //}
            //else
            //{
            //    return s;
            //}
        }

        //handling null values in datareader
        public static string SafeGetString( SqlDataReader reader, string colName)
        {
            int colIndex = reader.GetOrdinal(colName);
            //GetDataTypeName
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            return string.Empty;
        }
    }
}