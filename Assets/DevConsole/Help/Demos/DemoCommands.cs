using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevConsole_Demos
{
    /*
    The command below is a simple one. The user can enter 'hello' or 'hi' and the command will respond with 'world'.

    The command has:
        - Abbreviated help (used when listing commands)
        - Verbose help (used when requested help on an individual command)
        - No additional parameters
        - Is always available and visible in the help
    */
    [DevConsole.ConsoleCommand(new string[] {"hello", "hi"})]
	class HelloWorldCommand
	{
        // This method is mandatory.
        //   - It is used to get both the brief (command listing) help and the verbose help (when requested for the individual command).
		public static string Help(string command, bool verbose)
		{
			return verbose ? "Says hello to the user." : "Says hello to the user.";
		}

        // This method is mandatory
        //   - It handles the execution of the command itself.
        //   - Any arguments are in the tokens array
        //   - Returns a string that will be displayed to the user. Formatting tags are supported.
		public static string Execute(string[] tokens)
		{
			return "World!";
		}

        // This method is mandatory
        //   - It handles generating a list of autocomplete options.
        //   - command contains what the user entered (eg. if a command has multiple aliases it will have what the user actually typed)
        //   - tokens is a list of arguments that the user has already enterd
        //   - Returns a list of autocomplete options to show. 
        //      - It must include the command and any tokens.
        //      - Return null if there are no viable autocomplete options
		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			return null;
		}

        // This method is optional. 
        //   - If it is present then returning true means the command is available. 
        //   - Returning false means the command is not available.
        //   - The return value is not cached so it can change at runtime.
        //   - If the method is not included then the command is always available.
        public static bool IsAvailable()
        {
            return true;
        }

        // This method is optional. 
        //   - If it is present then returning true means the command is shown in help and can be autocompleted. 
        //   - Returning false means the command is not visible in help and cannot be autocompleted.
        //   - The return value is not cached so it can change at runtime.
        //   - If the method is not included then the command is always shown in help an can always be autocompleted.
        public static bool ShowInHelpAndAutocomplete()
        {
            return true;
        }
	}

    /*
    The command below is a simple one. The user can enter 'the' and then use autocomplete (or manually) enter the remaining part of the phrase.

    The command has:
        - Abbreviated help (used when listing commands)
        - Verbose help (used when requested help on an individual command)
        - Additional parameters
        - Autocomplete support
    */
    [DevConsole.ConsoleCommand(new string[] {"the"})]
	class TheMeaningOfLifeCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
                // Always include the command itself in the verbose help when there are arguments to make it easier for people using the console.
				return command + " [meaning] [of] [life] [is]" + System.Environment.NewLine +
				       "    Determines the meaning of life";
			}
			else
			{
				return "Answers an old question";
			}
		}

        // This method is mandatory
        //   - It handles the execution of the command itself.
        //   - Any arguments are in the tokens array
        //   - Returns a string that will be displayed to the user. Formatting tags are supported.
		public static string Execute(string[] tokens)
		{
            // check if the number of tokens is not correct
            if (tokens.Length < 4)
                return "[Error] There needs to be four words following the command."; // Including [Error] at the start will format the text in red
            if (tokens.Length > 4)
                return "[Error] There are too many words. Expected only 4.";

            // check all of the tokens
            if (tokens[0].ToLower() == "meaning" &&
                tokens[1].ToLower() == "of" &&
                tokens[2].ToLower() == "life" &&
                tokens[3].ToLower() == "is")
            {
                return "The meaning of life is .... coming soon in a future update :)";
            }

			return "[Error] Not all of the words were correct. Please try again.";
		}

        // This method is mandatory
        //   - It handles generating a list of autocomplete options.
        //   - command contains what the user entered (eg. if a command has multiple aliases it will have what the user actually typed)
        //   - tokens is a list of arguments that the user has already enterd
        //   - Returns a list of autocomplete options to show. 
        //      - It must include the command and any tokens.
        //      - Return null if there are no viable autocomplete options
		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
            // for this command we need to check what tokens have already been entered
            if (tokens.Length == 0)
                return new List<string>() {command + " meaning"};

            if (tokens.Length == 1)
            {
                if (tokens[0].ToLower() == "meaning")
                    return new List<string>() {command + " meaning of"};
            }

            if (tokens.Length == 2)
            {
                if (tokens[0].ToLower() == "meaning" && tokens[1].ToLower() == "of")
                    return new List<string>() {command + " meaning of life"};
            }

            if (tokens.Length == 3)
            {
                if (tokens[0].ToLower() == "meaning" && tokens[1].ToLower() == "of" && tokens[2].ToLower() == "life")
                    return new List<string>() {command + " meaning of life is"};
            }

			return null;
		}
	}
}