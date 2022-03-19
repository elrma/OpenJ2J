using CommandLine;
using OpenJ2J.Extensions;
using OpenJ2J.J2J;
using OpenJ2J.J2J.Abstractions;
using OpenJ2J.J2J.IO;
using OpenJ2J.J2J.V3;
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
        [Option('i', "input", Required = true, HelpText = "Sets the path to the input file.")]
        public string? Input { get; set; }

        [Option('o', "output", Required = false, HelpText = "Sets the path to the output file.")]
        public string? Output { get; set; }

        [Option('p', "password", Required = false, HelpText = "Sets a password of a J2J file.")]
        public string? Password { get; set; }

        [Option('n', "method-number", Required = false, HelpText = "Sets a method which is used by modulator(1, 2, 3).", Default = 3)]
        public int MethodNumber { get; set; }

        [Option('f', "use-forcer", Required = false, HelpText = "Sets whether to force the file to be recovered.", Default = false)]
        public bool UseForcer { get; set; }

        // Modes

        [Option('s', "version-select", Required = false, HelpText = "Checks the version of a J2J file.")]
        public bool IsVersionSelectMode { get; set; }

        [Option('v', "validate", Required = false, HelpText = "Validates a J2J file.")]
        public bool IsValidateMode { get; set; }

        [Option('m', "modulate", Required = false, HelpText = "Modulates a file with J2J format.")]
        public bool IsModulateMode { get; set; }

        [Option('d', "demodulate", Required = false, HelpText = "Demodulates a J2J file.")]
        public bool IsDemodulateMode { get; set; }
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

                    if (o.IsVersionSelectMode)
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

                    if (o.IsValidateMode)
                    {
                        isModeSelected = true;

                        try
                        {
                            using (J2JValidator validator = new V3Validator(J2JFileStream.Open(o.Input ?? string.Empty)))
                            {
                                _stopwatch.Restart();

                                bool validationResult = validator.Validate();

                                bool checksumValidationResult = false;

                                if (string.IsNullOrEmpty(o.Password))
                                {
                                    checksumValidationResult = validator.ValidateWithChecksum();
                                }
                                else
                                {
                                    checksumValidationResult = validator.ValidateWithChecksum(o.Password);
                                }

                                _stopwatch.Stop();

                                Log.Information($"Signature Validity : {validationResult}");
                                Log.Information($"CRC32 Checksum Validity : {checksumValidationResult}");
                                Log.Information($"Validating...OK.({_stopwatch.ElapsedMilliseconds}ms)");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Validating...FAILURE.({_stopwatch.ElapsedMilliseconds}ms)");
                        }
                    }

                    if (o.IsModulateMode)
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

                    if (o.IsDemodulateMode)
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