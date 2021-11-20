using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif // ENABLE_INPUT_SYSTEM

[System.Serializable]
public class DevConsoleVisibilityEvent : UnityEvent<bool> {}

namespace DevConsole
{
	public class DevConsoleUI : MonoBehaviour 
	{
		public enum State
		{
			Default,
			AutocompleteSelection
		};

		[Header("Console Behaviour")]
		[Tooltip("Autofill with the first autocomplete option if there is more than one option?")]
		public bool fillFirstAutocompleteIfMultiple = false;
		[Tooltip("Use tab as well as arrows to cycle autocomplete options")]
		public bool tabToCycleAutocompleteOptions = true;
		[Tooltip("Is the developer console listening for a key press?")]
		public bool listeningForKey = true;

#if ENABLE_INPUT_SYSTEM
		[Tooltip("Action to activate or deactivate the console")]
		public InputAction activationAction = new InputAction("DevConsole/Toggle", binding: "<Keyboard>/backquote", interactions: "press");
		[Tooltip("Action to attempt to autocomplete current command")]
		public InputAction autocompleteAction = new InputAction("DevConsole/Autocomplete", binding: "<Keyboard>/tab", interactions: "press");
		[Tooltip("Action to use in combintion with autocomplete key to move backwards")]
		public InputAction autocompleteReverseAction = new InputAction("DevConsole/AutocompleteReverse", binding: "<Keyboard>/leftShift", interactions: "press");
		[Tooltip("Action to use to submit the input")]
		public InputAction submitAction = new InputAction("DevConsole/SubmitAction", binding: "<Keyboard>/enter", interactions: "press");
		[Tooltip("Automatically disable keyboard input (for others) when open")]
		public bool AutoDisableKeyboardInputForOthers = false;
#else // ENABLE_INPUT_SYSTEM 
        [Tooltip("Key to press to activate or deactivate the console")]
        public KeyCode activationKey = KeyCode.BackQuote;
        [Tooltip("Key to press to attempt to autocomplete current command")]
        public KeyCode autocompleteKey = KeyCode.Tab;
        [Tooltip("Key to press in combintion with autocomplete key to move backwards")]
        public KeyCode autocompleteReverseModifier = KeyCode.LeftShift;
#endif // ENABLE_INPUT_SYSTEM

		private bool input_ActivationToggle 		= false;
		private bool input_Autocomplete 			= false;
		private bool input_AutocompleteReverse 		= false;
		private bool input_AnyKeyPressed			= false;
		private bool input_EnterPressed				= false;
		private bool input_NextOptionPressed		= false;
		private bool input_PreviousOptionPressed	= false;

        [Tooltip("Should spaces be automatically added at the end of autocompleted commands")]
		public bool addSpaceAtEndOfAutocomplete = false;

		[Header("UI Links")]
		public TMPro.TMP_InputField commandInput;
		public Canvas consoleCanvas; 
		public TMPro.TextMeshProUGUI consoleOutput;
		public ScrollRect consoleOutputScrollRect;

		[Header("Events")]
		public DevConsoleVisibilityEvent OnToggleDevConsole = new DevConsoleVisibilityEvent();

		protected State currentState = State.Default;

		protected int maximumCharacters = 10000;
		protected bool isOpen = false;
		protected List<string> autocompleteList = new List<string>();
		protected int autocompleteIndex = 0;
		protected bool isFirstOpen = true;
		protected bool canCycleAutocomplete = false;

		// Use this for initialization
		void Start () 
		{
			consoleCanvas.enabled = isOpen;
			DevConsole.ConsoleDaemon.Instance.OnClearConsole.AddListener(ClearConsole);

#if ENABLE_INPUT_SYSTEM
            activationAction.Enable();
			activationAction.performed += OnActivationToggle;
            autocompleteAction.performed += OnAutocomplete;
            autocompleteReverseAction.performed += OnAutocompleteReverse;	
			submitAction.performed += OnSubmitAction;		
#endif // ENABLE_INPUT_SYSTEM
        }

#if ENABLE_INPUT_SYSTEM
		void OnActivationToggle(InputAction.CallbackContext context)
		{
            input_ActivationToggle = true;
		}

        void OnAutocomplete(InputAction.CallbackContext context)
        {
            input_Autocomplete = true;
        }

        void OnAutocompleteReverse(InputAction.CallbackContext context)
        {
            input_AutocompleteReverse = true;
        }

		void OnSubmitAction(InputAction.CallbackContext context)
		{
			input_EnterPressed = true;
		}

		void LateUpdate()
		{
			input_ActivationToggle = false;
			input_Autocomplete = false;
			input_AutocompleteReverse = false;
			input_EnterPressed = false;
		}		
#endif // ENABLE_INPUT_SYSTEM

        // Update is called once per frame
        void Update () 
		{
			// do nothing if we're not listening for a keypress
			if (!listeningForKey)
				return;

#if ENABLE_INPUT_SYSTEM
			input_AnyKeyPressed			= Keyboard.current.anyKey.wasPressedThisFrame;
			input_NextOptionPressed 	= Keyboard.current.downArrowKey.wasPressedThisFrame;
			input_PreviousOptionPressed = Keyboard.current.upArrowKey.wasPressedThisFrame;
#else // ENABLE_INPUT_SYSTEM
            input_ActivationToggle 		= Input.GetKeyDown(activationKey);
        	input_Autocomplete 			= Input.GetKeyDown(autocompleteKey);
        	input_AutocompleteReverse 	= Input.GetKeyDown(autocompleteReverseModifier);
        	input_AnyKeyPressed 		= Input.anyKeyDown;
            input_EnterPressed			= Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
			input_NextOptionPressed 	= Input.GetKeyDown(KeyCode.DownArrow);
			input_PreviousOptionPressed = Input.GetKeyDown(KeyCode.UpArrow);
#endif // ENABLE_INPUT_SYSTEM

            // reset the cycling of autocomplete if necessary
            if (input_AnyKeyPressed && !(input_Autocomplete || input_AutocompleteReverse))
				canCycleAutocomplete = false;

			// toggling the panel open/closed?
			if (input_ActivationToggle)
			{
				isOpen = !isOpen;

#if ENABLE_INPUT_SYSTEM
				// enable/disable the actions as needed
				if (isOpen)
				{
                    autocompleteAction.Enable();
					autocompleteReverseAction.Enable();
					submitAction.Enable();

					if (AutoDisableKeyboardInputForOthers)
						InputSystem.DisableDevice(Keyboard.current);
				}
				else
				{
                    autocompleteAction.Disable();
					autocompleteReverseAction.Disable();
					submitAction.Disable();

					if (AutoDisableKeyboardInputForOthers)
						InputSystem.EnableDevice(Keyboard.current);
				}
#endif // ENABLE_INPUT_SYSTEM

                // remove any trailing `
                if (commandInput.text.Length > 0 && commandInput.text[commandInput.text.Length - 1] == '`')
					commandInput.text = commandInput.text.Substring(0, commandInput.text.Length - 1);

				// deactivate the input field if we are closing
				if (!isOpen)
					commandInput.DeactivateInputField();
				else
					commandInput.ActivateInputField();

				consoleCanvas.enabled = isOpen;

                OnToggleDevConsole?.Invoke(isOpen);
			}

			// user pressed return/enter key?
			if (isOpen)
			{
				if (isFirstOpen)
				{
					isFirstOpen = false;

					consoleOutput.text += ConsoleDaemon.Instance.GetWelcomeMessage();
					ScrollToBottom();
				}
				
				if (input_EnterPressed)
				{
					ExecuteCommand();

					currentState = State.Default;
					return;
				}

				// should we be forcing advancing through autocomplete options
				// requires Tab + option enabled + autocomplete options
				bool advanceDueToTab = input_Autocomplete && 
										canCycleAutocomplete && 
										autocompleteList != null && 
										autocompleteList.Count > 0 &&
										currentState == State.AutocompleteSelection;

				if (input_Autocomplete && !advanceDueToTab)
				{
					autocompleteList = DevConsole.ConsoleDaemon.Instance.FetchAutocompleteOptions(commandInput.text);
					autocompleteIndex = 0;

					// if we're about to enable tab to cycle autocomplete options then start at the last so it becomes the first
					if (tabToCycleAutocompleteOptions && !canCycleAutocomplete && autocompleteList != null)
						autocompleteIndex = autocompleteList.Count - 1;

					// enable tab to autocycle if it is permitted
					canCycleAutocomplete = tabToCycleAutocompleteOptions;

					// does the autocomplete list contain nothing? - just display help
					if (autocompleteList == null || autocompleteList.Count == 0)
					{
						//consoleOutput.text += DevConsole.ConsoleDaemon.Instance.ExecuteCommand("help");

						currentState = State.Default;
					}
					else // otherwise it contains a list of potential completions
					{
						// only a single one matched so just update the text
						if (autocompleteList.Count == 1)
						{
							commandInput.text = autocompleteList[0] + (addSpaceAtEndOfAutocomplete ? " " : "");
							commandInput.selectionFocusPosition = commandInput.selectionAnchorPosition = commandInput.text.Length;

							currentState = State.Default;
						}
						else // otherwise just list out the different options
						{
							consoleOutput.text += "Autocomplete options:";
							foreach(string autocomplete in autocompleteList)
							{
								consoleOutput.text += System.Environment.NewLine + "    " + autocomplete;
							}
							consoleOutput.text += System.Environment.NewLine;

							// set the first option (if permitted)
							if (fillFirstAutocompleteIfMultiple)
							{
								commandInput.text = autocompleteList[0] + (addSpaceAtEndOfAutocomplete ? " " : "");
								commandInput.selectionFocusPosition = commandInput.selectionAnchorPosition = autocompleteList[0].Length;
							}

							currentState = State.AutocompleteSelection;
						}
					}

					// scroll to the bottom
					ScrollToBottom();
					return;
				}

				// attempting to navigate history or autocomplete options?
				bool navigate_Previous = input_PreviousOptionPressed || (advanceDueToTab && input_AutocompleteReverse);
				bool navigate_Next = input_NextOptionPressed || (advanceDueToTab && !input_AutocompleteReverse);
				if (navigate_Previous || navigate_Next)
				{
					string command = "";

					// are we attempting to autocomplete?
					if (currentState == State.AutocompleteSelection && autocompleteList.Count > 1)
					{
						// update the index (in this case we can wrap around)
						autocompleteIndex = (autocompleteIndex + (navigate_Previous ? -1 : 1) + autocompleteList.Count) % autocompleteList.Count;

						// extract the autocomplete option
						command = autocompleteList[autocompleteIndex] + (addSpaceAtEndOfAutocomplete ? " " : "");
					}
					else
					{
						command = navigate_Previous ? DevConsole.ConsoleDaemon.Instance.GetHistory_Previous() : DevConsole.ConsoleDaemon.Instance.GetHistory_Next();
					}

					// if the command was retrieved then update the text field
					if (!string.IsNullOrEmpty(command))
					{
						commandInput.text = command;
						commandInput.selectionFocusPosition = commandInput.selectionAnchorPosition = command.Length;
					}

					return;
				}
			}
		}

		string FormatOutput(string rawOutput)
		{
			return rawOutput;
		}

		public void ExecuteCommand()
		{
			// add the executed command to the output
			consoleOutput.text += System.Environment.NewLine + "$ " + commandInput.text;

			// execute the command and update the output
			string output = DevConsole.ConsoleDaemon.Instance.ExecuteCommand(commandInput.text);
			consoleOutput.text += System.Environment.NewLine + FormatOutput(output);
			consoleOutput.text += System.Environment.NewLine;

			// scroll to the bottom
			ScrollToBottom();

			// clear the current input and keep the input field having focus
			commandInput.text = "";
			commandInput.ActivateInputField();
		}

		protected void ScrollToBottom()
		{
			// if we have reached the maximum character limit then we need to truncate the oldest output
			if (consoleOutput.text.Length > maximumCharacters)
			{
				consoleOutput.text = consoleOutput.text.Remove(0, consoleOutput.text.Length - maximumCharacters);
			}

			Canvas.ForceUpdateCanvases();
			consoleOutputScrollRect.verticalNormalizedPosition = 0;
			Canvas.ForceUpdateCanvases();
		}

		public void ClearConsole()
		{
			consoleOutput.text = "";
			ScrollToBottom();
		}
	}
}
