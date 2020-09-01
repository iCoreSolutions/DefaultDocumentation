using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System;

namespace MarkDocGen
{
   public class ColoredConsoleSink : ILogEventSink
   {
      private readonly ConsoleColor _defaultForeground = Console.ForegroundColor;
      private readonly ConsoleColor _defaultBackground = Console.BackgroundColor;

      private readonly ITextFormatter _formatter;

      public ColoredConsoleSink(ITextFormatter formatter)
      {
         _formatter = formatter;
      }

      public void Emit(LogEvent logEvent)
      {
         if (logEvent.Level >= LogEventLevel.Fatal)
         {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
         }
         else if (logEvent.Level >= LogEventLevel.Error)
         {
            Console.ForegroundColor = ConsoleColor.Red;
         }
         else if (logEvent.Level >= LogEventLevel.Warning)
         {
            Console.ForegroundColor = ConsoleColor.Yellow;
         }

         _formatter.Format(logEvent, Console.Out);
         Console.Out.Flush();

         Console.ResetColor();
      }
   }

   public static class ColoredConsoleSinkExtensions
   {
      public static LoggerConfiguration ColoredConsole(
          this LoggerSinkConfiguration loggerConfiguration,
          LogEventLevel minimumLevel = LogEventLevel.Verbose,
          string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
          IFormatProvider formatProvider = null)
      {
         return loggerConfiguration.Sink(new ColoredConsoleSink(new MessageTemplateTextFormatter(outputTemplate, formatProvider)), minimumLevel);
      }
   }
}
