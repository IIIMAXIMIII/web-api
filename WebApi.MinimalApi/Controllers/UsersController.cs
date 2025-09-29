using System.Diagnostics;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
    private readonly IUserRepository userRepository;
    private readonly IMapper mapper;

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        this.userRepository = userRepository;
        this.mapper = mapper;
    }

    [HttpGet("{userId}", Name = nameof(GetUserById))]
    [Produces("application/json", "application/xml")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var user = userRepository.FindById(userId);
        if (user == null)
        {
            return NotFound();
        }
        var userDto = mapper.Map<UserDto>(user);
        
        return Ok(userDto);
    }

    [HttpPost]
    [Produces("application/json", "application/xml")]
    public IActionResult CreateUser([FromBody] UserCreateDto userDto)
    {
        if (userDto == null)
        {
            return BadRequest();
        }
        
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("Login", "Логин должен быть");
            return UnprocessableEntity(ModelState);
        }

        if (userDto.Login.Any(c => !char.IsLetterOrDigit(c)))
        {
            ModelState.AddModelError("Login", "Логин должен состоять только из цифр и букв");
            return UnprocessableEntity(ModelState);
        }
        
        var user = mapper.Map<UserEntity>(userDto);
        var createdUserEntity = userRepository.Insert(user);
        
        return CreatedAtRoute(
            nameof(GetUserById),
            new { userId = createdUserEntity.Id },
            createdUserEntity.Id);
    }
}