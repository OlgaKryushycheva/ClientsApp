// DataAnnotations нижче керують валідацією форми й мапінгом полів у БД.
﻿namespace ClientsApp.Models.Entities
{
    public enum ClientTaskStatusEnum
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2
    }
}
