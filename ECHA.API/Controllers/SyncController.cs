using Microsoft.AspNetCore.Mvc;
using EconomiaComHistoria.Infrastructure.Data;

namespace ECHA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly AppDbContext _context;

    public SyncController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("tentativas")]
    public async Task<IActionResult> UploadTentativas([FromBody] List<object> tentativas)
    {
        // TODO: Implement batch processing of offline quiz attempts
        // Validate client timestamp vs server
        return Ok(new { Message = "Sincronização concluída com sucesso." });
    }
}
