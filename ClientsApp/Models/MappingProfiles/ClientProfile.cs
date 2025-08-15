using AutoMapper;
using ClientsApp.Models.DTOs;
using ClientsApp.Models.Entities;

namespace ClientsApp.Models.MappingProfiles
{
    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            CreateMap<Client, ClientDto>().ReverseMap();  // Маппинг Client -> ClientDto и наоборот
        }
    }
}


