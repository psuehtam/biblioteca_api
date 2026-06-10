using biblioteca_api.Data;
using biblioteca_api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDataContext>(options =>
    options.UseSqlite("Data Source=biblioteca.db"));

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDataContext>();
    db.Database.Migrate();
}

//usuarios

app.MapGet("/usuarios", async (AppDataContext db) =>
    Results.Ok(await db.Usuarios.ToListAsync()));

app.MapGet("/usuarios/{id}", async (int id, AppDataContext db) =>
{
    var usuario = await db.Usuarios.FindAsync(id);
    return usuario is null ? Results.NotFound("Usuário não encontrado.") : Results.Ok(usuario);
});

app.MapGet("/usuarios/{id}/emprestimos", async (int id, AppDataContext db) =>
{
    var usuario = await db.Usuarios.FindAsync(id);
    if (usuario is null)
        return Results.NotFound("Usuário não encontrado.");

    var emprestimos = await db.Emprestimos
        .Include(e => e.Livro)
        .Include(e => e.Usuario)
        .Where(e => e.UsuarioId == id)
        .ToListAsync();

    return Results.Ok(emprestimos);
});

app.MapPost("/usuarios", async (Usuario usuario, AppDataContext db) =>
{
    if (string.IsNullOrWhiteSpace(usuario.Nome))
        return Results.BadRequest("Nome é obrigatório.");

    if (usuario.Idade < 0 || usuario.Idade > 120)
        return Results.BadRequest("Idade deve ser entre 0 e 120.");

    if (string.IsNullOrWhiteSpace(usuario.Telefone)
        || !usuario.Telefone.All(char.IsDigit)
        || usuario.Telefone.Length < 10
        || usuario.Telefone.Length > 11)
        return Results.BadRequest("Telefone deve conter apenas dígitos (10 ou 11 caracteres).");

    bool nomeExiste = await db.Usuarios.AnyAsync(u => u.Nome.ToLower() == usuario.Nome.ToLower());
    if (nomeExiste)
        return Results.Conflict("Já existe um usuário com este nome.");

    db.Usuarios.Add(usuario);
    await db.SaveChangesAsync();
    return Results.Created($"/usuarios/{usuario.Id}", usuario);
});

app.MapPut("/usuarios/{id}", async (int id, Usuario dados, AppDataContext db) =>
{
    var usuario = await db.Usuarios.FindAsync(id);
    if (usuario is null)
        return Results.NotFound("Usuário não encontrado.");

    if (string.IsNullOrWhiteSpace(dados.Nome))
        return Results.BadRequest("Nome é obrigatório.");

    if (dados.Idade < 0 || dados.Idade > 120)
        return Results.BadRequest("Idade deve ser entre 0 e 120.");

    if (string.IsNullOrWhiteSpace(dados.Telefone)
        || !dados.Telefone.All(char.IsDigit)
        || dados.Telefone.Length < 10
        || dados.Telefone.Length > 11)
        return Results.BadRequest("Telefone deve conter apenas dígitos (10 ou 11 caracteres).");

    bool nomeExiste = await db.Usuarios.AnyAsync(u => u.Nome.ToLower() == dados.Nome.ToLower() && u.Id != id);
    if (nomeExiste)
        return Results.Conflict("Já existe um usuário com este nome.");

    usuario.Nome = dados.Nome;
    usuario.Idade = dados.Idade;
    usuario.Telefone = dados.Telefone;
    await db.SaveChangesAsync();
    return Results.Ok(usuario);
});

app.MapDelete("/usuarios/{id}", async (int id, AppDataContext db) =>
{
    var usuario = await db.Usuarios.FindAsync(id);
    if (usuario is null)
        return Results.NotFound("Usuário não encontrado.");

    bool temEmprestimo = await db.Emprestimos.AnyAsync(e => e.UsuarioId == id);
    if (temEmprestimo)
        return Results.BadRequest("Usuário possui empréstimos ativos e não pode ser excluído.");

    db.Usuarios.Remove(usuario);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

//livros

app.MapGet("/livros", async (AppDataContext db) =>
    Results.Ok(await db.Livros.ToListAsync()));

app.MapGet("/livros/{id}", async (int id, AppDataContext db) =>
{
    var livro = await db.Livros.FindAsync(id);
    return livro is null ? Results.NotFound("Livro não encontrado.") : Results.Ok(livro);
});

app.MapPost("/livros", async (Livro livro, AppDataContext db) =>
{
    if (string.IsNullOrWhiteSpace(livro.Titulo))
        return Results.BadRequest("Título é obrigatório.");

    if (string.IsNullOrWhiteSpace(livro.Autor))
        return Results.BadRequest("Autor é obrigatório.");

    if (livro.AnoPublicacao < 0 || livro.AnoPublicacao > DateTime.Now.Year)
        return Results.BadRequest($"Ano de publicação deve ser entre 0 e {DateTime.Now.Year}.");

    bool duplicado = await db.Livros.AnyAsync(l =>
        l.Titulo.ToLower() == livro.Titulo.ToLower() &&
        l.Autor.ToLower() == livro.Autor.ToLower() &&
        l.AnoPublicacao == livro.AnoPublicacao);
    if (duplicado)
        return Results.Conflict("Já existe um livro com o mesmo título, autor e ano.");

    livro.Disponivel = true;
    db.Livros.Add(livro);
    await db.SaveChangesAsync();
    return Results.Created($"/livros/{livro.Id}", livro);
});

app.MapPut("/livros/{id}", async (int id, Livro dados, AppDataContext db) =>
{
    var livro = await db.Livros.FindAsync(id);
    if (livro is null)
        return Results.NotFound("Livro não encontrado.");

    if (string.IsNullOrWhiteSpace(dados.Titulo))
        return Results.BadRequest("Título é obrigatório.");

    if (string.IsNullOrWhiteSpace(dados.Autor))
        return Results.BadRequest("Autor é obrigatório.");

    if (dados.AnoPublicacao < 0 || dados.AnoPublicacao > DateTime.Now.Year)
        return Results.BadRequest($"Ano de publicação deve ser entre 0 e {DateTime.Now.Year}.");

    bool duplicado = await db.Livros.AnyAsync(l =>
        l.Titulo.ToLower() == dados.Titulo.ToLower() &&
        l.Autor.ToLower() == dados.Autor.ToLower() &&
        l.AnoPublicacao == dados.AnoPublicacao &&
        l.Id != id);
    if (duplicado)
        return Results.Conflict("Já existe um livro com o mesmo título, autor e ano.");

    livro.Titulo = dados.Titulo;
    livro.Autor = dados.Autor;
    livro.AnoPublicacao = dados.AnoPublicacao;
    await db.SaveChangesAsync();
    return Results.Ok(livro);
});

app.MapDelete("/livros/{id}", async (int id, AppDataContext db) =>
{
    var livro = await db.Livros.FindAsync(id);
    if (livro is null)
        return Results.NotFound("Livro não encontrado.");

    if (!livro.Disponivel)
        return Results.BadRequest("O livro está emprestado e não pode ser excluído.");

    db.Livros.Remove(livro);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

//emprestimos

app.MapGet("/emprestimos", async (AppDataContext db) =>
    Results.Ok(await db.Emprestimos
        .Include(e => e.Livro)
        .Include(e => e.Usuario)
        .ToListAsync()));

app.MapGet("/emprestimos/{id}", async (int id, AppDataContext db) =>
{
    var emprestimo = await db.Emprestimos
        .Include(e => e.Livro)
        .Include(e => e.Usuario)
        .FirstOrDefaultAsync(e => e.Id == id);
    return emprestimo is null ? Results.NotFound("Empréstimo não encontrado.") : Results.Ok(emprestimo);
});

app.MapPost("/emprestimos", async (EmprestimoRequest req, AppDataContext db) =>
{
    if (req.QuantidadeDias < 1 || req.QuantidadeDias > 30)
        return Results.BadRequest("Quantidade de dias deve ser entre 1 e 30.");

    var livro = await db.Livros.FindAsync(req.LivroId);
    if (livro is null)
        return Results.NotFound("Livro não encontrado.");

    if (!livro.Disponivel)
        return Results.BadRequest("O livro não está disponível para empréstimo.");

    var usuario = await db.Usuarios.FindAsync(req.UsuarioId);
    if (usuario is null)
        return Results.NotFound("Usuário não encontrado.");

    var emprestimo = new Emprestimo
    {
        LivroId = req.LivroId,
        UsuarioId = req.UsuarioId,
        DataEmprestimo = DateTime.Now,
        DataDevolucaoPrevista = DateTime.Now.AddDays(req.QuantidadeDias)
    };

    livro.Disponivel = false;
    db.Emprestimos.Add(emprestimo);
    await db.SaveChangesAsync();

    await db.Entry(emprestimo).Reference(e => e.Livro).LoadAsync();
    await db.Entry(emprestimo).Reference(e => e.Usuario).LoadAsync();

    return Results.Created($"/emprestimos/{emprestimo.Id}", emprestimo);
});

app.MapPut("/emprestimos/{id}", async (int id, EmprestimoAtualizarRequest req, AppDataContext db) =>
{
    var emprestimo = await db.Emprestimos.FindAsync(id);
    if (emprestimo is null)
        return Results.NotFound("Empréstimo não encontrado.");

    if (req.QuantidadeDias < 1 || req.QuantidadeDias > 30)
        return Results.BadRequest("Quantidade de dias deve ser entre 1 e 30.");

    emprestimo.DataDevolucaoPrevista = emprestimo.DataEmprestimo.AddDays(req.QuantidadeDias);
    await db.SaveChangesAsync();
    return Results.Ok(emprestimo);
});

app.MapDelete("/emprestimos/{id}", async (int id, AppDataContext db) =>
{
    var emprestimo = await db.Emprestimos
        .Include(e => e.Livro)
        .FirstOrDefaultAsync(e => e.Id == id);
    if (emprestimo is null)
        return Results.NotFound("Empréstimo não encontrado.");

    emprestimo.Livro.Disponivel = true;
    db.Emprestimos.Remove(emprestimo);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

record EmprestimoRequest(int LivroId, int UsuarioId, int QuantidadeDias);
record EmprestimoAtualizarRequest(int QuantidadeDias);
