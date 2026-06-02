using EconomiaComHistoria.API.DTOs;
using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using EconomiaComHistoria.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<List<QuizResponseDto>>> GetQuizzes([FromQuery] string? nivel, [FromQuery] string? tema)
    {
        var quizzes = await _quizRepository.GetAvailableQuizzesAsync(nivel, tema);
        var response = quizzes.Select(q => new QuizResponseDto(
            q.Id,
            q.Titulo,
            q.Descricao,
            q.NivelDificuldade,
            q.Tema,
            q.NumeroPerguntas,
            q.TempoPorPerguntaSegundos
        )).ToList();

        return Ok(response);
    }

    [HttpGet("{id}/start")]
    public async Task<ActionResult<QuizStartResponseDto>> StartQuiz(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var lastAttempt = await _quizRepository.GetLastAttemptAsync(userId, id);
        if (lastAttempt != null && (DateTime.UtcNow - lastAttempt.DataInicio).TotalHours < 24)
        {
            return BadRequest(new { message = "You must wait 24 hours between attempts for the same quiz." });
        }

        var quiz = await _quizRepository.GetByIdAsync(id);
        if (quiz == null) return NotFound();

        var tentativa = new TentativaQuiz
        {
            UtilizadorId = userId,
            QuizId = id,
            DataInicio = DateTime.UtcNow,
            Completa = false,
            Pontuacao = 0
        };

        await _quizRepository.CreateAttemptAsync(tentativa);

        var randomQuestions = quiz.Perguntas
            .OrderBy(x => Guid.NewGuid())
            .Take(quiz.NumeroPerguntas)
            .Select(p => new PerguntaStartDto(
                p.Id,
                p.Texto,
                p.Opcoes.Select(o => new OpcaoStartDto(o.Id, o.Texto)).ToList()
            )).ToList();

        return Ok(new QuizStartResponseDto(tentativa.Id, randomQuestions));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult> CreateQuiz([FromBody] CreateQuizDto dto)
    {
        var quiz = new Quiz
        {
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            NivelDificuldade = dto.NivelDificuldade,
            Tema = dto.Tema,
            NumeroPerguntas = dto.NumeroPerguntas,
            TempoPorPerguntaSegundos = dto.TempoPorPerguntaSegundos,
            Perguntas = dto.Perguntas.Select(p => new Pergunta
            {
                Texto = p.Texto,
                TempoLimiteSegundos = p.TempoLimiteSegundos,
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
        quiz.Descricao = dto.Descricao;
        quiz.NivelDificuldade = dto.NivelDificuldade;
        quiz.Tema = dto.Tema;
        quiz.NumeroPerguntas = dto.NumeroPerguntas;
        quiz.TempoPorPerguntaSegundos = dto.TempoPorPerguntaSegundos;

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

        if (dto.Respostas.Count < quiz.NumeroPerguntas)
        {
            return BadRequest(new { message = "All questions must be answered." });
        }

        var respostasPersistidas = new List<RespostaPergunta>();
        var detalhada = new List<RespostaDetalhadaDto>();

        foreach (var r in dto.Respostas)
        {
            var pergunta = await _dbContext.Perguntas.FindAsync(r.PerguntaId);
            if (pergunta == null) return BadRequest(new { message = $"Pergunta {r.PerguntaId} not found." });

            var opcao = await _dbContext.OpcoesRespostas.FindAsync(r.OpcaoId);
            if (opcao == null) return BadRequest(new { message = $"Opcao {r.OpcaoId} not found." });

            var isCorrecta = opcao.IsCorrecta;
            respostasPersistidas.Add(new RespostaPergunta
            {
                TentativaQuizId = tentativa.Id,
                PerguntaId = r.PerguntaId,
                OpcaoRespostaId = r.OpcaoId,
                TempoRespostaMs = r.TempoMs,
                IsCorrecta = isCorrecta
            });

            detalhada.Add(new RespostaDetalhadaDto(
                pergunta.Id,
                pergunta.Texto,
                opcao.Id,
                opcao.Texto,
                isCorrecta,
                opcao.Explicacao
            ));
        }

        await _quizRepository.AddRespostasAsync(respostasPersistidas);

        // Calculate total score using the scoring service
        int pontuacao = _scoringService.CalcularPontuacao(tentativa, respostasPersistidas);
        tentativa.Pontuacao = pontuacao;
        tentativa.Completa = true;
        tentativa.DataFim = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        double percentagem = (double)respostasPersistidas.Count(r => r.IsCorrecta) / quiz.NumeroPerguntas * 100;

        return Ok(new QuizSubmissionResponseDto(pontuacao, percentagem, detalhada));
    }
}
