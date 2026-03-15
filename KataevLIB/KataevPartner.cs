using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KataevLIB
{
    [Table("KataevPartners", Schema = "app")]
    public class KataevPartner
    {
        [Key]
        [Column("KataevId")] 
        public int Id { get; set; }

        [Column("KataevName")]
        public string Name { get; set; }

        [Column("KataevPartnerType")]
        public string PartnerType { get; set; }

        [Column("KataevRating")]
        public int Rating { get; set; }

        [Column("KataevAddress")]
        public string Address { get; set; }

        [Column("KataevINN")]
        public string INN { get; set; }

        [Column("KataevDirectorLastName")]
        public string DirectorLastName { get; set; }

        [Column("KataevDirectorFirstName")]
        public string DirectorFirstName { get; set; }

        [Column("KataevDirectorMiddleName")]
        public string DirectorMiddleName { get; set; }

        [Column("KataevPhone")]
        public string Phone { get; set; }

        [Column("KataevEmail")]
        public string Email { get; set; }

        public virtual ICollection<KataevSalesHistory> KataevSalesHistories { get; set; }
    }
}