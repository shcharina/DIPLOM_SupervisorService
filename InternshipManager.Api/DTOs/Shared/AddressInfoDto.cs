namespace InternshipManager.Api.DTOs.Shared;

public class AddressInfoDto
{
    public int IdAddress { get; set; }
    public int IdDepartment { get; set; }
    public string FullAddress { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? PostalCode { get; set; }
}