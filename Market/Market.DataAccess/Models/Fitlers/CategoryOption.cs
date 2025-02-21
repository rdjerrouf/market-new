using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.DataAccess.Models.Filters
{
    public class CategoryOption
    {
        public string Name { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}