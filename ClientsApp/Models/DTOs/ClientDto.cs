namespace ClientsApp.Models.DTOs
{
    public class ClientDto
    {
        public int ClientId { get; set; }  // Client identifier
        public string Name { get; set; }   // Client name
        public string Address { get; set; } // Client address
        public string Email { get; set; }   // Client email
        public string Phone { get; set; }   // Client phone
    }
}
