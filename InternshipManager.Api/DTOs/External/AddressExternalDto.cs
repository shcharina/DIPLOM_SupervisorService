namespace InternshipManager.Api.DTOs.External;

public class AddressExternalDto
{
    public AddressId IdAddress { get; set; }
    public DepartmentId IdDepartment { get; set; }
    public string FullAddress { get; set; } = string.Empty;
    public string? City { get; set; }
}