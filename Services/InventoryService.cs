using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectWarnerShared.Entities.UI;
using ProjectWarnerShared.Entities.Warner;
using WarnerEngine.Lib;
using WarnerEngine.Lib.Helpers;

namespace WarnerEngine.Services
{
    public class InventoryService : IService
    {
        private const string RENDER_TARGET_KEY = "inventory";

        private const int MAX_ENTRIES_PER_ROW = 12;
        private const int ENTRY_SIZE = 24;
        private const int INVENTORY_ROW_TOP = 140;

        private const int NAME_WINDOW_HORIZONTAL_PADDING = 8;
        private const int NAME_WINDOW_VERTICAL_PADDING = 2;
        private const int NAME_WINDOW_TOP = INVENTORY_ROW_TOP + ENTRY_SIZE + 8;
        private const int DESCRIPTION_WINDOW_PADDING = 8;
        private const int DESCRIPTION_MARGIN_Y = 4;

        private const float TARGET_FADE_DURATION = 100;

        public static readonly Rectangle SELECTED_SLOT_SOURCE = new Rectangle(32, 10, 24, 24);
        public static readonly Rectangle EMPTY_SLOT_SOURCE = new Rectangle(56, 10, 24, 24);
        public static readonly Rectangle EQUIPPED_SLOT_SOURCE = new Rectangle(80, 10, 24, 24);

        private AutoTween fadeIn;
        private AutoTween fadeOut;

        public WarnerInventory ActiveInventory { get; private set; }

        public bool IsActive { get; private set; }
        public int pauseKey;

        private int selectedInventoryEntryIndex;
        private int SelectedInventoryEntryIndex
        {
            get
            {
                return selectedInventoryEntryIndex;
            }

            set
            {
                if (ActiveInventory == null)
                {
                    return;
                }
                selectedInventoryEntryIndex = value % ActiveInventory.InventorySize;
                if (selectedInventoryEntryIndex < 0)
                {
                    selectedInventoryEntryIndex += ActiveInventory.InventorySize;
                }
            }
        }

        public InventoryService()
        {
            GameService.GetService<EventService>().Subscribe(
                Events.INTERNAL_RESOLUTION_CHANGED,
                _ =>
                {
                    RenderService renderService = GameService.GetService<RenderService>();
                    renderService.AddRenderTarget(
                        RENDER_TARGET_KEY,
                        renderService.InternalResolutionX,
                        renderService.InternalResolutionY,
                        RenderTargetUsage.PreserveContents
                    );
                }
            );
            fadeIn = new AutoTween(0, 1, TARGET_FADE_DURATION);
            fadeOut = new AutoTween(1, 0, TARGET_FADE_DURATION);
        }

        public void SetActiveInventory(WarnerInventory Inventory)
        {
            ActiveInventory = Inventory;
        }

        public void PreDraw(float DT)
        {
            if (GameService.GetService<IInputService>().WasActionPressed(InputAction.OpenInventory))
            {
                if (IsActive)
                {
                    if (GameService.GetService<SceneService>().CurrentScene.Unpause(pauseKey))
                    {
                        IsActive = false;
                        fadeOut.Start();
                    }
                }
                else
                {
                    int newPauseKey = GameService.GetService<SceneService>().CurrentScene.PauseAndLock();
                    if (newPauseKey != -1)
                    {
                        pauseKey = newPauseKey;
                        IsActive = true;
                        fadeIn.Start();
                    }
                }
            }
            if (!IsActive)
            {
                return;
            }
            IInputService inputService = GameService.GetService<IInputService>();
            if (inputService.WasActionPressed(InputAction.Left))
            {
                SelectedInventoryEntryIndex--;
            } 
            else if (inputService.WasActionPressed(InputAction.Right))
            {
                SelectedInventoryEntryIndex++;
            }
            else if (inputService.WasActionPressed(InputAction.ActionItem1))
            {
                ActiveInventory.TryEquipItem(0, selectedInventoryEntryIndex);
            }
            else if (inputService.WasActionPressed(InputAction.ActionItem2))
            {
                ActiveInventory.TryEquipItem(1, selectedInventoryEntryIndex);
            }
            else if (inputService.WasActionPressed(InputAction.Interact))
            {
                ActiveInventory.TryUseNonEquippableConsumableItem(selectedInventoryEntryIndex);
            }
        }

        public ServiceCompositionMetadata Draw()
        {
            if (!IsActive && !fadeOut.IsRunning)
            {
                return ServiceCompositionMetadata.Empty;
            }

            if (IsActive)
            {
                RenderService renderService = GameService.GetService<RenderService>();
                renderService
                    .SetRenderTarget(RENDER_TARGET_KEY, Color.Black * 0.65f)
                    .Start();

                float leftOffset = (renderService.InternalResolutionX - MAX_ENTRIES_PER_ROW * ENTRY_SIZE) / 2;

                for (int i = 0; i < ActiveInventory.InventorySize; i++)
                {
                    var inventoryEntry = ActiveInventory.Inventory[i];
                    Vector2 iconPosition = new Vector2(leftOffset + i * ENTRY_SIZE, INVENTORY_ROW_TOP);
                    if (inventoryEntry != null)
                    {
                        if (IsItemEquipped(inventoryEntry))
                        {
                            renderService.DrawQuad(
                                ProjectWarnerShared.Content.Constants.GAMEUI_STATS_OVERLAY,
                                iconPosition - new Vector2(4f),
                                EQUIPPED_SLOT_SOURCE,
                                Tint: Color.DarkSlateGray
                            );
                        }
                        renderService
                            .DrawQuad(
                                ProjectWarnerShared.Content.Constants.ITEMS_COLLECTABLES,
                                iconPosition,
                                GraphicsHelper.GetSheetCell(inventoryEntry.Metadata.IconIndex, 16, 16)
                            )
                            .DrawString(
                                "stats_overlay",
                                inventoryEntry.Count.ToString(),
                                iconPosition - new Vector2(1f),
                                Color.White
                            );
                        if (SelectedInventoryEntryIndex == i)
                        {
                            SpriteFont nameFont = GameService.GetService<IContentService>().GetSpriteFont("pixel_operator");
                            SpriteFont descriptionFont = GameService.GetService<IContentService>().GetSpriteFont("stats_overlay");
                            Vector2 nameSize = nameFont.MeasureString(inventoryEntry.Metadata.Name);
                            Vector2 descriptionSize = descriptionFont.MeasureString(inventoryEntry.Metadata.Description);
                            Vector2 nameWindowPosition = new Vector2(
                                (renderService.InternalResolutionX - nameSize.X) / 2 - NAME_WINDOW_HORIZONTAL_PADDING,
                                NAME_WINDOW_TOP
                            );
                            Vector2 descriptionDialogPosition = new Vector2(
                                (renderService.InternalResolutionX - descriptionSize.X) / 2 - DESCRIPTION_WINDOW_PADDING,
                                NAME_WINDOW_TOP + nameSize.Y + NAME_WINDOW_VERTICAL_PADDING * 2 + DESCRIPTION_MARGIN_Y
                            );
                            renderService
                                .DrawNinePatch(
                                    ProjectWarnerShared.Content.Constants.GAMEUI_DIALOG_WINDOW,
                                    new Rectangle(
                                        (int)nameWindowPosition.X,
                                        (int)nameWindowPosition.Y,
                                        (int)nameSize.X + NAME_WINDOW_HORIZONTAL_PADDING * 2,
                                        (int)nameSize.Y + NAME_WINDOW_VERTICAL_PADDING * 2
                                    ),
                                    8,
                                    8,
                                    Color.Gray
                                )
                                .DrawString(
                                    "pixel_operator",
                                    inventoryEntry.Metadata.Name,
                                    nameWindowPosition + new Vector2(NAME_WINDOW_HORIZONTAL_PADDING, NAME_WINDOW_VERTICAL_PADDING),
                                    Color.White
                                )
                                .DrawNinePatch(
                                    ProjectWarnerShared.Content.Constants.GAMEUI_DIALOG_WINDOW,
                                    new Rectangle(
                                        (int)descriptionDialogPosition.X,
                                        (int)descriptionDialogPosition.Y,
                                        (int)descriptionSize.X + DESCRIPTION_WINDOW_PADDING * 2,
                                        (int)descriptionSize.Y + DESCRIPTION_WINDOW_PADDING * 2
                                    ),
                                    8,
                                    8
                                )
                                .DrawString(
                                    "stats_overlay",
                                    inventoryEntry.Metadata.Description,
                                    descriptionDialogPosition + new Vector2(DESCRIPTION_WINDOW_PADDING),
                                    Color.Black
                                );
                        }
                    }
                    else
                    {
                        renderService.DrawQuad(
                            ProjectWarnerShared.Content.Constants.GAMEUI_STATS_OVERLAY,
                            iconPosition - new Vector2(4f),
                            EMPTY_SLOT_SOURCE
                        );
                    }
                    if (i == SelectedInventoryEntryIndex)
                    {
                        renderService.DrawQuad(
                            ProjectWarnerShared.Content.Constants.GAMEUI_STATS_OVERLAY,
                            iconPosition - new Vector2(4f),
                            SELECTED_SLOT_SOURCE
                        );
                    }
                }
                renderService.End();
            }

            return new ServiceCompositionMetadata()
            {
                RenderTargetKey = RENDER_TARGET_KEY,
                Position = Vector2.Zero,
                Priority = 50,
                Tint = Color.White * (fadeOut.IsRunning ? fadeOut.GetTween() : fadeIn.GetTween()),
            };
        }

        private bool IsItemEquipped(ProjectWarnerShared.Entities.Warner.Inventory.WarnerInventoryItem Item)
        {
            if (ActiveInventory == null)
            {
                return false;
            }
            return ActiveInventory.IsItemEquipped(Item);
        }

        public void PostDraw() { }

        public Type GetBackingInterfaceType()
        {
            return typeof(InventoryService);
        }
    }
}
