using AutoMapper;
using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Errors;
using ECommerce.API.Admin.Application.Services;
using ECommerce.Application.Shared;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces.Repositories;
using ECommerce.Domain.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ECommerce.Application.Services
{
    public class CustomerAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IValidator<RegisterUserDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IValidator<UpdateProfileDto> _updateProfileValidator;
        private readonly IValidator<ForgotPasswordDto> _forgotPasswordValidator;
        private readonly IValidator<ResetPasswordDto> _resetPasswordValidator;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CustomerAuthService> _logger;
        private readonly JwtTokenHelper _jwtTokenHelper;
        private readonly IEmailService _emailService;
        private readonly IWishlistRepository _wishlistRepository;
        private readonly CartService _cartService;

        public CustomerAuthService(
         JwtTokenHelper jwtTokenHelper,
         IUserRepository userRepository,
         UserManager<User> userManager,
         SignInManager<User> signInManager,
         IValidator<RegisterUserDto> registerValidator,
         IValidator<LoginDto> loginValidator,
         IValidator<UpdateProfileDto> updateProfileValidator,
         IValidator<ForgotPasswordDto> forgotPasswordValidator,
         IValidator<ResetPasswordDto> resetPasswordValidator,
         IMapper mapper,
         IConfiguration configuration,
         ILogger<CustomerAuthService> logger,
         IEmailService emailService,
         IWishlistRepository wishlistRepository,
         CartService cartService) 
        {
            _cartService = cartService ??throw new ArgumentNullException(nameof(cartService));
            _jwtTokenHelper = jwtTokenHelper ?? throw new ArgumentNullException(nameof(jwtTokenHelper));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _registerValidator = registerValidator ?? throw new ArgumentNullException(nameof(registerValidator));
            _loginValidator = loginValidator ?? throw new ArgumentNullException(nameof(loginValidator));
            _updateProfileValidator = updateProfileValidator ?? throw new ArgumentNullException(nameof(updateProfileValidator));
            _forgotPasswordValidator = forgotPasswordValidator ?? throw new ArgumentNullException(nameof(forgotPasswordValidator));
            _resetPasswordValidator = resetPasswordValidator ?? throw new ArgumentNullException(nameof(resetPasswordValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _wishlistRepository = wishlistRepository ?? throw new ArgumentNullException(nameof(wishlistRepository));
        }

        // -------------------- REGISTER --------------------
        public async Task<AppResponse<AuthResponseDto>> RegisterAsync(RegisterUserDto dto)
        {
            try
            {
                var errors = new List<string>();

                if (!await _userRepository.IsEmailUniqueAsync(dto.Email?.Trim()))
                {
                    errors.Add("Email already exists.");
                    return AppResponse<AuthResponseDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);
                }

                var validationResult = await _registerValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                    errors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));

                if (errors.Any())
                    return AppResponse<AuthResponseDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);

                var currentTime = DateTime.UtcNow;

                var user = new User
                {
                    UserName = dto.UserName?.Trim(),
                    Email = dto.Email?.Trim(),
                    PhoneNumber = dto.Phone?.Trim(),
                    IsActive = true,
                    CreatedAt = currentTime,
                    UpdatedAt = currentTime
                };

                var result = await _userManager.CreateAsync(user, dto.Password?.Trim());
                if (!result.Succeeded)
                {
                    errors.AddRange(result.Errors.Select(e => e.Description));
                    return AppResponse<AuthResponseDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);
                }

                await _userManager.AddToRoleAsync(user, "Customer");

               
                // Automatically create an empty wishlist for the new user
                var wishlist = new Wishlist
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _wishlistRepository.CreateWishlistAsync(wishlist, saveChanges: true);

                // Automatically create an empty cart for the new user
                
                await _cartService.CreateAsync(user.Id);

                var token = await _jwtTokenHelper.GenerateTokenAsync(user);

                var userResponse = _mapper.Map<UserReadDto>(user);
                userResponse.Roles ??= new List<string>();
                userResponse.Roles.Add("Customer");

                var authResponse = new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = _jwtTokenHelper.GetTokenExpiryDate(),
                    User = userResponse
                };

                return AppResponse<AuthResponseDto>.SuccessResult(authResponse, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {email}", dto?.Email);
                return AppResponse<AuthResponseDto>.ErrorResult(
                    new List<string> { "An error occurred while registering the user." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- LOGIN --------------------
        public async Task<AppResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            try
            {
                var errors = new List<string>();

                var validation = await _loginValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                if (errors.Any())
                    return AppResponse<AuthResponseDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);

                var user = await _userManager.FindByEmailAsync(dto.Email?.Trim());
                if (user == null || !user.IsActive)
                    return AppResponse<AuthResponseDto>.ErrorResult(
                        new List<string> { "Invalid email or password." },
                        (int)HttpStatusCode.Unauthorized);

                var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, true);
                if (!result.Succeeded)
                    return AppResponse<AuthResponseDto>.ErrorResult(
                        new List<string> { "Invalid email or password." },
                        (int)HttpStatusCode.Unauthorized);

                var token = await _jwtTokenHelper.GenerateTokenAsync(user);
                var userResponse = _mapper.Map<UserReadDto>(user);
                userResponse.Roles = (await _userManager.GetRolesAsync(user)).ToList();

                var authResponse = new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = _jwtTokenHelper.GetTokenExpiryDate(),
                    User = userResponse
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

        // -------------------- FORGOT PASSWORD --------------------
        public async Task<AppResponse<object>> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            try
            {
                var errors = new List<string>();

                var validation = await _forgotPasswordValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                if (errors.Any())
                    return AppResponse<object>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);

                var user = await _userManager.FindByEmailAsync(dto.Email?.Trim());
                if (user != null && user.IsActive)
                {
                    // prevent spamming
                    if (user.ResetPasswordExpiry != null && user.ResetPasswordExpiry > DateTime.UtcNow)
                    {
                        return AppResponse<object>.ErrorResult(
                            new List<string> { "A reset code has already been sent. Please wait until it expires." },
                            (int)HttpStatusCode.TooManyRequests);
                    }

                    var resetCode = GenerateResetCode();
                    user.ResetPasswordCode = resetCode;
                    user.ResetPasswordExpiry = DateTime.UtcNow.AddHours(1);
                    user.UpdatedAt = DateTime.UtcNow;

                    await _userManager.UpdateAsync(user);


                    await _emailService.SendEmailAsync(
                             dto.Email,
                          "Password Reset Code",
                          $"<p>Your reset code is: <b>{resetCode}</b></p>"
                    );
                    _logger.LogInformation("Password reset code generated for user {Email}", dto.Email);
                }

                return AppResponse<object>.SuccessResult(null, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password request for {Email}", dto?.Email);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while processing the request." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- RESET PASSWORD --------------------
        public async Task<AppResponse<object>> ResetPasswordAsync(ResetPasswordDto dto)
        {
            try
            {
                var errors = new List<string>();

                var validation = await _resetPasswordValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                if (errors.Any())
                    return AppResponse<object>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);

                var user = await _userRepository.GetByResetCodeAsync(dto.ResetCode?.Trim(), asNoTracking: false);
                if (user == null || user.Email != dto.Email?.Trim())
                    return AppResponse<object>.ErrorResult(
                        new List<string> { "Invalid reset code or email." },
                        (int)HttpStatusCode.BadRequest);

                // check expiry
                if (user.ResetPasswordExpiry == null || user.ResetPasswordExpiry < DateTime.UtcNow)
                {
                    return AppResponse<object>.ErrorResult(
                        new List<string> { "Reset code has expired." },
                        (int)HttpStatusCode.BadRequest);
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

                if (!result.Succeeded)
                {
                    var passwordErrors = result.Errors.Select(e => e.Description).ToList();
                    return AppResponse<object>.ErrorResult(passwordErrors, (int)HttpStatusCode.BadRequest);
                }

                user.ResetPasswordCode = null;
                user.ResetPasswordExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return AppResponse<object>.SuccessResult(null, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {Email}", dto?.Email);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while resetting the password." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- UPDATE PROFILE --------------------
        public async Task<AppResponse<UserReadDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            try
            {
                var errors = new List<string>();

                var validation = await _updateProfileValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || !user.IsActive)
                    errors.Add($"User with Id {userId} not found.");

                var newEmail = dto.Email?.Trim();
                if (user != null && !string.Equals(user.Email?.Trim(), newEmail, StringComparison.OrdinalIgnoreCase))
                {
                    if (!await _userRepository.IsEmailUniqueAsync(newEmail, userId))
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
                _logger.LogError(ex, "Error updating user profile {UserId}", userId);
                return AppResponse<UserReadDto>.ErrorResult(
                    new List<string> { "An error occurred while updating the profile." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- GET PROFILE --------------------
        public async Task<AppResponse<UserReadDto>> GetProfileAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, asNoTracking: true);
                if (user == null || !user.IsActive)
                    return AppResponse<UserReadDto>.ErrorResult(
                        new List<string> { $"User with Id {userId} not found." },
                        (int)HttpStatusCode.NotFound);

                var dto = _mapper.Map<UserReadDto>(user);
                dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                return AppResponse<UserReadDto>.SuccessResult(dto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile {UserId}", userId);
                return AppResponse<UserReadDto>.ErrorResult(
                    new List<string> { "An error occurred while retrieving the profile." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }
        public async Task<AppResponse<object>> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"User with Id {userId} not found." },
                        (int)HttpStatusCode.NotFound);
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return AppResponse<object>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);
                }


                // Delete wishlist after user deletion
                await _wishlistRepository.DeleteWishlistAsync(userId, saveChanges: true);

                // Delete cart after user deletion
                await _cartService.DeleteAsync(user.Id);


                return AppResponse<object>.SuccessResult(null, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while deleting the user." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }
        // -------------------- HELPERS --------------------
        private string GenerateResetCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
