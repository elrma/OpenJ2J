using CommandLine;
using OpenJ2J.Extensions;
using OpenJ2J.J2J;
using OpenJ2J.J2J.Abstractions;
using OpenJ2J.J2J.IO;
using OpenJ2J.J2J.V3;
using OpenJ2J.Signature;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OpenJ2J
{
    public class Options
    {
        // Variables
        [Option('i', "input", Required = true, HelpText = "Set output to verbose messages.")]
        public string? Input { get; set; }

        [Option('o', "output", Required = false, HelpText = "Set output to verbose messages.")]
        public string? Output { get; set; }

        [Option('p', "password", Required = false, HelpText = "Set output to verbose messages.")]
        public string? Password { get; set; }

        // Modes

        [Option('v', "validate", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Validate { get; set; }

        [Option('m', "modulate", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Modulate { get; set; }

        [Option('d', "demodulate", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Demodulate { get; set; }
    }

    public class Program
    {
        private static Stopwatch _stopwatch = new Stopwatch();

        public static void Main(string[] args)
        {
            Console.WriteLine(@" _____                    ___  _____    ___ 
|  _  |                  |_  |/ __  \  |_  |
| | | |_ __   ___ _ __     | |`' / /'    | |
| | | | '_ \ / _ \ '_ \    | |  / /      | |
\ \_/ / |_) |  __/ | | /\__/ /./ /___/\__/ /
 \___/| .__/ \___|_| |_\____/ \_____/\____/ 
      | |                                   
      |_|                                   ");

            // Initializes a Serilog Logger.
            string fileName = @"data\logs\log-.log";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(fileName, restrictedToMinimumLevel: LogEventLevel.Verbose, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 100000)
                .CreateLogger();

            // Parses Commandline Arguments.
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    bool isModeSelected = false;

                    if (o.Validate)
                    {
                        isModeSelected = true;

                        try
                        {
                            using (J2JValidator validator = new V3Validator(J2JFileStream.Open(o.Input ?? string.Empty)))
                            {
                                _stopwatch.Restart();

                                bool result = validator.Validate();

                                _stopwatch.Stop();

                                Log.Information($"Validating...OK.({_stopwatch.ElapsedMilliseconds}ms)");
                                Log.Information($"J2J Validation Result : {result}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Validating...FAILURE.({_stopwatch.ElapsedMilliseconds}ms)");
                        }
                    }

                    if (o.Modulate)
                    {
                        isModeSelected = true;

                        try
                        {
                            using (J2JModulator modulator = new V3Modulator(J2JFileStream.Open(o.Input ?? string.Empty)))
                            {
                                Log.Information($"J2J Modulator is initialized. (INPUT : {o.Input}, OUTPUT : {o.Output})");
                                _stopwatch.Restart();

                                bool result = false;

                                if (string.IsNullOrEmpty(o.Password))
                                {
                                    result = modulator.Modulate(o.Output ?? string.Empty);
                                }
                                else
                                {
                                    result = modulator.Modulate(o.Output ?? string.Empty, o.Password);
                                }

                                _stopwatch.Stop();

                                string resultString = result ? "OK" : "FAILURE";
                                Log.Information($"Modulating...{resultString}.({_stopwatch.ElapsedMilliseconds}ms)");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Modulating...FAILURE.({_stopwatch.ElapsedMilliseconds}ms)");
                        }
                    }

                    if (o.Demodulate)
                    {
                        isModeSelected = true;

                        try
                        {
                            using (J2JDemodulator modulator = new V3Demodulator(J2JFileStream.Open(o.Input ?? string.Empty)))
                            {
                                Log.Information($"J2J Demodulator is initialized. (INPUT : {o.Input}, OUTPUT : {o.Output})");
                                _stopwatch.Restart();

                                bool result = false;

                                if (string.IsNullOrEmpty(o.Password))
                                {
                                    result = modulator.Demodulate(o.Output ?? string.Empty);
                                }
                                else
                                {
                                    result = modulator.Demodulate(o.Output ?? string.Empty, o.Password);
                                }

                                _stopwatch.Stop();

                                string resultString = result ? "OK" : "FAILURE";
                                Log.Information($"Demodulating...{resultString}.({_stopwatch.ElapsedMilliseconds}ms)");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Demodulating...FAILURE.({_stopwatch.ElapsedMilliseconds}ms)");
                        }
                    }

                    if (!isModeSelected)
                    {
                        Log.Warning("Operation mode is not selected.");
                    }
                });
        }
    }
}