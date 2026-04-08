namespace InternshipManager.Api.DTOs.External;

public class AddressExternalDto
{
    public int IdAddress { get; set; }
    public int IdDepartment { get; set; }
    public string FullAddress { get; set; } = string.Empty;
    public string? City { get; set; }
}
