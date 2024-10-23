using _AptekFarma.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.Models
{
    public class Point
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }
        public int Points { get; set; }
        public DateTime Fecha { get; set; }

    }
}
