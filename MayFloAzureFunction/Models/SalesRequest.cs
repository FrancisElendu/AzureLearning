using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MayFloAzureFunction.Models
{
    public class SalesRequest
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
        [DataType(DataType.Date)]
        public DateTime? CreatedDate { get; set; }
    }
}
