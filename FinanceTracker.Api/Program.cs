using CsvHelper;
using CsvHelper.Configuration;
using FinanceTracker.Api.Data;
using FinanceTracker.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using FinanceTracker.Api.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=finance.db"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:4173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/api/health", () => Results.Ok(new { ok = true }));


app.MapPost("/api/import/csv", async (HttpRequest request, AppDbContext db) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest("bad file upload. please use multipart/form-data.");

    var form = await request.ReadFormAsync();
    var file = form.Files["file"];
    if (file is null || file.Length == 0)
        return Results.BadRequest("No file uploaded.");

    using var stream = file.OpenReadStream();

    List<Transaction> tx;
    try
    {
        tx = BankCsvParser.Parse(stream);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }

    db.Transactions.AddRange(tx);
    await db.SaveChangesAsync();

    return Results.Ok(new { imported = tx.Count });
});

app.MapGet("/api/summary/monthly", async (int months, AppDbContext db) =>
{
    months = months <= 0 ? 6 : Math.Min(months, 24);
    var start = DateTime.Today.AddMonths(-months);

    var data = await db.Transactions
        .Where(t => t.Date >= start)
        .GroupBy(t => new { t.Date.Year, t.Date.Month })
        .Select(g => new
        {
            year = g.Key.Year,
            month = g.Key.Month,
            income = g.Where(x => x.Amount > 0).Sum(x => x.Amount),
            expense = -g.Where(x => x.Amount < 0).Sum(x => x.Amount),
            net = g.Sum(x => x.Amount)
        })
        .OrderBy(x => x.year).ThenBy(x => x.month)
        .ToListAsync();

    return Results.Ok(data);
});

app.MapGet("/api/top/merchants", async (string month, AppDbContext db) =>
{
    if (!DateTime.TryParse(month + "-01", out var dt))
        return Results.BadRequest("month must be YYYY-MM");

    var start = new DateTime(dt.Year, dt.Month, 1);
    var end = start.AddMonths(1);

    var data = await db.Transactions
        .Where(t => t.Date >= start && t.Date < end && t.Amount < 0)
        .GroupBy(t => t.Merchant)
        .Select(g => new { merchant = g.Key, spent = -g.Sum(x => x.Amount) })
        .OrderByDescending(x => x.spent)
        .Take(15)
        .ToListAsync();

    return Results.Ok(data);
});

app.MapGet("/api/biggest", async (string month, AppDbContext db) =>
{
    if (!DateTime.TryParse(month + "-01", out var dt))
        return Results.BadRequest("month must be YYYY-MM");

    var start = new DateTime(dt.Year, dt.Month, 1);
    var end = start.AddMonths(1);

    var data = await db.Transactions
        .Where(t => t.Date >= start && t.Date < end && t.Amount < 0)
        .OrderBy(t => t.Amount)
        .Take(20)
        .Select(t => new
        {
            date = t.Date.ToString("yyyy-MM-dd"),
            description = t.Description,
            merchant = t.Merchant,
            spent = -t.Amount
        })
        .ToListAsync();

    return Results.Ok(data);
});

app.MapGet("/api/fraud", async (string month, AppDbContext db) =>
{
    if (!DateTime.TryParse(month + "-01", out var dt))
        return Results.BadRequest("month must be YYYY-MM");

    var start = new DateTime(dt.Year, dt.Month, 1);
    var end = start.AddMonths(1);
    const decimal fraudThreshold = 400m;

    var data = await db.Transactions
        .Where(t => t.Date >= start && t.Date < end && t.Amount < 0 && -t.Amount >= fraudThreshold)
        .OrderBy(t => t.Amount)
        .Select(t => new
        {
            date = t.Date.ToString("yyyy-MM-dd"),
            description = t.Description,
            merchant = t.Merchant,
            spent = -t.Amount
        })
        .ToListAsync();

    return Results.Ok(data);
});

app.Run();

static string GuessMerchant(string description)
{
    var d = (description ?? "").Trim();
    if (d.Length == 0) return "Unknown";
    var split = d.Split(new[] { " - ", "*", "  " }, StringSplitOptions.RemoveEmptyEntries);
    return (split.Length > 0 ? split[0] : d).Trim();
}
