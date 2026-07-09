using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Services;

public class SeedService : ISeedService
{
    private readonly AppDbContext _context;

    public SeedService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> SeedDataAsync()
    {
        // Verificar se já existem quizzes
        var quizzesExistentes = await _context.Quizzes.CountAsync();
        if (quizzesExistentes > 0)
        {
            return "Dados já foram inseridos anteriormente.";
        }

        try
        {
            // Quiz 1: História de Angola - Período Pré-Colonial
            var quiz1 = new Quiz
            {
                Titulo = "História de Angola: Período Pré-Colonial",
                Tema = "História",
                Nivel = NivelDificuldade.Basico,
                TotalPerguntas = 5,
                TempoLimiteSeg = 120,
                Ativo = true
            };

            var pergunta1_1 = new Pergunta
            {
                Enunciado = "Qual era o reino mais poderoso em Angola antes da colonização?",
                Explicacao = "O Reino do Kongo foi um dos reinos mais poderosos da região de Angola durante o período pré-colonial.",
                Pontos = 10,
                Tema = "História Pré-Colonial",
                Dificuldade = NivelDificuldade.Basico,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "Reino do Kongo", IsCorrecta = true, Explicacao = "Correto! O Reino do Kongo era muito poderoso." },
                    new OpcaoResposta { Texto = "Império Zulu", IsCorrecta = false, Explicacao = "O Império Zulu era na África do Sul." },
                    new OpcaoResposta { Texto = "Império Português", IsCorrecta = false, Explicacao = "Portugal ainda não dominava a região." },
                    new OpcaoResposta { Texto = "Califado Islâmico", IsCorrecta = false, Explicacao = "O Califado era mais a norte." }
                }
            };

            var pergunta1_2 = new Pergunta
            {
                Enunciado = "Em que ano os portugueses chegaram a Angola?",
                Explicacao = "Os portugueses chegaram à costa de Angola em 1484 com Diogo Cão.",
                Pontos = 10,
                Tema = "História Pré-Colonial",
                Dificuldade = NivelDificuldade.Intermedio,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "1484", IsCorrecta = true, Explicacao = "Correto! Diogo Cão chegou em 1484." },
                    new OpcaoResposta { Texto = "1500", IsCorrecta = false, Explicacao = "Data posterior." },
                    new OpcaoResposta { Texto = "1450", IsCorrecta = false, Explicacao = "Data anterior." },
                    new OpcaoResposta { Texto = "1510", IsCorrecta = false, Explicacao = "Data posterior." }
                }
            };

            var pergunta1_3 = new Pergunta
            {
                Enunciado = "Qual era a principal atividade económica em Angola antes da colonização?",
                Explicacao = "A agricultura, pecuária e comércio eram as principais atividades económicas.",
                Pontos = 10,
                Tema = "Economia Pré-Colonial",
                Dificuldade = NivelDificuldade.Basico,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "Agricultura e pecuária", IsCorrecta = true, Explicacao = "Correto! Eram as principais atividades." },
                    new OpcaoResposta { Texto = "Indústria manufatureira", IsCorrecta = false, Explicacao = "Não havia indústria nesse período." },
                    new OpcaoResposta { Texto = "Turismo", IsCorrecta = false, Explicacao = "O turismo é uma atividade moderna." },
                    new OpcaoResposta { Texto = "Tecnologia", IsCorrecta = false, Explicacao = "A tecnologia é uma atividade moderna." }
                }
            };

            var pergunta1_4 = new Pergunta
            {
                Enunciado = "Qual era a língua predominante nos reinos de Angola?",
                Explicacao = "Bantu era a língua predominante nos reinos de Angola no período pré-colonial.",
                Pontos = 10,
                Tema = "História Pré-Colonial",
                Dificuldade = NivelDificuldade.Basico,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "Bantu", IsCorrecta = true, Explicacao = "Correto! Bantu era predominante." },
                    new OpcaoResposta { Texto = "Árabe", IsCorrecta = false, Explicacao = "Árabe era mais a norte." },
                    new OpcaoResposta { Texto = "Português", IsCorrecta = false, Explicacao = "Português era a língua dos colonizadores." },
                    new OpcaoResposta { Texto = "Inglês", IsCorrecta = false, Explicacao = "Inglês não era predominante." }
                }
            };

            var pergunta1_5 = new Pergunta
            {
                Enunciado = "Qual era a estrutura política dos reinos de Angola?",
                Explicacao = "Os reinos de Angola tinham uma estrutura monárquica com um rei (Mani) no topo.",
                Pontos = 10,
                Tema = "História Pré-Colonial",
                Dificuldade = NivelDificuldade.Intermedio,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "Monarquia com um Mani (Rei)", IsCorrecta = true, Explicacao = "Correto! Era uma monarquia." },
                    new OpcaoResposta { Texto = "Democracia", IsCorrecta = false, Explicacao = "Não era democracia." },
                    new OpcaoResposta { Texto = "República", IsCorrecta = false, Explicacao = "Não era república." },
                    new OpcaoResposta { Texto = "Ditadura militar", IsCorrecta = false, Explicacao = "Não era ditadura militar." }
                }
            };

            quiz1.Perguntas.Add(pergunta1_1);
            quiz1.Perguntas.Add(pergunta1_2);
            quiz1.Perguntas.Add(pergunta1_3);
            quiz1.Perguntas.Add(pergunta1_4);
            quiz1.Perguntas.Add(pergunta1_5);

            // Quiz 2: Colônia Portuguesa de Angola
            var quiz2 = new Quiz
            {
                Titulo = "A Colônia Portuguesa de Angola",
                Tema = "História",
                Nivel = NivelDificuldade.Intermedio,
                TotalPerguntas = 5,
                TempoLimiteSeg = 150,
                Ativo = true
            };

            var pergunta2_1 = new Pergunta
            {
                Enunciado = "Em que ano Angola se tornou uma colônia portuguesa oficial?",
                Explicacao = "Angola foi declarada colônia portuguesa em 1575 com a fundação de Luanda.",
                Pontos = 10,
                Tema = "Colonialismo",
                Dificuldade = NivelDificuldade.Intermedio,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "1575", IsCorrecta = true, Explicacao = "Correto! Luanda foi fundada em 1575." },
                    new OpcaoResposta { Texto = "1600", IsCorrecta = false, Explicacao = "Data posterior à fundação." },
                    new OpcaoResposta { Texto = "1484", IsCorrecta = false, Explicacao = "Era apenas um contato inicial." },
                    new OpcaoResposta { Texto = "1700", IsCorrecta = false, Explicacao = "Data muito posterior." }
                }
            };

            var pergunta2_2 = new Pergunta
            {
                Enunciado = "Quem fundou Luanda em 1575?",
                Explicacao = "Paulo Dias de Novais fundou Luanda como capital da colônia portuguesa.",
                Pontos = 10,
                Tema = "Colonialismo",
                Dificuldade = NivelDificuldade.Intermedio,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "Paulo Dias de Novais", IsCorrecta = true, Explicacao = "Correto! Fundador de Luanda." },
                    new OpcaoResposta { Texto = "Vasco da Gama", IsCorrecta = false, Explicacao = "Vasco da Gama explorou a Índia." },
                    new OpcaoResposta { Texto = "Bartolomeu Dias", IsCorrecta = false, Explicacao = "Bartolomeu Dias explorou a costa." },
                    new OpcaoResposta { Texto = "Cristóvão Colombo", IsCorrecta = false, Explicacao = "Cristóvão Colombo foi ao Brasil." }
                }
            };

            var pergunta2_3 = new Pergunta
            {
                Enunciado = "Qual era o produto mais importante do comércio colonial em Angola?",
                Explicacao = "O tráfico de escravos era o comércio mais importante durante o período colonial.",
                Pontos = 10,
                Tema = "Economia Colonial",
                Dificuldade = NivelDificuldade.Basico,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "Escravos", IsCorrecta = true, Explicacao = "Infelizmente, era o comércio mais importante." },
                    new OpcaoResposta { Texto = "Ouro", IsCorrecta = false, Explicacao = "Havia ouro, mas era secundário." },
                    new OpcaoResposta { Texto = "Diamantes", IsCorrecta = false, Explicacao = "Diamantes eram em outras colônias." },
                    new OpcaoResposta { Texto = "Especiarias", IsCorrecta = false, Explicacao = "Especiarias eram da Índia." }
                }
            };

            var pergunta2_4 = new Pergunta
            {
                Enunciado = "Quantos anos durou o domínio português sobre Angola?",
                Explicacao = "Portugal dominou Angola de 1575 até 1975 (400 anos).",
                Pontos = 10,
                Tema = "Colonialismo",
                Dificuldade = NivelDificuldade.Intermedio,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "400 anos", IsCorrecta = true, Explicacao = "Correto! De 1575 a 1975." },
                    new OpcaoResposta { Texto = "300 anos", IsCorrecta = false, Explicacao = "Durou mais tempo." },
                    new OpcaoResposta { Texto = "200 anos", IsCorrecta = false, Explicacao = "Durou mais tempo." },
                    new OpcaoResposta { Texto = "500 anos", IsCorrecta = false, Explicacao = "Durou menos tempo." }
                }
            };

            var pergunta2_5 = new Pergunta
            {
                Enunciado = "Em que ano Angola conquistou a independência?",
                Explicacao = "Angola conquistou a independência em 11 de novembro de 1975.",
                Pontos = 10,
                Tema = "Independência",
                Dificuldade = NivelDificuldade.Basico,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "1975", IsCorrecta = true, Explicacao = "Correto! 11 de novembro de 1975." },
                    new OpcaoResposta { Texto = "1974", IsCorrecta = false, Explicacao = "Data anterior." },
                    new OpcaoResposta { Texto = "1980", IsCorrecta = false, Explicacao = "Data posterior." },
                    new OpcaoResposta { Texto = "1960", IsCorrecta = false, Explicacao = "Data muito anterior." }
                }
            };

            quiz2.Perguntas.Add(pergunta2_1);
            quiz2.Perguntas.Add(pergunta2_2);
            quiz2.Perguntas.Add(pergunta2_3);
            quiz2.Perguntas.Add(pergunta2_4);
            quiz2.Perguntas.Add(pergunta2_5);

            // Quiz 3: Economia de Angola
            var quiz3 = new Quiz
            {
                Titulo = "Economia de Angola",
                Tema = "Economia",
                Nivel = NivelDificuldade.Avancado,
                TotalPerguntas = 5,
                TempoLimiteSeg = 180,
                Ativo = true
            };

            var pergunta3_1 = new Pergunta
            {
                Enunciado = "Qual é o principal produto de exportação de Angola?",
                Explicacao = "O petróleo é o principal produto de exportação de Angola, representando mais de 90% das exportações.",
                Pontos = 10,
                Tema = "Economia Moderna",
                Dificuldade = NivelDificuldade.Basico,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "Petróleo", IsCorrecta = true, Explicacao = "Correto! Petróleo é o principal." },
                    new OpcaoResposta { Texto = "Diamantes", IsCorrecta = false, Explicacao = "Diamantes também são importantes, mas secundários ao petróleo." },
                    new OpcaoResposta { Texto = "Café", IsCorrecta = false, Explicacao = "Café é um produto menor." },
                    new OpcaoResposta { Texto = "Magia", IsCorrecta = false, Explicacao = "Magia não é um produto económico." }
                }
            };

            var pergunta3_2 = new Pergunta
            {
                Enunciado = "Qual é a moeda oficial de Angola?",
                Explicacao = "O Kwanza (AOA) é a moeda oficial de Angola desde 1995.",
                Pontos = 10,
                Tema = "Economia Moderna",
                Dificuldade = NivelDificuldade.Basico,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "Kwanza (AOA)", IsCorrecta = true, Explicacao = "Correto! Kwanza é a moeda." },
                    new OpcaoResposta { Texto = "Dólar", IsCorrecta = false, Explicacao = "Dólar é dos EUA." },
                    new OpcaoResposta { Texto = "Euro", IsCorrecta = false, Explicacao = "Euro é europeu." },
                    new OpcaoResposta { Texto = "Real", IsCorrecta = false, Explicacao = "Real é brasileiro." }
                }
            };

            var pergunta3_3 = new Pergunta
            {
                Enunciado = "Qual é a capital económica de Angola?",
                Explicacao = "Luanda é a capital administrativa e económica de Angola.",
                Pontos = 10,
                Tema = "Geografia Económica",
                Dificuldade = NivelDificuldade.Basico,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "Luanda", IsCorrecta = true, Explicacao = "Correto! Luanda é a capital." },
                    new OpcaoResposta { Texto = "Benguela", IsCorrecta = false, Explicacao = "Benguela é uma cidade importante, mas não a capital." },
                    new OpcaoResposta { Texto = "Huambo", IsCorrecta = false, Explicacao = "Huambo é importante no interior." },
                    new OpcaoResposta { Texto = "Namibe", IsCorrecta = false, Explicacao = "Namibe é um porto importante." }
                }
            };

            var pergunta3_4 = new Pergunta
            {
                Enunciado = "Qual é a população aproximada de Angola?",
                Explicacao = "Angola tem uma população de aproximadamente 36 milhões de habitantes (2024).",
                Pontos = 10,
                Tema = "Demografia",
                Dificuldade = NivelDificuldade.Intermedio,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "36 milhões", IsCorrecta = true, Explicacao = "Correto! Aproximadamente 36 milhões." },
                    new OpcaoResposta { Texto = "10 milhões", IsCorrecta = false, Explicacao = "A população é maior." },
                    new OpcaoResposta { Texto = "50 milhões", IsCorrecta = false, Explicacao = "A população é menor." },
                    new OpcaoResposta { Texto = "100 milhões", IsCorrecta = false, Explicacao = "A população é muito menor." }
                }
            };

            var pergunta3_5 = new Pergunta
            {
                Enunciado = "Qual é o maior desafio económico atual de Angola?",
                Explicacao = "A diversificação económica é um dos maiores desafios, pois a economia é muito dependente do petróleo.",
                Pontos = 10,
                Tema = "Economia Moderna",
                Dificuldade = NivelDificuldade.Avancado,
                Opcoes = new List<OpcaoResposta>
                {
                    new OpcaoResposta { Texto = "Diversificação da economia", IsCorrecta = true, Explicacao = "Correto! Angola precisa diversificar." },
                    new OpcaoResposta { Texto = "Falta de petróleo", IsCorrecta = false, Explicacao = "Angola tem reservas significativas." },
                    new OpcaoResposta { Texto = "Excesso de indústria", IsCorrecta = false, Explicacao = "Falta indústria, não há excesso." },
                    new OpcaoResposta { Texto = "Demasiado crescimento", IsCorrecta = false, Explicacao = "O crescimento é insuficiente." }
                }
            };

            quiz3.Perguntas.Add(pergunta3_1);
            quiz3.Perguntas.Add(pergunta3_2);
            quiz3.Perguntas.Add(pergunta3_3);
            quiz3.Perguntas.Add(pergunta3_4);
            quiz3.Perguntas.Add(pergunta3_5);

            // Adicionar quizzes ao contexto
            await _context.Quizzes.AddAsync(quiz1);
            await _context.Quizzes.AddAsync(quiz2);
            await _context.Quizzes.AddAsync(quiz3);

            // Guardar tudo
            await _context.SaveChangesAsync();

            return "Dados de Angola inseridos com sucesso!";
        }
        catch (Exception ex)
        {
            return $"Erro ao inserir dados: {ex.Message}";
        }
    }
}
