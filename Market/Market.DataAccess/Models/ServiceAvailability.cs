using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.DataAccess.Models
{
    public enum ServiceAvailability
    {
        FullTime,       // Available during regular business hours
        PartTime,       // Available for limited hours
        Weekends,       // Available on weekends
        Evenings,       // Available during evening hours
        Flexible        // Flexible availability based on client needs
    }
}