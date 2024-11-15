using AptekFarma.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace _AptekFarma.DTO
{
    public class UserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Nif { get; set; }
        public string FechaNacimiento { get; set; }
        public string rol { get; set; }
        public int PharmacyId { get; set; }
        public decimal Points { get; set; }



    }
}
