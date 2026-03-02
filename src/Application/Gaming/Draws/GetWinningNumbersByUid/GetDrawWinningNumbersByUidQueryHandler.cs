using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using SharedKernel;

namespace Application.Gaming.Draws.GetWinningNumbersByUid;

internal sealed class GetDrawWinningNumbersByUidQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetDrawWinningNumbersByUidQuery, DrawWinningNumbersDto>
{
    public async Task<Result<DrawWinningNumbersDto>> Handle(
        GetDrawWinningNumbersByUidQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                d.id AS DrawId,
                d.draw_at AS DrawDateUtc,
                d.winning_numbers_raw AS WinningNumbersRaw
            FROM gaming.draws d
            WHERE d.id = @id
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        DrawWinningNumbersDataModel? data = await connection.QuerySingleOrDefaultAsync<DrawWinningNumbersDataModel>(
            sql,
            new { request.id });

        if (data is null)
        {
            return Result.Failure<DrawWinningNumbersDto>(
                Error.NotFound(
                    "Draw.NotFound",
                    $"Draw with uid '{request.id}' was not found."));
        }

        DrawWinningNumbersDto dto = DrawWinningNumbersDto.Create(
            data.DrawId,
            data.DrawDateUtc,
            data.WinningNumbersRaw);

        return Result.Success(dto);
    }

    private sealed record DrawWinningNumbersDataModel(
        Guid DrawId,
        DateTime DrawDateUtc,
        string WinningNumbersRaw);
}
