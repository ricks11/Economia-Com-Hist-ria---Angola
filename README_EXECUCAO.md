# Economia Com História - Angola 🇦🇴

Este é um sistema educacional gamificado focado no ensino de História e Economia de Angola. O projeto consiste em uma API Backend (.NET 10 + MySQL) e um Aplicativo Mobile (.NET MAUI).

---

## 🛠 Requisitos Necessários

1.  **.NET SDK 10.0** ou superior.
2.  **Workload MAUI** instalado (`dotnet workload install maui`).
3.  **XAMPP** (para o servidor MySQL).
4.  **MySQL Workbench** (opcional, para visualização de dados).

---

## 🚀 Guia de Execução (Passo a Passo)

### 1. Configuração do Banco de Dados (MySQL)
1.  Abra o **XAMPP Control Panel** e inicie (**Start**) o serviço **MySQL**.
2.  Certifique-se de que a porta `3306` está ativa.
3.  A API está configurada para conectar com o usuário `root` e **senha vazia**.

### 2. Preparação do Ambiente (.NET)
Abra um terminal na pasta raiz do projeto e execute:
```powershell
# Restaurar todas as dependências
dotnet restore "Economia Com História - Angola.slnx"

# Instalar a ferramenta de banco de dados (caso não tenha)
dotnet tool install --global dotnet-ef
```

### 3. Criar as Tabelas do Banco de Dados
Na pasta raiz do projeto, execute:
```powershell
dotnet ef database update --project ECHA.Infrastructure --startup-project ECHA.API
```

### 4. Executar o Backend (API)
Abra um terminal na pasta `ECHA.API` e execute:
```powershell
dotnet run
```
*   A API estará disponível em: `http://localhost:5194`
*   Documentação Swagger: `http://localhost:5194/swagger`

### 5. Inserir Dados Iniciais (Seed)
Com a API rodando, você **precisa** popular o banco com o conteúdo de Angola:
1.  Acesse `http://localhost:5194/swagger`.
2.  Procure por **POST `/api/seed`**.
3.  Clique em **Try it out** e depois em **Execute**.
4.  Deverá retornar a mensagem: *"Dados de Angola inseridos com sucesso!"*

### 6. Executar o Frontend (App Mobile)
Abra um **novo terminal** na pasta `ECHA.Mobile` e execute a versão Windows:
```powershell
dotnet run -f net10.0-windows10.0.19041.0
```

---

## 📖 Funcionalidades do Projeto
*   **Temas de Estudo:** Listagem de conteúdos históricos de Angola consumidos via API.
*   **Sistema de Quiz:** Interface interativa para responder perguntas sobre os temas.
*   **Gamificação:** Sistema de pontuação e verificação de respostas corretas em tempo real.
*   **Interface Moderna:** Visual escuro (Dark Mode) otimizado para educação.

---

## ⚠️ Solução de Problemas
*   **Erro "Address already in use":** Outra instância da API está aberta. Feche todos os terminais `dotnet` ou finalize o processo no Gerenciador de Tarefas.
*   **Lista Vazia no App:** Certifique-se de que a API está rodando e que você executou o passo do **Seed** no Swagger.
*   **Erro de Versão .NET:** O projeto foi atualizado para .NET 10.0 para compatibilidade com o ambiente atual.
