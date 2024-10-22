using Microsoft.AspNetCore.Identity;

namespace _AptekFarma.Models
{
    public class User : IdentityUser
    {

        public string? nombre { get; set; }
        public string? apellidos { get; set; }
        public string? nif { get; set; }
        public string? fecha_nacimiento { get; set; }
    }
}
