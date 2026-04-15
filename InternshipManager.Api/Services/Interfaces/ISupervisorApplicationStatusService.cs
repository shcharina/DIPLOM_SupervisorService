namespace InternshipManager.Api.Services.Interfaces;

public interface ISupervisorApplicationStatusService
{
    Task CheckAndUpdateApplicationStatus(
        SupervisorApplicationId supervisorApplicationId);
}