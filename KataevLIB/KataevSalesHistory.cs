using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KataevLIB
{
    [Table("KataevSalesHistory", Schema = "app")]
    public class KataevSalesHistory
    {
        [Key]
        [Column("KataevId")]
        public int Id { get; set; }

        [Column("KataevPartnerId")]
        public int PartnerId { get; set; }

        [Column("KataevProductId")]
        public int ProductId { get; set; }

        [Column("KataevQuantity")]
        public int Quantity { get; set; }

        [Column("KataevSaleDate")]
        public DateTime SaleDate { get; set; }

        [ForeignKey("PartnerId")]
        public virtual KataevPartner KataevPartner { get; set; }

        [ForeignKey("ProductId")]
        public virtual KataevProduct KataevProduct { get; set; }
    }
}