using System.ComponentModel.DataAnnotations;

namespace Tubes_POS_API.Entities
{
    public class TransactionHistory
    {
        [Key]
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(30)]
        public string Code { get; set; } = string.Empty;

        public int TransactionId { get; set; }

        public DateTime TransactionDate { get; set; }

        public string PaymentMethod { get; set; } = "";

        public decimal TotalAmount { get; set; }
    }
}