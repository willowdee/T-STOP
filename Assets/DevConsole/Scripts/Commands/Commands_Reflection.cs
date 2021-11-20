using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace DevConsole
{
	[ConsoleCommand(new string[]{"getval", "getvalue"})]
	class GetCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [path]" + System.Environment.NewLine +
				       "    Retrieves the value of a public variable/property." + System.Environment.NewLine +
					   "    path is either a path to a variable on a game object's component of the form SceneName:GameObject.Component.Variable" + System.Environment.NewLine +
					   "    OR path is a path to a variable on a static class of the form ClassName.Variable";
			}
			else
			{
				return "Retrieves the value of a public variable/property.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens?
			if (tokens.Length != 1)
				return "[Error] You must provide a path to a variable/property retrieve.";

			return CommandHelpers.GetValue(tokens[0]);
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// at the variable autocomplete stage (ie. need reflection)
			if (tokens.Length > 0 && tokens[0].Count(character => character == '.') > 1)
			{
				return CommandHelpers.GetAutocompleteOptions(tokens[0], command, CommandHelpers.AutocompleteCandidates.Variables);
			}

			// fetch the game object autocomplete options
			string path = tokens.Length > 0 ? tokens[0] : "";
			List<string> autocompleteOptions = CommandHelpers.GetComponentAutocompleteOptions(path, command);
			if (autocompleteOptions == null)
				autocompleteOptions = new List<string>();

			// user has entered exact path to a component - switch to variable autocomplete stage
			if ((tokens.Length > 0) && (autocompleteOptions.Count <= 1) && (CommandHelpers.GetComponent(tokens[0]) != null))
			{
				return CommandHelpers.GetAutocompleteOptions(tokens[0], command, CommandHelpers.AutocompleteCandidates.Variables);
			}

			// append the type based autocomplete options
			autocompleteOptions.AddRange(ConsoleDaemon.Instance.AvailableTypes.Keys
										 	.Where(typeName => typeName.StartsWith(path))
											.Select(typeName => command + " " + typeName).ToList());

			// if we have no autocomplete options at this stage then we are working with a class
			if (tokens.Length > 0 && (autocompleteOptions == null || autocompleteOptions.Count <= 1))
			{
				string[] pathElements = tokens[0].Split('.');

				// check if the class is in the cached types
				if (ConsoleDaemon.Instance.AvailableTypes.ContainsKey(pathElements[0]))
					return CommandHelpers.GetAutocompleteOptions(tokens[0], command, CommandHelpers.AutocompleteCandidates.Variables);
			}

			return autocompleteOptions;
		}
	}

	[ConsoleCommand(new string[]{"setval", "setvalue"})]
	class SetCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [path] [value]" + System.Environment.NewLine +
				       "    Sets the value of a public variable/property." + System.Environment.NewLine +
					   "    path is either a path to a variable on a game object's component of the form SceneName:GameObject.Component.Variable" + System.Environment.NewLine +
					   "    OR path is a path to a variable on a static class of the form ClassName.Variable" + System.Environment.NewLine + 
					   "    value must be in quotation marks and is the value to attempt to set.";
			}
			else
			{
				return "Sets the value of a public variable/property.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens?
			if (tokens.Length != 2)
				return "[Error] You must provide a path to a variable/property set and a value to set.";

			return CommandHelpers.SetValue(tokens[0], tokens[1]);
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// don't autocomplete once we are entering the variable
			if (tokens.Length >= 2)
				return null;
				
			// at the variable autocomplete stage (ie. need reflection)
			if (tokens.Length > 0 && tokens[0].Count(character => character == '.') > 1)
			{
				return CommandHelpers.GetAutocompleteOptions(tokens[0], command, CommandHelpers.AutocompleteCandidates.Variables);
			}

			// fetch the game object autocomplete options
			string path = tokens.Length > 0 ? tokens[0] : "";
			List<string> autocompleteOptions = CommandHelpers.GetComponentAutocompleteOptions(path, command);
			if (autocompleteOptions == null)
				autocompleteOptions = new List<string>();

			// user has entered exact path to a component - switch to variable autocomplete stage
			if ((tokens.Length > 0) && (autocompleteOptions.Count <= 1) && (CommandHelpers.GetComponent(tokens[0]) != null))
			{
				return CommandHelpers.GetAutocompleteOptions(tokens[0], command, CommandHelpers.AutocompleteCandidates.Variables);
			}

			// append the type based autocomplete options
			autocompleteOptions.AddRange(ConsoleDaemon.Instance.AvailableTypes.Keys
										 	.Where(typeName => typeName.StartsWith(path))
											.Select(typeName => command + " " + typeName).ToList());

			// if we have no autocomplete options at this stage then we are working with a class
			if (tokens.Length > 0 && (autocompleteOptions == null || autocompleteOptions.Count <= 1))
			{
				string[] pathElements = tokens[0].Split('.');

				// check if the class is in the cached types
				if (ConsoleDaemon.Instance.AvailableTypes.ContainsKey(pathElements[0]))
					return CommandHelpers.GetAutocompleteOptions(tokens[0], command, CommandHelpers.AutocompleteCandidates.Variables);
			}

			return autocompleteOptions;
		}
	}

	[ConsoleCommand(new string[]{"invoke", "execute"})]
	class ExecuteMethodCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [path]" + System.Environment.NewLine +
				       "    Execute a public function/method." + System.Environment.NewLine +
					   "    path is either a path to a function on a game object's component of the form SceneName:GameObject.Component.Function" + System.Environment.NewLine +
					   "    OR path is a path to a function on a static class of the form ClassName.Function";
			}
			else
			{
				return "Execute a public function/method.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens?
			if (tokens.Length != 1)
				return "[Error] You must provide a path to a function/method to execute.";

			return CommandHelpers.Execute(tokens[0], tokens, 1);
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// at the variable autocomplete stage (ie. need reflection)
			if (tokens.Length > 0 && tokens[0].Count(character => character == '.') > 1)
			{
				return CommandHelpers.GetAutocompleteOptions(tokens[0], command, CommandHelpers.AutocompleteCandidates.Functions);
			}

			// fetch the game object autocomplete options
			string path = tokens.Length > 0 ? tokens[0] : "";
			List<string> autocompleteOptions = CommandHelpers.GetComponentAutocompleteOptions(path, command);
			if (autocompleteOptions == null)
				autocompleteOptions = new List<string>();

			// user has entered exact path to a component - switch to variable autocomplete stage
			if ((tokens.Length > 0) && (autocompleteOptions.Count <= 1) && (CommandHelpers.GetComponent(tokens[0]) != null))
			{
				return CommandHelpers.GetAutocompleteOptions(tokens[0], command, CommandHelpers.AutocompleteCandidates.Functions);
			}

			// append the type based autocomplete options
			autocompleteOptions.AddRange(ConsoleDaemon.Instance.AvailableTypes.Keys
										 	.Where(typeName => typeName.StartsWith(path))
											.Select(typeName => command + " " + typeName).ToList());

			// if we have no autocomplete options at this stage then we are working with a class
			if (tokens.Length > 0 && (autocompleteOptions == null || autocompleteOptions.Count <= 1))
			{
				string[] pathElements = tokens[0].Split('.');

				// check if the class is in the cached types
				if (ConsoleDaemon.Instance.AvailableTypes.ContainsKey(pathElements[0]))
					return CommandHelpers.GetAutocompleteOptions(tokens[0], command, CommandHelpers.AutocompleteCandidates.Functions);
			}

			return autocompleteOptions;
		}
	}
}