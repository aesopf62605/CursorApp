namespace InvoiceApi.Models
{
    public class InvoiceCalculationResponse
    {
        public decimal PreTaxTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal ExchangeRate { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
} 