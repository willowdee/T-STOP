using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine.Events;

namespace DevConsole
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ConsoleCommandAttribute : Attribute
	{
		private string[] names;

		public ConsoleCommandAttribute(string[] _Names)
		{
			names = _Names;
		}

		public string[] Names
		{
			get
			{
				return names;
			}
		}
	}

	public class UnityStringEvent : UnityEvent<string> {}

	public class ConsoleDaemon : MonoBehaviour 
	{
		public const string Version = "1.2.1";
		public string Colour_Command = "#00ff00ff";
		public string Colour_Error = "#ff0000ff";

		#region Singleton Boiler Plate
		// Based on: http://wiki.unity3d.com/index.php/Singleton

		protected static ConsoleDaemon _Instance;
		protected static bool _IsQuitting = false;

		public class CommandInterface
		{
			MethodInfo _Execute;
			MethodInfo _Help;
			MethodInfo _FetchAutocomplete;
			MethodInfo _IsAvailable;
			MethodInfo _ShowInHelpAndAutocomplete;

			public CommandInterface(MethodInfo _execute, MethodInfo _help, MethodInfo _fetchAutocomplete, MethodInfo _isAvailable, MethodInfo _showInHelpAndAutocomplete)
			{
				_Execute = _execute;
				_Help = _help;
				_FetchAutocomplete = _fetchAutocomplete;
				_IsAvailable = _isAvailable;
				_ShowInHelpAndAutocomplete = _showInHelpAndAutocomplete;
			}

			public string Help(string commandId, bool verbose)
			{
				return _Help.Invoke(null, new object[] { commandId, verbose }) as string;
			}

			public string Execute(List<string> tokens)
			{
				return _Execute.Invoke(null, new object[] { tokens.ToArray() }) as string;
			}

			public List<string> FetchAutocompleteMethods(string originalCommand, List<string> tokens)
			{
				return _FetchAutocomplete.Invoke(null, new object[] {originalCommand, tokens.ToArray()}) as List<string>;
			}

			public bool IsAvailable
			{
				get
				{
					return (_IsAvailable == null) || (bool)_IsAvailable.Invoke(null, null);
				}
			}

			public bool ShowInHelpAndAutocomplete
			{
				get
				{
					return IsAvailable && ((_ShowInHelpAndAutocomplete == null) || (bool)_ShowInHelpAndAutocomplete.Invoke(null, null));
				}
			}
		}

		public void OnDestroy()
		{
			_IsQuitting = true;
		}

		public static ConsoleDaemon Instance
		{
			get
			{
				// if we're quitting then don't give back the instance
				if (_IsQuitting)
				{
					return null;
				}

				// instance not yet set to anything
				if (_Instance == null)
				{
					// attempt to find an existing instance
					_Instance = FindObjectOfType<ConsoleDaemon>();

					// no instance found? need to create it
					if (_Instance == null)
					{
						// instantiate the singleton
						GameObject newObject = new GameObject();
						_Instance = newObject.AddComponent<ConsoleDaemon>();

						// prevent single from being destroyed
						DontDestroyOnLoad(newObject);
					}
				}

				return _Instance;
			}
		}
		#endregion

		#region Method and Type Caches
		protected Dictionary<string, string> AlternateCommandNames = new Dictionary<string, string>();
		protected Dictionary<string, CommandInterface> Commands = new Dictionary<string, CommandInterface>();
		protected Dictionary<string, Type> KnownTypes = new Dictionary<string, Type>();
		protected Dictionary<string, Type> KnownComponents = new Dictionary<string, Type>();
		protected Dictionary<string, GameObject> KnownPrefabs = new Dictionary<string, GameObject>();

		void Awake()
		{
			PopulateCommandRegistry();
			PopulateTypeCache();
			PopulateComponentCache();
			PopulatePrefabCache();
		}

		void PopulatePrefabCache()
		{
			// search for all of the prefabs
			UnityEngine.Object[] foundPrefabs = Resources.LoadAll("", typeof(GameObject));

			// add the prefabs into the cache
			foreach(UnityEngine.Object prefab in foundPrefabs)
			{
				KnownPrefabs[prefab.name] = prefab as GameObject;
			}
		}

		void PopulateComponentCache()
		{
			var knownTypes = 	from assembly in AppDomain.CurrentDomain.GetAssemblies()
								where assembly.GlobalAssemblyCache == false
								from type in assembly.GetTypes()
								where type.IsGenericType == false && 
									  type.IsAbstract == false && type.IsPublic == true &&
									  type.IsSubclassOf(typeof(Component))
								select new { Type = type };

			// register all of the types
			foreach(var typeInfo in knownTypes)
			{
				KnownComponents[typeInfo.Type.Name] = typeInfo.Type;
			}
		}

		void PopulateTypeCache()
		{
			var knownTypes = 	from assembly in AppDomain.CurrentDomain.GetAssemblies()
								where assembly.GlobalAssemblyCache == false && 
									  (assembly.FullName.StartsWith("UnityEngine") || assembly.FullName.StartsWith("Assembly"))
								from type in assembly.GetTypes()
								where type.IsGenericType == false && type.IsEnum == false &&
									  type.IsAbstract == false && type.IsPublic == true
								select new { Type = type };

			// register all of the types
			foreach(var typeInfo in knownTypes)
			{
				// needs to contain at least one of public static functions, properties or variables to include
				if(typeInfo.Type.GetMethods(BindingFlags.Public | BindingFlags.Static).Length == 0 &&
				   typeInfo.Type.GetFields(BindingFlags.Public | BindingFlags.Static).Length == 0 &&
				   typeInfo.Type.GetProperties(BindingFlags.Public | BindingFlags.Static).Length == 0)
				   continue;

				KnownTypes[typeInfo.Type.Name] = typeInfo.Type;
			}
		}

		void PopulateCommandRegistry()
		{
			// Retrieve all of the console commands
			var knownConsoleCommands = 	from assembly in AppDomain.CurrentDomain.GetAssemblies()
										where assembly.GlobalAssemblyCache == false
    									from type in assembly.GetTypes()
									    let attributes = type.GetCustomAttributes(typeof(ConsoleCommandAttribute), true)
    									where attributes != null && attributes.Length > 0
    									select new { Type = type, Attributes = attributes.Cast<ConsoleCommandAttribute>() };

			// register each console command
			foreach(var commandInfo in knownConsoleCommands)
			{
				MethodInfo executeInfo 		= commandInfo.Type.GetMethod("Execute", 
																		 BindingFlags.Public | BindingFlags.Static, 
																		 null, 
																		 CallingConventions.Standard, 
																		 new Type[] {typeof (string[])}, null);

				MethodInfo helpInfo    		= commandInfo.Type.GetMethod("Help", 
																		 BindingFlags.Public | BindingFlags.Static, 
																		 null, 
																		 CallingConventions.Standard, 
																		 new Type[] {typeof(string), typeof (bool)}, null);

				MethodInfo autocompleteInfo	= commandInfo.Type.GetMethod("FetchAutocompleteOptions", 
																		 BindingFlags.Public | BindingFlags.Static, 
																		 null, 
																		 CallingConventions.Standard, 
																		 new Type[] {typeof(string), typeof (string[])}, null);

				MethodInfo isAvailableInfo =  commandInfo.Type.GetMethod("IsAvailable",
																		 BindingFlags.Public | BindingFlags.Static, 
																		 null, 
																		 CallingConventions.Standard, 
																		 new Type[] {}, null);

				MethodInfo showInHelpAndAutocompleteInfo =  commandInfo.Type.GetMethod("ShowInHelpAndAutocomplete",
																						BindingFlags.Public | BindingFlags.Static, 
																						null, 
																						CallingConventions.Standard, 
																						new Type[] {}, null);

				// are the three core methods valid?
				// IsAvailable and ShowInHelpAndAutocomplete were introduced in v1.1.0 and are optional
				if (executeInfo != null && helpInfo != null && autocompleteInfo != null)
				{
					bool mainCommandAdded = false;
					string mainCommandName = "";

					// iterate over the attribute info
					foreach(var attributeInfo in commandInfo.Attributes)
					{
						// iterate over the command names
						foreach(string commandName in attributeInfo.Names)
						{
							// not yet registered the main command
							if (!mainCommandAdded)
							{
								// add to the registry
								Commands[commandName.ToLower()] = new CommandInterface(executeInfo, helpInfo, autocompleteInfo, isAvailableInfo, showInHelpAndAutocompleteInfo);

								// store the details for the main command
								mainCommandAdded = true;
								mainCommandName = commandName;

								// always add the base command (saves extra queries later)
								AlternateCommandNames[commandName] = commandName;
							}
							else
								AlternateCommandNames[commandName] = mainCommandName;
						}
					}

					continue;
				}

				// display help on the missing commands
				if (executeInfo == null)
				{
					Debug.LogError("DevConsole: Failed to find Execute method for " + commandInfo.Type.Name);
					Debug.LogError("Method should have signature: public string Execute(string[] tokens)");
				}
				if (helpInfo == null)
				{
					Debug.LogError("DevConsole: Failed to find Help method for " + commandInfo.Type.Name);
					Debug.LogError("Method should have signature: public string Help(bool verbose)");
				}
				if (autocompleteInfo == null)
				{
					Debug.LogError("DevConsole: Failed to find Autocomplete method for " + commandInfo.Type.Name);
					Debug.LogError("Method should have signature: public List<string> FetchAutocompleteOptions(string command, string[] tokens)");
				}
			}
		}

		public Dictionary<string, GameObject> AvailablePrefabs
		{
			get
			{
				return KnownPrefabs;
			}
		}

		public Dictionary<string, Type> AvailableComponents
		{
			get
			{
				return KnownComponents;
			}
		}

		public Dictionary<string, Type> AvailableTypes
		{
			get
			{
				return KnownTypes;
			}
		}

		public List<string> CommandList
		{
			get
			{
				List<string> commands = Commands.Keys.ToList();
				
				// remove any that are not visible in help or autocomplete
				commands.RemoveAll(command => !Commands[AlternateCommandNames[command]].ShowInHelpAndAutocomplete);
				
				commands.Sort();
				return commands;
			}
		}
		#endregion

		public UnityEvent OnClearConsole = new UnityEvent();
		public UnityStringEvent OnCommandEntered = new UnityStringEvent();

		protected List<string> CommandHistory = new List<string>();
		protected int MaxHistory = 100;
		protected int HistoryIndex = 0;

		public string GetWelcomeMessage()
		{
			// calculate the actual number of commands available
			int commandsAvailable = 0;
			foreach(CommandInterface command in Commands.Values)
			{
				if (command.IsAvailable)
					++commandsAvailable;
			}

			return "<b>Developer Console v" + Version + "</b>" + System.Environment.NewLine +
			       "Console is now active with " + commandsAvailable + " commands available" + System.Environment.NewLine +
				   "Console has found " + KnownComponents.Count + " components and " + KnownPrefabs.Count + " prefabs" + System.Environment.NewLine + 
				   "Console has also found " + KnownTypes.Count + " classes with static members" + System.Environment.NewLine +
				   System.Environment.NewLine +
				   "Type <b>help</b> and press <b>enter/return</b> to view available commands. Press <b>tab</b> when entering a command to attempt autocomplete.";
		}

		public string GetHelp(string commandId, bool verbose)
		{
			// does the command exist?
			if (AlternateCommandNames.ContainsKey(commandId))
			{
				CommandInterface command = Commands[AlternateCommandNames[commandId]];

				return command.ShowInHelpAndAutocomplete ? command.Help(commandId, verbose) : "";
			}

			return "";
		}

		public IEnumerator CaptureScreen(string fileName, int resolutionMultiplier)
		{
			// Wait and then find the developer console and turn off the canvas
			yield return null;
			GameObject.FindGameObjectWithTag("DevConsole").GetComponent<Canvas>().enabled = false;

			// Pause until rendering is complete
			yield return new WaitForEndOfFrame();

			// Save the screenshot
			ScreenCapture.CaptureScreenshot(fileName, resolutionMultiplier);

			// Re-enable the canvas
			GameObject.FindGameObjectWithTag("DevConsole").GetComponent<Canvas>().enabled = true;
		}

		public List<string> History
		{
			get
			{
				return new List<string>(CommandHistory);
			}
		}

		public string GetHistory_Previous()
		{
			if (HistoryIndex < CommandHistory.Count)
			{
				string command = CommandHistory[HistoryIndex];

				HistoryIndex = HistoryIndex >= 1 ? HistoryIndex - 1 : HistoryIndex;

				return command;
			}

			return null;
		}

		public string GetHistory_Next()
		{
			if (HistoryIndex < CommandHistory.Count)
			{
				string command = CommandHistory[HistoryIndex];

				HistoryIndex = HistoryIndex < (CommandHistory.Count - 1) ? HistoryIndex + 1 : HistoryIndex;

				return command;
			}

			return null;
		}

		public string ExecuteCommand(string commandString)
		{
			string result = ExecuteCommandInternal(commandString);

			// split the result into separate lines
			string[] resultLines = result.Split(new string[] {System.Environment.NewLine}, StringSplitOptions.None);
			for (int index = 0; index < resultLines.Length; ++index)
			{
				// apply formatting to any error lines
				if (resultLines[index].StartsWith("[Error]"))
				{
					resultLines[index] = "<color=" + Colour_Error + ">" + resultLines[index] + "</color>";
				}
			}

			return string.Join(System.Environment.NewLine, resultLines);
		}

		protected string ExecuteCommandInternal(string commandString)
		{
			// notify any listeners that a command was entered
			OnCommandEntered.Invoke(commandString);

			// add the new command to the history and ensure the length remains below the maximum
			CommandHistory.Add(commandString);
			while(CommandHistory.Count > MaxHistory)
				CommandHistory.RemoveAt(0);

			// update the history index
			HistoryIndex = CommandHistory.Count - 1;

			// tokenise the command string
			List<string> tokens = TokeniseString(commandString);

			// early out if there was no data
			if (tokens.Count == 0)
			{
				return "[Error] Tried to execute a blank command.";
			}

			// extract the command
			string commandId = tokens[0].ToLower();
			tokens.RemoveAt(0);

			// does the command exist?
			if (AlternateCommandNames.ContainsKey(commandId))
			{
				CommandInterface command = Commands[AlternateCommandNames[commandId]];

				if (command.IsAvailable)
				{
					return command.Execute(tokens);
				}
			}

			return "[Error] The command \'" + commandId + "\' could not be found.";
		}

		public List<string> FetchAutocompleteOptions(string commandString)
		{
			// handle if the string is empty
			if (string.IsNullOrEmpty(commandString))
			{
				return null;
			}

			// attempt to tokenise the string
			List<string> tokens = TokeniseString(commandString);

			// do we have multiple tokens? if so that means our autocomplete needs info from the command itself
			if (tokens.Count > 1)
			{
				List<string> autocompleteOptions = FetchCommandAutocomplete(tokens);
				if (autocompleteOptions != null)
					autocompleteOptions.Sort();

				return autocompleteOptions;
			}

			// attempt to find a matching command
			List<string> matchingCommands = AlternateCommandNames.Keys.Where(name => name.StartsWith(tokens[0].ToLower())).ToList();

			// filter any commands that are not enabled
			matchingCommands.RemoveAll(command => !Commands[AlternateCommandNames[command]].ShowInHelpAndAutocomplete);

			// none matched - return failure indication
			if (matchingCommands.Count == 0)
				return null;
			// only one matched (and it's identical to the token) so attempt to retrieve help for this command
			if (matchingCommands.Contains(tokens[0].ToLower()))
			{
				List<string> autocompleteOptions = FetchCommandAutocomplete(tokens);
				if (autocompleteOptions != null)
					autocompleteOptions.Sort();

				return autocompleteOptions;
			}

			// otherwise return all of the matching commands
			return matchingCommands;
		}

		protected List<string> FetchCommandAutocomplete(List<string> tokens)
		{
			// extract the command
			string originalCommand = tokens[0].ToLower();
			if (!AlternateCommandNames.ContainsKey(originalCommand))
				return null;

			string command = AlternateCommandNames[originalCommand];
			tokens.RemoveAt(0);

			// retrieve the autocomplete options.
			List<string> autocompleteOptions = Commands[command].FetchAutocompleteMethods(originalCommand, tokens);

			if (autocompleteOptions != null)
				autocompleteOptions.Sort();

			return autocompleteOptions;
		}

		List<string> TokeniseString(string inputString)
		{
			List<string> tokenisedString = new List<string>();

			// tokenise the string
			string currentToken = "";
			bool inQuotationMarks = false;
			for (int index = 0; index < inputString.Length; ++index)
			{
				// current character is a space?
				if (inputString[index] == ' ')
				{
					// not in quotation marks?
					if (!inQuotationMarks)
					{
						// add this token and reset the current token
						if (currentToken.Length > 0)
							tokenisedString.Add(currentToken);
						currentToken = "";
					}
					else
						currentToken += inputString[index];
				}
				else if (inputString[index] == '\"')
				{
					// are we currently in quotation marks?
					if (inQuotationMarks)
					{
						inQuotationMarks = false;
					}
					else
						inQuotationMarks = true;
				}
				else
					currentToken += inputString[index];
			}

			// if there is data in the current token then add it
			if (currentToken.Length > 0)
				tokenisedString.Add(currentToken);

			return tokenisedString;
		}
	}
}
