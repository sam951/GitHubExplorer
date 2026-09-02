using GitHubExplorer.Contracts.DTO;
using MySqlConnector;

namespace GitHubExplorer.Api.Data;

public sealed class FavoritesRepository : IFavoritesRepository
{
    private readonly MySqlConnectionFactory _factory;

    public FavoritesRepository(MySqlConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<FavoriteDto>> GetAllAsync(CancellationToken ct)
    {
        const string sql = """
        SELECT id, github_id, name, full_name, owner, html_url, description, stars, note, created_at
        FROM favorites
        ORDER BY created_at DESC;
        """;

        return await QueryAsync(sql, _ => { }, MapToFavoriteDto, ct);
    }

    public async Task<int> AddAsync(CreateFavoriteRequest request, CancellationToken ct)
    {
        const string sql = """
        INSERT INTO favorites (github_id, name, full_name, owner, html_url, description, stars, note)
        VALUES (@github_id, @name, @full_name, @owner, @html_url, @description, @stars, @note);
        SELECT LAST_INSERT_ID();
        """;

        var newId = await ExecuteScalarAsync<long>(sql, cmd =>
        {
            AddParam(cmd, "@github_id", request.GithubId);
            AddParam(cmd, "@name", request.Name);
            AddParam(cmd, "@full_name", request.FullName);
            AddParam(cmd, "@owner", request.Owner);
            AddParam(cmd, "@html_url", request.HtmlUrl);
            AddParam(cmd, "@description", request.Description);
            AddParam(cmd, "@stars", request.Stars);
            AddParam(cmd, "@note", request.Note);
        }, ct);

        return (int)newId;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var affectedRows = await ExecuteNonQueryAsync(
            "DELETE FROM favorites WHERE id = @id;",
            cmd => AddParam(cmd, "@id", id), ct);

        return affectedRows > 0;
    }

    public async Task<bool> UpdateNoteAsync(int id, string? note, CancellationToken ct)
    {
        var affectedRows = await ExecuteNonQueryAsync(
            "UPDATE favorites SET note = @note WHERE id = @id;",
            cmd =>
            {
                AddParam(cmd, "@id", id);
                AddParam(cmd, "@note", note);
            }, ct);

        return affectedRows > 0;
    }

    public async Task<bool> ExistsAsync(long githubId, CancellationToken ct)
    {
        var count = await ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM favorites WHERE github_id = @github_id;",
            cmd => AddParam(cmd, "@github_id", githubId), ct);

        return count > 0;
    }

    private async Task<int> ExecuteNonQueryAsync(
        string sql, Action<MySqlCommand> configure, CancellationToken ct)
    {
        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        configure(cmd);
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    private async Task<T?> ExecuteScalarAsync<T>(
        string sql, Action<MySqlCommand> configure, CancellationToken ct)
    {
        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        configure(cmd);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result is null or DBNull ? default : (T)Convert.ChangeType(result, typeof(T));
    }

    private async Task<List<T>> QueryAsync<T>(
        string sql, Action<MySqlCommand> configure, Func<MySqlDataReader, T> map, CancellationToken ct)
    {
        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        configure(cmd);

        var results = new List<T>();
        await using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            results.Add(map(reader));

        return results;
    }

    private static void AddParam(MySqlCommand cmd, string name, object? value)
        => cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);

    private static FavoriteDto MapToFavoriteDto(MySqlDataReader reader) => new(
        Id: reader.GetInt32(reader.GetOrdinal("id")),
        GithubId: reader.GetInt64(reader.GetOrdinal("github_id")),
        Name: reader.GetString(reader.GetOrdinal("name")),
        FullName: reader.GetString(reader.GetOrdinal("full_name")),
        Owner: reader.GetString(reader.GetOrdinal("owner")),
        HtmlUrl: reader.GetString(reader.GetOrdinal("html_url")),
        Description: reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
        Stars: reader.GetInt32(reader.GetOrdinal("stars")),
        Note: reader.IsDBNull(reader.GetOrdinal("note")) ? null : reader.GetString(reader.GetOrdinal("note")),
        CreatedAt: reader.GetDateTime(reader.GetOrdinal("created_at")));
}
