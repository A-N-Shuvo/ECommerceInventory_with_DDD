using ECommerceInventory.Application.DTOs;
using ECommerceInventory.Core.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceInventory.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<UserRegisterDto, User>();

            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<CategoryCreateDto, Category>();

            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl));
 
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>();

            // For create/update DTO -> entity: ignore ImageUrl mapping (we set it manually after saving file)
            CreateMap<ProductCreateDto, Product>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

            CreateMap<ProductUpdateDto, Product>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());
        }
    }
}
