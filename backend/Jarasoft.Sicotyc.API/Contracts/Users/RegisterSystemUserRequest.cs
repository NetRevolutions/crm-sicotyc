using System.ComponentModel.DataAnnotations;

namespace Jarasoft.Sicotyc.Api.Contracts.Users;

public sealed class RegisterSystemUserRequest
{
    [Required]
    public required RegisterUserDataRequest User { get; init; }

    [Required]
    public required RegisterCompanyDataRequest Company { get; init; }
}

public sealed class RegisterUserDataRequest
{
    [Required]
    [MaxLength(100)]
    public required string FirstName { get; init; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; init; }

    [Required]
    [MaxLength(20)]
    public required string DocumentType { get; init; }

    [Required]
    [MaxLength(50)]
    public required string DocumentNumber { get; init; }

    [Required]
    [MaxLength(256)]
    public required string UserName { get; init; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public required string Email { get; init; }

    [Required]
    [MaxLength(50)]
    public required string PhoneNumber { get; init; }

    [Required]
    public required Guid ApplicationRoleId { get; init; }

    [Required]
    [MinLength(8)]
    public required string Password { get; init; }
}

public sealed class RegisterCompanyDataRequest
{
    [Required]
    [MaxLength(20)]
    public required string Ruc { get; init; }

    [Required]
    [MaxLength(200)]
    public required string BusinessName { get; init; }

    [Required]
    [MaxLength(300)]
    public required string Address { get; init; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public required string Email { get; init; }

    [Required]
    [MaxLength(50)]
    public required string Phone { get; init; }

    public bool IsTransportCompany { get; init; }
}