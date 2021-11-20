using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevConsole
{
	[System.Serializable]
	public class TutorialElement
	{
		public string RequiredCommand;

		[TextArea(3, 10)]
		public string Prompt;
	}

	public class DevConsoleTutorial : MonoBehaviour 
	{
		public UnityEngine.UI.Text tutorialTextDisplay;
		public UnityEngine.UI.Slider tutorialProgressDisplay;
		public List<string> requiredScenes;
		public List<TutorialElement> tutorialElements;

		protected int currentElement = -1;

		public static string TestVariable = "Hello!";

		public static void Method1()
		{
			Debug.Log("Ran Method1");
		}

		public void Method2()
		{
			Debug.Log("Ran Method2");
		}

		// Use this for initialization
		void Start () 
		{
			ConsoleDaemon.Instance.OnCommandEntered.AddListener(OnCommandEntered);

			// if there are required scenes make sure all are present
			if (requiredScenes.Count > 0)
			{
				List<string> availableScenes = CommandHelpers.GetSceneNamesInBuild();

				// check if all are present
				bool allPresent = true;
				foreach(string sceneName in requiredScenes)
				{
					if (!availableScenes.Contains(sceneName.ToLower()))
					{
						allPresent = false;
						break;
					}
				}
				
				// not all of the required scenes were present
				if (!allPresent)
				{
					currentElement = tutorialElements.Count;
					return;
				}
			}

			AdvanceToNextTutorialElement();
		}
		
		// Update is called once per frame
		void Update () 
		{
			
		}

		public void OnCommandEntered(string command)
		{
			// are there no valid elements left?
			if (currentElement >= tutorialElements.Count)
				return;

			// did the entered command match?
			if (command == tutorialElements[currentElement].RequiredCommand)
				AdvanceToNextTutorialElement();
			else if (tutorialTextDisplay.text == tutorialElements[currentElement].Prompt)
			{
				tutorialTextDisplay.text += System.Environment.NewLine + System.Environment.NewLine +
											"<b>The command was not entered correctly. Check that you have matched case and spelling exactly.</b>";
			}
		}

		void AdvanceToNextTutorialElement()
		{
			// increment the element and check if there are no further ones to process
			++currentElement;
			if (currentElement >= tutorialElements.Count)
			{
				tutorialTextDisplay.text = "This tutorial is now complete. Thankyou for using the Developer Console!";
				tutorialProgressDisplay.value = 1f;
				return;
			}

			// update the prompt text
			tutorialTextDisplay.text = tutorialElements[currentElement].Prompt;

			// update the progress
			tutorialProgressDisplay.value = (float)currentElement/(float)(tutorialElements.Count);
		}
	}
}
