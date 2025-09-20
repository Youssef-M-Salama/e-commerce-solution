using AutoMapper;
using ECommerce.API.Admin.Application.DTOs;
using ECommerce.Domain.Entities;

namespace ECommerce.API.Admin.Application.Mapping
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // =========================
            // Product
            // =========================
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Product, ProductReadDto>().ReverseMap();

            // =========================
            // Category
            // =========================
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Category, CategoryReadDto>()
                .ForMember(dest => dest.ParentCategoryName,
                           opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
                .ForMember(dest => dest.SubCategories,
                           opt => opt.MapFrom(src => src.SubCategories));
            CreateMap<Category, CategoryChildDto>();

            // =========================
            // Brand
            // =========================
            CreateMap<Brand, BrandDto>().ReverseMap();
            CreateMap<Brand, BrandReadDto>().ReverseMap();

            // =========================
            // ProductImage
            // =========================
            CreateMap<ProductImage, ProductImageDto>().ReverseMap();
            CreateMap<ProductImage, ProductImageReadDto>().ReverseMap();

            // =========================
            // ProductCategory
            // =========================
            CreateMap<ProductCategory, ProductCategoryDto>().ReverseMap();
            CreateMap<ProductCategory, ProductCategoryReadDto>().ReverseMap();

            // =========================
            // User (Customer + Admin APIs)
            // =========================

            // Register User (Customer API)
            CreateMap<RegisterUserDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone));

            // Update Profile (Customer API)
            CreateMap<UpdateProfileDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone));

            // Create User (Admin API)
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // Identity handles hashing

            // Update User (Admin API)
            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            // Read User (Response DTO)
            CreateMap<User, UserReadDto>()
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Roles populated separately

            // UserAddress mappings
            CreateMap<CreateUserAddressDto, UserAddress>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()); // set CreatedAt in service

            CreateMap<UpdateUserAddressDto, UserAddress>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)); // optional: only map non-null

            CreateMap<UserAddress, UserAddressReadDto>().ReverseMap();

        }
    }
}
