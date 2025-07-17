using AutoMapper;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.BLL.Healpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<Payment, PaymentDTO>().ReverseMap();
        }
    }
}
