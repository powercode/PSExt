using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;

namespace PSExt.Host
{
	internal class HostUserInterface : PSHostUserInterface, IHostUISupportsMultipleChoiceSelection
	{
		private readonly IDebugger _debugger;
		private readonly ConsoleColorProxy _consoleColors;
		private readonly PSHost _host;

		/// <summary>
		///     A reference to the MyRawUserInterface implementation.
		/// </summary>
		private readonly RawUserInterface _rawUi;

		public HostUserInterface(IDebugger debugger, ConsoleColorProxy consoleColors, PSHost host)
		{
			_debugger = debugger;
			_consoleColors = consoleColors;
			_host = host;
			_rawUi = new RawUserInterface();
		}

		/// <summary>
		///     Gets an instance of the PSRawUserInterface object for this host
		///     application.
		/// </summary>
		public override PSHostRawUserInterface RawUI => _rawUi;

		#region IHostUISupportsMultipleChoiceSelection Members

		/// <summary>
		///     Provides a set of choices that enable the user to choose a one or more options
		///     from a set of options.
		/// </summary>
		/// <param name="caption">A title that proceeds the choices.</param>
		/// <param name="message">
		///     An introduction  message that describes the
		///     choices.
		/// </param>
		/// <param name="choices">A collection of ChoiceDescription objects that describe each choice.</param>
		/// <param name="defaultChoices">
		///     The index of the label in the Choices parameter
		///     collection that indicates the default choice used if the user does not specify
		///     a choice. To indicate no default choice, set to -1.
		/// </param>
		/// <returns>
		///     The index of the Choices parameter collection element that corresponds
		///     to the choices selected by the user.
		/// </returns>
		public Collection<int> PromptForChoice(
			string caption,
			string message,
			Collection<ChoiceDescription> choices,
			IEnumerable<int> defaultChoices)
		{
			// Write the caption and message strings in Blue.
			WriteLine(				
				RawUI.ForegroundColor,
				RawUI.BackgroundColor,
				caption + "\n" + message + "\n");

			// Convert the choice collection into something that's a
			// little easier to work with
			// See the BuildHotkeysAndPlainLabels method for details.
			var promptData = BuildHotkeysAndPlainLabels(choices);

			// Format the overall choice prompt string to display...
			var sb = new StringBuilder();
			for (var element = 0; element < choices.Count; element++)
			{
				sb.Append(string.Format(
					CultureInfo.CurrentCulture,
					"|{0}> {1} ",
					promptData[0, element],
					promptData[1, element]));
			}

			var defaultResults = new Collection<int>();
			if (defaultChoices != null)
			{
				var countDefaults = 0;
				var enumerable = defaultChoices as int[] ?? defaultChoices.ToArray();
				foreach (var defaultChoice in enumerable)
				{
					++countDefaults;
					defaultResults.Add(defaultChoice);
				}

				if (countDefaults != 0)
				{
					sb.Append(countDefaults == 1 ? "[Default choice is " : "[Default choices are ");
					foreach (var defaultChoice in enumerable)
					{
						sb.AppendFormat(
							CultureInfo.CurrentCulture,
							"\"{0}\",",
							promptData[0, defaultChoice]);
					}

					sb.Remove(sb.Length - 1, 1);
					sb.Append("]");
				}
			}

			WriteLine(RawUI.ForegroundColor, RawUI.BackgroundColor, sb.ToString());

			// loop reading prompts until a match is made, the default is
			// chosen or the loop is interrupted with ctrl-C.
			var results = new Collection<int>();
			while (true)
			{
				ReadNext:
				var prompt = string.Format(CultureInfo.CurrentCulture, "Choice[{0}]:", results.Count);
				Write(RawUI.ForegroundColor, RawUI.BackgroundColor, prompt);
				var data = _debugger.ReadLine().Trim().ToUpper(CultureInfo.CurrentCulture);

				// if the choice string was empty, no more choices have been made.
				// if there were no choices made, return the defaults
				if (data.Length == 0)
				{
					return results.Count == 0 ? defaultResults : results;
				}

				// see if the selection matched and return the
				// corresponding index if it did...
				for (var i = 0; i < choices.Count; i++)
				{
					if (promptData[0, i] == data)
					{
						results.Add(i);
						goto ReadNext;
					}
				}

				WriteErrorLine("Invalid choice: " + data);
			}
		}

		#endregion

		/// <summary>
		///     Prompts the user for input.
		/// </summary>
		/// <param name="caption">Text that preceeds the prompt (a title).</param>
		/// <param name="message">Text of the prompt.</param>
		/// <param name="descriptions">
		///     A collection of FieldDescription objects
		///     that contains the user input.
		/// </param>
		/// <returns>A dictionary object that contains the results of the user prompts.</returns>
		public override Dictionary<string, PSObject> Prompt(
			string caption,
			string message,
			Collection<FieldDescription> descriptions)
		{
			Write(
				RawUI.ForegroundColor,
				RawUI.BackgroundColor,
				caption + "\n" + message + " ");
			var results =
				new Dictionary<string, PSObject>();
			foreach (var fd in descriptions)
			{
				var label = GetHotkeyAndLabel(fd.Name);
				Write(label[1] + ": ");
				var userData = _debugger.ReadLine();
				if (userData == null)
				{
					return null;
				}

				results[fd.Name] = PSObject.AsPSObject(userData);
			}

			return results;
		}

		/// <summary>
		///     Provides a set of choices that enable the user to choose a single option
		///     from a set of options.
		/// </summary>
		/// <param name="caption">A title that proceeds the choices.</param>
		/// <param name="message">
		///     An introduction  message that describes the
		///     choices.
		/// </param>
		/// <param name="choices">
		///     A collection of ChoiceDescription objects that describ
		///     each choice.
		/// </param>
		/// <param name="defaultChoice">
		///     The index of the label in the Choices parameter
		///     collection that indicates the default choice used if the user does not specify
		///     a choice. To indicate no default choice, set to -1.
		/// </param>
		/// <returns>
		///     The index of the Choices parameter collection element that corresponds
		///     to the option that is selected by the user.
		/// </returns>
		public override int PromptForChoice(
			string caption,
			string message,
			Collection<ChoiceDescription> choices,
			int defaultChoice)
		{
			// Write the caption and message strings in Blue.
			WriteLine(
				RawUI.ForegroundColor,
				RawUI.BackgroundColor,
				caption + "\n" + message + "\n");

			// Convert the choice collection into something that's a
			// little easier to work with
			// See the BuildHotkeysAndPlainLabels method for details.
			var promptData = BuildHotkeysAndPlainLabels(choices);

			// Format the overall choice prompt string to display...
			var sb = new StringBuilder();
			for (var element = 0; element < choices.Count; element++)
			{
				sb.Append(string.Format(
					CultureInfo.CurrentCulture,
					"|{0}> {1} ",
					promptData[0, element],
					promptData[1, element]));
			}

			sb.Append(string.Format(
				CultureInfo.CurrentCulture,
				"[Default is ({0}]",
				promptData[0, defaultChoice]));

			// loop reading prompts until a match is made, the default is
			// chosen or the loop is interrupted with ctrl-C.
			while (true)
			{
				WriteLine(RawUI.ForegroundColor, RawUI.BackgroundColor, sb.ToString());
				var data = _debugger.ReadLine().Trim().ToUpper(CultureInfo.CurrentCulture);

				// if the choice string was empty, use the default selection
				if (data.Length == 0)
				{
					return defaultChoice;
				}

				// see if the selection matched and return the
				// corresponding index if it did...
				for (var i = 0; i < choices.Count; i++)
				{
					if (promptData[0, i] == data)
					{
						return i;
					}
				}

				WriteErrorLine("Invalid choice: " + data);
			}
		}

		/// <summary>
		///     Prompts the user for credentials with a specified prompt window
		///     caption, prompt message, user name, and target name.
		/// </summary>
		/// <param name="caption">The caption of the message window.</param>
		/// <param name="message">The text of the message.</param>
		/// <param name="userName">The user name whose credential is to be prompted for.</param>
		/// <param name="targetName">The name of the target for which the credential is collected.</param>
		/// <returns>Throws a NotImplementException exception.</returns>
		public override PSCredential PromptForCredential(
			string caption, string message, string userName, string targetName)
		{
			throw new NotImplementedException(
				"The method PromptForCredential() is not implemented by MyHost.");
		}

		/// <summary>
		///     Prompts the user for credentials by using a specified prompt window
		///     caption, prompt message, user name and target name, credential types
		///     allowed to be returned, and UI behavior options.
		/// </summary>
		/// <param name="caption">The caption of the message window.</param>
		/// <param name="message">The text of the message.</param>
		/// <param name="userName">The user name whose credential is to be prompted for.</param>
		/// <param name="targetName">The name of the target for which the credential is collected.</param>
		/// <param name="allowedCredentialTypes">
		///     PSCredentialTypes cconstants that identify the type of
		///     credentials that can be returned.
		/// </param>
		/// <param name="options">
		///     A PSCredentialUIOptions constant that identifies the UI behavior
		///     when it gathers the credentials.
		/// </param>
		/// <returns>Throws a NotImplementException exception.</returns>
		public override PSCredential PromptForCredential(
			string caption,
			string message,
			string userName,
			string targetName,
			PSCredentialTypes allowedCredentialTypes,
			PSCredentialUIOptions options)
		{
			throw new NotImplementedException(
				"The method PromptForCredential() is not implemented by MyHost.");
		}

		/// <summary>
		///     Reads characters that are entered by the user until a
		///     newline (carriage return) is encountered.
		/// </summary>
		/// <returns>The characters entered by the user.</returns>
		public override string ReadLine()
		{
			return _debugger.ReadLine();
		}

		/// <summary>
		///     Reads characters entered by the user until a newline (carriage return)
		///     is encountered and returns the characters as a secure string.
		/// </summary>
		/// <returns>A secure string of the characters entered by the user.</returns>
		public override SecureString ReadLineAsSecureString()
		{
			throw new NotImplementedException(
				"The method ReadLineAsSecureString() is not implemented by MyHost.");
		}

		/// <summary>
		///     Writes a line of characters to the output display of the host
		///     and appends a newline (carriage return).
		/// </summary>
		/// <param name="value">The characters to be written.</param>
		public override void Write(string value)
		{
			_debugger.Write(value);
		}

		/// <summary>
		///     Writes characters to the output display of the host with possible
		///     foreground and background colors.
		/// </summary>
		/// <param name="foregroundColor">The color of the characters.</param>
		/// <param name="backgroundColor">The backgound color to use.</param>
		/// <param name="value">The characters to be written.</param>
		public override void Write(
			ConsoleColor foregroundColor,
			ConsoleColor backgroundColor,
			string value)
		{
			var oldFg = Console.ForegroundColor;
			var oldBg = Console.BackgroundColor;
			Console.ForegroundColor = foregroundColor;
			Console.BackgroundColor = backgroundColor;
			Write(value);
			Console.ForegroundColor = oldFg;
			Console.BackgroundColor = oldBg;
		}

		/// <summary>
		///     Writes a line of characters to the output display of the host
		///     with foreground and background colors and appends a newline (carriage return).
		/// </summary>
		/// <param name="foregroundColor">The forground color of the display. </param>
		/// <param name="backgroundColor">The background color of the display. </param>
		/// <param name="value">The line to be written.</param>
		public override void WriteLine(
			ConsoleColor foregroundColor,
			ConsoleColor backgroundColor,
			string value)
		{
			var oldFg = Console.ForegroundColor;
			var oldBg = Console.BackgroundColor;
			Console.ForegroundColor = foregroundColor;
			Console.BackgroundColor = backgroundColor;
			WriteLine(value);
			Console.ForegroundColor = oldFg;
			Console.BackgroundColor = oldBg;
		}

		/// <summary>
		///     Writes a debug message to the output display of the host.
		/// </summary>
		/// <param name="message">The debug message that is displayed.</param>
		public override void WriteDebugLine(string message)
		{
			WriteLine(
				ConsoleColor.DarkYellow,
				ConsoleColor.Black,
				string.Format(CultureInfo.CurrentCulture, "DEBUG: {0}", message));
		}

		/// <summary>
		///     Writes an error message to the output display of the host.
		/// </summary>
		/// <param name="value">The error message that is displayed.</param>
		public override void WriteErrorLine(string value)
		{
			WriteLine(_consoleColors.ErrorForegroundColor, _consoleColors.ErrorBackgroundColor, value);
		}

		/// <summary>
		///     Writes a newline character (carriage return)
		///     to the output display of the host.
		/// </summary>
		public override void WriteLine()
		{
			_debugger.Write(Environment.NewLine);
		}

		/// <summary>
		///     Writes a line of characters to the output display of the host
		///     and appends a newline character(carriage return).
		/// </summary>
		/// <param name="value">The line to be written.</param>
		public override void WriteLine(string value)
		{
			_debugger.Write(value + Environment.NewLine);
		}

		/// <summary>
		///     Writes a verbose message to the output display of the host.
		/// </summary>
		/// <param name="message">The verbose message that is displayed.</param>
		public override void WriteVerboseLine(string message)
		{
			WriteLine(
				_consoleColors.VerboseForegroundColor,
				_consoleColors.VerboseBackgroundColor,
				string.Format(CultureInfo.CurrentCulture, "VERBOSE: {0}", message));
		}

		/// <summary>
		///     Writes a warning message to the output display of the host.
		/// </summary>
		/// <param name="message">The warning message that is displayed.</param>
		public override void WriteWarningLine(string message)
		{
			WriteLine(
				_consoleColors.WarningForegroundColor,
				_consoleColors.WarningBackgroundColor,
				string.Format(CultureInfo.CurrentCulture, "WARNING: {0}", message));
		}

		/// <summary>
		///     Writes a progress report to the output display of the host.
		///     Wrinting a progress report is not required for the cmdlet to
		///     work so it is better to do nothing instead of throwing an
		///     exception.
		/// </summary>
		/// <param name="sourceId">Unique identifier of the source of the record. </param>
		/// <param name="record">A ProgressReport object.</param>
		public override void WriteProgress(long sourceId, ProgressRecord record)
		{
			// Do nothing.
		}

		/// <summary>
		///     Parse a string containing a hotkey character.
		///     Take a string of the form
		///     Yes to &amp;all
		///     and returns a two-dimensional array split out as
		///     "A", "Yes to all".
		/// </summary>
		/// <param name="input">The string to process</param>
		/// <returns>
		///     A two dimensional array containing the parsed components.
		/// </returns>
		private static string[] GetHotkeyAndLabel(string input)
		{
			string[] result = {string.Empty, string.Empty};
			var fragments = input.Split('&');
			if (fragments.Length == 2)
			{
				if (fragments[1].Length > 0)
				{
					result[0] = fragments[1][0].ToString().
						ToUpper(CultureInfo.CurrentCulture);
				}

				result[1] = (fragments[0] + fragments[1]).Trim();
			}
			else
			{
				result[1] = input;
			}

			return result;
		}

		/// <summary>
		///     This is a private worker function splits out the
		///     accelerator keys from the menu and builds a two
		///     dimentional array with the first access containing the
		///     accelerator and the second containing the label string
		///     with the &amp; removed.
		/// </summary>
		/// <param name="choices">The choice collection to process</param>
		/// <returns>
		///     A two dimensional array containing the accelerator characters
		///     and the cleaned-up labels
		/// </returns>
		private static string[,] BuildHotkeysAndPlainLabels(
			Collection<ChoiceDescription> choices)
		{
			// we will allocate the result array
			var hotkeysAndPlainLabels = new string[2, choices.Count];

			for (var i = 0; i < choices.Count; ++i)
			{
				var hotkeyAndLabel = GetHotkeyAndLabel(choices[i].Label);
				hotkeysAndPlainLabels[0, i] = hotkeyAndLabel[0];
				hotkeysAndPlainLabels[1, i] = hotkeyAndLabel[1];
			}

			return hotkeysAndPlainLabels;
		}
	}
}