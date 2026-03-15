using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KataevLIB
{
    [Table("KataevProducts", Schema = "app")]
    public class KataevProduct
    {
        [Key]
        [Column("KataevId")]
        public int Id { get; set; }

        [Column("KataevName")]
        public string Name { get; set; }

        [Column("KataevArticle")]
        public string Article { get; set; }

        [Column("KataevType")]
        public string Type { get; set; }

        public virtual ICollection<KataevSalesHistory> KataevSalesHistories { get; set; }
    }
}