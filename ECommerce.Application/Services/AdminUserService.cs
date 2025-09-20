using AutoMapper;
using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Errors;
using ECommerce.API.Admin.Application.Extensions;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;


namespace ECommerce.API.Admin.Application.Services
{
    public class AdminUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IValidator<CreateUserDto> _createUserValidator;
        private readonly IValidator<UpdateUserDto> _updateUserValidator;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminUserService> _logger;

        public AdminUserService(
            SignInManager<User> signInManager,
            IUserRepository userRepository,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IConfiguration configuration,
        IValidator<CreateUserDto> createUserValidator,
            IValidator<UpdateUserDto> updateUserValidator,
            IMapper mapper,
            ILogger<AdminUserService> logger)
        {

            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _configuration=configuration ?? throw new ArgumentNullException(nameof(configuration));
            _createUserValidator = createUserValidator ?? throw new ArgumentNullException(nameof(createUserValidator));
            _updateUserValidator = updateUserValidator ?? throw new ArgumentNullException(nameof(updateUserValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // -------------------- GET ALL --------------------
        public async Task<AppPaginatedResponse<UserReadDto>> GetAllAsync(int page = 1, string search = "", int pageSize = 10)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Max(1, pageSize);

                var allUsers = await _userRepository.GetAllAsync(search, asNoTracking: true);
                var totalUsers = allUsers.Count();

                var pagination = (totalUsers, page, pageSize).BuildPagination();

                if (totalUsers == 0)
                    return PaginationExtensions.EmptyPageResult<UserReadDto>(pageSize);

                if (page > pagination.TotalPages && pagination.TotalPages > 0)
                    return pagination.NotFoundPageResult<UserReadDto>();

                var pagedUsers = await _userRepository.GetAllAsync(page, search ?? string.Empty, pageSize, asNoTracking: true);
                var userDtos = new List<UserReadDto>();

                foreach (var user in pagedUsers)
                {
                    var userDto = _mapper.Map<UserReadDto>(user);
                    userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                    userDtos.Add(userDto);
                }

                return new AppPaginatedResponse<UserReadDto>(
                    userDtos,
                    pagination,
                    (int)HttpStatusCode.OK
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users.");
                return new AppPaginatedResponse<UserReadDto>(
                    Enumerable.Empty<UserReadDto>(),
                    new Pagination { CurrentPage = page, PageSize = pageSize, TotalItems = 0, TotalPages = 0 },
                    (int)HttpStatusCode.InternalServerError,
                    errors: new List<string> { "An error occurred while retrieving users." }
                );
            }
        }

        // -------------------- GET BY ID --------------------
        public async Task<AppResponse<UserReadDto>> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _userRepository.GetByIdAsync(id, asNoTracking: true);
                if (entity == null)
                    return AppResponse<UserReadDto>.ErrorResult(
                        new List<string> { $"User with Id {id} not found." },
                        (int)HttpStatusCode.NotFound);

                var dto = _mapper.Map<UserReadDto>(entity);
                dto.Roles = (await _userManager.GetRolesAsync(entity)).ToList();
                return AppResponse<UserReadDto>.SuccessResult(dto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
                return AppResponse<UserReadDto>.ErrorResult(
                    new List<string> { "An error occurred while retrieving the user." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- CREATE --------------------
        public async Task<AppResponse<UserReadDto>> CreateAsync(CreateUserDto dto)
        {
            try
            {
                var errors = new List<string>();

                var validation = await _createUserValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                if (!await _userRepository.IsEmailUniqueAsync(dto.Email?.Trim()))
                    errors.Add("Email already exists.");

                if (!await _roleManager.RoleExistsAsync(dto.Role))
                    errors.Add($"Role '{dto.Role}' does not exist.");

                if (errors.Any())
                    return AppResponse<UserReadDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);

                var user = new User
                {
                    Email = dto.Email?.Trim() ?? string.Empty,
                    UserName = dto.Email?.Trim() ?? string.Empty,
                    PhoneNumber = dto.Phone?.Trim(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    errors.AddRange(result.Errors.Select(e => e.Description));
                    return AppResponse<UserReadDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);
                }

                await _userManager.AddToRoleAsync(user, dto.Role);

                var createdDto = _mapper.Map<UserReadDto>(user);
                createdDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                return AppResponse<UserReadDto>.SuccessResult(createdDto, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Email}", dto?.Email);
                return AppResponse<UserReadDto>.ErrorResult(
                    new List<string> { "An error occurred while creating the user." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- UPDATE --------------------
        public async Task<AppResponse<UserReadDto>> UpdateAsync(int id, UpdateUserDto dto)
        {
            try
            {
                var errors = new List<string>();

                var validation = await _updateUserValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                    errors.Add($"User with Id {id} not found.");

                var newEmail = dto.Email?.Trim();
                if (user != null && !string.Equals(user.Email?.Trim(), newEmail, StringComparison.OrdinalIgnoreCase))
                {
                    if (!await _userRepository.IsEmailUniqueAsync(newEmail, id))
                        errors.Add("Email already exists.");
                }

                if (errors.Any())
                {
                    var statusCode = errors.Any(e => e.Contains("not found"))
                        ? (int)HttpStatusCode.NotFound
                        : (int)HttpStatusCode.BadRequest;
                    return AppResponse<UserReadDto>.ErrorResult(errors, statusCode);
                }

                user.Email = newEmail;
                user.UserName = newEmail;
                user.PhoneNumber = dto.Phone?.Trim();
                user.IsActive = dto.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    errors.AddRange(result.Errors.Select(e => e.Description));
                    return AppResponse<UserReadDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);
                }

                var updatedDto = _mapper.Map<UserReadDto>(user);
                updatedDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                return AppResponse<UserReadDto>.SuccessResult(updatedDto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return AppResponse<UserReadDto>.ErrorResult(
                    new List<string> { "An error occurred while updating the user." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- DELETE --------------------
        public async Task<AppResponse<object>> DeleteAsync(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"User with Id {id} not found." },
                        (int)HttpStatusCode.NotFound);

                // Soft delete by setting IsActive = false
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return AppResponse<object>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);
                }

                return AppResponse<object>.SuccessResult(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while deleting the user." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- ASSIGN ROLE --------------------
        public async Task<AppResponse<object>> AssignRoleAsync(int userId, AssignRoleDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"User with Id {userId} not found." },
                        (int)HttpStatusCode.NotFound);

                if (!await _roleManager.RoleExistsAsync(dto.Role))
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"Role '{dto.Role}' does not exist." },
                        (int)HttpStatusCode.BadRequest);

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, dto.Role);

                return AppResponse<object>.SuccessResult(null, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {Role} to user {UserId}", dto.Role, userId);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while assigning the role." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }
        public async Task<AppResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email.Trim());
                if (user == null || !user.IsActive)
                {
                    return AppResponse<AuthResponseDto>.ErrorResult(
                        new List<string> { "Invalid email or password." },
                        (int)HttpStatusCode.Unauthorized);
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
                if (!result.Succeeded)
                {
                    return AppResponse<AuthResponseDto>.ErrorResult(
                        new List<string> { "Invalid email or password." },
                        (int)HttpStatusCode.Unauthorized);
                }

                // Generate token
                var token = await GenerateJwtTokenAsync(user);
                var roles = await _userManager.GetRolesAsync(user);

                var dtoUser = new UserReadDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    Phone = user.PhoneNumber,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                };

                var authResponse = new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    User = dtoUser
                };

                return AppResponse<AuthResponseDto>.SuccessResult(authResponse, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user {Email}", dto?.Email);
                return AppResponse<AuthResponseDto>.ErrorResult(
                    new List<string> { "An error occurred while logging in." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim("uid", user.Id.ToString())
            };

            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
            claims.AddRange(userClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));//exption reson
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}