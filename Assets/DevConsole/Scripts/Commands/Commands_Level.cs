using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevConsole
{
	[ConsoleCommand(new string[] {"loadscene", "loadlevel"})]	
	class LoadLevelCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [additive] [scene 1 name] ... [scene n name]" + System.Environment.NewLine +
				       "    Loads one or more scenes by name. Scenes must be in the build settings." + System.Environment.NewLine + 
					   "    Include the keyword additive to perform additive loading. If there are multiple scenes additive is automatically set.";
			}
			else
			{
				return "Loads one or more scenes.";
			}
		}

		public static string Execute(string[] tokens)
		{
			if (tokens.Length == 0)
				return "[Error] You must provide the name of at least one scene to load.";

			// get the names of the scenes in the build
			List<string> sceneNamesInBuild = CommandHelpers.GetSceneNamesInBuild();

			// force additive if there are multiple scenes
			bool isAdditive = tokens.Length > 1 ? true : false;

			// check that all of the provided scene names are valid
			foreach(string sceneName in tokens)
			{
				string workingSceneName = sceneName.ToLower();

				if (workingSceneName == "additive")
					isAdditive = true;
				else if (sceneNamesInBuild.IndexOf(workingSceneName) < 0)
					return "[Error] Unable to load scene \'" + sceneName + "\' as it is not in the build settings.";
			}

			// load the requested scenes
			foreach(string sceneName in tokens)
			{
				// skip if trying to do additive loading
				if (sceneName.ToLower() == "additive")
					continue;

				SceneManager.LoadScene(sceneName, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
			}

			return "Loaded the requested scenes.";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// get the names of the scenes in the build
			List<string> sceneNamesInBuild = CommandHelpers.GetSceneNamesInBuild();

			List<string> autocompleteOptions = new List<string>();
			string baseCommand = command;;

			// check if the user has already indicated additive
			bool needAdditiveInOptions = true;
			foreach(string token in tokens)
			{
				if (token.ToLower() == "additive")
				{
					needAdditiveInOptions = false;
					break;
				}
			}

			// handle additive keyword by sneakily adding it as a scene name unless the tokens already contain that keyword
			if (needAdditiveInOptions)
				sceneNamesInBuild.Add("additive");

			// if there are no tokens then nothing further to do. the current base command is fine
			if (tokens == null || tokens.Length == 0)
			{
			} // check if the last token exactly matches a scene name
			else  if (sceneNamesInBuild.Contains(tokens[tokens.Length - 1].ToLower()))
			{
				// construct the base command
				foreach(string sceneName in tokens)
				{
					// remove the scene from the available options
					sceneNamesInBuild.Remove(sceneName.ToLower());

					baseCommand += " " + (sceneName.Contains(" ") ? "\"" + sceneName + "\"" : sceneName);
				}
			} // otherwise the token contains a partial name
			else
			{
				// construct the base command (exclude the final token)
				for(int index = 0; index < tokens.Length - 1; ++index)
				{
					// remove the scene from the available options
					sceneNamesInBuild.Remove(tokens[index].ToLower());

					baseCommand += " " + (tokens[index].Contains(" ") ? "\"" + tokens[index] + "\"" : tokens[index]);
				}

				// filter out any scene names that do not match the potential candidates
				string partialName = tokens[tokens.Length - 1].ToLower();
				if (partialName != "additive")
				{
					for (int index = 0; index < sceneNamesInBuild.Count; ++index)
					{
						if (!sceneNamesInBuild[index].StartsWith(partialName))
						{
							sceneNamesInBuild.RemoveAt(index);
							--index;
						}
					}
				}
				else
					baseCommand += " additive";

				// if we ended up with no valid scene names then error out
				if (sceneNamesInBuild.Count == 0)
					return null;
			}

			// fill out the list of autocomplete options
			foreach(string sceneName in sceneNamesInBuild)
			{
				string workingSceneName = sceneName.Contains(" ") ? "\"" + sceneName + "\"" : sceneName;

				autocompleteOptions.Add(baseCommand + " " + workingSceneName);
			}

			return autocompleteOptions;
		}
	}

	[ConsoleCommand(new string[] {"unloadscene", "unloadlevel"})]
	class UnloadLevelCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + " [scene 1] ... [scene n]" + System.Environment.NewLine +
				       "    Unloads one or more currently loaded scenes by name.";
			}
			else
			{
				return "Unloads one or more currently loaded scenes by name.";
			}
		}

		public static string Execute(string[] tokens)
		{
			if (tokens.Length == 0)
				return "[Error] You must provide the name of at least one scene to unload.";

			// get the names of all currently loaded scenes
			List<string> loadedSceneNames = new List<string>();
			for (int index = 0; index < SceneManager.sceneCount; ++index)
				loadedSceneNames.Add(SceneManager.GetSceneAt(index).name.ToLower());

			// check that all of the provided scene names are valid
			foreach(string sceneName in tokens)
			{
				string workingSceneName = sceneName.ToLower();

				if (loadedSceneNames.IndexOf(workingSceneName) < 0)
					return "[Error] Unable to unload the scene \'" + sceneName + "\' as it is not currently loaded.";
			}

			// load the requested scenes
			foreach(string sceneName in tokens)
			{
				SceneManager.UnloadSceneAsync(sceneName);
			}

			return "Requested unloading of the current scenes.";
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			// get the names of the currently loaded scenes
			List<string> loadedSceneNames = new List<string>();
			for (int index = 0; index < SceneManager.sceneCount; ++index)
				loadedSceneNames.Add(SceneManager.GetSceneAt(index).name.ToLower());

			List<string> autocompleteOptions = new List<string>();
			string baseCommand = command;

			// if there are no tokens then nothing further to do. the current base command is fine
			if (tokens == null || tokens.Length == 0)
			{
			} // check if the last token exactly matches a scene name
			else  if (loadedSceneNames.Contains(tokens[tokens.Length - 1].ToLower()))
			{
				// construct the base command
				foreach(string sceneName in tokens)
				{
					// update the base command
					baseCommand += " " + (sceneName.Contains(" ") ? "\"" + sceneName + "\"" : sceneName);

					// remove this scene from the potential ones to unload
					loadedSceneNames.Remove(sceneName.ToLower());
				}

				// if we ended up with no valid scene names then error out
				if (loadedSceneNames.Count == 0)
					return null;				
			} // otherwise the token contains a partial name
			else
			{
				// construct the base command (exclude the final token)
				for(int index = 0; index < tokens.Length - 1; ++index)
				{
					baseCommand += " " + (tokens[index].Contains(" ") ? "\"" + tokens[index] + "\"" : tokens[index]);

					// remove this scene from the potential ones to unload
					loadedSceneNames.Remove(tokens[index].ToLower());
				}

				// filter out any scene names that do not match the potential candidates
				string partialName = tokens[tokens.Length - 1].ToLower();
				for (int index = 0; index < loadedSceneNames.Count; ++index)
				{
					if (!loadedSceneNames[index].StartsWith(partialName))
					{
						loadedSceneNames.RemoveAt(index);
						--index;
					}
				}

				// if we ended up with no valid scene names then error out
				if (loadedSceneNames.Count == 0)
					return null;
			}

			// fill out the list of autocomplete options
			foreach(string sceneName in loadedSceneNames)
			{
				string workingSceneName = sceneName.Contains(" ") ? "\"" + sceneName + "\"" : sceneName;

				autocompleteOptions.Add(baseCommand + " " + workingSceneName);
			}

			return autocompleteOptions;
		}
	}

	[ConsoleCommand(new string[] {"listscenes", "listlevels"})]
	class ListLevelsCommand
	{
		public static string Help(string command, bool verbose)
		{
			if (verbose)
			{
				return command + System.Environment.NewLine +
				       "    Lists all of the known scenes in the build and their status.";
			}
			else
			{
				return "Lists all of the known scenes in the build and their status.";
			}
		}

		public static string Execute(string[] tokens)
		{
			List<string> sceneNamesInBuild = CommandHelpers.GetSceneNamesInBuild();

			// find the longest scene name
			int longestName = 0;
			foreach (string sceneName in sceneNamesInBuild)
				longestName = Mathf.Max(longestName, sceneName.Length);

			// assemble the list of scenes and their status
			string result = "Known scenes: ";			
			foreach(string sceneName in sceneNamesInBuild)
			{
				result += System.Environment.NewLine + "    " + sceneName.PadRight(longestName);
				if (SceneManager.GetSceneByName(sceneName).isLoaded)
					result += " - Loaded";
				else
					result += " - Not loaded";
			}

			return result;
		}

		public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
		{
			return null;
		}
	}
}