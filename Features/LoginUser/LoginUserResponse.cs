using MapeAda_Middleware.SharedModels.Users;

namespace MapeAda_Middleware.Features.LoginUser;

public sealed record LoginUserResponse(string Token, Usuario Usuario);