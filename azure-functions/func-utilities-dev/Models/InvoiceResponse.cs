namespace func_utilities_dev.Models;

public class InvoiceResponse
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public InvoiceOrderSection Order { get; set; } = new();
    public InvoiceAddressSection BillTo { get; set; } = new();
    public InvoiceAddressSection ShipTo { get; set; } = new();
    public List<InvoiceLineItem> LineItems { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Shipping { get; set; }
    public decimal Total { get; set; }
    public string Footer { get; set; } = string.Empty;
}

public class InvoiceOrderSection
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class InvoiceAddressSection
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class InvoiceLineItem
{
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}
