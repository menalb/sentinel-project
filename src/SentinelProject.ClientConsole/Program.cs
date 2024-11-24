using System.Net.Http.Json;

var apiUri = "https://localhost:7140/transactions";

var client = new HttpClient();
client.DefaultRequestHeaders.Add("x-api-key", "dG43cQyut?JH3-w");
Random rnd = new Random();

int i = 0;
while (i < 100)
{
    var mex = new SentinelProject.API.Features.ProcessTransaction.ProcessTransactionRequest(
        Guid.NewGuid(),
        Guid.Parse(i % 2 == 0 ? "f2887467-a266-4554-9b8c-51d8e52c7771" : "819af267-9ac2-4121-85d1-5bf6eab0cb25"),
        rnd.Next(0, 150),
        i % 10 == 0 ? "Trusted Country" : "Hostile Country",
        "Amazon",
        "Mobile",
        "Purchase",
        DateTime.UtcNow);

    try
    {
        var response = await client.PostAsJsonAsync(apiUri, mex);
        response.EnsureSuccessStatusCode();

        Console.WriteLine(response.Headers.Location);
    }
    catch(Exception e)
    {
        Console.WriteLine(e.Message);
    }
    Thread.Sleep(500);
    i++;
}

Console.ReadKey();