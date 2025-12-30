using Application.Auth;
using Application.IntegrationTests.Infrastructure;
using Domain.Members;
using Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.IntegrationTests.Auth;

public class LineLoginTests : BaseIntegrationTest
{
    public LineLoginTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Handle_Should_CreateMemberAndUser_WhenFirstLogin()
    {
        // 準備：建立第一次登入所需的 Line 資料
        string lineUserId = $"U{Faker.Random.AlphaNumeric(8)}";
        string lineUserName = Faker.Name.FullName();
        var command = new LoginOrRegisterByLineCommand(lineUserId, lineUserName);

        // 執行：送出匿名登入指令
        Result<LineLoginResultDto> result = await Sender.Send(command);

        // 驗證：確認成功且資料已建立
        result.IsSuccess.Should().BeTrue();
        result.Value.IsNewMember.Should().BeTrue();
        result.Value.AccessToken.Should().NotBeNullOrEmpty();

        Member? member = await DbContext.Members.FindAsync(result.Value.MemberId);
        User? user = await DbContext.Users.FindAsync(result.Value.UserId);
        MemberExternalIdentity? identity = await DbContext.MemberExternalIdentities
            .FirstOrDefaultAsync(x => x.Provider == "line" && x.ExternalUserId == lineUserId);

        member.Should().NotBeNull();
        user.Should().NotBeNull();
        identity.Should().NotBeNull();
        identity!.ExternalUserName.Should().Be(lineUserName);
    }

    [Fact]
    public async Task Handle_Should_ReuseExistingMember_WhenLoginAgain()
    {
        // 準備：先完成第一次登入並取得基準資料筆數
        string lineUserId = $"U{Faker.Random.AlphaNumeric(8)}";
        string lineUserName = Faker.Name.FullName();
        var firstCommand = new LoginOrRegisterByLineCommand(lineUserId, lineUserName);

        Result<LineLoginResultDto> firstResult = await Sender.Send(firstCommand);
        int memberCount = await DbContext.Members.CountAsync();
        int userCount = await DbContext.Users.CountAsync();
        int identityCount = await DbContext.MemberExternalIdentities.CountAsync();

        // 執行：再次使用相同 Line 身分登入
        Result<LineLoginResultDto> secondResult = await Sender.Send(firstCommand);

        // 驗證：確認未新增額外會員與使用者資料
        secondResult.IsSuccess.Should().BeTrue();
        secondResult.Value.IsNewMember.Should().BeFalse();
        secondResult.Value.MemberId.Should().Be(firstResult.Value.MemberId);
        secondResult.Value.UserId.Should().Be(firstResult.Value.UserId);

        (await DbContext.Members.CountAsync()).Should().Be(memberCount);
        (await DbContext.Users.CountAsync()).Should().Be(userCount);
        (await DbContext.MemberExternalIdentities.CountAsync()).Should().Be(identityCount);
    }

    [Fact]
    public async Task Handle_Should_UpdateExternalUserName_WhenNameChanged()
    {
        // 準備：先以舊名稱登入，再以新名稱登入
        string lineUserId = $"U{Faker.Random.AlphaNumeric(8)}";
        string originalName = Faker.Name.FullName();
        string updatedName = Faker.Name.FullName();

        var firstCommand = new LoginOrRegisterByLineCommand(lineUserId, originalName);
        await Sender.Send(firstCommand);

        var secondCommand = new LoginOrRegisterByLineCommand(lineUserId, updatedName);

        // 執行：使用新名稱再次登入
        Result<LineLoginResultDto> result = await Sender.Send(secondCommand);

        // 驗證：確認外部身份名稱已更新
        result.IsSuccess.Should().BeTrue();
        result.Value.IsNewMember.Should().BeFalse();

        MemberExternalIdentity identity = await DbContext.MemberExternalIdentities
            .FirstAsync(x => x.Provider == "line" && x.ExternalUserId == lineUserId);

        identity.ExternalUserName.Should().Be(updatedName);
    }
}
