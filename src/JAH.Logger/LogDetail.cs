using System;
using System.Collections.Generic;

namespace JAH.Logger
{
    public class LogDetail
    {
        public LogDetail()
        {
            TimeStamp = DateTime.Now;
            AdditionalInfo = new Dictionary<string, object>();
        }

        public DateTime TimeStamp { get; }

        public string Message { get; set; }

        // Where
        public string Product { get; set; }

        public string Layer { get; set; }

        public string Location { get; set; }

        public string Hostname { get; set; }

        // Who
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        // Everything Else
        public string CorrelationId { get; set; } // exception shielding from server to client

        public long? ElapsedMilliseconds { get; set; } // only for performance entries

        public Exception Exception { get; set; } // the exception for error logging

        public Dictionary<string, object> AdditionalInfo { get; } // catch-all for anything else
    }
}
