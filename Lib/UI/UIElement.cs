using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.UI
{
    public abstract class UIElement<TElement> : IUIElement where TElement : UIElement<TElement>
    {
        public static readonly Dictionary<string, object> DEFAULT_EVENT_STATE = new Dictionary<string, object>()
        {
            { "isMouseInside", false },
        };

        private UIEnums.Positioning positioning;
        private UIEnums.ChildOrdering childOrdering;

        protected string key;
        protected UIRenderer uiRendererInstance;

        private UISize width;
        private UISize height;
        private int x;
        private int y;
        private IUIElement[] children;

        protected Action<Vector2> onClick;
        protected Action<Vector2> onRightClick;
        protected Action<Vector2> onEnter;
        protected Action<Vector2> onMove;
        protected Action<Vector2> onExit;
        protected Action<Vector2> onPress;
        protected Action<Vector2> onRelease;
        protected Action<Vector2> onPressMiddle;
        protected Action<Vector2> onReleaseMiddle;

        protected bool isLocked;

        public UIElement(string Key, UIRenderer UIRendererInstance)
        {
            key = Key;
            uiRendererInstance = UIRendererInstance;
            isLocked = false;
            children = new IUIElement[0];
            positioning = UIEnums.Positioning.Relative;
        }

        public string GetKey()
        {
            return key;
        }

        public UISize GetWidth()
        {
            return width;
        }

        public TElement SetWidth(UISize Width)
        {
            CheckCanModify();
            width = Width;
            return (TElement)this;
        }

        public UISize GetHeight()
        {
            return height;
        }

        public TElement SetHeight(UISize Height)
        {
            CheckCanModify();
            height = Height;
            return (TElement)this;
        }

        public int GetX()
        {
            return x;
        }

        public TElement SetX(int X)
        {
            CheckCanModify();
            x = X;
            return (TElement)this;
        }

        public int GetY()
        {
            return y;
        }

        public TElement SetY(int Y)
        {
            CheckCanModify();
            y = Y;
            return (TElement)this;
        }

        public TElement SetChildren(params IUIElement[] Children)
        {
            CheckCanModify();
            if (Children == null)
            {
                children = new IUIElement[0];
            }
            else
            {
                children = Children.Where(child => child != null).ToArray();
            }
            return (TElement)this;
        }

        public UIEnums.Positioning GetPositioning()
        {
            return positioning;
        }

        public TElement SetPositioning(UIEnums.Positioning Positioning)
        {
            CheckCanModify();
            positioning = Positioning;
            return (TElement)this;
        }

        public UIEnums.ChildOrdering GetChildOrdering()
        {
            return childOrdering;
        }

        public TElement SetChildOrdering(UIEnums.ChildOrdering ChildOrdering)
        {
            CheckCanModify();
            childOrdering = ChildOrdering;
            return (TElement)this;
        }

        public TElement SetOnClick(Action<Vector2> OnClick)
        {
            CheckCanModify();
            onClick = OnClick;
            return (TElement)this;
        }

        public TElement SetOnRightClick(Action<Vector2> OnRightClick)
        {
            CheckCanModify();
            onRightClick = OnRightClick;
            return (TElement)this;
        }

        public TElement SetOnEnter(Action<Vector2> OnEnter)
        {
            CheckCanModify();
            onEnter = OnEnter;
            return (TElement)this;
        }

        public TElement SetOnMove(Action<Vector2> OnMove)
        {
            CheckCanModify();
            onMove = OnMove;
            return (TElement)this;
        }

        public TElement SetOnExit(Action<Vector2> OnExit)
        {
            CheckCanModify();
            onExit = OnExit;
            return (TElement)this;
        }

        public TElement SetOnPress(Action<Vector2> OnPress)
        {
            CheckCanModify();
            onPress = OnPress;
            return (TElement)this;
        }

        public TElement SetOnRelease(Action<Vector2> OnRelease)
        {
            CheckCanModify();
            onRelease = OnRelease;
            return (TElement)this;
        }

        public TElement SetOnPressMiddle(Action<Vector2> OnPress)
        {
            CheckCanModify();
            onPressMiddle = OnPress;
            return (TElement)this;
        }

        public TElement SetOnReleaseMiddle(Action<Vector2> OnRelease)
        {
            CheckCanModify();
            onReleaseMiddle = OnRelease;
            return (TElement)this;
        }

        public TElement Finalize()
        {
            CheckCanModify();
            uiRendererInstance.RegisterComponent(this);
            var result = FinalizeImplementation();
            isLocked = true;
            return result;
        }

        protected abstract TElement FinalizeImplementation();

        protected void CheckCanModify()
        {
            if (!isLocked)
            {
                return;
            }
            throw new Exception("Trying to modify finalized UIElement");
        }

        public List<UIDrawCall> RenderAsRoot()
        {
            if (width.Sizing != UIEnums.Sizing.Fixed || height.Sizing != UIEnums.Sizing.Fixed || positioning != UIEnums.Positioning.Absolute)
            {
                throw new Exception("Root must be of fixed size and absolute positioning");
            }
            return Render(width.Size, height.Size, x, y);
        }

        public void PreDrawBase(float DT, UIDrawCall DrawCall)
        {
            IInputService inputService = GameService.GetService<IInputService>();
            Vector2 cursorPosition = inputService.GetMouseInScreenSpace();
            bool isMouseInside = GetEventState<bool>("isMouseInside");
            Vector2 interiorPosition = new Vector2(cursorPosition.X - DrawCall.X, cursorPosition.Y - DrawCall.Y);
            if (DrawCall.ContainsPoint(cursorPosition))
            {
                if (!isMouseInside)
                {
                    onEnter?.Invoke(interiorPosition);

                    if (inputService.IsLeftMouseButtonHeld())
                    {
                        onPress?.Invoke(interiorPosition);
                    }

                    if (inputService.IsMiddleMouseButtonHeld())
                    {
                        onPressMiddle?.Invoke(interiorPosition);
                    }

                    SetEventState("isMouseInside", true);
                }
                else
                {
                    if (inputService.WasLeftMouseButtonClicked())
                    {
                        onClick?.Invoke(interiorPosition);
                        onRelease?.Invoke(interiorPosition);
                        onEnter?.Invoke(interiorPosition);
                    }
                    else if (inputService.IsLeftMouseButtonHeld())
                    {
                        onPress?.Invoke(interiorPosition);
                    }

                    if (inputService.WasMiddleMouseButtonClicked())
                    {
                        //onClick?.Invoke();
                        onReleaseMiddle?.Invoke(interiorPosition);
                    }
                    else if (inputService.IsMiddleMouseButtonHeld())
                    {
                        onPressMiddle?.Invoke(interiorPosition);
                    }

                    if (inputService.WasRightMouseButtonClicked())
                    {
                        onRightClick?.Invoke(interiorPosition);
                    }
                }
                if (inputService.GetCurrentMouseState().Position != inputService.GetRolledbackMouseState(1).Position)
                {
                    onMove?.Invoke(interiorPosition);
                }
            }
            else
            {
                if (isMouseInside)
                {
                    onExit?.Invoke(interiorPosition);
                    SetEventState("isMouseInside", false);
                }
            }
            PreDraw(DT);
        }

        public virtual void PreDraw(float DT) { }

        public List<UIDrawCall> Render(int RenderedWidth, int RenderedHeight, int RenderedX, int RenderedY)
        {
            // Insert own draw call
            List<UIDrawCall> ownDrawCalls = new List<UIDrawCall>();
            ownDrawCalls.Add(new UIDrawCall(this, RenderedWidth, RenderedHeight, RenderedX, RenderedY));

            // Find out how much space the fixed elements need, and what the total sum of relative sizes are
            int fixedSpaceNeeds = 0;
            int restSpaceSums = 0;
            int restSpaceMinimumNeeds = 0;
            int maxCrossDirectionSize = 0;
            int cursorX = RenderedX;
            int cursorY = RenderedY;
            int runBeginIndex = 0;
            for (int i = 0; i < children.Length; )
            {
                IUIElement child = children[i];

                if (child.GetPositioning() == UIEnums.Positioning.Absolute)
                {
                    ownDrawCalls.AddRange(
                        child.Render(
                            child.GetWidth().Sizing == UIEnums.Sizing.Fixed ? child.GetWidth().Size : (int)(RenderedWidth * (child.GetWidth().Size / 100f)),
                            child.GetHeight().Sizing == UIEnums.Sizing.Fixed ? child.GetHeight().Size : (int)(RenderedHeight * (child.GetHeight().Size / 100f)),
                            RenderedX + child.GetX(),
                            RenderedY + child.GetY()
                        )
                    );
                    i++;
                    continue;
                }
                else if (child.GetPositioning() == UIEnums.Positioning.Fixed)
                {
                    ownDrawCalls.AddRange(
                        child.Render(
                            child.GetWidth().Sizing == UIEnums.Sizing.Fixed ? child.GetWidth().Size : (int)(RenderedWidth * (child.GetWidth().Size / 100f)),
                            child.GetHeight().Sizing == UIEnums.Sizing.Fixed ? child.GetHeight().Size : (int)(RenderedHeight * (child.GetHeight().Size / 100f)),
                            child.GetX(),
                            child.GetY()
                         )
                    );
                    i++;
                    continue;
                }

                // We don't want to commit these values until we know we haven't reached capacity
                int tempFixedSpaceNeeds = fixedSpaceNeeds;
                int tempRestSpaceSums = restSpaceSums;
                int tempRestSpaceMinimumNeeds = restSpaceMinimumNeeds;
                int tempMaxCrossDirectionSize = maxCrossDirectionSize;

                bool isLastChild = i == children.Length - 1;
                if (!isLastChild && child.GetPositioning() == UIEnums.Positioning.Relative)
                {
                    bool didFindRelative = false;
                    for (int j = i + 1; j < children.Length && !isLastChild; j++)
                    {
                        if (children[j].GetPositioning() == UIEnums.Positioning.Relative)
                        {
                            didFindRelative = true;
                            break;
                        }
                    }
                    if (!didFindRelative)
                    {
                        isLastChild = true;
                    }
                }
                if (childOrdering == UIEnums.ChildOrdering.Row)
                {
                    if (child.GetWidth().Sizing == UIEnums.Sizing.Fixed)
                    {
                        tempFixedSpaceNeeds += child.GetWidth().Size;
                    }
                    else if (child.GetWidth().Sizing == UIEnums.Sizing.Relative)
                    {
                        tempFixedSpaceNeeds += (int)(child.GetWidth().Size / 100f * RenderedWidth) - child.GetWidth().FullMargin;
                    }
                    else if (child.GetWidth().Sizing == UIEnums.Sizing.Rest)
                    {
                        tempRestSpaceSums += child.GetWidth().Size;
                        tempRestSpaceMinimumNeeds += child.GetWidth().Minimum;
                    }
                    tempFixedSpaceNeeds += child.GetWidth().FullMargin;

                    if (child.GetHeight().Sizing == UIEnums.Sizing.Fixed)
                    {
                        tempMaxCrossDirectionSize = Math.Max(tempMaxCrossDirectionSize, child.GetHeight().Size + child.GetHeight().FullMargin);
                    }
                    else if (child.GetHeight().Sizing == UIEnums.Sizing.Relative)
                    {
                        tempMaxCrossDirectionSize += Math.Max(tempMaxCrossDirectionSize, (int)(child.GetHeight().Size / 100f * RenderedHeight) + child.GetHeight().FullMargin);
                    }
                    else
                    {
                        throw new Exception("Rest sizing is not allowed in a child's cross direction");
                    }

                    // Check if we're over capacity
                    bool wasCapacityReached = tempFixedSpaceNeeds + tempRestSpaceMinimumNeeds > RenderedWidth;
                    if (wasCapacityReached || isLastChild)
                    {
                        if (!wasCapacityReached && isLastChild)
                        {
                            fixedSpaceNeeds = tempFixedSpaceNeeds;
                            restSpaceSums = tempRestSpaceSums;
                            restSpaceMinimumNeeds = tempRestSpaceMinimumNeeds;
                            maxCrossDirectionSize = tempMaxCrossDirectionSize;
                        }
                        int realCursorX = cursorX;
                        for (int j = runBeginIndex; j <= i; j++)
                        {
                            if (wasCapacityReached && j == i)
                            {
                                continue;
                            }
                            IUIElement renderedChild = children[j];
                            if (renderedChild.GetPositioning() != UIEnums.Positioning.Relative)
                            {
                                continue;
                            }
                            int childWidth, childHeight;
                            switch (renderedChild.GetWidth().Sizing) {
                                case UIEnums.Sizing.Fixed:
                                    childWidth = renderedChild.GetWidth().Size;
                                    break;
                                case UIEnums.Sizing.Relative:
                                    childWidth = (int)(renderedChild.GetWidth().Size / 100f * RenderedWidth);
                                    break;
                                case UIEnums.Sizing.Rest:
                                    childWidth = Math.Max(
                                        (int)((float)renderedChild.GetWidth().Size / restSpaceSums * (RenderedWidth - fixedSpaceNeeds - (restSpaceMinimumNeeds - renderedChild.GetWidth().Minimum))),
                                        renderedChild.GetWidth().Minimum
                                    );
                                    break;
                                default:
                                    throw new Exception("Unsupported width sizing");
                            }
                            switch (renderedChild.GetHeight().Sizing)
                            {
                                case UIEnums.Sizing.Fixed:
                                    childHeight = renderedChild.GetHeight().Size;
                                    break;
                                case UIEnums.Sizing.Relative:
                                    childHeight = (int)(renderedChild.GetHeight().Size / 100f * RenderedHeight);
                                    break;
                                default:
                                    throw new Exception("Unsupported height sizing on cross direction");
                            }
                            ownDrawCalls.AddRange(renderedChild.Render(childWidth, childHeight, realCursorX + renderedChild.GetWidth().MarginStart, cursorY + renderedChild.GetHeight().MarginStart));
                            realCursorX += childWidth + renderedChild.GetWidth().FullMargin;
                        }
                        if (!wasCapacityReached && isLastChild)
                        {
                            i++;
                            continue;
                        }
                        cursorX = RenderedX;
                        cursorY += maxCrossDirectionSize;
                        fixedSpaceNeeds = 0;
                        restSpaceSums = 0;
                        restSpaceMinimumNeeds = 0;
                        maxCrossDirectionSize = 0;
                        runBeginIndex = i;
                        continue;
                    }
                }
                else if (childOrdering == UIEnums.ChildOrdering.Column)
                {
                    if (child.GetHeight().Sizing == UIEnums. Sizing.Fixed)
                    {
                        tempFixedSpaceNeeds += child.GetHeight().Size;
                    }
                    else if (child.GetHeight().Sizing == UIEnums.Sizing.Relative)
                    {
                        tempFixedSpaceNeeds += (int)(child.GetHeight().Size / 100f * RenderedHeight) - child.GetHeight().FullMargin;
                    }
                    else if (child.GetHeight().Sizing == UIEnums.Sizing.Rest)
                    {
                        tempRestSpaceSums += child.GetHeight().Size;
                        tempRestSpaceMinimumNeeds += child.GetHeight().Minimum;
                    }
                    tempFixedSpaceNeeds += child.GetHeight().FullMargin;

                    if (child.GetWidth().Sizing == UIEnums.Sizing.Fixed)
                    {
                        tempMaxCrossDirectionSize = Math.Max(tempMaxCrossDirectionSize, child.GetWidth().Size + child.GetWidth().FullMargin);
                    }
                    else if (child.GetWidth().Sizing == UIEnums.Sizing.Relative)
                    {
                        tempMaxCrossDirectionSize = Math.Max(tempMaxCrossDirectionSize, (int)(child.GetWidth().Size / 100f * RenderedWidth + child.GetWidth().FullMargin));
                    }
                    else
                    {
                        throw new System.Exception("Rest sizing is not allowed in a child's cross direction");
                    }

                    // Check if we're over capacity
                    bool wasCapacityReached = tempFixedSpaceNeeds + tempRestSpaceMinimumNeeds > RenderedHeight;
                    if (wasCapacityReached || isLastChild)
                    {
                        if (!wasCapacityReached && isLastChild)
                        {
                            fixedSpaceNeeds = tempFixedSpaceNeeds;
                            restSpaceSums = tempRestSpaceSums;
                            restSpaceMinimumNeeds = tempRestSpaceMinimumNeeds;
                            maxCrossDirectionSize = tempMaxCrossDirectionSize;
                        }
                        int realCursorY = cursorY;
                        for (int j = runBeginIndex; j <= i; j++)
                        {
                            if (wasCapacityReached && j == i)
                            {
                                continue;
                            }
                            IUIElement renderedChild = children[j];
                            if (renderedChild.GetPositioning() != UIEnums.Positioning.Relative)
                            {
                                continue;
                            }
                            int childWidth, childHeight;
                            switch (renderedChild.GetWidth().Sizing)
                            {
                                case UIEnums.Sizing.Fixed:
                                    childWidth = renderedChild.GetWidth().Size;
                                    break;
                                case UIEnums.Sizing.Relative:
                                    childWidth = (int)(renderedChild.GetWidth().Size / 100f * RenderedWidth);
                                    break;
                                default:
                                    throw new Exception("Unsupported width sizing on cross direction");
                            }
                            switch (renderedChild.GetHeight().Sizing)
                            {
                                case UIEnums.Sizing.Fixed:
                                    childHeight = renderedChild.GetHeight().Size;
                                    break;
                                case UIEnums.Sizing.Relative:
                                    childHeight = (int)(renderedChild.GetHeight().Size / 100f * RenderedHeight);
                                    break;
                                case UIEnums.Sizing.Rest:
                                    childHeight = Math.Max(
                                        (int)((float)renderedChild.GetHeight().Size / restSpaceSums * (RenderedHeight - fixedSpaceNeeds - (restSpaceMinimumNeeds - renderedChild.GetHeight().Minimum))),
                                        renderedChild.GetHeight().Minimum
                                    );
                                    break;
                                default:
                                    throw new Exception("Unsupported height sizing");
                            }
                            ownDrawCalls.AddRange(renderedChild.Render(childWidth, childHeight, cursorX + renderedChild.GetWidth().MarginStart, realCursorY + renderedChild.GetHeight().MarginStart));
                            realCursorY += childHeight + renderedChild.GetHeight().FullMargin;
                        }
                        if (!wasCapacityReached && isLastChild)
                        {
                            i++;
                            continue;
                        }
                        cursorX += maxCrossDirectionSize;
                        cursorY = RenderedY;
                        fixedSpaceNeeds = 0;
                        restSpaceSums = 0;
                        restSpaceMinimumNeeds = 0;
                        maxCrossDirectionSize = 0;
                        runBeginIndex = i;
                        continue;
                    }
                }
                fixedSpaceNeeds = tempFixedSpaceNeeds;
                restSpaceSums = tempRestSpaceSums;
                restSpaceMinimumNeeds = tempRestSpaceMinimumNeeds;
                maxCrossDirectionSize = tempMaxCrossDirectionSize;
                i++;
            }

            return ownDrawCalls;
        }

        public abstract void Draw(int RenderedWidth, int RenderedHeight, int RenderedX, int RenderedY);

        public void SetUIRendererInstance(UIRenderer Renderer)
        {
            uiRendererInstance = Renderer;
        }

        public virtual Dictionary<string, object> GetDefaultState()
        {
            return new Dictionary<string, object>();
        }

        private TState GetEventState<TState>(string StateKey)
        {
            return uiRendererInstance.GetComponentEventState<TState>(key, StateKey);
        }

        private void SetEventState<TState>(string StateKey, TState StateValue)
        {
            uiRendererInstance.SetComponentEventState(key, StateKey, StateValue);
        }

        public TState GetState<TState>(string StateKey)
        {
            return uiRendererInstance.GetComponentState<TState>(key, StateKey);
        }

        public void SetState<TState>(string StateKey, TState StateValue)
        {
            uiRendererInstance.SetComponentState(key, StateKey, StateValue);
        }
    }
}
