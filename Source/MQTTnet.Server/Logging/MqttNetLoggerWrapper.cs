using Microsoft.Extensions.Logging;
using MQTTnet.Diagnostics;
using System;
using System.Threading;

namespace MQTTnet.Server.Logging
{
    public sealed class MqttNetLoggerWrapper : IMqttNetLogger
    {
        readonly ILogger<MqttNetLoggerWrapper> _logger;

        public MqttNetLoggerWrapper(ILogger<MqttNetLoggerWrapper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public event EventHandler<MqttNetLogMessagePublishedEventArgs> LogMessagePublished;

        public IMqttNetScopedLogger CreateScopedLogger(string source)
        {
            return new MqttNetScopedLogger(this, source);
        }

        public void Publish(MqttNetLogLevel level, string source, string message, object[] parameters, Exception exception)
        {
            var convertedLogLevel = ConvertLogLevel(level);
            _logger.Log(convertedLogLevel, exception, message, parameters);

            if (LogMessagePublished == null)
            {
                return;
            }
            
            var logMessage = new MqttNetLogMessage
            {
                Timestamp = DateTime.UtcNow,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Source = source,
                Level = level,
                Message = message,
                Exception = exception
            };

            LogMessagePublished?.Invoke(this, new MqttNetLogMessagePublishedEventArgs(logMessage));
        }
        
        static LogLevel ConvertLogLevel(MqttNetLogLevel logLevel)
        {
            switch (logLevel)
            {
                case MqttNetLogLevel.Error: return LogLevel.Error;
                case MqttNetLogLevel.Warning: return LogLevel.Warning;
                case MqttNetLogLevel.Info: return LogLevel.Information;
                case MqttNetLogLevel.Verbose: return LogLevel.Debug;
            }

            return LogLevel.Debug;
        }
    }
}
