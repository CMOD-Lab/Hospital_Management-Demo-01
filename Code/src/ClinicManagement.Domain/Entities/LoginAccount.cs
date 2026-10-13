namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Login credentials (maps to dbo.logintable).
/// </summary>
public class LoginAccount
{
    public int LoginId { get; set; }
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    /// <summary>1 = patient, 2 = doctor, 3 = admin.</summary>
    public int Type { get; set; }
}
