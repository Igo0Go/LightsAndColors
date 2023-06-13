using UnityEngine;

public class InGameConsole : MonoBehaviour
{
    private string input = string.Empty;
    private bool showHelpWindow;
    private Vector2 scroll = Vector2.zero;

    private void Awake()
    {
        ConsoleEventCenter.Reload();

        ConsoleEventCenter.Help.Execute.AddListener(ShowHelp);
    }

    private void Update()
    {
        ShowConsoleToggle();
    }

    private void OnGUI()
    {
        if (!ConsoleEventCenter.ShowConsole)
            return;

        float y = 0;    

        GUI.Box(new Rect(0, y, Screen.width, 30), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        input = GUI.TextArea(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);

        if(input.Contains("\n"))
        {
            HandleInput();
            input = string.Empty;
        }

        y += 30;

        if (showHelpWindow)
        {
            GUI.backgroundColor = new Color(0, 0, 0, 1);
            GUI.Box(new Rect(0, y, Screen.width, 100), "");

            Rect viewport = new Rect(0, y, Screen.width - 30, 20 * ConsoleEventCenter.commandList.Count);

            scroll = GUI.BeginScrollView(new Rect(0, y + 5f, Screen.width, 90), scroll, viewport);

            for (int i = 0; i < ConsoleEventCenter.commandList.Count; i++)
            {
                BaseDebugCommand command = ConsoleEventCenter.commandList[i];

                string label = command.CommandFormat + " - " + command.CommandDescription;
                GUI.backgroundColor = new Color(0, 0, 0, 0);
                Rect labelRect = new Rect(5, y+20 * i, viewport.width, 20);

                GUI.Label(labelRect, label);
            }

            GUI.EndScrollView();
        }
    }

    private void ShowConsoleToggle()
    {
        if(Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote))
        {
            ConsoleEventCenter.ShowConsole = !ConsoleEventCenter.ShowConsole;
            if(!ConsoleEventCenter.ShowConsole)
            {
                showHelpWindow = false;
            }
        }
    }

    private void ShowHelp()
    {
        showHelpWindow = true;
    }

    private void HandleInput()
    {
        string[] properties = input.TrimEnd('\n').Split(' ');

        for (int i = 0; i < ConsoleEventCenter.commandList.Count; i++)
        {
            BaseDebugCommand baseCommand = ConsoleEventCenter.commandList[i];
            try
            {
                if (properties[0].Contains(baseCommand.CommandId))
                {
                    if (baseCommand is DebugCommand command)
                    {
                        command.Invoke();
                        return;
                    }

                    if (baseCommand is DebugCommand<int> intCommand)
                    {
                        intCommand.Invoke(int.Parse(properties[1]));
                        return;
                    }

                    if (baseCommand is DebugCommand<string> stringCommand)
                    {
                        if (properties[1] != null)
                        {
                            stringCommand.Invoke(properties[1]);
                            return;
                        }
                    }

                    if (baseCommand is DebugCommand<int, int, int> vectorCommand)
                    {
                        vectorCommand.Invoke(int.Parse(properties[1]),
                            int.Parse(properties[2]),
                            int.Parse(properties[3]));
                        return;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}
