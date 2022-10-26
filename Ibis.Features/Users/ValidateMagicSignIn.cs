using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ibis.Features.Users;

public record MagicSignInRequest(string Token, string Email, string? ReturnUrl);
public class ValidateMagicSignIn : PublicFeature<MagicSignInRequest, string>
{
    public ValidateMagicSignIn(IRepository<User> users, UserManager<User> userManager, IConfiguration configuration)
    {
        Users = users;
        UserManager = userManager;
        Configuration = configuration;
    }

    public IRepository<User> Users { get; }
    public UserManager<User> UserManager { get; }
    public IConfiguration Configuration { get; }

    public override async Task<string> ExecuteAsync(MagicSignInRequest request)
    {
        var user = Users.Query.FirstOrDefault(x => x.Email == request.Email);
        if (user == null)
            throw new NotAuthorizedException("Can't find user {request.Email}");

        var isValid = await UserManager.VerifyUserTokenAsync(user, "Default", "passwordless-auth", request.Token);
        
        if (isValid)
        {
            await SignInAsync(user);
            return CreateJwt(request);
        }
        return string.Empty;
    }

    private async Task SignInAsync(User user)
    {
        await UserManager.UpdateSecurityStampAsync(user);
        await HttpContext.SignInAsync(
         IdentityConstants.ApplicationScheme,
         new ClaimsPrincipal(
             new ClaimsIdentity(
                 new List<Claim>
                 {
                        new Claim("sub", user.Id)
                 },
                 IdentityConstants.ApplicationScheme
             )));
    }

    record Tokens(string? Token, string? RefreshToken);
    string CreateJwt(MagicSignInRequest request)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenKey = Encoding.UTF8.GetBytes(Configuration["JWT:Key"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.Email, request.Email)
            }),
            Expires = DateTime.UtcNow.AddMinutes(60*24),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
        };
        var jwToken = tokenHandler.CreateToken(tokenDescriptor);
        if (request.ReturnUrl is null)
        {
            return tokenHandler.WriteToken(jwToken);
        }

        return null;
    }
}
