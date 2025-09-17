namespace ECommerce.Domain.Entities
{
    //public class Category
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //    public string Description { get; set; }
    //    public string ImageUrl { get; set; }
    //    public bool IsActive { get; set; }
    //    public DateTime CreatedAt { get; set; }
    //    public DateTime UpdatedAt { get; set; }

    //    // Navigation
    //    //public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    //    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    //}
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } // optional
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Self-referencing
        public int? ParentCategoryId { get; set; }
        public Category ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        // Products (Many-to-Many via ProductCategory)
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    }


}
