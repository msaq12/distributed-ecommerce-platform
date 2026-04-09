IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [OrderStatuses] (
    [OrderStatusId] int NOT NULL IDENTITY,
    [StatusCode] nvarchar(50) NOT NULL,
    [StatusName] nvarchar(100) NOT NULL,
    [StatusDescription] nvarchar(500) NULL,
    [DisplayOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_OrderStatuses] PRIMARY KEY ([OrderStatusId])
);

CREATE TABLE [Orders] (
    [OrderId] int NOT NULL IDENTITY,
    [OrderNumber] nvarchar(50) NOT NULL,
    [OrderDate] datetime2 NOT NULL,
    [CustomerEmail] nvarchar(255) NOT NULL,
    [CustomerName] nvarchar(255) NOT NULL,
    [ShippingAddressLine1] nvarchar(255) NOT NULL,
    [ShippingAddressLine2] nvarchar(max) NULL,
    [ShippingCity] nvarchar(100) NOT NULL,
    [ShippingState] nvarchar(max) NOT NULL,
    [ShippingZipCode] nvarchar(max) NOT NULL,
    [ShippingCountry] nvarchar(max) NOT NULL,
    [BillingAddressLine1] nvarchar(max) NOT NULL,
    [BillingAddressLine2] nvarchar(max) NULL,
    [BillingCity] nvarchar(max) NOT NULL,
    [BillingState] nvarchar(max) NOT NULL,
    [BillingZipCode] nvarchar(max) NOT NULL,
    [BillingCountry] nvarchar(max) NOT NULL,
    [SubtotalAmount] decimal(18,2) NOT NULL,
    [TaxAmount] decimal(18,2) NOT NULL,
    [ShippingAmount] decimal(18,2) NOT NULL,
    [DiscountAmount] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [OrderStatusId] int NOT NULL,
    [PaymentMethod] nvarchar(max) NULL,
    [PaymentStatus] nvarchar(max) NOT NULL,
    [PaymentTransactionId] nvarchar(max) NULL,
    [FulfillmentStatus] nvarchar(max) NOT NULL,
    [ShippingCarrier] nvarchar(max) NULL,
    [TrackingNumber] nvarchar(max) NULL,
    [ShippedDate] datetime2 NULL,
    [DeliveredDate] datetime2 NULL,
    [CustomerNotes] nvarchar(max) NULL,
    [InternalNotes] nvarchar(max) NULL,
    [CreatedDate] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [ModifiedDate] datetime2 NOT NULL,
    [ModifiedBy] nvarchar(max) NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([OrderId]),
    CONSTRAINT [FK_Orders_OrderStatuses_OrderStatusId] FOREIGN KEY ([OrderStatusId]) REFERENCES [OrderStatuses] ([OrderStatusId]) ON DELETE NO ACTION
);

CREATE TABLE [OrderItems] (
    [OrderItemId] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [ProductSku] nvarchar(50) NOT NULL,
    [ProductName] nvarchar(255) NOT NULL,
    [ProductDescription] nvarchar(max) NULL,
    [VariantSku] nvarchar(max) NULL,
    [VariantName] nvarchar(max) NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [LineTotal] decimal(18,2) NOT NULL,
    [DiscountAmount] decimal(18,2) NOT NULL,
    [FinalLineTotal] decimal(18,2) NOT NULL,
    [FulfillmentStatus] nvarchar(450) NOT NULL,
    [FulfilledQuantity] int NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([OrderItemId]),
    CONSTRAINT [CK_OrderItems_Quantity] CHECK (Quantity > 0),
    CONSTRAINT [CK_OrderItems_UnitPrice] CHECK (UnitPrice >= 0),
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]) ON DELETE CASCADE
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'OrderStatusId', N'CreatedDate', N'DisplayOrder', N'IsActive', N'StatusCode', N'StatusDescription', N'StatusName') AND [object_id] = OBJECT_ID(N'[OrderStatuses]'))
    SET IDENTITY_INSERT [OrderStatuses] ON;
INSERT INTO [OrderStatuses] ([OrderStatusId], [CreatedDate], [DisplayOrder], [IsActive], [StatusCode], [StatusDescription], [StatusName])
VALUES (1, '2024-01-01T00:00:00.0000000Z', 1, CAST(1 AS bit), N'PENDING', N'Order has been created but not yet processed', N'Pending'),
(2, '2024-01-01T00:00:00.0000000Z', 2, CAST(1 AS bit), N'CONFIRMED', N'Order has been confirmed and payment received', N'Confirmed'),
(3, '2024-01-01T00:00:00.0000000Z', 3, CAST(1 AS bit), N'PROCESSING', N'Order is being prepared for shipment', N'Processing'),
(4, '2024-01-01T00:00:00.0000000Z', 4, CAST(1 AS bit), N'SHIPPED', N'Order has been shipped to customer', N'Shipped'),
(5, '2024-01-01T00:00:00.0000000Z', 5, CAST(1 AS bit), N'DELIVERED', N'Order has been delivered to customer', N'Delivered'),
(6, '2024-01-01T00:00:00.0000000Z', 6, CAST(1 AS bit), N'CANCELLED', N'Order has been cancelled', N'Cancelled'),
(7, '2024-01-01T00:00:00.0000000Z', 7, CAST(1 AS bit), N'REFUNDED', N'Order has been refunded', N'Refunded'),
(8, '2024-01-01T00:00:00.0000000Z', 8, CAST(1 AS bit), N'RETURNED', N'Order has been returned by customer', N'Returned');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'OrderStatusId', N'CreatedDate', N'DisplayOrder', N'IsActive', N'StatusCode', N'StatusDescription', N'StatusName') AND [object_id] = OBJECT_ID(N'[OrderStatuses]'))
    SET IDENTITY_INSERT [OrderStatuses] OFF;

CREATE INDEX [IX_OrderItems_FulfillmentStatus] ON [OrderItems] ([FulfillmentStatus]);

CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);

CREATE INDEX [IX_OrderItems_ProductSku] ON [OrderItems] ([ProductSku]);

CREATE INDEX [IX_Orders_CustomerEmail] ON [Orders] ([CustomerEmail]);

CREATE INDEX [IX_Orders_OrderDate] ON [Orders] ([OrderDate]);

CREATE UNIQUE INDEX [IX_Orders_OrderNumber] ON [Orders] ([OrderNumber]);

CREATE INDEX [IX_Orders_OrderStatusId] ON [Orders] ([OrderStatusId]);

CREATE UNIQUE INDEX [IX_OrderStatuses_StatusCode] ON [OrderStatuses] ([StatusCode]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260108142029_InitialCreate', N'10.0.0');

COMMIT;
GO

