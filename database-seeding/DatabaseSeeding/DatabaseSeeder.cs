using Bogus;
using DatabaseSeeding.Entities;

namespace DatabaseSeeding;

public class DatabaseSeeder
    {
        public IReadOnlyCollection<Product> Products { get; } = new List<Product>();
        public IReadOnlyCollection<ProductCategory> ProductCategories { get; } = new List<ProductCategory>();
        public IReadOnlyCollection<ProductProductCategory> ProductProductCategories { get; } = new List<ProductProductCategory>();
        
        public static IReadOnlyCollection<Product> GenerateProducts(int amount)
        {
            // var productId = 1;
            var productFaker = new Faker<Product>()
                .RuleFor(x => x.Name, f => f.Commerce.ProductName())
                .RuleFor(x => x.CreationDate, f => f.Date.FutureOffset(
                    refDate: new DateTimeOffset(1999, 12, 31, 23, 59, 59, TimeSpan.FromHours(0))))
                .RuleFor(x => x.Description, f => f.Commerce.ProductDescription());

            var products = Enumerable.Range(1, amount)
                .Select(i => SeedRow(productFaker, i))
                .ToList();

            return products;
        }

        public static IReadOnlyCollection<ProductCategory> GenerateProductCategories(int amount)
        {
            var categoryFaker = new Faker<ProductCategory>()
                .RuleFor(x => x.Name, f => f.Commerce.Categories(1).First());

            var categories = Enumerable.Range(1, amount)
                .Select(i => SeedRow(categoryFaker, i))
                .ToList();

            return categories;
        }

        public static IReadOnlyCollection<ProductProductCategory> GenerateProductProductCategories(
            int amount,
            IEnumerable<Product> products,
            IEnumerable<ProductCategory> productCategories)
        {
            var productProductCategoryFaker = new Faker<ProductProductCategory>()
                .RuleFor(x => x.ProductId, f => f.PickRandom(products).Id)
                .RuleFor(x => x.CategoryId, f => f.PickRandom(productCategories).Id);

            var productProductCategories = Enumerable.Range(1, amount)
                .Select(i => SeedRow(productProductCategoryFaker, i))
                .GroupBy(x => new { x.ProductId, x.CategoryId })
                .Select(x => x.First())
                .ToList();

            return productProductCategories;
        }

        private static T SeedRow<T>(Faker<T> faker, int rowId) where T : class
        {
            var recordRow = faker.UseSeed(rowId).Generate();
            return recordRow;
        }
    }