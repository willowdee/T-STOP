using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DevConsole
{
	[ConsoleCommand(new string[] {"version", "ver"})]
	class VersionCommand
	{
		public static string Help(string command, bool verbose)
		{
			return verbose ? "Displays the version of the developer console." : "Displays the version of the developer console.";
		}

		public static string Execute(string[] tokens)
		{
			return ConsoleDaemon.Version;
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			return null;
		}
	}

	[ConsoleCommand(new string[] {"help", "?"})]
	class HelpCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose) 
				return command + " [command]" + System.Environment.NewLine + 
					   "    Displays detailed help information about [command]. If no command is included then lists all commands.";
			else	
				return "Displays help information.";
		}

		public static string Execute(string[] tokens)
		{
			// display general help information
			if (tokens.Length == 0)
			{
				// retrieve the list of commands
				List<string> commands = ConsoleDaemon.Instance.CommandList;

				// for each command retrieve the corresponding help text
				int longestCommand = 0;
				foreach(string command in commands)
				{
					longestCommand = Mathf.Max(longestCommand, command.Length);
				}

				// construct the text
				string helpText = "";
				for (int index = 0; index < commands.Count; ++index)
				{
					helpText += "<color=" + ConsoleDaemon.Instance.Colour_Command + ">" + commands[index].PadRight(longestCommand + 4) + "</color>";
					helpText += "<i>" + ConsoleDaemon.Instance.GetHelp(commands[index], false) + "</i>";
					helpText += System.Environment.NewLine;
				}

				return helpText;
			}
			else
			{
				// assume in this case we're asking for the verbose help for a single command
				string help = ConsoleDaemon.Instance.GetHelp(tokens[0], true);

				return help;
			}
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			string enteredText = tokens.Length > 0 ? tokens[0] : "";

			// filter commands based on entered text
			List<string> filteredCommands = ConsoleDaemon.Instance.CommandList.Where(name => name.StartsWith(enteredText)).ToList();
			if (filteredCommands == null || filteredCommands.Count == 0)
				return null;
			
			// build the autocomplete list
			List<string> autocompleteOptions = new List<string>();
			foreach(string candidate in filteredCommands)
			{
				autocompleteOptions.Add(command + " " + candidate);
			}

			return autocompleteOptions;
		}
	}

	[ConsoleCommand(new string[] {"screenshot"})]
	class ScreenshotCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [image-scale] [filename]" + System.Environment.NewLine +
				       "    Captures a new screenshot with an optional name and scale.";
			}
			else
			{
				return "Captures a new screenshot with an optional name and scale.";
			}
		}

		public static string Execute(string[] tokens)
		{
			int scaleMultiplier = 1;
			string fileName = "Screenshot_" + System.DateTime.Now.ToString("d_M_yyyy_H_mm_ss") + "_" + Time.frameCount + ".png";

			// is there at least 1 token?
			if (tokens.Length >= 1)
			{
				// attempt to parse the resolution multipler
				if (!int.TryParse(tokens[0], out scaleMultiplier))
				{
					return "[Error]: Unrecognised parameter " + tokens[0] + " when looking for a number for the image scale.";
				}

				// sanity check the image scale
				if (scaleMultiplier < 1 || scaleMultiplier > 10)
				{
					return "[Error]: The image scale must be between 1 and 10";
				}

				// is there a second token?
				if (tokens.Length == 2)
				{
					fileName = tokens[1] + ".png";
				}
			}

			ConsoleDaemon.Instance.StartCoroutine(ConsoleDaemon.Instance.CaptureScreen(fileName, scaleMultiplier));

			return "Captured screenshot to " + fileName;
		}


		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			return null;
		}
	}

	[ConsoleCommand(new string[] {"history"})]
	class HistoryCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + System.Environment.NewLine +
				       "    Displays a list of all of the recently executed commands.";
			}
			else
			{
				return "Displays a list of all of the recently executed commands.";
			}
		}

		public static string Execute(string[] tokens)
		{
			List<string> commandHistory = ConsoleDaemon.Instance.History;

			// format the list of commands
			string result = "";
			foreach(string command in commandHistory)
			{
				if (!string.IsNullOrEmpty(command))
					result += System.Environment.NewLine;

				result += "    " + command;
			}

			return result;
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			return null;
		}
	}

	[ConsoleCommand(new string[] {"clear", "cls"})]
	class ClearCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + System.Environment.NewLine +
				       "    Clears the output of the console.";
			}
			else
			{
				return "Clears the output of the console.";
			}
		}

		public static string Execute(string[] tokens)
		{
			DevConsole.ConsoleDaemon.Instance.OnClearConsole.Invoke();

			return "";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			return null;
		}
	}
}