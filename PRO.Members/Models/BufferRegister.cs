using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using PRO.Members.Models;

namespace PRO.Members.Models
{

    public class BufferRegister
    {
        public Guid TokenID { get; set; }

        public Contact BufferContact { get; set; }

        //membership
        public Membership BufferMembership { get; set; }
        public PayTransaction BufferTransaction { get; set; }
        public Gateway BufferGateway { get; set; }

        public bool isBufferValid()
        {
            if (string.IsNullOrEmpty(BufferContact.City) || string.IsNullOrEmpty(BufferContact.FirstName) ||
                string.IsNullOrEmpty(BufferContact.LastName) || string.IsNullOrEmpty(BufferContact.Email))
                return false;
            else
                return true;
        }

    }
}