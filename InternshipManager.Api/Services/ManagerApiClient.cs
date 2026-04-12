using InternshipManager.Api.DTOs.External;

namespace InternshipManager.Api.Services;

public class ManagerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ManagerApiClient> _logger;

    public ManagerApiClient(
        HttpClient httpClient,
        ILogger<ManagerApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<EmployeeExternalDto?> GetSupervisorByIdAsync(EmployeeId id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/employees/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<EmployeeExternalDto>();

        }

        catch (Exception ex)
        {
            _logger.LogError("Сервис менеджера недоступен: {error}", ex.Message);
            
            return new EmployeeExternalDto
            {
                IdEmployee = id,
                FirstName = "Заглушка",
                LastName = "Руководитель",
                Role = 1
            };
        }
    }

    public async Task<ScheduledPracticeExternalDto?> GetScheduledPracticeAsync(ScheduledPracticeId id)
    {
        try
        {
            var response = await _httpClient
                .GetAsync($"/api/v1/scheduled-practices/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<ScheduledPracticeExternalDto>();

        }

        catch (Exception ex)
        {
            _logger.LogError("Сервис менеджера недоступен: {error}", ex.Message);
            return null;
        }
    }

    public async Task<List<SpecializationExternalDto>> GetSpecializationsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/Specialization");
            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<SpecializationExternalDto>>()
                   ?? new List<SpecializationExternalDto>();
        }

        catch (Exception ex)
        {
            _logger.LogError("Сервис менеджера недоступен: {error}", ex.Message);
            return new List<SpecializationExternalDto>();
        }
    }

    public async Task<List<DepartmentExternalDto>> GetDepartmentsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/Department");
            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<DepartmentExternalDto>>()
                   ?? new List<DepartmentExternalDto>();

        }

        catch (Exception ex)
        {
            _logger.LogError("Сервис менеджера недоступен: {error}", ex.Message);
            return new List<DepartmentExternalDto>();
        }

    }

    public async Task<List<AddressExternalDto>> GetAddressesAsync(
        DepartmentId? departmentId = null)
    {
        try
        {
            var url = departmentId.HasValue
                ? $"/api/v1/addresses?departmentId={departmentId}"
                : "/api/v1/addresses";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<AddressExternalDto>>()
                   ?? new List<AddressExternalDto>();

        }

        catch (Exception ex)
        {
            _logger.LogError("Сервис менеджера недоступен: {error}", ex.Message);
            return new List<AddressExternalDto>();
        }
    }

    public async Task<List<ScheduledPracticeExternalDto>> GetScheduledPracticesAsync()
    {
        try
        {
            var response = await _httpClient
                .GetAsync("/api/v1/scheduled-practices");

            response.EnsureSuccessStatusCode();
            return await response.Content
                .ReadFromJsonAsync<List<ScheduledPracticeExternalDto>>()
                   ?? new List<ScheduledPracticeExternalDto>();
        }

        catch (Exception ex)
        {
            _logger.LogError("Сервис менеджера недоступен: {error}", ex.Message);
            return new List<ScheduledPracticeExternalDto>();
        }
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
                "Сервис менеджера недоступен: {error}", ex.Message);

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
                "Сервис менеджера недоступен: {error}", ex.Message);

            return null;
        }
    }
}    