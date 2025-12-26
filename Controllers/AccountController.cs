using Microsoft.AspNetCore.Mvc;

namespace BlazorApp1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAccounts()
    {
        var accounts = new[]
        {
            new { Id = 1, Name = "Demo User", Email = "demo@example.com" }
        };

        return Ok(accounts);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetAccount(int id)
    {
        if (id != 1)
        {
            return NotFound();
        }

        var account = new { Id = 1, Name = "Demo User", Email = "demo@example.com" };
        return Ok(account);
    }
}
