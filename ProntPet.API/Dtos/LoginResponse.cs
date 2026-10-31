namespace ProntPet.Dtos;

public record class LoginResponse(
    string Token,
    string TokenType,
    DateTime ExpiresAt,
    int TutorId,
    string Name,
    string Email
);
