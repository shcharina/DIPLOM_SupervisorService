using InternshipManager.Api.DTOs.External;

namespace InternshipManager.Api.Services;

public class StudentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StudentApiClient> _logger;
    public StudentApiClient(
        HttpClient httpClient,
        ILogger<StudentApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<StudentApplicationExternalDto?> GetStudentApplicationAsync(
        StudentApplicationId id)
    {
        try
        {
            var response = await _httpClient
                .GetAsync($"/api/v1/Application/{id}");

            if (!response.IsSuccessStatusCode) return null;
            return await response.Content
                .ReadFromJsonAsync<StudentApplicationExternalDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Сервис студента недоступен: {error}", ex.Message);

            return null;
        }
    }

    public async Task<QuestionnaireExternalDto?> GetQuestionnaireAsync(
        StudentApplicationId studentApplicationId)
    {
        try
        {
            var response = await _httpClient
                .GetAsync($"/api/v1/Questionnaire/{studentApplicationId}");

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content
                .ReadFromJsonAsync<QuestionnaireExternalDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "Анкета недоступна (эндпоинт ещё не реализован): {error}",
                ex.Message);
            return null;
        }
    }
}