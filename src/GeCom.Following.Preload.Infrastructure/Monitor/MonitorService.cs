using Dapper;
using GeCom.Following.Preload.Application.Abstractions.Monitor;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GeCom.Following.Preload.Infrastructure.Monitor;

/// <summary>
/// Service for querying the Monitores database using Dapper and direct SQL.
/// </summary>
internal sealed class MonitorService : IMonitorService
{
    private readonly string _connectionString;
    private readonly ILogger<MonitorService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonitorService"/> class.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="logger">Logger instance.</param>
    public MonitorService(IConfiguration configuration, ILogger<MonitorService> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);

        string? connectionString = configuration.GetConnectionString("MonitoresConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'MonitoresConnection' not found.");
        }

        _connectionString = connectionString;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> GetSapDocumentNumberAsync(GetSapDocumentNumberRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting SapDocumentNumber for DocumentNumber: {DocumentNumber}, ProviderNumber: {ProviderNumber}, ClientNumber: {ClientNumber}, SalePoint: {SalePoint}, Letter: {Letter}",
            request.DocumentNumber, request.ProviderNumber, request.ClientNumber, request.SalePoint, request.Letter);

        ArgumentNullException.ThrowIfNull(request);

        const string sql = """
            SELECT SapDocumentNumber AS Value
            FROM [Monitores].[dbo].[Documents]
            WHERE DocumentNumber LIKE '%@DocumentNumber%'
              AND ProviderNumber = @ProviderNumber
              AND ClientNumber = @ClientNumber
              AND SalePoint = @SalePoint
              AND Letter = @Letter
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        int result = await connection.QuerySingleOrDefaultAsync<int>(
            new CommandDefinition(
                sql,
                new
                {
                    request.DocumentNumber,
                    request.ProviderNumber,
                    request.ClientNumber,
                    request.SalePoint,
                    request.Letter
                },
                cancellationToken: cancellationToken)).ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<int>> GetDocumentIdsBySapDocumentNumberAsync(int sapDocumentNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting DocumentIds for SapDocumentNumber: {SapDocumentNumber}", sapDocumentNumber);

        const string sql = """
            SELECT PD.DocId
            FROM [Precarga].[dbo].[Documentos] PD
            INNER JOIN [Monitores].[dbo].[Documents] MD on PD.idDocument = MD.idDocument
            WHERE MD.SapDocumentNumber = @SapDocumentNumber
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        IEnumerable<int> result = await connection.QueryAsync<int>(
            new CommandDefinition(
                sql,
                new { SapDocumentNumber = sapDocumentNumber },
                cancellationToken: cancellationToken)).ConfigureAwait(false);

        var list = result.ToList();
        _logger.LogInformation("Found {Count} DocumentId(s) for SapDocumentNumber: {SapDocumentNumber}", list.Count, sapDocumentNumber);

        return list;
    }
}
