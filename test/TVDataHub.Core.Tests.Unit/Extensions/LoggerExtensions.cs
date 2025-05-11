using Microsoft.Extensions.Logging;
using Moq;

namespace TVDataHub.Core.Tests.Unit.Extensions;

public static class LoggerExtensions
{
    public static void VerifyLogError<T>(this Mock<ILogger<T>> logger, string messageContains, Exception ex, Func<Times> times)
    {
        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(messageContains)),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    public static void VerifyLogInfo<T>(this Mock<ILogger<T>> logger, string messageContains, Func<Times> times)
    {
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(messageContains)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), times);
    }

    public static void VerifyLogWarning<T>(this Mock<ILogger<T>> logger, string messageContains, Func<Times> times)
    {
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(messageContains)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), times);
    }
}