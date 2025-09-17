    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace ECommerce.Domain.Entities
    {
        public class Wishlist
        {
            public int Id { get; set; } 
            public int UserId { get; set; }

            public int ProductId {  get; set; }

            public DateTime CreatedAt { get; set; }

            // Navigation property

            public Product Product { get; set; }

            //user navigation add soon

        }
    }
