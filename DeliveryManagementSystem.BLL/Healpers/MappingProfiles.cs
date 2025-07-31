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
            CreateMap<RegisterUserDTO, User>();
            CreateMap<Payment, PaymentDTO>().ReverseMap();
            CreateMap<Payment, PaymentResponseDTO>().ReverseMap();
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<Category, CreateCategoryDTO>().ReverseMap();
            CreateMap<Category, UpdateCategoryDTO>().ReverseMap();

            CreateMap<Payment, PaymentDTO>();

            CreateMap<Order, OrderDTO>();
            CreateMap<Order, CreateOrderDTO>().ReverseMap();
            CreateMap<Order, UpdateOrderStatusDTO>().ReverseMap();
            CreateMap<Order, OrderWithItemsDTO>();
            CreateMap<Order, OrderTrackingDTO>();
            CreateMap<Order, OrderHistoryDTO>();

            CreateMap<OrderItem, OrderItemDTO>();
            CreateMap<OrderItem, CreateOrderItemDTO>().ReverseMap();
            CreateMap<OrderItem, UpdateOrderItemDTO>().ReverseMap();
            CreateMap<OrderItem, OrderItemWithMealDTO>();

            CreateMap<Restaurant, RestaurantDTO>();
            CreateMap<Restaurant, CreateRestaurantDTO>().ReverseMap();
            CreateMap<Restaurant, UpdateRestaurantDTO>().ReverseMap();
            CreateMap<Restaurant, RestaurantWithMenuDTO>();
            CreateMap<Restaurant, RestaurantReviewsDTO>();
            CreateMap<CreateRestaurantDTO, Restaurant>();

            CreateMap<Meal, MealDTO>()
            .ForMember(dest => dest.RestaurantID, opt => opt.MapFrom(src => src.Resturant.ID))
            .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Resturant.Name));

            CreateMap<Meal, CreateMealDTO>().ReverseMap();
            CreateMap<Meal, UpdateMealDTO>().ReverseMap();
            CreateMap<Meal, MealDetailsDTO>();
            CreateMap<Meal, PopularMealDTO>();
            CreateMap<RestaurantMenuCategory, RestaurantMenuCategoryDTO>();
            CreateMap<Review, ReviewDTO>();
            CreateMap<Review, CreateReviewDTO>().ReverseMap();
            // CreateMap<Reservation, ReservationDTO>();

            CreateMap<Reservation, ReservationDTO>()
          .ForMember(dest => dest.TableNumber, opt => opt.MapFrom(src => src.Table.ID))
            .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

        }
    }
}
