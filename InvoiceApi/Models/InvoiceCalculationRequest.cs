using System;

namespace InvoiceApi.Models
{
    public class InvoiceCalculationRequest
    {
        public DateTime InvoiceDate { get; set; }
        public decimal PreTaxAmount { get; set; }
        public string PaymentCurrency { get; set; } = string.Empty;
    }
} 