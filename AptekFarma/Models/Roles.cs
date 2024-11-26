using Microsoft.AspNetCore.Identity;

namespace AptekFarma.Models
{
    public class Roles : IdentityRole
    {
       public string Descripcion { get; set; }
    }
}
