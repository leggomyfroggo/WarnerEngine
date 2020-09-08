using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WarnerEngine.Services
{
    public class TerminalService : Service
    {
        private const string RENDER_TARGET_KEY = "terminal";

        private static readonly Vector2 LAST_OUTPUT_POSITION = new Vector2(4f);
        private static readonly Vector2 CURRENT_COMMAND_POSITION = new Vector2(4f, 20f);

        public bool IsActive { get; private set; }

        private int pauseKey;

        // Dictionary containing all console commands and their respective functions
        protected Dictionary<string, Func<string[], string>> consoleCommands;

        protected List<string> previousCommands;
        protected int previousPointer;
        protected StringBuilder currentCommand;
        protected string activeCommand;
        protected Keys[] lastPressedKeys;
        protected int holdBackspaceTimer;
        protected int holdBackspaceLimiter;
        protected int blinkTimer;

        protected string lastOutput;
        protected bool wasLastOutputSuccessful;

        public override HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>() { typeof(EventService), typeof(IInputService) };
        }

        public override void Initialize()
        {
            GameService.GetService<EventService>().Subscribe(
                Events.INTERNAL_RESOLUTION_CHANGED,
                _ =>
                {
                    RenderService renderService = GameService.GetService<RenderService>();
                    renderService.AddRenderTarget(
                        RENDER_TARGET_KEY,
                        renderService.InternalResolutionX,
                        32,
                        RenderTargetUsage.PreserveContents
                    );
                }
            );

            IsActive = false;

            previousCommands = new List<string>();
            previousPointer = 0;
            currentCommand = new StringBuilder();
            activeCommand = null;
            holdBackspaceTimer = 0;
            holdBackspaceLimiter = 0;
            blinkTimer = 0;

            lastOutput = "";
            wasLastOutputSuccessful = true;

            RegisterConsoleCommands();
        }

        public override void PreDraw(float DT)
        {
            if (GameService.GetService<IInputService>().WasKeyPressed(Keys.F12))
            {
                if (IsActive)
                {
                    GameService.GetService<SceneService>().CurrentScene.Unpause(pauseKey);
                    IsActive = false;
                }
                else
                {
                    int newPauseKey = GameService.GetService<SceneService>().CurrentScene.PauseAndLock();
                    if (newPauseKey != -1)
                    {
                        pauseKey = newPauseKey;
                        IsActive = true;
                    }
                }
            }
            if (!IsActive)
            {
                return;
            }
            blinkTimer = (blinkTimer + 1 < 60) ? blinkTimer + 1 : 0;
            ProcessKeyboardInput();
            ProcessActiveCommand();
        }

        private void RegisterConsoleCommands()
        {
            consoleCommands = new Dictionary<string, Func<string[], string>> {
                { "set_level", x => ConsoleSetLevel(x) },
            };
        }

        protected void ProcessKeyboardInput()
        {
            KeyboardState ks = Keyboard.GetState();
            Keys[] keysPressed = ks.GetPressedKeys();

            foreach (Keys k in keysPressed)
            {
                if (!lastPressedKeys.Contains(k))
                {
                    string pressedKey = k.ToString();
                    if (pressedKey.Length == 1)
                    {
                        char keyChar = pressedKey[0];
                        if ((!keysPressed.Contains(Keys.LeftShift) && !keysPressed.Contains(Keys.RightShift)) && keyChar >= 65 && keyChar <= 90)
                        {
                            keyChar = (char)((byte)keyChar + 32);
                        }
                        currentCommand.Append(keyChar);
                    }
                    else
                    {
                        if ((pressedKey.Length >= 6 && pressedKey.Substring(0, 6).Equals("NumPad")) || (pressedKey[0] == 'D' && pressedKey.Length == 2))
                        {
                            currentCommand.Append(pressedKey[pressedKey.Length - 1]);
                        }
                        else if (k == Keys.Space)
                        {
                            currentCommand.Append(' ');
                        }
                        else if (k == Keys.OemComma)
                        {
                            currentCommand.Append(',');
                        }
                        else if (k == Keys.OemMinus)
                        {
                            if (!keysPressed.Contains(Keys.LeftShift) && !keysPressed.Contains(Keys.RightShift))
                            {
                                currentCommand.Append('-');
                            }
                            else
                            {
                                currentCommand.Append('_');
                            }
                        }
                        else if (k == Keys.OemPeriod)
                        {
                            currentCommand.Append('.');
                        }
                        else if (k == Keys.OemPipe)
                        {
                            if (!keysPressed.Contains(Keys.LeftShift) && !keysPressed.Contains(Keys.RightShift))
                            {
                                currentCommand.Append('\\');
                            }
                            else
                            {
                                currentCommand.Append('|');
                            }
                        }
                    }
                }
            }

            // Reset the back space timer if the key isn't held
            if (!keysPressed.Contains(Keys.Back))
            {
                holdBackspaceTimer = 0;
                holdBackspaceLimiter = 0;
            }
            else
            {
                if (currentCommand.Length > 0)
                {
                    if (holdBackspaceTimer == 0 || (holdBackspaceTimer >= 30 && holdBackspaceLimiter < 2))
                    {
                        currentCommand.Remove(currentCommand.Length - 1, 1);
                    }

                    holdBackspaceTimer = MathHelper.Clamp(holdBackspaceTimer + 1, 0, 30);
                    holdBackspaceLimiter = (holdBackspaceLimiter + 1) % 4;
                }
            }

            // Check if enter was pressed
            if (GameService.GetService<IInputService>().WasKeyPressed(Keys.Enter))
            {
                activeCommand = currentCommand.ToString();
                currentCommand.Clear();
            }
            // Check if the user is trying to access archived commands
            else if (GameService.GetService<IInputService>().WasKeyPressed(Keys.Up))
            {
                if (previousCommands.Count > 0)
                {
                    if (previousPointer > 0)
                    {
                        previousPointer--;
                    }
                    currentCommand = new StringBuilder(previousCommands[previousPointer]);
                }
            }
            else if (GameService.GetService<IInputService>().WasKeyPressed(Keys.Down))
            {
                if (previousPointer < previousCommands.Count)
                {
                    previousPointer++;
                    if (previousPointer == previousCommands.Count)
                    {
                        currentCommand.Clear();
                    }
                    else
                    {
                        currentCommand = new StringBuilder(previousCommands[previousPointer]);
                    }
                }
            }

            lastPressedKeys = keysPressed;
        }

        private void ProcessActiveCommand()
        {
            if (activeCommand != null)
            {
                // Split the command by spaces
                string[] commands = activeCommand.Split(' ');
                var allCommands = consoleCommands;
                var localCommands = GameService.GetService<SceneService>().CurrentScene.GetLocalTerminalCommands();
                if (localCommands != null)
                {
                    allCommands = consoleCommands
                        .Union(GameService.GetService<SceneService>().CurrentScene.GetLocalTerminalCommands())
                        .ToDictionary(k => k.Key, v => v.Value);
                }
                if (allCommands.ContainsKey(commands[0]))
                {
                    try
                    {
                        lastOutput = allCommands[commands[0]](commands.ToList().GetRange(1, commands.Count() - 1).ToArray());
                        wasLastOutputSuccessful = true;
                    }
                    catch (Exception e)
                    {
                        lastOutput = e.Message;
                        wasLastOutputSuccessful = false;
                    }
                }
                else
                {
                    lastOutput = $"Invalid command: {commands[0]}";
                    wasLastOutputSuccessful = false;
                }
                // Archive this command and then clear it
                previousCommands.Add(activeCommand);
                activeCommand = null;
                // Make sure the command archive doesn't get too big
                if (previousCommands.Count > 256)
                {
                    previousCommands.RemoveAt(0);
                }
                // Set the pointer to the previous command
                previousPointer = previousCommands.Count;
            }
        }

        private string ConsoleSetLevel(string[] Args)
        {
            //LevelManager.SetLevel(Args[0]);

            return "Level changed";
        }

        public override ServiceCompositionMetadata Draw()
        {
            if (!IsActive)
            {
                return ServiceCompositionMetadata.Empty;
            }
            GameService.GetService<RenderService>()
                .SetRenderTarget(RENDER_TARGET_KEY, Color.Black)
                .Start()
                .DrawString("stats_overlay", GetCurrentCommandForDraw(), CURRENT_COMMAND_POSITION, Color.White)
                .DrawString("stats_overlay", lastOutput, LAST_OUTPUT_POSITION, wasLastOutputSuccessful ? Color.Green : Color.Red)
                .End();
            return new ServiceCompositionMetadata()
            {
                RenderTargetKey = RENDER_TARGET_KEY,
                Position = Vector2.Zero,
                Priority = 100,
                Tint = Color.White,
            };
        }

        public string GetCurrentCommandForDraw()
        {
            return currentCommand.ToString() + (blinkTimer < 30 ? "_" : "");
        }

        public override Type GetBackingInterfaceType()
        {
            return typeof(TerminalService);
        }
    }
}
