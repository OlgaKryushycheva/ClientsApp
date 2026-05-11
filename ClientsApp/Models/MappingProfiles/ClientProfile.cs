// Профіль AutoMapper описує правила перетворення між DTO і доменними моделями.
// Це прибирає ручне копіювання полів у контролерах і сервісах.
﻿using AutoMapper;
using ClientsApp.Models.DTOs;
using ClientsApp.Models.Entities;

namespace ClientsApp.Models.MappingProfiles
{
// ClientProfile: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            CreateMap<Client, ClientDto>().ReverseMap();
        }
    }
}
