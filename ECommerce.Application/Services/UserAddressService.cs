using AutoMapper;
using ECommerce.API.Admin.Application.DTOs;
using ECommerce.API.Admin.Application.Errors;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Domain.Helpers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ECommerce.API.Admin.Application.Services
{
    public class UserAddressService
    {
        private readonly IUserAddressRepository _addressRepository;
        private readonly IUserRepository _userRepository;
        private readonly IValidator<CreateUserAddressDto> _createValidator;
        private readonly IValidator<UpdateUserAddressDto> _updateValidator;
        private readonly IMapper _mapper;
        private readonly ILogger<UserAddressService> _logger;

        public UserAddressService(
            IUserAddressRepository addressRepository,
            IUserRepository userRepository,
            IValidator<CreateUserAddressDto> createValidator,
            IValidator<UpdateUserAddressDto> updateValidator,
            IMapper mapper,
            ILogger<UserAddressService> logger)
        {
            _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // -------------------- GET BY ID --------------------
        public async Task<AppResponse<UserAddressReadDto>> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _addressRepository.GetByIdAsync(id, asNoTracking: true);
                if (entity == null)
                    return AppResponse<UserAddressReadDto>.ErrorResult(
                        new List<string> { $"Address with Id {id} not found." },
                        (int)HttpStatusCode.NotFound);

                var dto = _mapper.Map<UserAddressReadDto>(entity);
                return AppResponse<UserAddressReadDto>.SuccessResult(dto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user address {AddressId}", id);
                return AppResponse<UserAddressReadDto>.ErrorResult(
                    new List<string> { "An error occurred while retrieving the user address." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- GET BY USER --------------------
        public async Task<AppResponse<IEnumerable<UserAddressReadDto>>> GetByUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, asNoTracking: true);
                if (user == null)
                    return AppResponse<IEnumerable<UserAddressReadDto>>.ErrorResult(
                        new List<string> { $"User with Id {userId} not found." },
                        (int)HttpStatusCode.NotFound);

                var addresses = await _addressRepository.GetByUserIdAsync(userId, asNoTracking: true);
                var dtos = addresses.Select(a => _mapper.Map<UserAddressReadDto>(a)).ToList();

                return AppResponse<IEnumerable<UserAddressReadDto>>.SuccessResult(dtos, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving addresses for user {UserId}", userId);
                return AppResponse<IEnumerable<UserAddressReadDto>>.ErrorResult(
                    new List<string> { "An error occurred while retrieving user addresses." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- CREATE --------------------
        public async Task<AppResponse<UserAddressReadDto>> CreateAsync(CreateUserAddressDto dto)
        {
            try
            {
                var errors = new List<string>();

                var validation = await _createValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                var user = await _userRepository.GetByIdAsync(dto.UserId, asNoTracking: true);
                if (user == null)
                    errors.Add($"User with Id {dto.UserId} not found.");

                if (errors.Any())
                    return AppResponse<UserAddressReadDto>.ErrorResult(errors, (int)HttpStatusCode.BadRequest);

                var entity = _mapper.Map<UserAddress>(dto);
                entity.CreatedAt = DateTime.UtcNow;

                await _addressRepository.AddAsync(entity, saveChanges: true);

                var createdDto = _mapper.Map<UserAddressReadDto>(entity);
                return AppResponse<UserAddressReadDto>.SuccessResult(createdDto, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user address for user {UserId}", dto.UserId);
                return AppResponse<UserAddressReadDto>.ErrorResult(
                    new List<string> { "An error occurred while creating the user address." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- UPDATE --------------------
        public async Task<AppResponse<UserAddressReadDto>> UpdateAsync(int id, UpdateUserAddressDto dto)
        {
            try
            {
                var errors = new List<string>();

                var entity = await _addressRepository.GetByIdAsync(id, asNoTracking: false);
                if (entity == null)
                    errors.Add($"Address with Id {id} not found.");

                var validation = await _updateValidator.ValidateAsync(dto);
                if (!validation.IsValid)
                    errors.AddRange(validation.Errors.Select(e => e.ErrorMessage));

                if (errors.Any())
                {
                    var statusCode = errors.Any(e => e.Contains("not found"))
                        ? (int)HttpStatusCode.NotFound
                        : (int)HttpStatusCode.BadRequest;
                    return AppResponse<UserAddressReadDto>.ErrorResult(errors, statusCode);
                }

                _mapper.Map(dto, entity!);

                await _addressRepository.UpdateAsync(entity!, saveChanges: true);

                var updatedDto = _mapper.Map<UserAddressReadDto>(entity);
                return AppResponse<UserAddressReadDto>.SuccessResult(updatedDto, (int)HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user address {AddressId}", id);
                return AppResponse<UserAddressReadDto>.ErrorResult(
                    new List<string> { "An error occurred while updating the user address." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        // -------------------- DELETE --------------------
        public async Task<AppResponse<object>> DeleteAsync(int id)
        {
            try
            {
                var entity = await _addressRepository.GetByIdAsync(id, asNoTracking: false);
                if (entity == null)
                    return AppResponse<object>.ErrorResult(
                        new List<string> { $"Address with Id {id} not found." },
                        (int)HttpStatusCode.NotFound);

                await _addressRepository.DeleteAsync(entity, saveChanges: true);
                return AppResponse<object>.SuccessResult(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user address {AddressId}", id);
                return AppResponse<object>.ErrorResult(
                    new List<string> { "An error occurred while deleting the user address." },
                    (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
