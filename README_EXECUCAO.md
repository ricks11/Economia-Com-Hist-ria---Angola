# Economia Com História - Angola 🇦🇴

Este é um sistema educacional gamificado focado no ensino de História e Economia de Angola. O projeto consiste em uma API Backend (.NET 10 + MySQL), uma Aplicação Web (ASP.NET Core MVC) e um Aplicativo Mobile (.NET MAUI).

---

## 🛠 Requisitos Necessários

1. **.NET SDK 10.0** ou superior.
2. **Workload MAUI** instalado (`dotnet workload install maui`).

---

## 🚀 Guia de Execução (Passo a Passo)

### 1. Preparação do Ambiente (.NET)
Abra um terminal na pasta raiz do projeto e execute:
```powershell
# Restaurar todas as dependências
dotnet restore "Economia Com História - Angola.slnx"
```

### 2. Executar o Backend (API)
Abra um terminal na pasta `ECHA.API` e execute:
```powershell
dotnet run
```
*   A API estará disponível em: `http://localhost:5194`
*   Documentação Swagger: `http://localhost:5194/swagger`
*   A base de dados já está hospedada e populada com os dados de Angola!

### 3. Executar a Aplicação Web (Admin/Editor)
Abra um **novo terminal** na pasta `ECHA.Web` e execute:
```powershell
dotnet run
```
*   A Web App estará disponível em: `http://localhost:5193`
*   Acesse `/PainelAdmin` para a área de gestão (requer login como Admin/Editor).

### 4. Executar o App Mobile
Abra um **novo terminal** na pasta `ECHA.Mobile` e execute a versão Windows (ou outra plataforma):
```powershell
# Versão Windows
dotnet run -f net10.0-windows10.0.19041.0

# Versão Android (requer Android SDK)
dotnet run -f net10.0-android
```

---

## 📖 Funcionalidades do Projeto
*   **Painel Web (Admin/Editor):** Gestão de conteúdos, escolas, turmas, gamificação (badges e métricas) e relatórios.
*   **Temas de Estudo:** Listagem de conteúdos históricos e econômicos de Angola consumidos via API.
*   **Sistema de Quiz:** Interface interativa para responder perguntas sobre os temas, com pontuação e bônus de velocidade.
*   **Gamificação:** Sistema de pontuação, níveis, badges e rankings (conquistas).
*   **Fórum:** Discussões sobre conteúdos e temas.
*   **Interface Moderna:** Dark Mode e Tailwind CSS.

---

## ⚠️ Solução de Problemas
*   **Erro "Address already in use":** Outra instância da API ou Web App está aberta. Feche todos os terminais `dotnet` ou finalize o processo no Gerenciador de Tarefas.
*   **Lista Vazia no App ou Web:** Certifique-se de que a API está rodando.
*   **Erro de Versão .NET:** O projeto foi atualizado para .NET 10.0 para compatibilidade com o ambiente atual.
*   **Erro Android Build Path Too Long:** Mova o projeto para um diretório com caminho mais curto (ex.: `C:\ECHA`).

---

## 🔑 Credenciais Padrão (para Testes)
*   **Admin:** username: admin, password: Admin123!
*   **Editor:** username: editor, password: Editor123!
*   **Usuário Comum:** username: user, password: User123!
