using AutoMapper;
using ECommerce.API.Admin.Application.DTOs;
using ECommerce.Domain.Entities;

namespace ECommerce.API.Admin.Application.Mapping
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();

            CreateMap<Category, CategoryReadDto>()
                .ForMember(dest => dest.ParentCategoryName,
                           opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
                .ForMember(dest => dest.SubCategories,
                           opt => opt.MapFrom(src => src.SubCategories));

            CreateMap<Category, CategoryChildDto>();

            CreateMap<ProductImage, ProductImageDto>().ReverseMap();
            CreateMap<ProductCategory, ProductCategoryDto>().ReverseMap();
            CreateMap<Brand, BrandDto>().ReverseMap();
            CreateMap<Brand, BrandReadDto>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Product, ProductReadDto>().ReverseMap();
            CreateMap<Category, CategoryChildDto>();
            CreateMap<ProductImage, ProductImageDto>().ReverseMap();
            CreateMap<ProductImage, ProductImageReadDto>().ReverseMap();
            CreateMap<ProductCategory, ProductCategoryDto>().ReverseMap();
            CreateMap<ProductCategory, ProductCategoryReadDto>().ReverseMap();

        }
    }
}
