// DTO ClientDto визначає формат даних для передачі між шарами.
// У DTO залишаються лише поля, потрібні для конкретного сценарію обміну даними.
﻿namespace ClientsApp.Models.DTOs
{
// ClientDto: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ClientDto
    {
        public int ClientId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
