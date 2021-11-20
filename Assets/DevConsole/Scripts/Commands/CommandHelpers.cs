using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Reflection;

namespace DevConsole
{
	class CommandHelpers
	{
		#region Level Related
		public static List<string> GetSceneNamesInBuild()
		{
			List<string> sceneNames = new List<string>();
			for (int index = 0; index < SceneManager.sceneCountInBuildSettings; ++index)
			{
				sceneNames.Add(System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(index)).ToLower());
			}

			return sceneNames;
		}
		#endregion

		#region Game Object Related
		public static List<string> GetTopLevelGameObjectNames()
		{
			List<string> topLevelObjectNames = new List<string>();

			// grab the list of game objects in each scene
			for(int index = 0; index < SceneManager.sceneCount; ++index)
			{
				// Retrieve the scene and the objects
				Scene scene = SceneManager.GetSceneAt(index);
				GameObject[] topLevelObjects = scene.GetRootGameObjects();

				// setup the prefix
				string namePrefix = SceneManager.sceneCount == 1 ? "" : (scene.name + ":");

				// retrieve the names
				foreach(GameObject topLevelObject in topLevelObjects)
				{
					topLevelObjectNames.Add(namePrefix + topLevelObject.name);
				}
			}

			return topLevelObjectNames;
		}

		public static GameObject GetGameObject(string path)
		{
			// split up the path and determine if the scene is present
			bool pathIncludesScene = path.Contains(":");
			string[] pathElements = path.Split(new string[]{":", "/"}, System.StringSplitOptions.RemoveEmptyEntries);

			// no path elements? exit out
			if (pathElements.Length == 0 || string.IsNullOrEmpty(pathElements[0]))
				return null;

			GameObject workingObject = null;

			// if the path includes the scene we can use a shorter approach to get the game object
			if (pathIncludesScene)
			{
				// retrieve the scene and the game objects
				Scene scene = SceneManager.GetSceneByName(pathElements[0]);
				GameObject[] topLevelObjects = scene.GetRootGameObjects();

				// if an object has been specified
				if (pathElements.Length > 1)
				{
					// search for the matching name
					foreach(GameObject activeObject in topLevelObjects)
					{
						if (activeObject.name == pathElements[1])
						{
							workingObject = activeObject;
							break;
						}
					}
				} // no object has been specified so fail
				else 
					return null;
			}
			else
			{
				// Attempt to find the object
				for(int index = 0; index < SceneManager.sceneCount; ++index)
				{
					// Retrieve the scene and the objects
					GameObject[] topLevelObjects = SceneManager.GetSceneAt(index).GetRootGameObjects();

					// retrieve the names
					foreach(GameObject topLevelObject in topLevelObjects)
					{
						if (topLevelObject.name == pathElements[0])
						{
							workingObject = topLevelObject;
							break;
						}
					}
				}
			}

			// no working object so nothing more we can do. means the user provided an invalid name
			if (workingObject == null)
				return null;			

			// traverse the list of path elements until we've found the last one
			for (int index = pathIncludesScene ? 2 : 1; index < pathElements.Length; ++index)
			{
				// Get the children of this object and see if any match
				bool childMatched = false;
				for (int childIndex = 0; childIndex < workingObject.transform.childCount; ++childIndex)
				{
					GameObject child = workingObject.transform.GetChild(childIndex).gameObject;

					// does the child match?
					if (child.name == pathElements[index])
					{
						workingObject = child;
						childMatched = true;
						break;
					}
				}

				// child did not match?
				if (!childMatched)
					return null;
			}

			return workingObject;
		}

		public static List<string> GetNamesOfChildren(string path)
		{
			// split up the path and determine if the scene is present
			bool pathIncludesScene = path.Contains(":");
			string[] pathElements = path.Split(new string[]{":", "/"}, System.StringSplitOptions.RemoveEmptyEntries);

			// no path elements? exit out
			if (pathElements.Length == 0 || string.IsNullOrEmpty(pathElements[0]))
				return GetTopLevelGameObjectNames();

			GameObject workingObject = null;

			// if the path includes the scene we can use a shorter approach to get the game object
			if (pathIncludesScene)
			{
				// retrieve the scene and the game objects
				Scene scene = SceneManager.GetSceneByName(pathElements[0]);
				GameObject[] topLevelObjects = scene.GetRootGameObjects();

				// if an object has been specified
				if (pathElements.Length > 1)
				{
					// search for the matching name
					foreach(GameObject activeObject in topLevelObjects)
					{
						if (activeObject.name.ToLower() == pathElements[1].ToLower())
						{
							workingObject = activeObject;
							break;
						}
					}
				} // no object has been specified so just return a list of the names
				else 
				{
					List<string> objectNames = new List<string>(topLevelObjects.Length);
					foreach(GameObject activeObject in topLevelObjects)
					{
						objectNames.Add(scene.name + ":" + activeObject.name);
					}

					return objectNames;
				}
			}
			else
			{
				// Attempt to find the object
				for(int index = 0; index < SceneManager.sceneCount; ++index)
				{
					// Retrieve the scene and the objects
					GameObject[] topLevelObjects = SceneManager.GetSceneAt(index).GetRootGameObjects();

					// retrieve the names
					foreach(GameObject topLevelObject in topLevelObjects)
					{
						if (topLevelObject.name.ToLower() == pathElements[0].ToLower())
						{
							workingObject = topLevelObject;
							break;
						}
					}
				}
			}

			// no working object so nothing more we can do. means the user provided an invalid name
			if (workingObject == null)
			{
				List<string> topLevelNames = GetTopLevelGameObjectNames();

				if (SceneManager.sceneCount == 1 && !pathIncludesScene)
					return topLevelNames.Where(name => name.ToLower().StartsWith(pathElements[0].ToLower())).ToList();
				else
					return topLevelNames.Where(name => name.ToLower().Contains(":" + pathElements[0].ToLower())).ToList();
			}

			// traverse the list of path elements until we've found the last one
			bool reachedLastChild = false;
			bool evaluatedAnyElement = false;
			for (int index = pathIncludesScene ? 2 : 1; index < pathElements.Length; ++index)
			{
				evaluatedAnyElement = true;

				// Get the children of this object and see if any match
				bool childMatched = false;
				for (int childIndex = 0; childIndex < workingObject.transform.childCount; ++childIndex)
				{
					GameObject child = workingObject.transform.GetChild(childIndex).gameObject;

					// does the child match?
					if (child.name.ToLower() == pathElements[index].ToLower())
					{
						reachedLastChild = index == pathElements.Length - 1;
						workingObject = child;
						childMatched = true;
						break;
					}
				}

				// child did not match?
				if (!childMatched)
					break;
			}

			// do we need to chop off the last element?
			string basePath = path;
			if (!reachedLastChild && evaluatedAnyElement)
			{
				int index = basePath.LastIndexOf('/');
				if (index > 0)
					basePath = basePath.Substring(0, index);
			}

			// if no elements were reached then assume we're already on the last child
			reachedLastChild |= !evaluatedAnyElement;

			// no working object so nothing more we can do. means the user provided an invalid name
			if (workingObject == null)
				return null;

			// build the list of child object names
			{
				List<string> objectNames = workingObject.transform.childCount > 0 ? new List<string>(workingObject.transform.childCount) : null;
				string pathPrefix = (pathIncludesScene && pathElements.Length == 1) ? (basePath + ":") : (basePath + "/");
				for(int childIndex = 0; childIndex < workingObject.transform.childCount; ++childIndex)
				{
					string childName = workingObject.transform.GetChild(childIndex).gameObject.name;

					// if we did not reach the last child then the last child is actually the start of a name so only add children that start with that
					if (!reachedLastChild && !childName.ToLower().StartsWith(pathElements[pathElements.Length - 1].ToLower()))
						continue;

					objectNames.Add(pathPrefix + childName);
				}

				return objectNames;
			}
		}

		public static List<string> GetGameObjectAutocompleteOptions(string basePath, string baseCommand)
		{
			// fetch the autocomplete options
			List<string> autocompleteOptions = CommandHelpers.GetNamesOfChildren(basePath);

			// if we got no autocomplete options then nothing to return
			if (autocompleteOptions == null || autocompleteOptions.Count == 0)
				return null;

			// otherwise appeand the command to each of the options
			for (int index = 0; index < autocompleteOptions.Count; ++index)
			{
				string objectName = autocompleteOptions[index];

				// add in quotation mark if needed
				if (objectName.Contains(" "))
					objectName = "\"" + objectName + "\"";

				autocompleteOptions[index] = baseCommand + " " + objectName;
			}

			return autocompleteOptions;
		}

		public static List<string> GetComponentsOnObject(GameObject objectToQuery)
		{
			Dictionary<string, Component> componentMapping = GetComponentMapping(objectToQuery);

			// no components found (or failed to retrieve ones)
			if (componentMapping == null)
				return null;

			return componentMapping.Keys.ToList();
		}

		public static Dictionary<string, Component> GetComponentMapping(GameObject objectToQuery)
		{
			// retrieve all of the components
			Component[] allComponents = objectToQuery.GetComponents<Component>();
			
			// build the list of component type names
			Dictionary<string, int> componentTypeCounts = new Dictionary<string, int>();
			Dictionary<string, int> visitedCounts = new Dictionary<string, int>();
			foreach(Component component in allComponents)
			{
				string typeName = component.GetType().Name;

				if (componentTypeCounts.ContainsKey(typeName))
					componentTypeCounts[typeName] = componentTypeCounts[typeName] + 1;
				else
				{
					componentTypeCounts[typeName] = 1;
					visitedCounts[typeName] = 0;
				}
			}

			// build the component map
			Dictionary<string, Component> componentMapping = new Dictionary<string, Component>();
			foreach(Component component in allComponents)
			{
				string typeName = component.GetType().Name;

				// extract the type and visit counts
				int typeCount = componentTypeCounts[typeName];
				int visitCount = visitedCounts[typeName];

				// construct the component name
				string componentName = typeName;
				if (typeCount > 1)
				{
					componentName += "_" + (visitCount + 1);
					visitedCounts[typeName] = visitCount + 1;
				}

				// update the dictionary
				componentMapping[componentName] = component;
			}

			return componentMapping;			
		}

		public static List<string> GetComponentAutocompleteOptions(string path, string baseCommand)
		{
			string[] pathElements = path.Split('.');

			// no path elements or not at the component listing stage - run the game object autocomplete
			if (pathElements.Length == 0 || !path.Contains("."))
			{
				List<string> autocompleteOptions = GetGameObjectAutocompleteOptions(path, baseCommand);

				if (autocompleteOptions != null && autocompleteOptions.Count > 0)
					return autocompleteOptions;
			}

			// retrieve the game object to query
			GameObject objectToQuery = GetGameObject(pathElements[0]);
			if (objectToQuery == null)
				return null;

			// grab the component names and filter by what has already been entered
			List<string> componentNames = GetComponentsOnObject(objectToQuery);
			if (pathElements.Length > 1 && !string.IsNullOrEmpty(pathElements[1]))
			{
				componentNames = componentNames.Where(name => name.ToLower().StartsWith(pathElements[1].ToLower())).ToList();
			}

			// no component names left?
			if (componentNames == null || componentNames.Count == 0)
				return null;

			// assemble and return the list of autocomplete options
			{
				List<string> autocompleteOptions = new List<string>();
				foreach(string componentName in componentNames)
				{
					// construct the working path
					string workingPath = pathElements[0] + "." + componentName;
					if (workingPath.Contains(" "))
						workingPath = "\"" + workingPath + "\"";

					autocompleteOptions.Add(baseCommand + " " + workingPath);
				}

				return autocompleteOptions;
			}
		}

		public static Component GetComponent(string path)
		{
			string[] pathElements = path.Split('.');

			// no path elements or not at the component listing stage
			if (pathElements.Length < 2 || !path.Contains("."))
				return null;

			// retrieve the game object to query
			GameObject objectToQuery = GetGameObject(pathElements[0]);
			if (objectToQuery == null)
				return null;

			// attempt to find the component in the map
			Dictionary<string, Component> componentMap = GetComponentMapping(objectToQuery);
			if (componentMap == null || !componentMap.ContainsKey(pathElements[1]))
				return null;

			return componentMap[pathElements[1]];
		}
		#endregion

		#region Data Conversion Related
		public static List<float> FloatListFromTokens(string[] tokens, int startIndex)
		{
			List<float> values = new List<float>();

			// split all of the tokens
			for(int index = startIndex; index < tokens.Length; ++index)
			{
				string[] subtokens = tokens[index].Split(new string[] {","}, System.StringSplitOptions.RemoveEmptyEntries);

				// parse each subtoken
				foreach(string tokenValue in subtokens)
				{
					float floatValue = 0;

					// if the token matches then add it
					if (float.TryParse(tokenValue, out floatValue))
						values.Add(floatValue);
				}
			}

			return values;
		}

		public static bool Vector2FromTokens(string[] tokens, int startIndex, ref Vector2 vectorValue)
		{
			List<float> values = FloatListFromTokens(tokens, startIndex);

			// no values found at all? or too many values found?
			if (values.Count == 0 || values.Count > 2)
				return false;

			// need to extend the number of values?
			if (values.Count == 1)
				values.Add(values[0]);

			vectorValue = new Vector2(values[0], values[1]);
				
			return true;
		}

		public static bool Vector3FromTokens(string[] tokens, int startIndex, ref Vector3 vectorValue)
		{
			List<float> values = FloatListFromTokens(tokens, startIndex);

			// no values found at all? or too many values found?
			if (values.Count == 0 || values.Count > 3)
				return false;

			// need to extend the number of values?
			if (values.Count == 1)
			{
				values.Add(values[0]);
				values.Add(values[0]);
			}
			else if (values.Count == 2)
				values.Add(0);

			vectorValue = new Vector3(values[0], values[1], values[2]);
				
			return true;
		}

		public static bool Vector4FromTokens(string[] tokens, int startIndex, ref Vector4 vectorValue)
		{
			List<float> values = FloatListFromTokens(tokens, startIndex);

			// no values found at all? or too many values found?
			if (values.Count == 0 || values.Count > 4)
				return false;

			// need to extend the number of values?
			if (values.Count == 1)
			{
				values.Add(values[0]);
				values.Add(values[0]);
				values.Add(values[0]);
			}
			else if (values.Count == 2)
			{
				values.Add(0);
				values.Add(0);
			}
			else if (values.Count == 3)
				values.Add(0);

			vectorValue = new Vector4(values[0], values[1], values[2], values[3]);
				
			return true;
		}
		#endregion

		#region Reflection Related
		public enum AutocompleteCandidates
		{
			Functions,
			Variables
		}

		public static List<string> GetAutocompleteOptions(string path, string command, AutocompleteCandidates candidateType)
		{
			// if we're at this point we know we have at least either a component or a class with static members

			// split based on the . as this might indicate a component or variable split
			string[] pathElements = path.Split('.');

			System.Type foundType = null;
			Component foundComponent = null;

			// attempt to find the type
			if (ConsoleDaemon.Instance.AvailableTypes.ContainsKey(pathElements[0]))
				foundType = ConsoleDaemon.Instance.AvailableTypes[pathElements[0]];

			// if there are multiple path elements then we may have a component
			if (pathElements.Length >= 2)
			{
				// attempt to find the component, if found then retrieve the type
				foundComponent = CommandHelpers.GetComponent(pathElements[0] + "." + pathElements[1]);
				if (foundComponent != null)
					foundType = foundComponent.GetType();
			}

			// no matching type should only happen if invalid information has been entered
			if (foundType == null)
				return null;

			// track the index into the path for the earliest spot a variable may be
			int variableStart = foundComponent != null ? 2 : 1;

			// cache the binding flags (can only search for instance if we have a component)
			BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
			if (foundComponent != null)
				flags |= BindingFlags.Instance;

			// traverse the hierarchy of the path to find the last node
			bool lastElementIsPartial = false;
			for (int pathIndex = variableStart; pathIndex < pathElements.Length; ++pathIndex)
			{
				FieldInfo field = foundType.GetField(pathElements[pathIndex], flags);
				lastElementIsPartial = true;

				if (field != null)
				{
					// even if the parent was static after we are one field in we are dealing with an actual variable so add instance
					flags |= BindingFlags.Instance;

					foundType = field.FieldType;
					lastElementIsPartial = false;
				}
				else
				{
					PropertyInfo property = foundType.GetProperty(pathElements[pathIndex], flags);

					if (property != null)
					{
						// even if the parent was static after we are one field in we are dealing with an actual variable so add instance
						flags |= BindingFlags.Instance;

						foundType = property.PropertyType;
						lastElementIsPartial = false;
					}
				}
			}
			
			// find all candidate fields
			string partialName = lastElementIsPartial ? pathElements[pathElements.Length - 1] : "";
			List<FieldInfo> fieldInfos = foundType.GetFields(flags).Where(field => field.Name.StartsWith(partialName)).ToList();
			List<PropertyInfo> propertyInfos = foundType.GetProperties(flags).Where(property => property.Name.StartsWith(partialName)).ToList();
			List<MethodInfo> methodInfos = null;

			// if we're doing a method search then also check methods
			if (candidateType == AutocompleteCandidates.Functions)
			{
				methodInfos = foundType.GetMethods(flags).Where(method => method.Name.StartsWith(partialName)).ToList();
			}

			// no valid items found
			if ((fieldInfos == null || fieldInfos.Count == 0) && 
				(propertyInfos == null || propertyInfos.Count == 0) &&
				(methodInfos == null || methodInfos.Count == 0))
				return null;

			// assemble the base command
			string basePath = "";
			int lastIndex = lastElementIsPartial ? pathElements.Length - 1 : pathElements.Length;
			for (int index = 0; index < lastIndex; ++index)
			{
				basePath += pathElements[index] + ".";
			}

			// build the autocomplete options
			List<string> autocompleteOptions = new List<string>(fieldInfos.Count);
			bool needsEscaping = basePath.Contains(" ");
			foreach(FieldInfo fieldInfo in fieldInfos)
			{
				string option = (command + " ") + (needsEscaping ? ("\"" + basePath + fieldInfo.Name + "\"") : (basePath + fieldInfo.Name));
				autocompleteOptions.Add(option);
			}
			foreach(PropertyInfo propertyInfo in propertyInfos)
			{
				string option = (command + " ") + (needsEscaping ? ("\"" + basePath + propertyInfo.Name + "\"") : (basePath + propertyInfo.Name));
				autocompleteOptions.Add(option);
			}
			if (candidateType == AutocompleteCandidates.Functions)
			{
				foreach(MethodInfo methodInfo in methodInfos)
				{
					string option = (command + " ") + (needsEscaping ? ("\"" + basePath + methodInfo.Name + "\"") : (basePath + methodInfo.Name));

					// for now only include those without parameters
					if (methodInfo.GetParameters().Length > 0)
						continue;

					if (!autocompleteOptions.Contains(option))
						autocompleteOptions.Add(option);
				}
			}

			return autocompleteOptions;
		}

		public static System.Object ConstructObjectFromString(System.Type type, string value)
		{
			// If the type is a string then our process is simple
			if (type == typeof(string))
				return value;
			// if the type is a vector 2, 3 or 4 then handle it
			if (type == typeof(Vector2))
			{
				// convert the string to a vector 2
				Vector2 vectorValue = Vector2.zero;
				if (CommandHelpers.Vector2FromTokens(new string[] {value}, 0, ref vectorValue))
					return vectorValue;
				
				return null;
			}
			if (type == typeof(Vector3))
			{
				// convert the string to a vector 3
				Vector3 vectorValue = Vector3.zero;
				if (CommandHelpers.Vector3FromTokens(new string[] {value}, 0, ref vectorValue))
					return vectorValue;
				
				return null;
			}
			if (type == typeof(Vector4))
			{
				// convert the string to a vector 4
				Vector4 vectorValue = Vector4.zero;
				if (CommandHelpers.Vector4FromTokens(new string[] {value}, 0, ref vectorValue))
					return vectorValue;
				
				return null;
			}
			if (type == typeof(Quaternion))
			{
				// convert the string to a quaternion
				Vector4 vectorValue = Vector4.zero;
				if (CommandHelpers.Vector4FromTokens(new string[] {value}, 0, ref vectorValue))
					return new Quaternion(vectorValue.x, vectorValue.y, vectorValue.z, vectorValue.w);
				
				return null;
			}
			if (type == typeof(Color))
			{
				// is the value being provided as a web style string?
				if (value.Contains('#'))
				{
					Color colour = Color.magenta;
					if (ColorUtility.TryParseHtmlString(value, out colour))
						return colour;
				} // otherwise assume it is a RGBA set
				else
				{
					// convert the string to a color
					Vector4 vectorValue = Vector4.zero;
					if (CommandHelpers.Vector4FromTokens(new string[] {value}, 0, ref vectorValue))
						return new Color(vectorValue.x, vectorValue.y, vectorValue.z, vectorValue.w);
				}
				
				return null;
			}

			// does the type have a parse method that takes a string?
			MethodInfo parseMethod = type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, 
													CallingConventions.Any, new System.Type[] {typeof(string)}, null);
			if (parseMethod != null)
			{
				try
				{
					return parseMethod.Invoke(null, new object[] {value});
				}
				catch(System.Exception ex)
				{
					Debug.LogError("Failed to parse type: " + ex.Message);
					return null;
				}
			}

			// does the type have a constructor that takes a string?
			ConstructorInfo constructor = type.GetConstructor(new System.Type[]{typeof(string)});
			if (constructor != null)
			{
				try
				{
					return constructor.Invoke(new object[] {value});
				}
				catch(System.Exception ex)
				{
					Debug.LogError("Failed to construct type: " + ex.Message);
					return null;
				}
			}

			return null;
		}

		public static string AttemptToSetField(System.Object targetObject, FieldInfo field, string value)
		{
			try
			{
				field.SetValue(targetObject, ConstructObjectFromString(field.FieldType, value));

				return "Set value to " + value;
			}
			catch(System.Exception ex)
			{
				return "[Error] Unable to set value: " + ex.Message;
			}
		}

		public static string AttemptToSetProperty(System.Object targetObject, PropertyInfo property, string value)
		{
			try
			{
				property.SetValue(targetObject, ConstructObjectFromString(property.PropertyType, value), null);

				return "Set value to " + value;
			}
			catch(System.Exception ex)
			{
				return "[Error] Unable to set value: " + ex.Message;
			}
		}

		public static string SetValue(string path, string value)
		{
			// split based on the . as this might indicate a component or variable split
			string[] pathElements = path.Split('.');

			System.Type foundType = null;
			Component foundComponent = null;
			System.Object foundObject = null;

			// attempt to find the type
			if (ConsoleDaemon.Instance.AvailableTypes.ContainsKey(pathElements[0]))
				foundType = ConsoleDaemon.Instance.AvailableTypes[pathElements[0]];

			// if there are multiple path elements then we may have a component
			if (pathElements.Length >= 2)
			{
				// attempt to find the component, if found then retrieve the type
				foundComponent = CommandHelpers.GetComponent(pathElements[0] + "." + pathElements[1]);
				if (foundComponent != null)
				{
					foundType = foundComponent.GetType();
					foundObject = foundComponent;
				}
			}

			// no matching type should only happen if invalid information has been entered
			if (foundType == null)
				return "[Error] Failed to find a matching component or type. Check the entered path.";

			// track the index into the path for the earliest spot a variable may be
			int variableStart = foundComponent != null ? 2 : 1;

			// cache the binding flags (can only search for instance if we have a component)
			BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
			if (foundComponent != null)
				flags |= BindingFlags.Instance;

			// traverse the hierarchy of the path to find the last node
			for (int pathIndex = variableStart; pathIndex < pathElements.Length; ++pathIndex)
			{
				FieldInfo field = foundType.GetField(pathElements[pathIndex], flags);

				if (field != null)
				{
					// if this is the last element then attempt to set it
					if (pathIndex == (pathElements.Length - 1))
						return AttemptToSetField(foundObject, field, value);

					// even if the parent was static after we are one field in we are dealing with an actual variable so add instance
					flags |= BindingFlags.Instance;

					foundType = field.FieldType;

					foundObject = field.GetValue(foundObject);
				}
				else
				{
					PropertyInfo property = foundType.GetProperty(pathElements[pathIndex], flags);

					if (property != null)
					{
					// if this is the last element then attempt to set it
					if (pathIndex == (pathElements.Length - 1))
						return AttemptToSetProperty(foundObject, property, value);

						// even if the parent was static after we are one field in we are dealing with an actual variable so add instance
						flags |= BindingFlags.Instance;

						foundType = property.PropertyType;

						foundObject = property.GetValue(foundObject, null);
					}
				}
			
				// no found object
				if (foundObject == null)
					return "[Error] Failed to access the object. It may have been destroyed.";
			}

			return "[Error] Failed to access the object. It may have been destroyed.";
		}

		public static string GetValue(string path)
		{
			// split based on the . as this might indicate a component or variable split
			string[] pathElements = path.Split('.');

			System.Type foundType = null;
			Component foundComponent = null;
			System.Object foundObject = null;

			// attempt to find the type
			if (ConsoleDaemon.Instance.AvailableTypes.ContainsKey(pathElements[0]))
				foundType = ConsoleDaemon.Instance.AvailableTypes[pathElements[0]];

			// if there are multiple path elements then we may have a component
			if (pathElements.Length >= 2)
			{
				// attempt to find the component, if found then retrieve the type
				foundComponent = CommandHelpers.GetComponent(pathElements[0] + "." + pathElements[1]);
				if (foundComponent != null)
				{
					foundType = foundComponent.GetType();
					foundObject = foundComponent;
				}
			}

			// no matching type should only happen if invalid information has been entered
			if (foundType == null)
				return "[Error] Failed to find a matching component or type. Check the entered path.";

			// track the index into the path for the earliest spot a variable may be
			int variableStart = foundComponent != null ? 2 : 1;

			// cache the binding flags (can only search for instance if we have a component)
			BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
			if (foundComponent != null)
				flags |= BindingFlags.Instance;

			// traverse the hierarchy of the path to find the last node
			bool lastElementIsPartial = false;
			for (int pathIndex = variableStart; pathIndex < pathElements.Length; ++pathIndex)
			{
				FieldInfo field = foundType.GetField(pathElements[pathIndex], flags);
				lastElementIsPartial = true;

				if (field != null)
				{
					// even if the parent was static after we are one field in we are dealing with an actual variable so add instance
					flags |= BindingFlags.Instance;

					foundType = field.FieldType;
					lastElementIsPartial = false;

					foundObject = field.GetValue(foundObject);
				}
				else
				{
					PropertyInfo property = foundType.GetProperty(pathElements[pathIndex], flags);

					if (property != null)
					{
						// even if the parent was static after we are one field in we are dealing with an actual variable so add instance
						flags |= BindingFlags.Instance;

						foundType = property.PropertyType;
						lastElementIsPartial = false;

						foundObject = property.GetValue(foundObject, null);
					}
				}
			
				// no found object
				if (foundObject == null)
					return "[Error] Failed to access the object. It may have been destroyed.";
			}

			// if the last element is partial then an error has occurred
			if (lastElementIsPartial)
				return "[Error] An incomplete path to a variable was supplied. Check the path. Paths are case sensitive.";
			
			return "The value is: " + foundObject.ToString();
		}

		public static string Execute(string path, string[] parameters, int parameterStartIndex)
		{
			// split based on the . as this might indicate a component or variable split
			string[] pathElements = path.Split('.');

			System.Type foundType = null;
			Component foundComponent = null;
			System.Object foundObject = null;

			// attempt to find the type
			if (ConsoleDaemon.Instance.AvailableTypes.ContainsKey(pathElements[0]))
				foundType = ConsoleDaemon.Instance.AvailableTypes[pathElements[0]];

			// if there are multiple path elements then we may have a component
			if (pathElements.Length >= 2)
			{
				// attempt to find the component, if found then retrieve the type
				foundComponent = CommandHelpers.GetComponent(pathElements[0] + "." + pathElements[1]);
				if (foundComponent != null)
				{
					foundType = foundComponent.GetType();
					foundObject = foundComponent;
				}
			}

			// no matching type should only happen if invalid information has been entered
			if (foundType == null)
				return "[Error] Failed to find a matching component or type. Check the entered path.";

			// track the index into the path for the earliest spot a variable may be
			int variableStart = foundComponent != null ? 2 : 1;

			// cache the binding flags (can only search for instance if we have a component)
			BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
			if (foundComponent != null)
				flags |= BindingFlags.Instance;

			// traverse the hierarchy of the path to find the last node
			bool lastElementIsPartial = false;
			for (int pathIndex = variableStart; pathIndex < pathElements.Length - 1; ++pathIndex)
			{
				FieldInfo field = foundType.GetField(pathElements[pathIndex], flags);
				lastElementIsPartial = true;

				if (field != null)
				{
					// even if the parent was static after we are one field in we are dealing with an actual variable so add instance
					flags |= BindingFlags.Instance;

					foundType = field.FieldType;
					lastElementIsPartial = false;

					foundObject = field.GetValue(foundObject);
				}
				else
				{
					PropertyInfo property = foundType.GetProperty(pathElements[pathIndex], flags);

					if (property != null)
					{
						// even if the parent was static after we are one field in we are dealing with an actual variable so add instance
						flags |= BindingFlags.Instance;

						foundType = property.PropertyType;
						lastElementIsPartial = false;

						foundObject = property.GetValue(foundObject, null);
					}
				}
			
				// no found object
				if (foundObject == null || foundType == null)
					return "[Error] Failed to retrieve the object. It may have been destroyed.";
			}

			// if the last element is partial then an error has occurred
			if (lastElementIsPartial)
				return "[Error] An incomplete path to a function was supplied. Check the path. Paths are case sensitive.";

			// attempt to get the methods with the matching name
			MethodInfo foundMethod = foundType.GetMethod(pathElements[pathElements.Length - 1], flags, null, CallingConventions.Any, new System.Type[] {}, null);

			if (foundMethod == null)
				return "[Error] Unable to find a matching method. Only void (ie. parameterless) functions are supported.";

			System.Object result = foundMethod.Invoke(foundObject, null);
			
			return "Executed method." + ((result == null) ? " There was no return value." : (" Returned: " + result.ToString()));
		}
		#endregion
	}	
}