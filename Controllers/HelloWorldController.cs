using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GaesdeApi.Services.Interfaces;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloWorldController : ControllerBase
{
    private readonly IMongoTestService _mongoTestService;

    public HelloWorldController(IMongoTestService mongoTestService)
    {
        _mongoTestService = mongoTestService;
    }

    [HttpGet("publico")]
    public IActionResult GetPublic()
    {
        return Ok(new { message = Messages.Hello.Public });
    }

    [Authorize]
    [HttpGet("privado")]
    public IActionResult GetPrivate()
    {
        var dbStatus = _mongoTestService.TestConnection();
        return Ok(new { 
            message = Messages.Hello.Private,
            mongoStatus = dbStatus
        });
    }

    [Authorize]
    [HttpGet("administrador")]
    public IActionResult GetAdministrator()
    {
        return Utils.CheckAccessLevel(User, this, 0, Messages.Hello.Administrator);
    }

    [Authorize]
    [HttpGet("professor")]
    public IActionResult GetProfessor()
    {
        return Utils.CheckAccessLevel(User, this, 2, Messages.Hello.Professor);
    }

    [Authorize]
    [HttpGet("aluno")]
    public IActionResult GetStudent()
    {
        return Utils.CheckAccessLevel(User, this, 3, Messages.Hello.Student);
    }

    [Authorize]
    [HttpGet("vendedor")]
    public IActionResult GetSeller()
    {
        return Utils.CheckAccessLevel(User, this, 4, Messages.Hello.Seller);
    }
}