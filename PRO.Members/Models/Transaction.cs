using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PRO.Members.Models
{
    public class PayTransaction
    {
        public Guid TransactionID { get; set; }

        public string TransactionNumber { get; set; }

        public DateTime TransactionDate { get; set; }

        public string TransactionToken { get; set; }

        public string PayerID { get; set; }

        public decimal Amount { get; set; }

        public decimal GatewayId { get; set; }

        public Guid? ContactId { get; set; }

    }

}