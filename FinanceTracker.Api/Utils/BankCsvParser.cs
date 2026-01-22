using CsvHelper;
using CsvHelper.Configuration;
using FinanceTracker.Api.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FinanceTracker.Api.Utils;

public static class BankCsvParser
{
    public static List<Transaction> Parse(Stream csvStream)
    {
        using var reader = new StreamReader(csvStream);

        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            BadDataFound = null,
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim,
            DetectDelimiter = true,
            PrepareHeaderForMatch = args => Normalize(args.Header)
        };

        using var csv = new CsvReader(reader, cfg);

        csv.Read();
        csv.ReadHeader();

        var headers = csv.HeaderRecord ?? Array.Empty<string>();
        var headerSet = new HashSet<string>(headers.Select(Normalize));

        // pick best matches
        string? dateCol = Pick(headerSet, new[]
        {
            "date", "transactiondate", "postingdate", "postdate", "posteddate", "transdate"
        });

        string? descCol = Pick(headerSet, new[]
        {
            "description", "details", "memo", "name", "payee", "merchant", "transaction", "transactiondescription"
        });

        string? amountCol = Pick(headerSet, new[]
        {
            "amount", "transactionamount", "amt", "value"
        });

        string? debitCol = Pick(headerSet, new[] { "debit", "withdrawal", "withdrawals" });
        string? creditCol = Pick(headerSet, new[] { "credit", "deposit", "deposits" });

        string? typeCol = Pick(headerSet, new[] { "type", "transactiontype", "debitcredit", "drcr" });

        if (dateCol is null)
            throw new Exception("Couldn’t find a Date column. Common names: Date, Transaction Date, Posting Date.");

        if (descCol is null)
            throw new Exception("Couldn’t find a Description column. Common names: Description, Details, Memo, Name.");

        if (amountCol is null && (debitCol is null || creditCol is null))
            throw new Exception("Couldn’t find Amount OR a Debit+Credit pair. Your CSV needs either Amount, or both Debit and Credit.");

        var results = new List<Transaction>();

        while (csv.Read())
        {
            var dateRaw = Get(csv, dateCol);
            if (!TryParseDate(dateRaw, out var date)) continue;

            var descRaw = Get(csv, descCol);
            var description = (descRaw ?? "").Trim();
            if (string.IsNullOrWhiteSpace(description)) description = "Unknown";

            decimal amount;

            if (amountCol is not null)
            {
                var amtRaw = Get(csv, amountCol);
                if (!TryParseMoney(amtRaw, out amount)) continue;

                if (typeCol is not null && amount != 0 && amount > 0)
                {
                    var t = (Get(csv, typeCol) ?? "").ToLowerInvariant();
                    if (LooksDebit(t)) amount = -Math.Abs(amount);
                    else if (LooksCredit(t)) amount = Math.Abs(amount);
                }
            }
            else
            {
                // debit/credit split
                var debitRaw = Get(csv, debitCol!);
                var creditRaw = Get(csv, creditCol!);

                decimal debit = 0, credit = 0;
                TryParseMoney(debitRaw, out debit);
                TryParseMoney(creditRaw, out credit);

                amount = Math.Abs(credit) - Math.Abs(debit);
                if (amount == 0) continue;
            }

            results.Add(new Transaction
            {
                Date = date.Date,
                Description = description,
                Amount = amount,
                Merchant = GuessMerchant(description)
            });
        }

        return results;
    }
    // helpers
    static string? Get(CsvReader csv, string col)
    {
        try { return csv.GetField(col); }
        catch { return null; }
    }

    static string Normalize(string s)
        => Regex.Replace((s ?? "").Trim().ToLowerInvariant(), @"[^a-z0-9]+", "");

    static string? Pick(HashSet<string> headerSet, IEnumerable<string> candidates)
        => candidates.FirstOrDefault(headerSet.Contains);

    static bool TryParseDate(string? raw, out DateTime dt)
    {
        dt = default;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        // try multiple cultures
        return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
            || DateTime.TryParse(raw, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out dt)
            || DateTime.TryParse(raw, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt);
    }

    static bool TryParseMoney(string? raw, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var s = raw.Trim();

        // make parentheses mean negative
        var neg = s.StartsWith("(") && s.EndsWith(")");
        s = s.Trim('(', ')');

        // remove currency symbols and commas and whitespace
        s = s.Replace("$", "").Replace("£", "").Replace("€", "").Replace(",", "").Trim();

        if (!decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value) &&
            !decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out value) &&
            !decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
            return false;

        if (neg) value = -Math.Abs(value);
        return true;
    }

    static bool LooksDebit(string t)
        => t.Contains("debit") || t.Contains("withdraw") || t == "dr";

    static bool LooksCredit(string t)
        => t.Contains("credit") || t.Contains("deposit") || t == "cr";

    static string GuessMerchant(string description)
    {
        var d = (description ?? "").Trim();
        if (d.Length == 0) return "Unknown";
        var split = d.Split(new[] { " - ", "*", "  " }, StringSplitOptions.RemoveEmptyEntries);
        return (split.Length > 0 ? split[0] : d).Trim();
    }
}
