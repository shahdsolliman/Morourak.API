using System.Text.Json;
using Morourak.API.Extensions.EnumParsing;
using Morourak.API.Extensions.JsonConverters;
using Morourak.Application.Enums.Admin;
using Morourak.Domain.Enums.Appointments;
using Morourak.Domain.Enums.Common;
using Xunit;

namespace Morourak.Tests;

public sealed class EnumDisplayNameParserTests
{
    [Fact]
    public void TryParse_ReplacementType_Allows_EnglishName_And_ArabicDisplay()
    {
        Assert.True(EnumDisplayNameParser.TryParse(typeof(ReplacementType), "Lost", out var lost1));
        Assert.Equal(ReplacementType.Lost, Assert.IsType<ReplacementType>(lost1));

        Assert.True(EnumDisplayNameParser.TryParse(typeof(ReplacementType), "lost", out var lost2));
        Assert.Equal(ReplacementType.Lost, Assert.IsType<ReplacementType>(lost2));

        Assert.True(EnumDisplayNameParser.TryParse(typeof(ReplacementType), "بدل فاقد", out var lost3));
        Assert.Equal(ReplacementType.Lost, Assert.IsType<ReplacementType>(lost3));
    }

    [Fact]
    public void TryParse_AppRole_Allows_IdentityRoleCodes_And_ArabicDisplay()
    {
        Assert.True(EnumDisplayNameParser.TryParse(typeof(AppRole), "ADMIN", out var admin1));
        Assert.Equal(AppRole.Admin, Assert.IsType<AppRole>(admin1));

        Assert.True(EnumDisplayNameParser.TryParse(typeof(AppRole), "مسؤول", out var admin2));
        Assert.Equal(AppRole.Admin, Assert.IsType<AppRole>(admin2));
    }

    [Fact]
    public void TryParse_AppointmentType_Allows_EnglishName_And_ArabicDisplay()
    {
        Assert.True(EnumDisplayNameParser.TryParse(typeof(AppointmentType), "Medical", out var medical1));
        Assert.Equal(AppointmentType.Medical, Assert.IsType<AppointmentType>(medical1));

        Assert.True(EnumDisplayNameParser.TryParse(typeof(AppointmentType), "كشف طبي", out var medical2));
        Assert.Equal(AppointmentType.Medical, Assert.IsType<AppointmentType>(medical2));
    }

    [Fact]
    public void TryParse_InvalidValue_ReturnsFalse()
    {
        Assert.False(EnumDisplayNameParser.TryParse(typeof(ReplacementType), "not-a-real-value", out _));
    }
}

public sealed class ArabicEnumConverterTests
{
    private sealed record Wrapper(ReplacementType ReplacementType);

    [Fact]
    public void JsonConverter_Serializes_As_ArabicDisplayName()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new ArabicEnumConverter());

        var json = JsonSerializer.Serialize(new Wrapper(ReplacementType.Lost), options);

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("بدل فاقد", doc.RootElement.GetProperty("replacementType").GetString());
    }

    [Theory]
    [InlineData("\"Lost\"", ReplacementType.Lost)]
    [InlineData("\"lost\"", ReplacementType.Lost)]
    [InlineData("\"بدل فاقد\"", ReplacementType.Lost)]
    [InlineData("0", ReplacementType.Lost)]
    public void JsonConverter_Deserializes_English_Arabic_And_Int(string jsonValue, ReplacementType expected)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new ArabicEnumConverter());

        var deserialized = JsonSerializer.Deserialize<ReplacementType>(jsonValue, options);
        Assert.Equal(expected, deserialized);
    }
}
