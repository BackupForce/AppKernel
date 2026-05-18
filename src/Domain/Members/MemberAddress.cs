using SharedKernel;

namespace Domain.Members;

public sealed class MemberAddress : Entity
{
    private MemberAddress(
        Guid id,
        Guid memberId,
        string receiverName,
        string phoneNumber,
        string country,
        string city,
        string district,
        string addressLine,
        bool isDefault)
        : base(id)
    {
        MemberId = memberId;
        ReceiverName = receiverName.Trim();
        PhoneNumber = phoneNumber.Trim();
        Country = country.Trim();
        City = city.Trim();
        District = district.Trim();
        AddressLine = addressLine.Trim();
        IsDefault = isDefault;
    }

    private MemberAddress()
    {
    }

    public Guid MemberId { get; private set; }

    public string ReceiverName { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string District { get; private set; } = string.Empty;

    public string AddressLine { get; private set; } = string.Empty;

    public bool IsDefault { get; private set; }

    public static MemberAddress Create(
        Guid memberId,
        string receiverName,
        string phoneNumber,
        string country,
        string city,
        string district,
        string addressLine,
        bool isDefault)
    {
        return new MemberAddress(
            Guid.NewGuid(),
            memberId,
            receiverName,
            phoneNumber,
            country,
            city,
            district,
            addressLine,
            isDefault);
    }

    public void Update(
        string receiverName,
        string phoneNumber,
        string country,
        string city,
        string district,
        string addressLine,
        bool isDefault)
    {
        ReceiverName = receiverName.Trim();
        PhoneNumber = phoneNumber.Trim();
        Country = country.Trim();
        City = city.Trim();
        District = district.Trim();
        AddressLine = addressLine.Trim();
        IsDefault = isDefault;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }
}
