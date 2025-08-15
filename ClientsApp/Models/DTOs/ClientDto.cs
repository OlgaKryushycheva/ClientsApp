namespace ClientsApp.Models.DTOs
{
    public class ClientDto
    {
        public int ClientId { get; set; }  // Идентификатор клиента
        public string Name { get; set; }   // Имя клиента
        public string Address { get; set; } // Адрес клиента
        public string Email { get; set; }   // Электронная почта клиента
        public string Phone { get; set; }   // Телефон клиента
    }
}
