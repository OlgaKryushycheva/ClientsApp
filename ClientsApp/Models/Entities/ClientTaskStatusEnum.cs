// Сутність ClientTaskStatusEnum відповідає таблиці/даним предметної області.
// DataAnnotations нижче керують валідацією форми й мапінгом полів у БД.
﻿namespace ClientsApp.Models.Entities
{
// ClientTaskStatusEnum: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public enum ClientTaskStatusEnum
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2
    }
}
