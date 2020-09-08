using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib;
using WarnerEngine.Lib.Dialog;

namespace WarnerEngine.Services
{
    public class DialogService : Service
    {
        public const string RENDER_TARGET_KEY = "dialog";

        public const int DIALOG_TARGET_WIDTH = 256;
        public const int DIALOG_TARGET_HEIGHT = 64;
        public const int DIALOG_TARGET_HALF_WIDTH = DIALOG_TARGET_WIDTH / 2;
        public const int DIALOG_TARGET_HALF_HEIGHT = DIALOG_TARGET_HEIGHT / 2;
        public const int DIALOG_WINDOW_WIDTH = 256;
        public const int DIALOG_WINDOW_HEIGHT = 40;

        private const int NAME_WINDOW_X = 0;
        private const int NAME_WINDOW_Y = 0;
        private const int NAME_WINDOW_HORIZONTAL_MARGIN = 8;
        private const int NAME_WINDOW_HEIGHT = 20;
        private static readonly Vector2 NAME_START_POINT = new Vector2(NAME_WINDOW_X + NAME_WINDOW_HORIZONTAL_MARGIN, NAME_WINDOW_Y + 4);

        private const int DIALOG_WINDOW_X = 0;
        private const int DIALOG_WINDOW_Y = 24;
        private static readonly Vector2 DIALOG_START_POINT = new Vector2(DIALOG_WINDOW_X + 8, DIALOG_WINDOW_Y + 8);
        private static readonly Color TARGET_COLOR = Color.Transparent;
        private const float TARGET_FADE_DURATION = 100;

        private int pauseKey;
        private DialogLink currentDialog;
        private int displayedLength;

        private AutoTween fadeIn;
        private AutoTween fadeOut;

        public override HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>() 
            { 
                typeof(EventService), 
                typeof(IInputService), 
            };
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
                        DIALOG_TARGET_WIDTH,
                        DIALOG_TARGET_HEIGHT,
                        RenderTargetUsage.PreserveContents
                    );
                }
            );

            currentDialog = null;
            displayedLength = 0;

            fadeIn = new AutoTween(0, 1, TARGET_FADE_DURATION);
            fadeOut = new AutoTween(1, 0, TARGET_FADE_DURATION);
        }

        public override void PreDraw(float DT)
        {
            if (currentDialog == null)
            {
                return;
            }
            if (!LineHasEnded)
            {
                displayedLength++;
            }
            else if (GameService.GetService<IInputService>().WasActionPressed(InputAction.Interact))
            {
                currentDialog = currentDialog.NextDialog;
                if (currentDialog != null)
                {
                    displayedLength = 0;
                } else
                {
                    fadeOut.Start();
                    GameService.GetService<SceneService>().CurrentScene.Unpause(pauseKey);
                }
            }
        }

        public override ServiceCompositionMetadata Draw()
        {
            if (currentDialog == null && !fadeOut.IsRunning)
            {
                return ServiceCompositionMetadata.Empty;
            }

            if (currentDialog != null)
            {
                int nameWidth = (int)GameService.GetService<IContentService>().GetxAsset<SpriteFont>("lana_pixel").MeasureString(currentDialog.CharacterName).X;
                GameService.GetService<RenderService>()
                    .SetRenderTarget(RENDER_TARGET_KEY, ClearColor: TARGET_COLOR)
                    .Start()
                    .DrawNinePatch(ProjectWarnerShared.Content.Constants.GAMEUI_DIALOG_WINDOW, new Rectangle(NAME_WINDOW_X, NAME_WINDOW_Y, nameWidth + NAME_WINDOW_HORIZONTAL_MARGIN * 2, NAME_WINDOW_HEIGHT), 8, 8, new Color(91, 86, 255))
                    .DrawNinePatch(ProjectWarnerShared.Content.Constants.GAMEUI_DIALOG_WINDOW, new Rectangle(DIALOG_WINDOW_X, DIALOG_WINDOW_Y, DIALOG_WINDOW_WIDTH, DIALOG_WINDOW_HEIGHT), 8, 8)
                    .DrawString("lana_pixel", currentDialog.CharacterName, NAME_START_POINT, Color.White)
                    .DrawDialogLink("lana_pixel", currentDialog, DIALOG_START_POINT, 240, displayedLength)
                    .End()
                    .Cleanup();
            }
            Vector2 targetPosition = new Vector2(
                GameService.GetService<RenderService>().InternalResolutionX / 2 - DIALOG_TARGET_HALF_WIDTH,
                GameService.GetService<RenderService>().InternalResolutionY - DIALOG_TARGET_HEIGHT - 8
            );

            return new ServiceCompositionMetadata()
            {
                RenderTargetKey = RENDER_TARGET_KEY,
                Position = targetPosition,
                Priority = 10,
                Tint = Color.White * (fadeOut.IsRunning ? fadeOut.GetTween() : fadeIn.GetTween()),
            };
        }

        public void DisplayDialog(string DialogKey)
        {
            if (currentDialog != null)
            {
                return;
            }
            int newPauseKey = GameService.GetService<SceneService>().CurrentScene.PauseAndLock();
            if (newPauseKey == -1)
            {
                return;
            }
            pauseKey = newPauseKey;
            fadeIn.Start();
            currentDialog = GameService.GetService<IContentService>().GetxAsset<DialogLink>(DialogKey);
            displayedLength = 0;
        }

        private bool LineHasEnded => displayedLength == currentDialog.Length;

        public override Type GetBackingInterfaceType()
        {
            return typeof(DialogService);
        }
    }
}
