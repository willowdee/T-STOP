using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace DevConsole
{
	[ConsoleCommand(new string[] {"destroy", "destroyobject"})]
	class DestroyCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [path]" + System.Environment.NewLine +
				       "    Destroys the specified game object (including its children)." + System.Environment.NewLine +
					   "    path is the path to the game object. The format for the path is: SceneName:ObjectName/ChildName/ObjectToDeleteName";
			}
			else
			{
				return "Destroys the specified game object (including its children).";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens found
			if (tokens.Length != 1)
				return "[Error] You must provide a valid path to the game object to destroy.";

			// attempt to find the object
			GameObject foundObject = CommandHelpers.GetGameObject(tokens[0]);

			// object not found?
			if (foundObject == null)
				return "[Error] Could not find the specified game object. Check the path. Paths must be case sensitive.";

			GameObject.Destroy(foundObject);

			return "Object destroyed.";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// can only autocomplete if there is a single token or no token
			if (tokens.Length > 1)
				return null;

			// fetch the autocomplete options
			return CommandHelpers.GetGameObjectAutocompleteOptions(tokens.Length > 0 ? tokens[0] : "", command);
		}	
	}

	[ConsoleCommand(new string[]{"setactive"})]
	class SetActiveCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [true/false/active/inactive] [path]" + System.Environment.NewLine +
				       "    Sets a game object to be active or inactive." + System.Environment.NewLine +
					   "    path is the path to the game object. The format for the path is: SceneName:ObjectName/ChildName/ObjectToChangeName";
			}
			else
			{
				return "Sets a game object to be active or inactive.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens found
			if (tokens.Length != 2)
				return "[Error] Incorrect number of parameters. You must provide the true/false/active/inactive flag and a valid path to the game object to modify.";

			// invalid flag provided
			string activeFlag = tokens[0].ToLower();
			if (activeFlag.ToLower() != "active" && activeFlag.ToLower() != "inactive" || activeFlag.ToLower() != "true" && activeFlag.ToLower() != "false")
				return "[Error] Unknown flag. You must provide the true/false/active/inactive flag and a valid path to the game object to modify.";

			// attempt to find the object
			GameObject foundObject = CommandHelpers.GetGameObject(tokens[1]);

			// object not found?
			if (foundObject == null)
				return "[Error] Could not find the specified game object. Check the path. Paths must be case sensitive.";

			foundObject.SetActive(activeFlag == "active" || activeFlag == "true");

			return "Object is now " + activeFlag;
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// no tokens so just return these options
			if (tokens.Length == 0)
				return new List<string>(new string[] {command + " active", command + " inactive", command + " true", command + " false"});

			// if we have one token then it may be a partial one
			if (tokens.Length == 1 && 
					tokens[0].ToLower() != "active" && tokens[0].ToLower() != "inactive" && 
					tokens[0].ToLower() != "true" && tokens[0].ToLower() != "false")
			{
				List<string> options = new List<string>();

				if ("active".StartsWith(tokens[0].ToLower()))
					options.Add(command + " active");
				if ("inactive".StartsWith(tokens[0].ToLower()))
					options.Add(command + " inactive");
				if ("true".StartsWith(tokens[0].ToLower()))
					options.Add(command + " true");
				if ("false".StartsWith(tokens[0].ToLower()))
					options.Add(command + " false");

				return options;
			}

			// invalid number of tokens so no autocompletion
			if (tokens.Length > 2)
				return null;

			// fetch the autocomplete options
			return CommandHelpers.GetGameObjectAutocompleteOptions(tokens.Length > 1 ? tokens[1] : "", command + " " + tokens[0]);
		}	
	}

	[ConsoleCommand(new string[]{"getloc", "getpos", "getlocation", "getposition"})]
	class GetLocationCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [path]" + System.Environment.NewLine +
				       "    Gets the location of the specified game object." + System.Environment.NewLine +
					   "    path is the path to the game object. The format for the path is: SceneName:ObjectName/ChildName/ObjectName";
			}
			else
			{
				return "Gets the location of the specified game object.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens found
			if (tokens.Length != 1)
				return "[Error] You must provide a valid path to the game object to query.";

			// attempt to find the object
			GameObject foundObject = CommandHelpers.GetGameObject(tokens[0]);

			// object not found?
			if (foundObject == null)
				return "[Error] Could not find the specified game object. Check the path. Paths must be case sensitive.";

			return foundObject.name + " is at " + foundObject.transform.position + " (world) " + foundObject.transform.localPosition + " (local)";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// can only autocomplete if there is a single token or no token
			if (tokens.Length > 1)
				return null;

			// fetch the autocomplete options
			return CommandHelpers.GetGameObjectAutocompleteOptions(tokens.Length > 0 ? tokens[0] : "", command);
		}
	}

	[ConsoleCommand(new string[]{"getrot", "getrotation"})]
	class GetRotationCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [path]" + System.Environment.NewLine +
				       "    Gets the rotation of the specified game object." + System.Environment.NewLine +
					   "    path is the path to the game object. The format for the path is: SceneName:ObjectName/ChildName/ObjectName";
			}
			else
			{
				return "Gets the rotation of the specified game object.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens found
			if (tokens.Length != 1)
				return "[Error] You must provide a valid path to the game object to query.";

			// attempt to find the object
			GameObject foundObject = CommandHelpers.GetGameObject(tokens[0]);

			// object not found?
			if (foundObject == null)
				return "[Error] Could not find the specified game object. Check the path. Paths must be case sensitive.";

			return foundObject.name + " is rotated at " + foundObject.transform.eulerAngles + " (world) " + foundObject.transform.localEulerAngles + " (local)";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// can only autocomplete if there is a single token or no token
			if (tokens.Length > 1)
				return null;

			// fetch the autocomplete options
			return CommandHelpers.GetGameObjectAutocompleteOptions(tokens.Length > 0 ? tokens[0] : "", command);
		}
	}

	[ConsoleCommand(new string[]{"getscale"})]
	class GetScaleCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [path]" + System.Environment.NewLine +
				       "    Gets the scale of the specified game object." + System.Environment.NewLine +
					   "    path is the path to the game object. The format for the path is: SceneName:ObjectName/ChildName/ObjectName";
			}
			else
			{
				return "Gets the scale of the specified game object.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens found
			if (tokens.Length != 1)
				return "[Error] You must provide a valid path to the game object to query.";

			// attempt to find the object
			GameObject foundObject = CommandHelpers.GetGameObject(tokens[0]);

			// object not found?
			if (foundObject == null)
				return "[Error] Could not find the specified game object. Check the path. Paths must be case sensitive.";

			return foundObject.name + " has a scale of " + foundObject.transform.localScale + " (local) ";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// can only autocomplete if there is a single token or no token
			if (tokens.Length > 1)
				return null;

			// fetch the autocomplete options
			return CommandHelpers.GetGameObjectAutocompleteOptions(tokens.Length > 0 ? tokens[0] : "", command);
		}
	}

	[ConsoleCommand(new string[]{"setloc", "setpos", "setlocation", "setposition"})]
	class SetLocationCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [path] [local/world] [x, y, z]" + System.Environment.NewLine +
				       "    Sets the location of the specified game object." + System.Environment.NewLine +
					   "    path is the path to the game object. The format for the path is: SceneName:ObjectName/ChildName/ObjectName" + System.Environment.NewLine +
					   "    x, y, z is the location of the object in either local or world space";
			}
			else
			{
				return "Sets the location of the specified game object.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens found
			if (tokens.Length < 3)
				return "[Error] You must provide a valid path to the game object to modify.";

			// attempt to find the object
			GameObject foundObject = CommandHelpers.GetGameObject(tokens[1]);

			// object not found?
			if (foundObject == null)
				return "[Error] Could not find the specified game object. Check the path. Paths must be case sensitive.";

			// is the location being set as local or world?
			bool isLocal = false;
			if (tokens[0].ToLower() == "local")
				isLocal = true;
			else if (tokens[0].ToLower() == "world")
				isLocal = false;
			else
				return "[Error] No indication was provided for if the coordinates are in local or world space.";

			// check if the coordinates are correct
			Vector3 coordinates = Vector3.zero;
			if (!CommandHelpers.Vector3FromTokens(tokens, 2, ref coordinates))
				return "[Error] The incorrect number of coordinates were given. Coordinates should be x,y,z";

			// update the location
			if (isLocal)
				foundObject.transform.localPosition = coordinates;
			else
				foundObject.transform.position = coordinates;

			return foundObject.name + " is now at " + foundObject.transform.position + " (world) " + foundObject.transform.localPosition + " (local)";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// can only autocomplete if there is only 1 or two tokens
			if (tokens.Length > 2)
				return null;

			List<string> autocompleteOptions = new List<string>();

			// check if we're just at the specifying local or world stage
			if (tokens.Length <= 1)
			{
				string lowerCaseToken = tokens.Length == 1 ? tokens[0].ToLower() : "";

				// autocomplete the coordinate space
				if (lowerCaseToken != "local" && "local".StartsWith(lowerCaseToken))
					autocompleteOptions.Add(command + " local");
				else if (lowerCaseToken != "world" && "world".StartsWith(lowerCaseToken))
					autocompleteOptions.Add(command + " world");
				
				// found valid autocomplete options?
				if (autocompleteOptions.Count > 0)
					return autocompleteOptions;
			}

			// fetch the autocomplete options
			return CommandHelpers.GetGameObjectAutocompleteOptions(tokens.Length > 1 ? tokens[1] : "", command + " " + tokens[0]);
		}
	}

	[ConsoleCommand(new string[]{"setrot", "setrotation"})]
	class SetRotationCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [path] [local/world] [x, y, z]" + System.Environment.NewLine +
				       "    Sets the rotation of the specified game object." + System.Environment.NewLine +
					   "    path is the path to the game object. The format for the path is: SceneName:ObjectName/ChildName/ObjectName" + System.Environment.NewLine +
					   "    x, y, z is the rotation of the object in either local or world space";
			}
			else
			{
				return "Sets the rotation of the specified game object.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens found
			if (tokens.Length < 3)
				return "[Error] You must provide a valid path to the game object to modify.";

			// attempt to find the object
			GameObject foundObject = CommandHelpers.GetGameObject(tokens[1]);

			// object not found?
			if (foundObject == null)
				return "[Error] Could not find the specified game object. Check the path. Paths must be case sensitive.";

			// is the location being set as local or world?
			bool isLocal = false;
			if (tokens[0].ToLower() == "local")
				isLocal = true;
			else if (tokens[0].ToLower() == "world")
				isLocal = false;
			else
				return "[Error] No indication was provided for if the coordinates are in local or world space.";

			// check if the coordinates are correct
			Vector3 coordinates = Vector3.zero;
			if (!CommandHelpers.Vector3FromTokens(tokens, 2, ref coordinates))
				return "[Error] The incorrect number of coordinates were given. Coordinates should be x,y,z";

			// update the location
			if (isLocal)
				foundObject.transform.localEulerAngles = coordinates;
			else
				foundObject.transform.eulerAngles = coordinates;

			return foundObject.name + " is now rotated to " + foundObject.transform.eulerAngles + " (world) " + foundObject.transform.localEulerAngles + " (local)";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// can only autocomplete if there is only 1 or two tokens
			if (tokens.Length > 2)
				return null;

			List<string> autocompleteOptions = new List<string>();

			// check if we're just at the specifying local or world stage
			if (tokens.Length <= 1)
			{
				string lowerCaseToken = tokens.Length == 1 ? tokens[0].ToLower() : "";

				// autocomplete the coordinate space
				if (lowerCaseToken != "local" && "local".StartsWith(lowerCaseToken))
					autocompleteOptions.Add(command + " local");
				else if (lowerCaseToken != "world" && "world".StartsWith(lowerCaseToken))
					autocompleteOptions.Add(command + " world");
				
				// found valid autocomplete options?
				if (autocompleteOptions.Count > 0)
					return autocompleteOptions;
			}

			// fetch the autocomplete options
			return CommandHelpers.GetGameObjectAutocompleteOptions(tokens.Length > 1 ? tokens[1] : "", command + " " + tokens[0]);
		}
	}

	[ConsoleCommand(new string[]{"setscale"})]
	class SetScaleCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [path] [x, y, z]" + System.Environment.NewLine +
				       "    Sets the rotation of the specified game object." + System.Environment.NewLine +
					   "    path is the path to the game object. The format for the path is: SceneName:ObjectName/ChildName/ObjectName" + System.Environment.NewLine +
					   "    x, y, z is the scale of the object in either local or world space";
			}
			else
			{
				return "Sets the scale of the specified game object.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens found
			if (tokens.Length < 2)
				return "[Error] You must provide a valid path to the game object to modify.";

			// attempt to find the object
			GameObject foundObject = CommandHelpers.GetGameObject(tokens[0]);

			// object not found?
			if (foundObject == null)
				return "[Error] Could not find the specified game object. Check the path. Paths must be case sensitive.";

			// check if the coordinates are correct
			Vector3 coordinates = Vector3.zero;
			if (!CommandHelpers.Vector3FromTokens(tokens, 1, ref coordinates))
				return "[Error] The incorrect number of coordinates were given. Coordinates should be x,y,z";

			// update the scale
			foundObject.transform.localScale = coordinates;

			return foundObject.name + " is now scaled to " + foundObject.transform.localScale + " (local)";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// can only autocomplete if there is only 1 or no tokens
			if (tokens.Length > 1)
				return null;

			// fetch the autocomplete options
			return CommandHelpers.GetGameObjectAutocompleteOptions(tokens.Length > 0 ? tokens[0] : "", command);
		}
	}

	[ConsoleCommand(new string[]{"setparent"})]
	class SetParentCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [path] [path to new parent]" + System.Environment.NewLine +
				       "    Changes the parent of the specified game object." + System.Environment.NewLine +
					   "    path is the path to a game object. The format for the path is: SceneName:ObjectName/ChildName/ObjectName";
			}
			else
			{
				return "Changes the parent of the specified game object.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens found
			if (tokens.Length == 0)
				return "[Error] You must provide a valid path to the game object to update and optionally a path to the new parent.";

			// attempt to find the object
			GameObject foundObject = CommandHelpers.GetGameObject(tokens[0]);

			// object not found?
			if (foundObject == null)
				return "[Error] Could not find the game object to change. Check the path. Paths must be case sensitive.";

			// attempt to find the new parent object
			GameObject foundParentObject = tokens.Length > 1 ? CommandHelpers.GetGameObject(tokens[1]) : null;

			// object not found?
			if (tokens.Length > 1 && foundParentObject == null)
				return "[Error] Could not find the new parent game object. Check the path. Paths must be case sensitive.";

			// did the user give the same path?
			if (foundObject == foundParentObject)
				return "[Error] The object and the new parent cannot be the same. Please provide two different game objects.";

			// reparent the object
			foundObject.transform.SetParent(foundParentObject != null ? foundParentObject.transform : null);

			return "Parent updated";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// can only autocomplete if there are no, 1 or 2 tokens
			if (tokens.Length > 2)
				return null;

			// none or one token?
			if (tokens.Length <= 1)
			{
				List<string> autocompleteOptions = CommandHelpers.GetGameObjectAutocompleteOptions(tokens.Length > 0 ? tokens[0] : "", command);

				if (autocompleteOptions != null)
					return autocompleteOptions;
			}

			// this shouldn't happen! but just in case
			if (tokens.Length == 0)
			{
				Debug.LogError("Failed to find autocomplete options and have no tokens. This shouldn't be possible.");
				return null;
			}

			// setup the base path for the autocomplete options
			string basePath = tokens[0];
			if (basePath.Contains(" "))
				basePath = command + " \"" + tokens[0] + "\"";
			else
				basePath = command + " " + tokens[0];

			// fetch the autocomplete options
			return CommandHelpers.GetGameObjectAutocompleteOptions(tokens.Length > 1 ? tokens[1] : "", basePath);
		}	
	}

	[ConsoleCommand(new string[]{"addcomponent", "addcomp"})]
	class AddComponentCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + "[Component Type] [Path]" + System.Environment.NewLine +
				       "    Adds a new component to the specified game object." + System.Environment.NewLine +
					   "    Component Type is the type of the component to attach" + System.Environment.NewLine + 
					   "    path is the path to the game object. The format for the path is: SceneName:ObjectName/ChildName/ObjectName";
			}
			else
			{
				return "Adds a new component to the specified game object.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens found
			if (tokens.Length != 2)
				return "[Error] You must provide a component type and a valid path to the game object.";

			// attempt to retrieve the component
			System.Type componentType = null;
			if (ConsoleDaemon.Instance.AvailableComponents.ContainsKey(tokens[0]))
				componentType = ConsoleDaemon.Instance.AvailableComponents[tokens[0]];

			// type did not match?
			if (componentType == null)
				return "[Error] The specified component type could not be found. Please check the spelling.";

			// attempt to find the object
			GameObject foundObject = CommandHelpers.GetGameObject(tokens[1]);

			// object not found?
			if (foundObject == null)
				return "[Error] Could not find the specified game object. Check the path. Paths must be case sensitive.";

			// add the component
			foundObject.AddComponent(componentType);

			return "Added component to the game object";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// if there are no or only a single parameter then attempt to autocomplete the type
			if (tokens.Length <= 1)
			{
				string typeString = tokens.Length == 1 ? tokens[0] : "";

				// filter the options based on the type name
				List<string> autocompleteOptions = ConsoleDaemon.Instance.AvailableComponents.Keys.
														Where(typeName => typeName.StartsWith(typeString)).
														Select(typeName => command + " " + typeName).ToList();

				// if we found results then return them
				if (autocompleteOptions != null && autocompleteOptions.Count > 0)
				{
					// only return if there was not an exact match
					if (!(tokens.Length > 0 && autocompleteOptions.Contains(command + " " + tokens[0])))
						return autocompleteOptions;
				}
			}

			// if there are more than 2 tokens then don't run the autocomplete
			if (tokens.Length > 2)
				return null;

			// fetch the autocomplete options
			return CommandHelpers.GetGameObjectAutocompleteOptions(tokens.Length > 1 ? tokens[1] : "", command + " " + tokens[0]);
		}
	}

	[ConsoleCommand(new string[] {"delcomp", "delcomponent", "deletecomponent", "remcomp", "removecomponent"})]
	class RemoveComponentCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + System.Environment.NewLine +
				       "    Removes a specific component from a game object." + System.Environment.NewLine +
					   "    path is the path to the component. The format for the path is: SceneName:ObjectName/ChildName/ObjectName.Component";
			}
			else
			{
				return "Removes a specific component from a game object.";
			}
		}

		public static string Execute(string[] tokens)
		{
			if (tokens.Length < 1)
				return "[Error] You must provide a path to the component to remove.";

			// attempt to find the component
			Component foundComponent = CommandHelpers.GetComponent(tokens[0]);
			if (foundComponent == null)
				return "[Error] Could not find the specified component. Check that the path is correct.";

			GameObject.Destroy(foundComponent);
			
			return "Component destroyed";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// can only autocomplete if there is a single token or no token
			if (tokens.Length > 1)
				return null;

			// fetch the autocomplete options
			return CommandHelpers.GetComponentAutocompleteOptions(tokens.Length > 0 ? tokens[0] : "", command);
		}	
	}

	[ConsoleCommand(new string[] {"listcomp", "listcomponents", "getcomp", "getcomponents"})]
	class ListComponentsCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [path]" + System.Environment.NewLine +
				       "    Lists all of the components of the specified game object." + System.Environment.NewLine +
					   "    path is the path to the game object. The format for the path is: SceneName:ObjectName/ChildName/ObjectName";
			}
			else
			{
				return "Lists all of the components of the specified game object";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens found
			if (tokens.Length != 1)
				return "[Error] You must provide a valid path to the game object to query.";

			// attempt to find the object
			GameObject foundObject = CommandHelpers.GetGameObject(tokens[0]);

			// object not found?
			if (foundObject == null)
				return "[Error] Could not find the specified game object. Check the path. Paths must be case sensitive.";

			Dictionary<string, Component> componentMapping = CommandHelpers.GetComponentMapping(foundObject);
			List<string> componentNames = componentMapping.Keys.ToList();

			// find the longest component name
			int longestComponentName = 0;
			foreach(string componentName in componentNames)
			{
				longestComponentName = Mathf.Max(componentName.Length, longestComponentName);
			}

			// build the list of components
			string result = "Found the following components: ";
			foreach(string componentName in componentNames)
			{
				Component component = componentMapping[componentName];
				string status = component is Behaviour ? ((component as Behaviour).enabled ? "Enabled" : "Disabled") : "Always Enabled";

				result += System.Environment.NewLine + "    " + componentName.PadRight(longestComponentName) + "    : " + status;
			}

			return result;
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// can only autocomplete if there is a single token or no token
			if (tokens.Length > 1)
				return null;

			// fetch the autocomplete options
			return CommandHelpers.GetGameObjectAutocompleteOptions(tokens.Length > 0 ? tokens[0] : "", command);
		}
	}

	[ConsoleCommand(new string[] {"setenabled"})]
	class SetEnabledCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [true/false/enabled/disabled] [path]" + System.Environment.NewLine +
				       "    Sets a component to be enabled or disabled." + System.Environment.NewLine +
					   "    path is the path to the component. The format for the path is: SceneName:ObjectName/ChildName/ObjectName.ComponentName";
			}
			else
			{
				return "Sets a component to be enabled or disabled.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens found
			if (tokens.Length != 2)
				return "[Error] Incorrect number of parameters. You must provide the true/false/enabled/disabled flag and a valid path to the component to modify.";

			// invalid flag provided
			string enabledFlag = tokens[0].ToLower();
			if (enabledFlag.ToLower() != "enabled" && enabledFlag.ToLower() != "disabled" && 
				enabledFlag.ToLower() != "true" && enabledFlag.ToLower() != "false")
				return "[Error] Unknown flag. You must provide the true/false/enabled/disabled flag and a valid path to the component to modify.";

			// attempt to find the component
			Component foundComponent = CommandHelpers.GetComponent(tokens[1]);

			// component not found?
			if (foundComponent == null)
				return "[Error] Could not find the specified component. Check the path. Paths must be case sensitive.";

			// component cannot be enabled/disabled
			if (!(foundComponent is Behaviour))
				return "[Error] The component does not support being enabled or disabled.";

			// Update the component
			(foundComponent as Behaviour).enabled = (enabledFlag == "enabled") || (enabledFlag == "true");

			return "Component is now " + enabledFlag;
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// no tokens so just return these options
			if (tokens.Length == 0)
				return new List<string>(new string[] {command + " enabled", command + " disabled", command + " true", command + " false"});

			// if we have one token then it may be a partial one
			if (tokens.Length == 1 && 
				tokens[0].ToLower() != "enabled" && tokens[0].ToLower() != "disabled" && 
				tokens[0].ToLower() != "true" && tokens[0].ToLower() != "false")
			{
				List<string> options = new List<string>();

				if ("enabled".StartsWith(tokens[0].ToLower()))
					options.Add(command + " enabled");
				if ("disabled".StartsWith(tokens[0].ToLower()))
					options.Add(command + " disabled");
				if ("true".StartsWith(tokens[0].ToLower()))
					options.Add(command + " true");
				if ("false".StartsWith(tokens[0].ToLower()))
					options.Add(command + " false");

				return options;
			}

			// invalid number of tokens so no autocompletion
			if (tokens.Length > 2)
				return null;

			// fetch the autocomplete options
			return CommandHelpers.GetComponentAutocompleteOptions(tokens.Length > 1 ? tokens[1] : "", command + " " + tokens[0]);
		}	
	}

	[ConsoleCommand(new string[]{"instantiate", "spawn"})]
	class InstantiateCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [Prefab Name]" + System.Environment.NewLine +
				       "    Instantiates a new prefab by name. Prefabs can only be found if they are in a Resources folder.";
			}
			else
			{
				return "Instantiates a new prefab by name.";
			}
		}

		public static string Execute(string[] tokens)
		{
			// incorrect number of tokens found
			if (tokens.Length != 1)
				return "[Error] Incorrect number of parameters. Must provide only a single prefab name.";

			// check if the prefab exists
			GameObject prefabObject = null;
			if (ConsoleDaemon.Instance.AvailablePrefabs.ContainsKey(tokens[0]))
				prefabObject = ConsoleDaemon.Instance.AvailablePrefabs[tokens[0]];
			else
				return "[Error] Could not find a prefab with the given name. Check the case and that the prefab is in a resources folder.";

			GameObject newObject = GameObject.Instantiate(prefabObject);

			return "Instantiated new object " + newObject.name + " at " + newObject.transform.position;
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// too many tokens?
			if (tokens.Length > 1)
				return null;

			// find the matching prefabs
			string partialName = tokens.Length > 0 ? tokens[0] : "";
			List<string> matchingPrefabs = ConsoleDaemon.Instance.AvailablePrefabs.Keys.Where(prefabName => prefabName.StartsWith(partialName)).ToList();

			// create the autocomplete list by adding quotation marks where required and include the base command
			List<string> autocompleteOptions = new List<string>(matchingPrefabs.Count);
			foreach(string prefabName in matchingPrefabs)
			{
				// add in the quotation marks if needed
				string workingPrefabName = prefabName;
				if (workingPrefabName.Contains(" "))
					workingPrefabName = "\"" + prefabName + "\"";

				autocompleteOptions.Add(command + " " + workingPrefabName);
			}

			return autocompleteOptions;
		}	
	}
}