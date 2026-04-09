using Microsoft.EntityFrameworkCore;
using OrderService.Data.Entities;
using Microsoft.Extensions.Configuration;

namespace OrderService.Data.Data;

/// <summary>
/// Database context for OrdersDB
/// This is your connection to the database
/// Think of it as a "session" with the database
/// </summary>
public class OrdersDbContext : DbContext
{
    // ===========================
    // Constructor
    // ===========================

    /// <summary>
    /// Constructor - accepts configuration options
    /// Options include connection string, provider, etc.
    /// </summary>
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    // ===========================
    // Constructor
    // ===========================

    /// <summary>
    /// Constructor - No arguments 
    /// </summary>
    public OrdersDbContext() : base()
    {

    }

    // Configure the database provider for design-time
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Build configuration to read from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("OrdersDb");


            // For SQL Server:
            optionsBuilder.UseSqlServer(connectionString);

            // OR for PostgreSQL:
            // optionsBuilder.UseNpgsql("Host=localhost;Database=OrdersDb;Username=postgres;Password=yourpassword");

            // OR for SQLite (for testing):
            // optionsBuilder.UseSqlite("Data Source=orders.db");
        }
    }

    // ===========================
    // DbSets (Tables)
    // ===========================

    /// <summary>
    /// Orders table
    /// Use this for LINQ queries: context.Orders.Where(...)
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// OrderItems table
    /// </summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>
    /// OrderStatuses table
    /// </summary>
    public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();

    // ===========================
    // Model Configuration (Fluent API)
    // ===========================

    /// <summary>
    /// Configure how entities map to database tables
    /// This is called when EF Core builds the model
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureOrderStatus(modelBuilder);
        ConfigureOrder(modelBuilder);
        ConfigureOrderItem(modelBuilder);
    }

    /// <summary>
    /// Configure OrderStatus entity
    /// </summary>
    private void ConfigureOrderStatus(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderStatus>(entity =>
        {
            // Table name
            entity.ToTable("OrderStatuses");

            // Primary key
            entity.HasKey(e => e.OrderStatusId);

            // Properties
            entity.Property(e => e.StatusCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.StatusName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.StatusDescription)
                .HasMaxLength(500);

            // Unique constraint on StatusCode
            entity.HasIndex(e => e.StatusCode)
                .IsUnique();

            // Seed data (initial values)
            entity.HasData(
                new OrderStatus
                {
                    OrderStatusId = 1,
                    StatusCode = "PENDING",
                    StatusName = "Pending",
                    StatusDescription = "Order has been created but not yet processed",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new OrderStatus
                {
                    OrderStatusId = 2,
                    StatusCode = "CONFIRMED",
                    StatusName = "Confirmed",
                    StatusDescription = "Order has been confirmed and payment received",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new OrderStatus
                {
                    OrderStatusId = 3,
                    StatusCode = "PROCESSING",
                    StatusName = "Processing",
                    StatusDescription = "Order is being prepared for shipment",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new OrderStatus
                {
                    OrderStatusId = 4,
                    StatusCode = "SHIPPED",
                    StatusName = "Shipped",
                    StatusDescription = "Order has been shipped to customer",
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new OrderStatus
                {
                    OrderStatusId = 5,
                    StatusCode = "DELIVERED",
                    StatusName = "Delivered",
                    StatusDescription = "Order has been delivered to customer",
                    DisplayOrder = 5,
                    IsActive = true,
                    CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new OrderStatus
                {
                    OrderStatusId = 6,
                    StatusCode = "CANCELLED",
                    StatusName = "Cancelled",
                    StatusDescription = "Order has been cancelled",
                    DisplayOrder = 6,
                    IsActive = true,
                    CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new OrderStatus
                {
                    OrderStatusId = 7,
                    StatusCode = "REFUNDED",
                    StatusName = "Refunded",
                    StatusDescription = "Order has been refunded",
                    DisplayOrder = 7,
                    IsActive = true,
                    CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new OrderStatus
                {
                    OrderStatusId = 8,
                    StatusCode = "RETURNED",
                    StatusName = "Returned",
                    StatusDescription = "Order has been returned by customer",
                    DisplayOrder = 8,
                    IsActive = true,
                    CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        });
    }

    /// <summary>
    /// Configure Order entity
    /// </summary>
    private void ConfigureOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            // Table name
            entity.ToTable("Orders");

            // Primary key
            entity.HasKey(e => e.OrderId);

            // Properties with constraints
            entity.Property(e => e.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.CustomerEmail)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(255);

            // Address properties
            entity.Property(e => e.ShippingAddressLine1)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.ShippingCity)
                .IsRequired()
                .HasMaxLength(100);

            // Decimal properties (specify precision)
            entity.Property(e => e.SubtotalAmount)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.TaxAmount)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.ShippingAmount)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // Indexes
            entity.HasIndex(e => e.OrderNumber)
                .IsUnique();

            entity.HasIndex(e => e.CustomerEmail);

            entity.HasIndex(e => e.OrderDate);

            entity.HasIndex(e => e.OrderStatusId);

            // Relationships
            entity.HasOne(e => e.OrderStatus)
                .WithMany(s => s.Orders)
                .HasForeignKey(e => e.OrderStatusId)
                .OnDelete(DeleteBehavior.Restrict);  // Don't delete orders if status is deleted

            entity.HasMany(e => e.OrderItems)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);  // Delete items when order is deleted

            // Global query filter (soft delete)
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Ignore calculated properties (not in database)
            entity.Ignore(e => e.ShippingAddressFull);
            entity.Ignore(e => e.ItemCount);
            entity.Ignore(e => e.TotalQuantity);
        });
    }

    /// <summary>
    /// Configure OrderItem entity
    /// </summary>
    private void ConfigureOrderItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderItem>(entity =>
        {
            // Table name
            entity.ToTable("OrderItems");

            // Primary key
            entity.HasKey(e => e.OrderItemId);

            // Properties
            entity.Property(e => e.ProductSku)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(255);

            // Decimal properties
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.LineTotal)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.FinalLineTotal)
                .HasColumnType("decimal(18,2)");

            // Indexes
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ProductSku);
            entity.HasIndex(e => e.FulfillmentStatus);

            // Check constraints (data validation at database level)
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_OrderItems_Quantity",
                "Quantity > 0"
            ));

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_OrderItems_UnitPrice",
                "UnitPrice >= 0"
            ));

            // Global query filter (soft delete)
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Ignore calculated properties
            entity.Ignore(e => e.IsFullyFulfilled);
            entity.Ignore(e => e.RemainingQuantity);
            entity.Ignore(e => e.DiscountPercentage);
        });
    }
}