using Xunit;

namespace Cryptotrader.Tests;

public class HistoricalValueTest
{
    [Fact]
    public void Current_EmptyValue_ReturnsNull()
    {
        HistoricalValue<string> historicalValue = new();

        Assert.Null(historicalValue.Current);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(4)]
    [InlineData(108)]
    public void Current_WithValue_ReturnsValue(int val)
    {
        HistoricalValue<int> historicalValue = new();
        historicalValue.Set(val);

        Assert.Equal(val, historicalValue.Current);
    }
}