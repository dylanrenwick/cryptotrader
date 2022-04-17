using Moq;
using Xunit;

using Cryptotrader.Logging;

namespace Cryptotrader.Tests.Logging;

public class LoggerTest
{
    private readonly Mock<ILogDestination> logDestination;
    public LoggerTest()
    {
        logDestination = new Mock<ILogDestination>();
    }

    [Fact]
    public void LogMessage_CallsDestinationLog()
    {
        LogMessage message = new()
        {
            Level = LogLevel.Info,
            Message = "Test",
            Label = ""
        };

        logDestination.Setup(x => x.Log(message)).Verifiable();
        Logger log = new(logDestination.Object);

        log.LogMessage(message);

        logDestination.Verify(x => x.Log(message), Times.Once);
    }

    [Fact]
    public void Label_CreatesLinkedLogger()
    {
        LogMessage message = new()
        {
            Level = LogLevel.Info,
            Message = "Test",
            Label = ""
        };

        logDestination.Setup(x => x.Log(message)).Verifiable();
        Logger log = new Logger(logDestination.Object).Label("SUB");

        log.LogMessage(message);

        logDestination.Verify(x => x.Log(message), Times.Once);
    }

    [Fact]
    public void Label_WithLabel_PassesLabelToSublogger()
    {
        LogMessage? message = null;

        logDestination.Setup(x => x.Log(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(x => message = x);

        Logger log = new Logger(logDestination.Object).Label("SUB");

        log.Log("Test", LogLevel.Info);

        Assert.NotNull(message);
        Assert.Equal("SUB", message?.Label);
    }

    [Fact]
    public void Log_CallsDestinationLog()
    {
        LogMessage? message = null;

        logDestination.Setup(x => x.Log(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(x => message = x);

        Logger log = new(logDestination.Object);

        log.Log("Test", LogLevel.Info);

        Assert.NotNull(message);
        Assert.Equal("Test", message?.Message);
        Assert.Equal(LogLevel.Info, message?.Level);
    }

    [Fact]
    public void Log_WithLabel_PassesLabelToDestination()
    {
        LogMessage? message = null;

        logDestination.Setup(x => x.Log(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(x => message = x);

        Logger log = new(logDestination.Object, "LABEL");

        log.Log("Test", LogLevel.Info);

        Assert.NotNull(message);
        Assert.Equal("LABEL", message?.Label);
    }

    [Fact]
    public void Debug_SetsLogLevel()
    {
        LogMessage? message = null;

        logDestination.Setup(x => x.Log(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(x => message = x);

        Logger log = new(logDestination.Object);

        log.Debug("Test");

        Assert.NotNull(message);
        Assert.Equal(LogLevel.Debug, message?.Level);
    }

    [Fact]
    public void Info_SetsLogLevel()
    {
        LogMessage? message = null;

        logDestination.Setup(x => x.Log(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(x => message = x);

        Logger log = new(logDestination.Object);

        log.Info("Test");

        Assert.NotNull(message);
        Assert.Equal(LogLevel.Info, message?.Level);
    }

    [Fact]
    public void Alert_SetsLogLevel()
    {
        LogMessage? message = null;

        logDestination.Setup(x => x.Log(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(x => message = x);

        Logger log = new(logDestination.Object);

        log.Alert("Test");

        Assert.NotNull(message);
        Assert.Equal(LogLevel.Alert, message?.Level);
    }

    [Fact]
    public void Warn_SetsLogLevel()
    {
        LogMessage? message = null;

        logDestination.Setup(x => x.Log(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(x => message = x);

        Logger log = new(logDestination.Object);

        log.Warn("Test");

        Assert.NotNull(message);
        Assert.Equal(LogLevel.Warn, message?.Level);
    }

    [Fact]
    public void Error_SetsLogLevel()
    {
        LogMessage? message = null;

        logDestination.Setup(x => x.Log(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(x => message = x);

        Logger log = new(logDestination.Object);

        log.Error("Test");

        Assert.NotNull(message);
        Assert.Equal(LogLevel.Error, message?.Level);
    }

    [Fact]
    public void Crit_ThrowsException()
    {
        Logger log = new(logDestination.Object);

        Assert.Throws<CriticalException>(() => log.Crit("Test"));
    }

    [Fact]
    public void Crit_LogsMessage()
    {
        LogMessage? message = null;

        logDestination.Setup(x => x.Log(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(x => message = x).Verifiable();
        Logger log = new(logDestination.Object);

        try { log.Crit("Test"); }
        catch (CriticalException) { }
        finally
        {
            Assert.NotNull(message);
            Assert.Equal(LogLevel.Crit, message?.Level);

            logDestination.Verify(x => x.Log(It.IsAny<LogMessage>()), Times.Once);
        }
    }
}
