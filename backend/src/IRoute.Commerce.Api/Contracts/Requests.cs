using System.ComponentModel.DataAnnotations;

namespace IRoute.Commerce.Api.Contracts;

public record LoginRequest(
    [Required(ErrorMessage = "El usuario es requerido.")] string Username,
    [Required(ErrorMessage = "La contraseña es requerida.")] string Password);

public record ProcessRequest(
    [Required(ErrorMessage = "La fecha de proceso es requerida.")] DateOnly? ProcessDate);
