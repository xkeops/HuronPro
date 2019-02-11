using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PRO.Members.Models
{
    public class Membership
    {
        public Int64 MembershipID { get; set; }
        public string Title { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
    }

}