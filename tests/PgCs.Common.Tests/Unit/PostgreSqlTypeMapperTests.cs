using PgCs.Common.Services;

namespace PgCs.Common.Tests.Unit;

/// <summary>
/// Тесты для PostgreSqlTypeMapper - маппер PostgreSQL типов в C# типы
/// </summary>
public sealed class PostgreSqlTypeMapperTests
{
    private readonly PostgreSqlTypeMapper _mapper = new();

    #region Numeric Types

    [Fact]
    public void MapType_SmallInt_ReturnsShort()
    {
        Assert.Equal("short", _mapper.MapType("smallint", false, false));
        Assert.Equal("short?", _mapper.MapType("smallint", true, false));
    }

    [Fact]
    public void MapType_Integer_ReturnsInt()
    {
        Assert.Equal("int", _mapper.MapType("integer", false, false));
        Assert.Equal("int?", _mapper.MapType("int", true, false));
    }

    [Fact]
    public void MapType_BigInt_ReturnsLong()
    {
        Assert.Equal("long", _mapper.MapType("bigint", false, false));
        Assert.Equal("long?", _mapper.MapType("int8", true, false));
    }

    [Fact]
    public void MapType_Decimal_ReturnsDecimal()
    {
        Assert.Equal("decimal", _mapper.MapType("decimal", false, false));
        Assert.Equal("decimal?", _mapper.MapType("numeric", true, false));
    }

    [Fact]
    public void MapType_Real_ReturnsFloat()
    {
        Assert.Equal("float", _mapper.MapType("real", false, false));
        Assert.Equal("float?", _mapper.MapType("float4", true, false));
    }

    [Fact]
    public void MapType_DoublePrecision_ReturnsDouble()
    {
        Assert.Equal("double", _mapper.MapType("double precision", false, false));
        Assert.Equal("double?", _mapper.MapType("float8", true, false));
    }

    [Fact]
    public void MapType_Money_ReturnsDecimal()
    {
        Assert.Equal("decimal", _mapper.MapType("money", false, false));
        Assert.Equal("decimal?", _mapper.MapType("money", true, false));
    }

    #endregion

    #region String Types

    [Fact]
    public void MapType_Varchar_ReturnsString()
    {
        Assert.Equal("string", _mapper.MapType("varchar", false, false));
        Assert.Equal("string", _mapper.MapType("character varying", false, false));
        Assert.Equal("string", _mapper.MapType("varchar", true, false)); // strings are nullable by default
    }

    [Fact]
    public void MapType_Text_ReturnsString()
    {
        Assert.Equal("string", _mapper.MapType("text", false, false));
        Assert.Equal("string", _mapper.MapType("citext", false, false));
    }

    [Fact]
    public void MapType_Char_ReturnsString()
    {
        Assert.Equal("string", _mapper.MapType("char", false, false));
        Assert.Equal("string", _mapper.MapType("character", false, false));
    }

    #endregion

    #region DateTime Types

    [Fact]
    public void MapType_Timestamp_ReturnsDateTime()
    {
        Assert.Equal("DateTime", _mapper.MapType("timestamp", false, false));
        Assert.Equal("DateTime?", _mapper.MapType("timestamp without time zone", true, false));
    }

    [Fact]
    public void MapType_TimestampWithTimeZone_ReturnsDateTimeOffset()
    {
        Assert.Equal("DateTimeOffset", _mapper.MapType("timestamp with time zone", false, false));
        Assert.Equal("DateTimeOffset?", _mapper.MapType("timestamptz", true, false));
    }

    [Fact]
    public void MapType_Date_ReturnsDateOnly()
    {
        Assert.Equal("DateOnly", _mapper.MapType("date", false, false));
        Assert.Equal("DateOnly?", _mapper.MapType("date", true, false));
    }

    [Fact]
    public void MapType_Time_ReturnsTimeOnly()
    {
        Assert.Equal("TimeOnly", _mapper.MapType("time", false, false));
        Assert.Equal("TimeOnly?", _mapper.MapType("time without time zone", true, false));
    }

    [Fact]
    public void MapType_Interval_ReturnsTimeSpan()
    {
        Assert.Equal("TimeSpan", _mapper.MapType("interval", false, false));
        Assert.Equal("TimeSpan?", _mapper.MapType("interval", true, false));
    }

    #endregion

    #region Boolean and UUID

    [Fact]
    public void MapType_Boolean_ReturnsBool()
    {
        Assert.Equal("bool", _mapper.MapType("boolean", false, false));
        Assert.Equal("bool?", _mapper.MapType("bool", true, false));
    }

    [Fact]
    public void MapType_Uuid_ReturnsGuid()
    {
        Assert.Equal("Guid", _mapper.MapType("uuid", false, false));
        Assert.Equal("Guid?", _mapper.MapType("uuid", true, false));
    }

    #endregion

    #region Binary and JSON

    [Fact]
    public void MapType_Bytea_ReturnsByteArray()
    {
        Assert.Equal("byte[]", _mapper.MapType("bytea", false, false));
        // byte[] is a reference type, so nullable doesn't add ?
        Assert.Equal("byte[]", _mapper.MapType("bytea", true, false));
    }

    [Fact]
    public void MapType_Json_ReturnsString()
    {
        Assert.Equal("string", _mapper.MapType("json", false, false));
        Assert.Equal("string", _mapper.MapType("jsonb", false, false));
    }

    [Fact]
    public void MapType_Xml_ReturnsString()
    {
        Assert.Equal("string", _mapper.MapType("xml", false, false));
    }

    #endregion

    #region Array Types

    [Fact]
    public void MapType_IntegerArray_ReturnsIntArray()
    {
        Assert.Equal("int[]", _mapper.MapType("integer", false, true));
        Assert.Equal("int[]?", _mapper.MapType("integer", true, true));
    }

    [Fact]
    public void MapType_TextArray_ReturnsStringArray()
    {
        Assert.Equal("string[]", _mapper.MapType("text", false, true));
        Assert.Equal("string[]?", _mapper.MapType("text", true, true));
    }

    [Fact]
    public void MapType_BooleanArray_ReturnsBoolArray()
    {
        Assert.Equal("bool[]", _mapper.MapType("boolean", false, true));
        Assert.Equal("bool[]?", _mapper.MapType("boolean", true, true));
    }

    #endregion

    #region Type with Parameters

    [Fact]
    public void MapType_VarcharWithLength_ReturnsString()
    {
        Assert.Equal("string", _mapper.MapType("varchar(100)", false, false));
        Assert.Equal("string", _mapper.MapType("character varying(255)", false, false));
    }

    [Fact]
    public void MapType_NumericWithPrecision_ReturnsDecimal()
    {
        Assert.Equal("decimal", _mapper.MapType("numeric(10,2)", false, false));
        Assert.Equal("decimal?", _mapper.MapType("decimal(18,4)", true, false));
    }

    [Fact]
    public void MapType_CharWithLength_ReturnsString()
    {
        Assert.Equal("string", _mapper.MapType("char(10)", false, false));
    }

    #endregion

    #region Network Types

    [Fact]
    public void MapType_Inet_ReturnsIPAddress()
    {
        Assert.Equal("System.Net.IPAddress", _mapper.MapType("inet", false, false));
    }

    [Fact]
    public void MapType_Cidr_ReturnsString()
    {
        Assert.Equal("string", _mapper.MapType("cidr", false, false));
    }

    [Fact]
    public void MapType_Macaddr_ReturnsPhysicalAddress()
    {
        Assert.Equal("System.Net.NetworkInformation.PhysicalAddress", _mapper.MapType("macaddr", false, false));
        Assert.Equal("System.Net.NetworkInformation.PhysicalAddress", _mapper.MapType("macaddr8", false, false));
    }

    #endregion

    #region Range Types

    [Fact]
    public void MapType_Int4Range_ReturnsNpgsqlRange()
    {
        Assert.Equal("NpgsqlRange<int>", _mapper.MapType("int4range", false, false));
    }

    [Fact]
    public void MapType_Int8Range_ReturnsNpgsqlRange()
    {
        Assert.Equal("NpgsqlRange<long>", _mapper.MapType("int8range", false, false));
    }

    [Fact]
    public void MapType_DateRange_ReturnsNpgsqlRange()
    {
        Assert.Equal("NpgsqlRange<DateOnly>", _mapper.MapType("daterange", false, false));
    }

    #endregion

    #region Geometry Types

    [Fact]
    public void MapType_Geometry_ReturnsObject()
    {
        Assert.Equal("object", _mapper.MapType("geometry", false, false));
        Assert.Equal("object", _mapper.MapType("geography", false, false));
        Assert.Equal("object", _mapper.MapType("point", false, false));
    }

    #endregion

    #region Unknown Types

    [Fact]
    public void MapType_UnknownType_ReturnsObject()
    {
        Assert.Equal("object", _mapper.MapType("custom_type", false, false));
        Assert.Equal("object", _mapper.MapType("unknown", false, false));
    }

    #endregion

    #region GetRequiredNamespace Tests

    [Fact]
    public void GetRequiredNamespace_Uuid_ReturnsSystemNamespace()
    {
        Assert.Equal("System", _mapper.GetRequiredNamespace("uuid"));
    }

    [Fact]
    public void GetRequiredNamespace_Inet_ReturnsSystemNet()
    {
        Assert.Equal("System.Net", _mapper.GetRequiredNamespace("inet"));
    }

    [Fact]
    public void GetRequiredNamespace_Macaddr_ReturnsSystemNetNetworkInformation()
    {
        Assert.Equal("System.Net.NetworkInformation", _mapper.GetRequiredNamespace("macaddr"));
        Assert.Equal("System.Net.NetworkInformation", _mapper.GetRequiredNamespace("macaddr8"));
    }

    [Fact]
    public void GetRequiredNamespace_RangeTypes_ReturnsNpgsqlTypes()
    {
        Assert.Equal("NpgsqlTypes", _mapper.GetRequiredNamespace("int4range"));
        Assert.Equal("NpgsqlTypes", _mapper.GetRequiredNamespace("int8range"));
        Assert.Equal("NpgsqlTypes", _mapper.GetRequiredNamespace("daterange"));
    }

    [Fact]
    public void GetRequiredNamespace_BitTypes_ReturnsSystemCollections()
    {
        Assert.Equal("System.Collections", _mapper.GetRequiredNamespace("bit"));
        Assert.Equal("System.Collections", _mapper.GetRequiredNamespace("varbit"));
    }

    [Fact]
    public void GetRequiredNamespace_CommonTypes_ReturnsNull()
    {
        Assert.Null(_mapper.GetRequiredNamespace("integer"));
        Assert.Null(_mapper.GetRequiredNamespace("text"));
        Assert.Null(_mapper.GetRequiredNamespace("boolean"));
        Assert.Null(_mapper.GetRequiredNamespace("varchar"));
    }

    #endregion

    #region Case Insensitivity

    [Fact]
    public void MapType_CaseInsensitive_WorksCorrectly()
    {
        Assert.Equal("int", _mapper.MapType("INTEGER", false, false));
        Assert.Equal("string", _mapper.MapType("VARCHAR", false, false));
        Assert.Equal("bool", _mapper.MapType("BOOLEAN", false, false));
        Assert.Equal("Guid", _mapper.MapType("UUID", false, false));
    }

    [Fact]
    public void GetRequiredNamespace_CaseInsensitive_WorksCorrectly()
    {
        Assert.Equal("System", _mapper.GetRequiredNamespace("UUID"));
        Assert.Equal("System.Net", _mapper.GetRequiredNamespace("INET"));
    }

    #endregion
}
