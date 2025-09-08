using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities
{
    public class ProductCategory
    {
        public int CategoryId { get; set; }

        public int ProductId {  get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property
        public Category Category { get; set; }

        public Product Product { get; set; }
    }
}
