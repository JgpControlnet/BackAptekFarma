using AptekFarma.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace _AptekFarma.Models
{
    public class User : IdentityUser
    {

        public string? nombre { get; set; }
        public string? apellidos { get; set; }
        public string? nif { get; set; }
        public string? fecha_nacimiento { get; set; }
        [ForeignKey("PharmacyID")]
        public Pharmacy Pharmacy { get; set; }
        public int Points { get; set; }
        public bool RememberMe { get; set; }
    }
}
