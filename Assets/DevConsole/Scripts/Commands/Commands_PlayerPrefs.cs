using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevConsole
{
	[ConsoleCommand(new string[] {"getpref", "prefget"})]
	class GetPlayerPrefCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [key]" + System.Environment.NewLine + "    Retrieves the value of the specified key from player preferences.";
			}
			else
			{
				return "Gets the value of an individual key in the player preferences.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// only permits a single parameter
			if (tokens.Length != 1)
				return "[Error] You must provide only a single parameter.";

			// provide feedback if the key is invalid
			if (!PlayerPrefs.HasKey(tokens[0]))
				return "[Error] The key provided \'" + tokens[0] + "\' does not exist within the player preferences.";

			string value = PlayerPrefs.GetString(tokens[0]);

			// if the string is null or empty then it might be an integer or float
			if (string.IsNullOrEmpty(value))
			{
				string floatValue = PlayerPrefs.GetFloat(tokens[0]).ToString();
				string intValue = PlayerPrefs.GetInt(tokens[0]).ToString();

				if (floatValue == "0" && intValue != "0")
					value = intValue;
				else
					value = floatValue;
			}

			// retrieve and display the key
			return tokens[0] + " = " + value;
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			return null;
		}
	}

	[ConsoleCommand(new string[] {"setpref", "prefset"})]
	class SetPlayerPrefCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [key] [int/float/string] [value]" + 
				       System.Environment.NewLine + 
					   "    Stores [value] into the player preferences under [key] as the given typ.";
			}
			else
			{
				return "Sets an individual key in the player preferences.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// only permits a single parameter
			if (tokens.Length != 3)
				return "[Error] You must provide a key, value and type. Use \"\" if there are spaces.";

			// set and display the information set
			if (tokens[1].ToLower() == "string")
			{
				PlayerPrefs.SetString(tokens[0], tokens[2]);
			}
			else if (tokens[1].ToLower() == "int")
			{
				int intValue;
				if (int.TryParse(tokens[2], out intValue))
					PlayerPrefs.SetInt(tokens[0], intValue);
				else
					return "[Error] The value " + tokens[2] + " was not a valid integer.";
			}
			else if (tokens[1].ToLower() == "float")
			{
				float floatValue;
				if (float.TryParse(tokens[2], out floatValue))
					PlayerPrefs.SetFloat(tokens[0], floatValue);
				else
					return "[Error] The value " + tokens[2] + " was not a valid floating point number.";
			}
			else
			{
				return "[Error] Did not recognise the type " + tokens[1] + ". Expected int, float or string.";
			}

			PlayerPrefs.Save();
			return "Set " + tokens[0] + " to " + tokens[2];
		}	

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// if there are two tokens then we have key + at least part of type which is the only thing we can autocomplete
			// so only attempt autocomplete if we have 1 or 2 tokens
			if (tokens.Length != 1 && tokens.Length != 2)
				return null;

			// if the type is already fully there then do nothing
			if (tokens.Length == 2 && (tokens[1].ToLower() == "string" || tokens[1].ToLower() == "int" || tokens[1].ToLower() == "float"))
				return null;

			List<string> autocompleteOptions = new List<string>();

			string typeToken = tokens.Length == 2 ? tokens[1].ToLower() : "";
			string keyToken = tokens[0];

			// escape the key token if needed
			if (keyToken.Contains(" "))
				keyToken = "\"" + keyToken + "\"";

			// if we have a single token (key) then put in all options, otherwise check if it matches one of the types
			if (tokens.Length == 1 || "string".StartsWith(typeToken))
				autocompleteOptions.Add(command + " " + keyToken + " " + "string");
			if (tokens.Length == 1 || "int".StartsWith(typeToken))
				autocompleteOptions.Add(command + " " + keyToken + " " + "int");
			if (tokens.Length == 1 || "float".StartsWith(typeToken))
				autocompleteOptions.Add(command + " " + keyToken + " " + "float");

			return autocompleteOptions;
		}
	}

	[ConsoleCommand(new string[] {"delpref", "prefdel"})]
	class DeletePlayerPrefCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [key]" + System.Environment.NewLine + "    Deletes the specified key from player preferences.";
			}
			else
			{
				return "Deletes an individual key from player preferences.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// only permits a single parameter
			if (tokens.Length != 1)
				return "[Error] You must provide only a single parameter (the key to delete)";

			// provide feedback if the key is invalid
			if (!PlayerPrefs.HasKey(tokens[0]))
				return "[Error] The key provided \'" + tokens[0] + "\' does not exist within the player preferences.";

			// remove the key
			PlayerPrefs.DeleteKey(tokens[0]);
			PlayerPrefs.Save();

			return "Removed key " + tokens[0] + " from player preferences.";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			return null;
		}
	}

	[ConsoleCommand(new string[] {"delallpref", "prefdelall"})]
	class DeleteAllPlayerPrefsCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + System.Environment.NewLine + "    Deletes all stored player preferences.";
			}
			else
			{
				return "Deletes all stored player preferences.";
			}
		}

		public static string Execute(string[] tokens)
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();

			return "All stored player preferences deleted.";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			return null;
		}
	}

	// listing all player prefs would require per-platform implementation :( ... not doing or now
}