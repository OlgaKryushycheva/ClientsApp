namespace ClientsApp.Models.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }  // Первичный ключ для платежа

        public int ClientTaskId { get; set; }  // Идентификатор задачи, за которую сделан платеж
        public ClientTask ClientTask { get; set; }  // Навигационное свойство для задачи

        public decimal Amount { get; set; }  // Сумма платежа
        public DateTime PaymentDate { get; set; }  // Дата платежа
        public decimal BalanceDue { get; set; }  // Потенциальная задолженность по задаче
    }
}
