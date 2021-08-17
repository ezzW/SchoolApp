using Authentication;
using AutoMapper;
using DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Configuration;
using WebAPI.Models;


namespace WebAPI.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        private readonly JwtConfig _jwtConfig;
        private readonly TokenValidationParameters _tokenValidationParams;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AccountController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IOptionsMonitor<JwtConfig> optionsMonitor,
            TokenValidationParameters tokenValidationParams, IMapper mapper)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _jwtConfig = optionsMonitor.CurrentValue;
            _tokenValidationParams = tokenValidationParams;
            _unitOfWork = unitOfWork;
            _mapper = mapper;

        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await userManager.FindByNameAsync(model.Username);
                if (existingUser == null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                                "Invalid login request"
                            },
                        Success = false
                    });
                }

                var isCorrect = await userManager.CheckPasswordAsync(existingUser, model.Password);

                if (!isCorrect)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                                "Invalid login request"
                            },
                        Success = false
                    });
                }

                var accessToken = GenerateAccessToken(existingUser);
                return Ok(accessToken);
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>() {
                        "Invalid payload"
                    },
                Success = false
            });
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await userManager.FindByEmailAsync(model.Email);

                if (existingUser != null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                                "Email already in use"
                            },
                        Success = false
                    });
                }

                var newUser = new ApplicationUser() { Email = model.Email, UserName = model.Username };
                var isCreated = await userManager.CreateAsync(newUser, model.Password);
                if (isCreated.Succeeded)
                {
                    var accessToken =  GenerateAccessToken(newUser);
                    return Ok(accessToken);
                }
                else
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = isCreated.Errors.Select(x => x.Description).ToList(),
                        Success = false
                    });
                }
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>() {
                        "Invalid model"
                    },
                Success = false
            });
        }

        [HttpPost]
        [Route("RegisterWithRole")]
        public async Task<IActionResult> RegisterWithRole([FromBody] RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await userManager.FindByEmailAsync(model.Email);

                if (existingUser != null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                                "Email already in use"
                            },
                        Success = false
                    });
                }

                var newUser = new ApplicationUser() { Email = model.Email, UserName = model.Username };
                var isCreated = await userManager.CreateAsync(newUser, model.Password);
                if (isCreated.Succeeded)
                {
                    if (!string.IsNullOrWhiteSpace(model.userRole.ToString()))
                    {
                        if (!await roleManager.RoleExistsAsync(model.userRole.ToString()))
                            await roleManager.CreateAsync(new IdentityRole(model.userRole.ToString()));
                        if (await roleManager.RoleExistsAsync(model.userRole.ToString()))
                        {
                            await userManager.AddToRoleAsync(newUser, model.userRole.ToString());
                        }
                    }
                    var accessToken =  GenerateAccessToken(newUser);
                    return Ok(accessToken);
                }
                else
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = isCreated.Errors.Select(x => x.Description).ToList(),
                        Success = false
                    });
                }
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>() {
                        "Invalid Model"
                    },
                Success = false
            });
        }

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (ModelState.IsValid)
            {
                var refreshLifetime = Convert.ToInt32(_configuration["JwtConfig:RefreshLifetime"]);
                var expiredToken = GetExpiredToken(tokenRequest.Token, _configuration["JwtConfig:Key"]);

                if (expiredToken == null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                            "Invalid tokens"
                        },
                        Success = false
                    });
                }

                var userName = expiredToken.Identity.Name;
                var user = await userManager.FindByNameAsync(userName);

                var token = _unitOfWork.RefreshTokens.FindByCondition(a => a.Token == tokenRequest.RefreshToken && a.UserId == user.Id).SingleOrDefault();

                if (token == null || token.AddedDate.AddMinutes(refreshLifetime) < DateTime.Now)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                            "Invalid tokens"
                        },
                        Success = false
                    });
                }

                var refToken = new RefreshToken
                {
                    UserId = user.Id,
                    Token = GenerateRefreshToken(),
                    AddedDate = DateTime.Now
                };
                _unitOfWork.RefreshTokens.Remove(token);
                _unitOfWork.RefreshTokens.Add(refToken);
                _unitOfWork.Complete();

                var accessToken =  GenerateAccessToken(user);
                return Ok(accessToken);
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>() {
                    "Invalid model"
                },
                Success = false
            });
        }
        [NonAction]
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        [NonAction]
        public AuthResult GenerateAccessToken(ApplicationUser user)
        {
            string key = _configuration["JwtConfig:Key"];
            int lifeTimeMinutes = Convert.ToInt32(_configuration["JwtConfig:LifeTime"]);
            string audience = _configuration["JwtConfig:Audience"];
            string issuer= _configuration["JwtConfig:Issuer"];

            var timeNow = DateTime.Now;
            var claimsValues = new Dictionary<string, string>();
            claimsValues[JwtRegisteredClaimNames.Sub] = user.Id;
            claimsValues[JwtRegisteredClaimNames.UniqueName] = user.UserName;
            claimsValues[JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString();
            claimsValues[JwtRegisteredClaimNames.Iat] = timeNow.ToString();

            var claimsList = new List<Claim>();
            foreach (KeyValuePair<string, string> i in claimsValues)
                claimsList.Add(new Claim(i.Key, i.Value));
            var CurentUserRoles = userManager.GetRolesAsync(user).Result;
            foreach (var item in CurentUserRoles)
            {
                claimsList.Add(new Claim(ClaimTypes.Role, item));
            }
            var claims = claimsList.ToArray();
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                signingCredentials: signingCredentials,
                claims: claims,
                notBefore: timeNow,
                expires: timeNow.AddMinutes(lifeTimeMinutes),
                audience: audience,
                issuer: issuer
                );
            var refToken = new RefreshToken
            {
                UserId = user.Id,
                Token = GenerateRefreshToken(),
                AddedDate = DateTime.Now,
            };
            _unitOfWork.RefreshTokens.Add(refToken);
            _unitOfWork.Complete();

            return new  AuthResult
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                RefreshToken = refToken.Token,
                Success = true
            };
        }
        [NonAction]
        public ClaimsPrincipal GetExpiredToken(string token, string key)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                    ValidateLifetime = false
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return null;
                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
