using Microsoft.Extensions.Caching.Memory;

using InternshipManager.Api.DTOs.External;

namespace InternshipManager.Api.Services;

public class ManagerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ManagerApiClient> _logger;
    private readonly IMemoryCache _cache;

    public ManagerApiClient(
        HttpClient httpClient,
        ILogger<ManagerApiClient> logger,
        IMemoryCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
    }

    public async Task<EmployeeExternalDto?> GetSupervisorByIdAsync(EmployeeId id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/Employee/GetSupervisor/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<EmployeeExternalDto>();

        }
        catch (Exception ex)
        {
            _logger.LogError("Сервис менеджера недоступен: {error}", ex.Message);
            return null;
        }
    }

    public async Task<ScheduledPracticeExternalDto?> GetScheduledPracticeAsync(ScheduledPracticeId id)
    {
        try
        {
            var response = await _httpClient
                .GetAsync($"/api/v1/ScheduledPractice/GetScheduledPractice/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<ScheduledPracticeExternalDto>();

        }

        catch (Exception ex)
        {
            _logger.LogError("Ошибка получения практики из расписания: {error}", ex.Message);
            return null;
        }
    }

    public async Task<List<SpecializationExternalDto>> GetSpecializationsAsync()
    {
        return await _cache.GetOrCreateAsync("specializations", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            try
            {
                var response = await _httpClient
                    .GetAsync("/api/v1/Specialization/GetSpecializations");
                response.EnsureSuccessStatusCode();

                return await response.Content
                    .ReadFromJsonAsync<List<SpecializationExternalDto>>()
                       ?? new List<SpecializationExternalDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка получения специализаций: {error}", ex.Message);
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return new List<SpecializationExternalDto>();
            }
        }) ?? new List<SpecializationExternalDto>();
    }

    public async Task<List<DepartmentExternalDto>> GetDepartmentsAsync()
    {
        return await _cache.GetOrCreateAsync("departments", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            try
            {
                var response = await _httpClient.GetAsync("/api/v1/Department/GetDepartments");
                response.EnsureSuccessStatusCode();

                return await response.Content
                    .ReadFromJsonAsync<List<DepartmentExternalDto>>()
                       ?? new List<DepartmentExternalDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка получения подразделений: {error}", ex.Message);
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return new List<DepartmentExternalDto>();
            }
        }) ?? new List<DepartmentExternalDto>();
    }

    public async Task<List<AddressExternalDto>> GetAddressesAsync(
        DepartmentId? departmentId = null)
    {
        if (!departmentId.HasValue)
            return new List<AddressExternalDto>();

        var cacheKey = $"addresses_dept_{departmentId}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            try
            {
                var response = await _httpClient.GetAsync(
                    $"/api/v1/Address/GetAddressesByDepartment/{departmentId}"
                );
                response.EnsureSuccessStatusCode();

                return await response.Content
                    .ReadFromJsonAsync<List<AddressExternalDto>>()
                       ?? new List<AddressExternalDto>();

            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка получения адресов: {error}", ex.Message);
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return new List<AddressExternalDto>();
            }
        }) ?? new List<AddressExternalDto>();
    }

    public async Task<List<ScheduledPracticeExternalDto>> GetScheduledPracticesAsync()
    {
        return await _cache.GetOrCreateAsync("scheduled_practices", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            try
            {
                var response = await _httpClient
                    .GetAsync("/api/v1/ScheduledPractice/GetScheduledPractices");

                response.EnsureSuccessStatusCode();
                return await response.Content
                    .ReadFromJsonAsync<List<ScheduledPracticeExternalDto>>()
                       ?? new List<ScheduledPracticeExternalDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка получения расписания практик: {error}", ex.Message);
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return new List<ScheduledPracticeExternalDto>();
            }
        }) ?? new List<ScheduledPracticeExternalDto>();
    }

    public async Task<TestingResultExternalDto?> GetTestingResultAsync(
        StudentApplicationId studentApplicationId)
    {    
        try    
        {    
            var response = await _httpClient    
                .GetAsync($"/api/v1/Testing/{studentApplicationId}");

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content    
                .ReadFromJsonAsync<TestingResultExternalDto>();

        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Ошибка получения результатов тестирования: {error}", ex.Message);

            return null;
        }
    }

    public async Task<ManagerInterviewResultExternalDto?> GetManagerInterviewResultAsync(
        StudentApplicationId studentApplicationId)
    {
        try
        {
            var response = await _httpClient
                .GetAsync($"/api/v1/ManagerInterview/{studentApplicationId}");

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content
                .ReadFromJsonAsync<ManagerInterviewResultExternalDto>();
        }

        catch (Exception ex)
        {
            _logger.LogError(
                "Ошибка получения результатов собеседования менеджером: {error}", ex.Message);

            return null;
        }
    }
}    