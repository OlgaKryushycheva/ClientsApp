// Модель містить лише ті властивості, які реально використовуються у формі/представленні.
namespace ClientsApp.Models.ViewModels.Statistics
{
    public class ClientTaskCostViewModel
    {
        public string TaskTitle { get; set; } = string.Empty;
        public decimal TaskCost { get; set; }
        public decimal Payments { get; set; }
        public decimal BalanceDue { get; set; }
    }
}
