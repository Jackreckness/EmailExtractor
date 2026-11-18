using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EmailExtractor.Services;
using NUnit.Framework;

namespace EmailExtractor.Tests.Integration;

[TestFixture]
[Category("Integration")]
public class ExtractEndpointTests
{
    private const string ExtractUrl = "/api/extract";
    private const string ListeningPrefix = "Now listening on: ";

    [Test]
    public Task Extract_ReturnsFieldsAndTaxForTheAssessmentExample() => WithApi(async client =>
    {
        const string email = """
            Hi Patricia,
            <expense><cost_centre>DEV632</cost_centre><total>35,000</total><payment_method>personal
            card</payment_method></expense>

            Please create a reservation at the <vendor>Seaside Steakhouse</vendor>.
            """;

        using var response = await client.PostAsJsonAsync(ExtractUrl, new { text = email });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var fields = body.GetProperty("extractedFields");
        var expense = fields.GetProperty("expense");
        var calculation = body.GetProperty("calculation");

        Assert.Multiple(() =>
        {
            Assert.That(expense.GetProperty("costCentre").GetString(), Is.EqualTo("DEV632"));
            Assert.That(expense.GetProperty("total").GetString(), Is.EqualTo("35,000"));
            Assert.That(expense.GetProperty("paymentMethod").GetString()?.ReplaceLineEndings("\n"), Is.EqualTo("personal\ncard"));
            Assert.That(fields.GetProperty("vendor").GetString(), Is.EqualTo("Seaside Steakhouse"));
            Assert.That(calculation.GetProperty("total").GetDecimal(), Is.EqualTo(35000m));
            Assert.That(calculation.GetProperty("salesTax").GetDecimal(), Is.EqualTo(4565.22m));
            Assert.That(calculation.GetProperty("totalExcludingTax").GetDecimal(), Is.EqualTo(30434.78m));
        });
    });

    [Test]
    public Task Extract_RejectsWholeEmailWithUnclosedOpeningTag() => WithApi(async client =>
    {
        using var response = await client.PostAsJsonAsync(ExtractUrl, new
        {
            text = "<expense><total>115</total></expense><vendor>Shop"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(body.GetProperty("message").GetString(), Is.EqualTo("Missing closing tag </vendor>."));
        Assert.That(body.TryGetProperty("extractedFields", out _), Is.False);
    });

    private static async Task WithApi(Func<HttpClient, Task> test)
    {
        var listening = new TaskCompletionSource<Uri>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var api = CreateApiProcess();
        api.EnableRaisingEvents = true;
        api.OutputDataReceived += (_, entry) =>
        {
            if (entry.Data is { } line)
            {
                var start = line.IndexOf(ListeningPrefix, StringComparison.Ordinal);
                if (start >= 0)
                {
                    listening.TrySetResult(new Uri(line[(start + ListeningPrefix.Length)..].Trim()));
                }
            }
        };
        api.ErrorDataReceived += (_, entry) =>
        {
            if (entry.Data is not null)
            {
                TestContext.Progress.WriteLine(entry.Data);
            }
        };
        api.Exited += (_, _) => listening.TrySetException(
            new InvalidOperationException($"The API exited during startup with code {api.ExitCode}."));

        api.Start();
        try
        {
            api.BeginOutputReadLine();
            api.BeginErrorReadLine();
            using var client = new HttpClient
            {
                BaseAddress = await listening.Task.WaitAsync(TimeSpan.FromSeconds(15)),
                Timeout = TimeSpan.FromSeconds(10)
            };

            await test(client);
        }
        finally
        {
            if (!api.HasExited)
            {
                api.Kill(entireProcessTree: true);
            }

            await api.WaitForExitAsync();
        }
    }

    private static Process CreateApiProcess()
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = AppContext.BaseDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        string[] arguments =
        [
            typeof(EmailExtractorService).Assembly.Location,
            "--environment", "Production",
            "--urls", "http://127.0.0.1:0",
            "--Logging:LogLevel:Microsoft.Hosting.Lifetime", "Information",
            "--ExtractionRules:TaxRate", "0.15"
        ];
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return new Process { StartInfo = startInfo };
    }
}
