# Economia Com História - Angola 🇦🇴

Sistema educacional gamificado focado no ensino de História e Economia de Angola, composto por uma API Backend, uma Aplicação Web (admin/editor) e um Aplicativo Mobile (usuários finais).

---

## 📦 Estrutura do Projeto

| Projeto | Tipo | Tecnologias |
|---------|------|-------------|
| `Economia Com História - Angola.Core` | Biblioteca de Classes | .NET 10, DTOs, Enums, Interfaces, Value Objects |
| `ECHA.Infrastructure` | Biblioteca de Infraestrutura | .NET 10, EF Core 10, MySQL, Repositórios, Serviços |
| `ECHA.API` | API REST | .NET 10, ASP.NET Core, Swagger |
| `ECHA.Web` | Aplicação Web (MVC) | .NET 10, ASP.NET Core MVC, F#, Tailwind CSS |
| `ECHA.Mobile` | Aplicativo Multiplataforma | .NET 10, MAUI, CommunityToolkit.Mvvm, Native AOT |

---

## 🔑 Principais Funcionalidades

### ✨ Gamificação
- Pontos por quiz, comentários aprovados, streak diário e exploração de novo tema
- Níveis e barra de progresso
- Badges (conquistas) desbloqueáveis com animação
- Ranking semanal e histórico, filtrável por escola, município e país

### 📚 Conteúdo Educacional
- Conteúdos de História e Economia de Angola (textos, vídeos, podcasts)
- Conteúdos "com Jindungo" (opinião crítica e satírica) claramente identificados
- Filtros por tipo de conteúdo, tema, nível de dificuldade e região
- Modo de leitura offline para textos baixados
- Marcação de conteúdos como favoritos

### 🎯 Quiz Interativo
- Perguntas de múltipla escolha (4 opções) com temporizador regressivo
- Feedback imediato com explicação histórica
- Pontuação base + bônus de velocidade
- Resultado detalhado com percentagem de acertos e comparação com a média

### 🗨️ Fórum de Discussão
- Criação de tópicos (requer autenticação)
- Listagem por mais recentes, mais comentados e mais votados
- Respostas a tópicos e reações com emojis
- Denúncia de conteúdo (spam, desinformação, ofensivo)
- Moderação prévia para tópicos de usuários novos (< 5 publicações)
- Tópicos com resposta de especialista têm etiqueta visual de destaque

### 🔐 Gestão de Utilizadores
- Autenticação JWT
- Menores de 13 anos requerem consentimento parental
- Associação a escolas via código de convite
- Edição de perfil (nome, avatar, escola, preferências de notificações)
- Painel Admin/Editor (ECHA.Web) para gestão de conteúdos, escolas, turmas, badges e relatórios

---

## 🛠 Tecnologias Utilizadas
- **Back-end**: .NET 10, ASP.NET Core API, EF Core 10, MySQL
- **Front-end Web**: ASP.NET Core MVC (F#), Tailwind CSS
- **Mobile**: .NET 10 MAUI, CommunityToolkit.Mvvm
- **Padrões**: Clean Architecture, Unit of Work, Repository Pattern, MVVM

---

## 📄 Licença
[Detalhes da Licença aqui]
