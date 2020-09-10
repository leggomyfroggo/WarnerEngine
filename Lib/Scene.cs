using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;

using ProjectWarnerShared.Entities;
using WarnerEngine.Lib.Components;
using WarnerEngine.Lib.Entities;
using WarnerEngine.Lib.Structure;
using WarnerEngine.Services;

namespace WarnerEngine.Lib
{
    public abstract class Scene
    {
        protected List<ISceneEntity> entities;
        protected List<ISceneEntity> pendingEntities;
        protected List<ISceneEntity> removedEntities;

        protected Dictionary<IDraw, HashSet<IDraw>> outboundEdges;
        protected Dictionary<IDraw, HashSet<IDraw>> inboundEdges;

        protected Dictionary<string, object> localStore;

        protected Dictionary<Type, object> cachedTypeToEntities;
        protected HashSet<Type> invalidCacheKeys;

        public Camera Camera { get; protected set; }

        public int PauseTimer { get; set; }
        public bool IsPaused { get; private set; }
        private int pauseCount;

        public Scene()
        {
            Reset();
        }

        protected void Reset()
        {
            entities = new List<ISceneEntity>();
            pendingEntities = new List<ISceneEntity>();
            removedEntities = new List<ISceneEntity>();

            outboundEdges = new Dictionary<IDraw, HashSet<IDraw>>();
            inboundEdges = new Dictionary<IDraw, HashSet<IDraw>>();

            localStore = new Dictionary<string, object>();
            cachedTypeToEntities = new Dictionary<Type, object>();
            invalidCacheKeys = new HashSet<Type>();

            IsPaused = false;
            pauseCount = 0;
        }

        public virtual void PreDraw(float DT)
        {
            if (!IsPaused)
            {
                if (PauseTimer > 0)
                {
                    DT /= 4f;
                    PauseTimer--;
                }
                UpdateActiveEntities();
                List<IPreDraw> preDrawableEntities = GetEntitiesOfType<IPreDraw>();
                foreach (IPreDraw entity in preDrawableEntities)
                {
                    entity.PreDraw(DT);
                }
            }
        }

        public virtual void Draw()
        {
            List<IDraw> sortedEntities = GetSortedEntities<IDraw>(true);
            int currentIndex = 0;
            float totalEntities = sortedEntities.Count;
            foreach (IDraw entity in sortedEntities)
            {
                float depth = (currentIndex++) / totalEntities;
                GameService.GetService<IRenderService>().SetDepth(depth);
                entity.Draw();
            }
        }

        public List<T> GetSortedEntities<T>(bool ShouldReverse = false) where T : class, IDraw
        {
            List<T> visibleEntities = GetEntitiesOfType<T>()
                .Where(e => e.IsVisible() && e.GetBackingBox() != BackingBox.Dummy && Camera.ContainsBox(e.GetBackingBox().B))
                .ToList();
            foreach (T entity in visibleEntities)
            {
                outboundEdges[entity].Clear();
                inboundEdges[entity].Clear();
            }
            for (int i = 0; i < visibleEntities.Count; i++)
            {
                var ie = visibleEntities[i];
                var ib = ie.GetBackingBox();
                for (int j = i + 1; j < visibleEntities.Count; j++)
                {
                    var je = visibleEntities[j];
                    var jb = je.GetBackingBox();
                    if (ib.DoesProjectionOverlapOther(jb)) {
                        int comparison = ib.Compare(jb);

                        T inboundNode = null;
                        T outboundNode = null;

                        if (comparison >= 0)
                        {
                            outboundNode = ie;
                            inboundNode = je;
                        }
                        else if (comparison < 0)
                        {
                            outboundNode = je;
                            inboundNode = ie;
                        }
                        else
                        {
                            continue;
                        }
                        outboundEdges[outboundNode].Add(inboundNode);
                        inboundEdges[inboundNode].Add(outboundNode);
                    }
                }
            }
            List<T> sortedEntities = new List<T>(visibleEntities.Count);
            HashSet<T> sortedLookup = new HashSet<T>();
            int visibleLower = 0;
            int visibleUpper = visibleEntities.Count;
            while (sortedEntities.Count < visibleEntities.Count)
            {
                bool didLoopInfinitely = true;
                for (int i = visibleLower; i < visibleUpper; i++)
                {
                    var entity = visibleEntities[i];
                    if (sortedLookup.Contains(entity))
                    {
                        continue;
                    }
                    HashSet<IDraw> entityOutbound = outboundEdges[entity];
                    if (entityOutbound.Count == 0)
                    {
                        didLoopInfinitely = false;
                        foreach (T inboundEntity in inboundEdges[entity])
                        {
                            outboundEdges[inboundEntity].Remove(entity);
                        }
                        sortedLookup.Add(entity);
                        sortedEntities.Add(entity);
                        visibleLower = Math.Min(visibleLower, i);
                        if (i == visibleUpper - 1)
                        {
                            visibleUpper = i;
                        }
                        continue;
                    }
                }
                if (didLoopInfinitely)
                {
                    T minTop = null;
                    foreach (T outerEntity in visibleEntities)
                    {
                        if (sortedLookup.Contains(outerEntity))
                        {
                            continue;
                        }
                        if (
                            minTop == null || 
                            outerEntity.GetBackingBox().Top < minTop.GetBackingBox().Top ||
                            (outerEntity.GetBackingBox().Top == minTop.GetBackingBox().Top && outerEntity.GetBackingBox().Front < minTop.GetBackingBox().Front)
                        )
                        {
                            minTop = outerEntity;
                        }
                    }
                    outboundEdges[minTop].Clear();
                }
            }
            if (ShouldReverse)
            {
                sortedEntities.Reverse();
                sortedEntities = GetEntitiesOfType<T>()
                    .Where(e => e.IsVisible() && e.GetBackingBox() == BackingBox.Dummy)
                    .Concat(sortedEntities)
                    .ToList();
            }
            else
            {
                sortedEntities = sortedEntities
                .Concat(GetEntitiesOfType<T>()
                    .Where(e => e.IsVisible() && e.GetBackingBox() == BackingBox.Dummy))
                .ToList();
            }
            return sortedEntities;
        }

        public void PostDraw()
        {
            UpdateActiveEntities();
            List<IPostDraw> postDrawableEntities = GetEntitiesOfType<IPostDraw>();
            foreach (IPostDraw entity in postDrawableEntities)
            {
                entity.PostDraw();
            }
        }

        public abstract Dictionary<string, Func<string[], string>> GetLocalTerminalCommands();

        public int PauseAndLock()
        {
            if (IsPaused)
            {
                return -1;
            }
            IsPaused = true;
            return pauseCount;
        }

        public bool Unpause(int Key)
        {
            if (Key != pauseCount)
            {
                return false;
            }
            IsPaused = false;
            pauseCount++;
            return true;
        }

        public virtual Scene AddEntity(ISceneEntity Entity)
        {
            Entity.OnAdd(this);
            pendingEntities.Add(Entity);
            return this;
        }

        public virtual Scene RemoveEntity(ISceneEntity Entity)
        {
            Entity.OnRemove(this);
            removedEntities.Add(Entity);
            return this;
        }

        protected virtual Scene FlushEntities()
        {
            foreach (ISceneEntity entity in entities)
            {
                RemoveEntity(entity);
            }
            return this;
        }

        public Scene SetLocalValue<TValue>(string Key, TValue Value)
        {
            localStore[Key] = Value;
            return this;
        }

        public TValue GetLocalValue<TValue>(string Key)
        {
            return (TValue)localStore[Key];
        }

        public List<TActor> GetEntitiesOfType<TActor>()
        {
            Type genericType = typeof(TActor);
            if (!cachedTypeToEntities.ContainsKey(genericType))
            {
                cachedTypeToEntities[genericType] = entities.OfType<TActor>().ToList();
            }
            return (List<TActor>)cachedTypeToEntities[genericType];
        }

        public TActor GetFirstEntityOfType<TActor>()
        {
            List<TActor> entities = GetEntitiesOfType<TActor>();
            return GetEntitiesOfType<TActor>().First();
        }

        public abstract void OnSceneStart();
        public abstract void OnSceneEnd();

        private void UpdateActiveEntities()
        {
            foreach (ISceneEntity entity in removedEntities)
            {
                invalidCacheKeys.Add(entity.GetType());
                entities.Remove(entity);
                if (typeof(IDraw).IsAssignableFrom(entity.GetType()))
                {
                    outboundEdges.Remove((IDraw)entity);
                    inboundEdges.Remove((IDraw)entity);
                }
            }
            removedEntities.Clear();
            foreach (ISceneEntity entity in pendingEntities)
            {
                invalidCacheKeys.Add(entity.GetType());
                if (entities.Contains(entity))
                {
                    continue;
                }
                entities.Add(entity);
                if (typeof(IDraw).IsAssignableFrom(entity.GetType()))
                {
                    outboundEdges[(IDraw)entity] = new HashSet<IDraw>();
                    inboundEdges[(IDraw)entity] = new HashSet<IDraw>();
                }
            }
            pendingEntities.Clear();

            cachedTypeToEntities = cachedTypeToEntities
                .Where(kvp =>
                {
                    foreach (Type t in invalidCacheKeys)
                    {
                        if (kvp.Key.IsAssignableFrom(t))
                        {
                            return false;
                        }
                    }
                    return true;
                })
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            invalidCacheKeys.Clear();
        }

        public void FlushAllPending()
        {
            UpdateActiveEntities();
        }

        public virtual bool SceneContainsDrawableEntity<T>(T Entity) where T : ISceneEntity, IDraw
        {
            return entities.Contains(Entity);
        }

        public virtual Effect GetCompositeEffect()
        {
            return null;
        }
    }
}
