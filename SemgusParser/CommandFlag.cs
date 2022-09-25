using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser
{
    /// <summary>
    /// A command flag, with both yes and no options, e.g., --foo and --no-foo
    /// </summary>
    internal class CommandFlag
    {
        /// <summary>
        /// The yes (--foo) option
        /// </summary>
        private readonly Option<bool> _yesOption;

        /// <summary>
        /// The no (--no-foo) option
        /// </summary>
        private readonly Option<bool> _noOption;

        /// <summary>
        /// The default value for this flag
        /// </summary>
        private readonly bool _defaultValue;

        /// <summary>
        /// Configures the given command with this flag
        /// </summary>
        /// <param name="command">Command to configure</param>
        public void ConfigureCommand(Command command)
        {
            command.AddOption(_yesOption);
            command.AddOption(_noOption);
        }

        /// <summary>
        /// Gets the value of this flag for the given context
        /// </summary>
        /// <param name="ic">Context</param>
        /// <returns>Flag value</returns>
        public bool GetFlag(InvocationContext ic)
        {
            var yres = ic.ParseResult.FindResultFor(_yesOption);
            var nres = ic.ParseResult.FindResultFor(_noOption);

            if (yres is null && nres is null)
            {
                return _defaultValue;
            }
            if (yres is null && nres is not null)
            {
                return false;
            }
            if (yres is not null && nres is null)
            {
                return true;
            }
            // Library error - should not be thrown in regular usage
            throw new InvalidOperationException("Got mutually exclusive options");
        }

        /// <summary>
        /// Creates a new command flag
        /// </summary>
        /// <param name="name">The flag name, e.g., if "foo", then "--foo" and "--no-foo" will be options</param>
        /// <param name="description">Description for the flag</param>
        /// <param name="defaultValue">Flag value if no options are specified</param>
        /// <param name="isHidden">If true, the options do not show up in the help text</param>
        public CommandFlag(string name, string? description = default, bool defaultValue = true, bool isHidden = false)
        {
            _defaultValue = defaultValue;

            _yesOption = new Option<bool>(
                name: $"--{name}",
                description: description
                );

            _noOption = new Option<bool>(
                name: $"--no-{name}",
                description: description
                );
            
            _yesOption.IsHidden = isHidden;
            _noOption.IsHidden = isHidden;

            bool printedErrorMessage = false;

            _yesOption.AddValidator(x =>
            {
                if (x.FindResultFor(_noOption) is not null && !printedErrorMessage)
                {
                    x.ErrorMessage = $"Both --{name} and --no-{name} cannot be supplied.";
                    printedErrorMessage = true;
                }
            });

            _noOption.AddValidator(x =>
            {
                if (x.FindResultFor(_yesOption) is not null && !printedErrorMessage)
                {
                    x.ErrorMessage = $"Both --{name} and --no-{name} cannot be supplied.";
                    printedErrorMessage = true;
                }
            });
        }
    }

    /// <summary>
    /// Extension class for command flags
    /// </summary>
    internal static class CommandFlagExtensions
    {
        /// <summary>
        /// Adds a flag to this command
        /// </summary>
        /// <param name="command">This command</param>
        /// <param name="flag">Flag to add</param>
        public static void AddFlag(this Command command, CommandFlag flag)
        {
            flag.ConfigureCommand(command);
        }
    }
}
