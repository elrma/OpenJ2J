using CommandLine;
using OpenJ2J.Extensions;
using OpenJ2J.J2J;
using OpenJ2J.J2J.Abstractions;
using OpenJ2J.J2J.IO;
using OpenJ2J.J2J.V1;
using OpenJ2J.J2J.V2;
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

        [Option('n', "version-number", Required = false, HelpText = "Sets a method which is used by modulator(1, 2, 3).", Default = 3)]
        public int VersionNumber { get; set; }

        [Option('f', "use-forcer", Required = false, HelpText = "Sets whether to force the file to be recovered.", Default = false)]
        public bool UseForcer { get; set; }

        // Modes

        [Option('s', "select-version", Required = false, HelpText = "Checks the version of a J2J file.")]
        public bool IsVersionSelectorMode { get; set; }

        [Option('v', "validate", Required = false, HelpText = "Validates a J2J file.")]
        public bool IsValidatorMode { get; set; }

        [Option('m', "modulate", Required = false, HelpText = "Modulates a file with J2J format.")]
        public bool IsModulatorMode { get; set; }

        [Option('d', "demodulate", Required = false, HelpText = "Demodulates a J2J file.")]
        public bool IsDemodulatorMode { get; set; }
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

                    if (o.IsVersionSelectorMode)
                    {
                        isModeSelected = true;

                        try
                        {
                            _stopwatch.Restart();

                            using (FileStream stream = J2JFileStream.Open(o.Input ?? string.Empty))
                            {
                                J2JVersion version = J2JVersionSelector.SelectVersion(stream);

                                _stopwatch.Stop();

                                Log.Information($"J2J Version is selected. (Version : {version})");

                                Log.Information($"Selecting...OK.({_stopwatch.ElapsedMilliseconds}ms)");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Selecting...FAILURE.({_stopwatch.ElapsedMilliseconds}ms)");
                        }
                    }

                    if (o.IsValidatorMode)
                    {
                        isModeSelected = true;

                        try
                        {
                            _stopwatch.Restart();

                            Log.Information($"J2J Validator is initialized. (VERSION : Method{o.VersionNumber}, INPUT : {o.Input})");
                            using (J2JValidator validator = new V3Validator(J2JFileStream.Open(o.Input ?? string.Empty)))
                            {
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

                    if (o.IsModulatorMode)
                    {
                        isModeSelected = true;

                        try
                        {
                            _stopwatch.Restart();

                            // Sets the version of the file.
                            bool errorFlag = false;

                            J2JModulator? modulator = null;

                            switch (o.VersionNumber)
                            {
                                case 1:
                                    modulator = new V1Modulator(J2JFileStream.Open(o.Input ?? string.Empty));
                                    break;
                                case 2:
                                    modulator = new V2Modulator(J2JFileStream.Open(o.Input ?? string.Empty));
                                    break;
                                case 3:
                                    modulator = new V3Modulator(J2JFileStream.Open(o.Input ?? string.Empty));
                                    break;
                                default:
                                    errorFlag = true;
                                    break;
                            }

                            if (errorFlag)
                            {
                                Log.Warning($"An unavailable version has been selected. (VERSION NUMBER : {o.VersionNumber})");
                                return;
                            }

                            // Modulates the file.
                            Log.Information($"J2J Modulator is initialized. (VERSION : Method{o.VersionNumber}, INPUT : {o.Input}, OUTPUT : {o.Output ?? "Auto"})");

                            bool? result = false;

                            if (string.IsNullOrEmpty(o.Password))
                            {
                                result = modulator?.Modulate(o.Output ?? string.Empty);
                            }
                            else
                            {
                                result = modulator?.Modulate(o.Output ?? string.Empty, o.Password);
                            }

                            modulator?.Dispose();

                            _stopwatch.Stop();

                            string resultString = result == true ? "OK" : "FAILURE";
                            Log.Information($"Modulating...{resultString}.({_stopwatch.ElapsedMilliseconds}ms)");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Modulating...FAILURE.({_stopwatch.ElapsedMilliseconds}ms)");
                        }
                    }

                    if (o.IsDemodulatorMode)
                    {
                        isModeSelected = true;

                        try
                        {
                            _stopwatch.Restart();

                            FileStream stream = J2JFileStream.Open(o.Input ?? string.Empty);

                            // Checks the version of the file.
                            J2JVersion version = J2JVersionSelector.SelectVersion(stream);

                            bool errorFlag = false;

                            J2JDemodulator? demodulator = null;

                            switch (version)
                            {
                                case J2JVersion.Method1:
                                    demodulator = new V1Demodulator(stream);
                                    break;
                                case J2JVersion.Method2:
                                    demodulator = new V2Demodulator(stream);
                                    break;
                                case J2JVersion.Method3:
                                    demodulator = new V3Demodulator(stream);
                                    break;
                                default:
                                    errorFlag = true;
                                    break;
                            }

                            if (errorFlag)
                            {
                                Log.Warning($"An unavailable version has been selected. (VERSION NUMBER : {version})");
                                return;
                            }


                            // Demodulates the file.
                            Log.Information($"J2J Demodulator is initialized. (VERSION : Method{o.VersionNumber}, INPUT : {o.Input}, OUTPUT : {o.Output ?? "Auto"})");
                            _stopwatch.Restart();

                            bool? result = false;

                            if (string.IsNullOrEmpty(o.Password))
                            {
                                result = demodulator?.Demodulate(o.Output ?? string.Empty, o.UseForcer);
                            }
                            else
                            {
                                result = demodulator?.Demodulate(o.Output ?? string.Empty, o.UseForcer, o.Password);
                            }

                            demodulator?.Dispose();

                            _stopwatch.Stop();

                            string resultString = result == true ? "OK" : "FAILURE";
                            Log.Information($"Demodulating...{resultString}.({_stopwatch.ElapsedMilliseconds}ms)");

                            _stopwatch.Stop();
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