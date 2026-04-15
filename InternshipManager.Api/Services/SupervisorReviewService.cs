using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.DTOs.SupervisorReview;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.Repositories.Interfaces;
using InternshipManager.Api.Services.Interfaces;

namespace InternshipManager.Api.Services;

public class SupervisorReviewService : ISupervisorReviewService
{
    private readonly ISupervisorReviewRepository _reviewRepository;
    private readonly ISupervisorApplicationRepository _applicationRepository;
    private readonly IStudentSupervisorApplicationRepository _studentRepository;

    public SupervisorReviewService(
        ISupervisorReviewRepository reviewRepository,
        ISupervisorApplicationRepository applicationRepository,
        IStudentSupervisorApplicationRepository studentRepository)
    {
        _reviewRepository = reviewRepository;
        _applicationRepository = applicationRepository;
        _studentRepository = studentRepository;
    }

    public async Task<object> GetPendingReviewsAsync(EmployeeId supervisorId)
    {
        // 1. Получаем завершённые заявки руководителя (Satisfied + EndDate прошла)
        var activePractices = await _applicationRepository
            .GetCompletedApplicationIdsAsync(supervisorId);

        // 2. Получаем принятых студентов по этим заявкам
        var completedStudents = await _studentRepository
            .GetByApplicationIdsAndStatusAsync(
                activePractices,
                StudentSupervisorApplicationStatus.Accepted);

        // 3. Получаем уже написанные отзывы
        var reviewedStudentIds = await _reviewRepository
            .GetReviewedStudentIdsAsync(supervisorId);

        // 4. Оставляем только тех, на кого отзыва ещё нет
        var pendingReviews = completedStudents
            .Where(s => !reviewedStudentIds.Contains(s.IdStudentApplication))
            .Select(s => new
            {
                idStudentApplication = s.IdStudentApplication,
                idSupervisorApplication = s.IdSupervisorApplication,
                message = "Необходимо оставить отзыв"
            })
            .ToList();

        return new
        {
            pendingCount = pendingReviews.Count,
            students = pendingReviews
        };
    }

    public async Task<object> CreateAsync(CreateSupervisorReviewDto dto)
    {
        // === Бизнес-проверка: студент завершил практику ===
        var studentCompleted = await _studentRepository
            .ExistsByStudentAndStatusAsync(
                dto.IdStudentApplication,
                StudentSupervisorApplicationStatus.Accepted);

        if (!studentCompleted)
            throw new InvalidOperationException(
                "Нельзя оставить отзыв о студенте, не прошедшем практику");

        // === Бизнес-проверка: отзыв ещё не написан ===
        var reviewExists = await _reviewRepository.ExistsAsync(
            dto.IdEmployee, dto.IdStudentApplication);

        if (reviewExists)
            throw new InvalidOperationException(
                "Отзыв на этого студента уже существует");

        // === Создание сущности ===
        var review = new SupervisorReview
        {
            IdEmployee = dto.IdEmployee,
            IdStudentApplication = dto.IdStudentApplication,
            RecommendedForEmployment = dto.RecommendedForEmployment,
            PvScore = dto.PvScore,
            SkillsScore = dto.SkillsScore,
            IndependenceScore = dto.IndependenceScore,
            TeamworkScore = dto.TeamworkScore,
            OverallScore = dto.OverallScore,
            Comment = dto.Comment,
        };

        await _reviewRepository.AddAsync(review);

        return new
        {
            idEmployee = review.IdEmployee,
            idStudentApplication = review.IdStudentApplication,
            message = "Отзыв успешно сохранен"
        };
    }

    public async Task<object?> GetReviewAsync(
        EmployeeId idEmployee,
        StudentApplicationId idStudentApplication)
    {
        var review = await _reviewRepository.GetAsync(
            idEmployee, idStudentApplication);

        if (review == null)
            return null;

        return new
        {
            idEmployee = review.IdEmployee,
            idStudentApplication = review.IdStudentApplication,
            recommendedForEmployment = review.RecommendedForEmployment,
            pvScore = review.PvScore,
            skillsScore = review.SkillsScore,
            independenceScore = review.IndependenceScore,
            teamworkScore = review.TeamworkScore,
            overallScore = review.OverallScore,
            comment = review.Comment
        };
    }
}