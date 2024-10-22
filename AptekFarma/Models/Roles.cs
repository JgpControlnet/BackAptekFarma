using Microsoft.AspNetCore.Identity;

namespace _AptekFarma.Models
{
    public class Roles : IdentityRole
    {
       public string Descripcion { get; set; }
    }
}
