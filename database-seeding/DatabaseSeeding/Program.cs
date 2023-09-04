using DatabaseSeeding;
using DatabaseSeeding.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

const bool generateData = true;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<BogusContext>(x =>
        {
            x.UseNpgsql(hostContext.Configuration.GetConnectionString("Database"));
        });

    })
    .Build();

if (generateData)
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json");
            
    var config = configuration.Build();
    var connectionString = config.GetConnectionString("Database");
    
    var optionsBuilder = new DbContextOptionsBuilder<BogusContext>();
    optionsBuilder.UseNpgsql(connectionString);

    const int numberOfProductsToGenerate = 10000;
    const int numberOfProductCategoriesToGenerate = 10000;

    var productsAdded = 0;
        
    // while (true)
    // {
        var bogusContext = new BogusContext(optionsBuilder.Options);
        
        var products = DatabaseSeeder.GenerateProducts(numberOfProductsToGenerate);
        bogusContext.Products.AddRange(products);
        
        var categories = DatabaseSeeder.GenerateProductCategories(numberOfProductCategoriesToGenerate);
        bogusContext.ProductCategories.AddRange(categories);
        
        bogusContext.SaveChanges();
        
        var productCategories =
            DatabaseSeeder.GenerateProductProductCategories(
                numberOfProductsToGenerate,
                products,
                categories
            );
        
        bogusContext.ProductProductCategories.AddRange(productCategories);
        bogusContext.SaveChanges();

        Console.WriteLine(numberOfProductsToGenerate + " products added.");
        Console.WriteLine(numberOfProductCategoriesToGenerate + " categories added.");

        productsAdded += numberOfProductsToGenerate;
        
        Console.WriteLine(productsAdded + " total products added.");
    // }
}

