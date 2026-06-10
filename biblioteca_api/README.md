# Sistema de Biblioteca - Web API

## Descrição

API REST de gerenciamento de biblioteca desenvolvida com ASP.NET Core Minimal API, Entity Framework Core e banco de dados SQLite. Permite cadastrar livros e usuários, realizar empréstimos e devoluções com persistência de dados.

## Integrantes

- Gabriel Henrique Alvez Raatz
- Matheus Pupia

## Tecnologias

- .NET 10
- ASP.NET Core Minimal API
- Entity Framework Core
- SQLite

## Instruções de Execução

**Pré-requisito:** .NET 10 SDK instalado.

1. Clone o repositório:
   ```
   git clone <url-do-repositorio>
   cd biblioteca_api
   ```

2. Execute a API (as migrations são aplicadas automaticamente na inicialização):
   ```
   dotnet run
   ```

3. A API ficará disponível em `http://localhost:5175` (ou a porta exibida no terminal).

> Caso queira recriar o banco manualmente:
> ```
> dotnet ef database update
> ```

## Funcionalidades

### Usuários `/usuarios`
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/usuarios` | Lista todos os usuários |
| GET | `/usuarios/{id}` | Busca usuário por ID |
| GET | `/usuarios/{id}/emprestimos` | Busca emprestimos do usuário |
| POST | `/usuarios` | Cadastra novo usuário |
| PUT | `/usuarios/{id}` | Atualiza dados do usuário |
| DELETE | `/usuarios/{id}` | Remove usuário (bloqueia se tiver empréstimo ativo) |

### Livros `/livros`
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/livros` | Lista todos os livros |
| GET | `/livros/{id}` | Busca livro por ID |
| POST | `/livros` | Cadastra novo livro |
| PUT | `/livros/{id}` | Atualiza dados do livro |
| DELETE | `/livros/{id}` | Remove livro (bloqueia se estiver emprestado) |

### Empréstimos `/emprestimos`
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/emprestimos` | Lista todos os empréstimos (com livro e usuário) |
| GET | `/emprestimos/{id}` | Busca empréstimo por ID |
| POST | `/emprestimos` | Realiza empréstimo de um livro |
| PUT | `/emprestimos/{id}` | Atualiza prazo do empréstimo |
| DELETE | `/emprestimos/{id}` | Devolve livro e encerra empréstimo |

## Regras de Negócio

- Usuário não pode ser excluído se possuir empréstimo ativo
- Livro não pode ser excluído se estiver emprestado
- Empréstimo só pode ser realizado com livro disponível
- Prazo do empréstimo deve ser entre 1 e 30 dias
- Nomes de usuários são únicos (sem distinção de maiúsculas)
- Livros duplicados (mesmo título + autor + ano) não são permitidos
- Idade do usuário deve ser entre 0 e 120 anos
- Telefone deve conter apenas dígitos (10 ou 11 caracteres)
- Ano de publicação deve ser entre 0 e o ano atual
