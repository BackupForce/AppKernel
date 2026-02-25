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
                d.uid AS Uid,
                d.draw_at AS DrawDateUtc,
                COALESCE(ARRAY_AGG(dwn.number ORDER BY dwn.position), ARRAY[]::integer[]) AS WinningNumbers
            FROM gaming.draws d
            LEFT JOIN gaming.draw_winning_numbers dwn
                ON dwn.draw_id = d.id
            WHERE d.uid = @Uid
            GROUP BY d.id, d.uid, d.draw_at
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        DrawWinningNumbersDataModel? data = await connection.QuerySingleOrDefaultAsync<DrawWinningNumbersDataModel>(
            sql,
            new { request.Uid });

        if (data is null)
        {
            return Result.Failure<DrawWinningNumbersDto>(
                Error.NotFound(
                    "Draw.NotFound",
                    $"Draw with uid '{request.Uid}' was not found."));
        }

        DrawWinningNumbersDto dto = DrawWinningNumbersDto.Create(
            data.DrawId,
            data.Uid,
            data.DrawDateUtc,
            data.WinningNumbers);

        return Result.Success(dto);
    }

    private sealed record DrawWinningNumbersDataModel(
        Guid DrawId,
        string Uid,
        DateTime DrawDateUtc,
        int[] WinningNumbers);
}
