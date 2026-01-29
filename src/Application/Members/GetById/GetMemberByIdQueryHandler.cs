using System.Data;
using System.Linq;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Members.Dtos;
using Application.Members.Profiles;
using Dapper;
using Domain.Members;
using Domain.Users;
using SharedKernel;

namespace Application.Members.GetById;

internal sealed class GetMemberByIdQueryHandler(
    IDbConnectionFactory factory,
    ITenantContext tenantContext)
    : IQueryHandler<GetMemberByIdQuery, MemberDetailDto>
{
    public async Task<Result<MemberDetailDto>> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                m.id AS Id,
                m.user_id AS UserId,
                m.member_no AS MemberNo,
                m.display_name AS DisplayName,
                m.status AS Status,
                m.created_at AS CreatedAt,
                m.updated_at AS UpdatedAt,
                mp.member_id AS ProfileMemberId,
                mp.real_name AS ProfileRealName,
                mp.gender AS ProfileGender,
                mp.phone_number AS ProfilePhoneNumber,
                mp.phone_verified AS ProfilePhoneVerified,
                mp.updated_at_utc AS ProfileUpdatedAtUtc,
                lb.id AS LoginBindingId,
                lb.provider AS LoginBindingProvider,
                lb.provider_key AS LoginBindingProviderKey,
                lb.display_name AS LoginBindingDisplayName,
                lb.picture_url AS LoginBindingPictureUrl,
                lb.email AS LoginBindingEmail,
                lb.created_at_utc AS LoginBindingCreatedAtUtc
            FROM members m
            LEFT JOIN member_profiles mp ON mp.member_id = m.id
            LEFT JOIN user_login_bindings lb ON lb.user_id = m.user_id AND lb.tenant_id = m.tenant_id
            WHERE m.id = @MemberId
              AND m.tenant_id = @TenantId
            """;

        using IDbConnection connection = factory.GetOpenConnection();

        List<MemberDetailRow> rows = (await connection.QueryAsync<MemberDetailRow>(
            sql,
            new
            {
                request.MemberId,
                tenantContext.TenantId
            })).ToList();

        if (rows.Count == 0)
        {
            return Result.Failure<MemberDetailDto>(MemberErrors.MemberNotFound);
        }

        MemberDetailRow first = rows[0];

        MemberProfileDto? profile = null;
        if (first.ProfileMemberId.HasValue)
        {
            profile = new MemberProfileDto(
                first.ProfileMemberId.Value,
                first.ProfileRealName,
                (Gender)first.ProfileGender.GetValueOrDefault(),
                first.ProfilePhoneNumber,
                first.ProfilePhoneVerified.GetValueOrDefault(),
                first.ProfileUpdatedAtUtc.GetValueOrDefault());
        }

        List<LoginBindingDto> loginBindings = new();
        foreach (MemberDetailRow row in rows)
        {
            if (!row.LoginBindingId.HasValue)
            {
                continue;
            }

            loginBindings.Add(new LoginBindingDto(
                row.LoginBindingId.Value,
                (LoginProvider)row.LoginBindingProvider.GetValueOrDefault(),
                row.LoginBindingProviderKey ?? string.Empty,
                row.LoginBindingDisplayName,
                row.LoginBindingPictureUrl,
                row.LoginBindingEmail,
                row.LoginBindingCreatedAtUtc.GetValueOrDefault()));
        }

        return new MemberDetailDto(
            first.Id,
            first.UserId,
            first.MemberNo,
            first.DisplayName,
            first.Status,
            first.CreatedAt,
            first.UpdatedAt,
            profile,
            loginBindings);
    }

    private sealed record MemberDetailRow(
        Guid Id,
        Guid? UserId,
        string MemberNo,
        string DisplayName,
        short Status,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        Guid? ProfileMemberId,
        string? ProfileRealName,
        short? ProfileGender,
        string? ProfilePhoneNumber,
        bool? ProfilePhoneVerified,
        DateTime? ProfileUpdatedAtUtc,
        Guid? LoginBindingId,
        int? LoginBindingProvider,
        string? LoginBindingProviderKey,
        string? LoginBindingDisplayName,
        string? LoginBindingPictureUrl,
        string? LoginBindingEmail,
        DateTime? LoginBindingCreatedAtUtc);
}
