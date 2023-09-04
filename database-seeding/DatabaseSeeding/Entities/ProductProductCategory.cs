namespace DatabaseSeeding.Entities;

public class ProductProductCategory
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }

    public Product Product { get; set; } = null!;
    public ProductCategory Category { get; set; } = null!;
}
