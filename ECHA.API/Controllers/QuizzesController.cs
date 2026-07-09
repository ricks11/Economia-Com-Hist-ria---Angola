using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.API.Controllers;

[ApiController]
[Route("api/quizzes")]
[Authorize]
public class QuizzesController : ControllerBase
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuizScoringService _scoringService;
    private readonly AppDbContext _dbContext;

    public QuizzesController(IQuizRepository quizRepository, IQuizScoringService scoringService, AppDbContext dbContext)
    {
        _quizRepository = quizRepository;
        _scoringService = scoringService;
        _dbContext = dbContext;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<QuizResponseDto>>> GetQuizzes([FromQuery] NivelDificuldade? nivel, [FromQuery] string? tema)
    {
        var quizzes = await _quizRepository.GetAvailableQuizzesAsync(nivel, tema);
        var response = quizzes.Select(q => new QuizResponseDto(
            q.Id,
            q.Titulo,
            q.Tema,
            q.Nivel,
            q.TotalPerguntas,
            q.TempoLimiteSeg,
            q.Ativo
        )).ToList();

        return Ok(response);
    }

    [HttpGet("{id}/start")]
    public async Task<ActionResult<QuizStartResponseDto>> StartQuiz(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var lastAttempt = await _quizRepository.GetLastAttemptAsync(userId, id);
        if (lastAttempt != null && (DateTime.UtcNow - lastAttempt.DataHora).TotalHours < 24)
        {
            return BadRequest(new { message = "You must wait 24 hours between attempts for the same quiz." });
        }

        var quiz = await _quizRepository.GetByIdAsync(id);
        if (quiz == null) return NotFound();

        var tentativa = new TentativaQuiz
        {
            UtilizadorId = userId,
            QuizId = id,
            DataHora = DateTime.UtcNow,
            Completada = false,
            Pontuacao = 0
        };

        await _quizRepository.CreateAttemptAsync(tentativa);

        var randomQuestions = quiz.Perguntas
            .OrderBy(x => Guid.NewGuid())
            .Take(quiz.TotalPerguntas)
            .Select(p => new PerguntaStartDto(
                p.Id,
                p.Enunciado,
                p.Opcoes.Select(o => new OpcaoRespostaStartDto(o.Id, o.Texto)).ToList()
            )).ToList();

        return Ok(new QuizStartResponseDto(tentativa.Id, randomQuestions));
    }

    [HttpGet("{id}/stats")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult<QuizStatsDto>> GetQuizStats(int id)
    {
        var quiz = await _quizRepository.GetByIdAsync(id);
        if (quiz == null) return NotFound();

        var respostas = await _dbContext.RespostasPerguntas
            .Where(r => r.TentativaQuiz.QuizId == id)
            .ToListAsync();
        
        var totalTentativas = await _dbContext.TentativasQuiz.CountAsync(t => t.QuizId == id && t.Completada);

        var statsPerguntas = quiz.Perguntas.Select(p => {
            var respostasPergunta = respostas.Where(r => r.PerguntaId == p.Id).ToList();
            var totalRespostas = respostasPergunta.Count;
            var taxaAcerto = totalRespostas > 0 ? (double)respostasPergunta.Count(r => r.IsCorrecta) / totalRespostas * 100 : 0;
            var tempoMedio = totalRespostas > 0 ? respostasPergunta.Average(r => r.TempoRespostaMs) : 0;

            return new QuestionStatsDto(p.Id, p.Enunciado, totalRespostas, taxaAcerto, tempoMedio);
        }).ToList();

        return Ok(new QuizStatsDto(quiz.Id, quiz.Titulo, totalTentativas, statsPerguntas));
    }

    [HttpGet("pool")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult<List<PerguntaStartDto>>> GetQuestionPool([FromQuery] string? tema, [FromQuery] NivelDificuldade? nivel)
    {
        var query = _dbContext.Perguntas
            .Include(p => p.Opcoes)
            .Include(p => p.Quiz)
            .AsQueryable();

        if (!string.IsNullOrEmpty(tema))
            query = query.Where(p => p.Quiz.Tema == tema);
        
        if (nivel.HasValue)
            query = query.Where(p => p.Quiz.Nivel == nivel.Value);

        var perguntas = await query.ToListAsync();
        
        var response = perguntas.Select(p => new PerguntaStartDto(
            p.Id,
            p.Enunciado,
            p.Opcoes.Select(o => new OpcaoRespostaStartDto(o.Id, o.Texto)).ToList()
        )).ToList();

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult> CreateQuiz([FromBody] CreateQuizDto dto)
    {
        var quiz = new Quiz
        {
            Titulo = dto.Titulo,
            Nivel = dto.Nivel,
            Tema = dto.Tema,
            TotalPerguntas = dto.TotalPerguntas,
            TempoLimiteSeg = dto.TempoLimiteSeg,
            Perguntas = dto.Perguntas.Select(p => new Pergunta
            {
                Enunciado = p.Enunciado,
                Opcoes = p.Opcoes.Select(o => new OpcaoResposta
                {
                    Texto = o.Texto,
                    IsCorrecta = o.IsCorrecta,
                    Explicacao = o.Explicacao
                }).ToList()
            }).ToList()
        };

        await _quizRepository.CreateAsync(quiz);
        return CreatedAtAction(nameof(GetQuizzes), new { id = quiz.Id }, quiz);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult> UpdateQuiz(int id, [FromBody] UpdateQuizDto dto)
    {
        var quiz = await _quizRepository.GetByIdAsync(id);
        if (quiz == null) return NotFound();

        // In a real scenario, check if the current user is the author of the quiz
        quiz.Titulo = dto.Titulo;
        quiz.Tema = dto.Tema;
        quiz.Nivel = dto.Nivel;
        quiz.Tema = dto.Tema;
        quiz.TotalPerguntas = dto.TotalPerguntas;
        quiz.TempoLimiteSeg = dto.TempoLimiteSeg;

        await _quizRepository.UpdateAsync(quiz);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult> DeleteQuiz(int id)
    {
        var quiz = await _quizRepository.GetByIdAsync(id);
        if (quiz == null) return NotFound();

        await _quizRepository.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("tentativa")]
    public async Task<ActionResult<QuizSubmissionResponseDto>> SubmitQuiz([FromBody] SubmitTentativaDto dto)
    {
        var tentativa = await _quizRepository.GetAttemptByIdAsync(dto.TentativaId);
        if (tentativa == null) return NotFound();

        var quiz = await _quizRepository.GetByIdAsync(tentativa.QuizId);
        if (quiz == null) return NotFound();

        if (dto.Respostas.Count < quiz.TotalPerguntas)
        {
            return BadRequest(new { message = "All questions must be answered." });
        }

        var respostasPersistidas = new List<RespostaPergunta>();
        var detalhada = new List<RespostaDetalhadaDto>();

        var perguntaIds = dto.Respostas.Select(r => r.PerguntaId).ToList();
        var opcaoIds = dto.Respostas.Select(r => r.OpcaoRespostaId).ToList();

        var perguntas = await _dbContext.Perguntas
            .Where(p => perguntaIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var opcoes = await _dbContext.OpcoesResposta
            .Where(o => opcaoIds.Contains(o.Id))
            .ToDictionaryAsync(o => o.Id);

        foreach (var r in dto.Respostas)
        {
            if (!perguntas.TryGetValue(r.PerguntaId, out var pergunta))
                return BadRequest(new { message = $"Pergunta {r.PerguntaId} não encontrada." });

            if (!opcoes.TryGetValue(r.OpcaoRespostaId, out var opcao))
                return BadRequest(new { message = $"Opção {r.OpcaoRespostaId} não encontrada." });

            var isCorrecta = opcao.IsCorrecta;
            respostasPersistidas.Add(new RespostaPergunta
            {
                TentativaQuizId = (int)tentativa.Id,
                PerguntaId = r.PerguntaId,
                OpcaoRespostaId = r.OpcaoRespostaId,
                TempoRespostaMs = r.TempoRespostaSeg * 1000,
                IsCorrecta = isCorrecta
            });

            detalhada.Add(new RespostaDetalhadaDto(
                pergunta.Id,
                pergunta.Enunciado,
                opcao.Id,
                opcao.Texto,
                isCorrecta,
                opcao.Explicacao ?? string.Empty
            ));
        }

        await _quizRepository.AddRespostasAsync(respostasPersistidas);

        // Calculate total score using the scoring service
        int pontuacao = _scoringService.CalcularPontuacao(tentativa, respostasPersistidas);
        tentativa.Pontuacao = pontuacao;
        tentativa.Completada = true;
        tentativa.DataFim = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        double percentagem = (double)respostasPersistidas.Count(r => r.IsCorrecta) / quiz.TotalPerguntas * 100;

        return Ok(new QuizSubmissionResponseDto(pontuacao, percentagem, detalhada));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizResponseDto>> GetQuiz(
    int id,
    CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository.GetByIdAsync(id, cancellationToken);
        if (quiz is null)
            return NotFound(new { message = "Quiz não encontrado" });

        return Ok(new QuizResponseDto(
            quiz.Id,
            quiz.Titulo,
            quiz.Tema,
            quiz.Nivel,
            quiz.TotalPerguntas,
            quiz.TempoLimiteSeg,
            quiz.Ativo));
    }

    [HttpGet("{id}/perguntas")]
    [Authorize(Roles = "Admin,Editor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PerguntaStartDto>>> GetPerguntas(
    int id,
    CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository.GetByIdAsync(id, cancellationToken);
        if (quiz is null)
            return NotFound(new { message = "Quiz não encontrado" });

        var perguntas = quiz.Perguntas
            .Select(p => new PerguntaStartDto(
                p.Id,
                p.Enunciado,
                p.Opcoes.Select(o => new OpcaoRespostaStartDto(o.Id, o.Texto)).ToList()))
            .ToList();

        return Ok(perguntas);
    }

    [HttpGet("tentativas/historico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<QuizResultDto>>> GetHistoricoTentativas(
    CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var tentativas = await _dbContext.TentativasQuiz
            .Include(t => t.Quiz)
            .Where(t => t.UtilizadorId == userId && t.Completada)
            .OrderByDescending(t => t.DataHora)
            .Take(20)
            .Select(t => new QuizResultDto(
                t.QuizId,
                t.UtilizadorId,
                t.Pontuacao,
                t.BonusVelocidade,
                t.TempoGastoSeg,
                t.TotalPerguntas > 0
                    ? (float)t.Pontuacao / (t.TotalPerguntas * 100)
                    : 0f,
                t.TotalPerguntas,
                t.TotalCorretas,           // TotalCorretas — adiciona este campo à entidade ou calcula via join
                true))
            .ToListAsync(cancellationToken);

        return Ok(tentativas);
    }

    [HttpGet("{id}/detalhe")]
    [Authorize(Roles = "Admin,Editor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizDetalheDto>> GetQuizDetalhe(int id, CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository.GetByIdAsync(id, cancellationToken);
        if (quiz is null)
            return NotFound(new { message = "Quiz não encontrado" });

        var perguntas = quiz.Perguntas.Select(p => new PerguntaDetalheDto(
            p.Id,
            p.Enunciado,
            p.Opcoes.Select(o => new OpcaoRespostaDetalheDto(o.Id, o.Texto, o.IsCorrecta, o.Explicacao)).ToList()
        )).ToList();

        return Ok(new QuizDetalheDto(
            quiz.Id,
            quiz.Titulo,
            quiz.Tema,
            quiz.Nivel,
            quiz.TotalPerguntas,
            quiz.TempoLimiteSeg,
            quiz.Ativo,
            perguntas));
    }
}
