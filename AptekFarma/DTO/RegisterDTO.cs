namespace _AptekFarma.DTO
{
    public class RegisterDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Nif { get; set; }

        public string rol {  get; set; }
        public DateTime FechaNacimiento { get; set; }
    }
}
