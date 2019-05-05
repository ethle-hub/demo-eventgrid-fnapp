using System;
using System.Collections.Generic;
using System.Text;

namespace ChastelFunctionApp.Models
{
    public class PaxRequestsClientBookingModel
    {
        public long DispatchBookingId { get; set; }
        public int DispatchSystemId { get; set; }
        public long UserId { get; set; }
    }
}
