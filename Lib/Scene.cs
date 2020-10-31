using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib.Components;
using WarnerEngine.Lib.Entities;
using WarnerEngine.Services;

namespace WarnerEngine.Lib
{
    public abstract class Scene
    {
        private static ObjectPool<HashSet<IDraw>> hashSetPool;

        protected List<ISceneEntity> entities;
        protected List<ISceneEntity> pendingEntities;
        protected List<ISceneEntity> removedEntities;

        protected HashSet<Type> invalidCacheKeys;
        protected Dictionary<Type, object> cachedTypeToEntities;

        protected Dictionary<IDraw, HashSet<IDraw>> outboundEdges;
        protected Dictionary<IDraw, HashSet<IDraw>> inboundEdges;

        protected Dictionary<string, object> localStore;

        public Camera Camera { get; protected set; }

        public int PauseTimer { get; set; }
        public bool IsPaused { get; private set; }
        private int pauseCount;

        static Scene()
        {
            hashSetPool = new ObjectPool<HashSet<IDraw>>(ReturnAction: set => set.Clear());
        }

        public Scene()
        {
            Reset();
        }

        protected void Reset()
        {
            entities = new List<ISceneEntity>();
            pendingEntities = new List<ISceneEntity>();
            removedEntities = new List<ISceneEntity>();

            invalidCacheKeys = new HashSet<Type>();
            cachedTypeToEntities = new Dictionary<Type, object>();

            outboundEdges = new Dictionary<IDraw, HashSet<IDraw>>();
            inboundEdges = new Dictionary<IDraw, HashSet<IDraw>>();

            localStore = new Dictionary<string, object>();

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
                var preDrawableEntities = GetEntitiesOfType<IPreDraw>();
                for (int i = 0; i < preDrawableEntities.Count; i++)
                {
                    preDrawableEntities[i].PreDraw(DT);
                }
            }
        }

        public virtual void Draw()
        {
            using (DisposableArray<IDraw> sortedEntities = GetSortedEntities<IDraw>(true))
            {
                int currentIndex = 0;
                float totalEntities = sortedEntities.Length;
                for (int i = 0; i < sortedEntities.Length; i++)
                {
                    var entity = sortedEntities[i];
                    if (entity == null)
                    {
                        break;
                    }
                    float depth = (currentIndex++) / totalEntities;
                    GameService.GetService<IRenderService>().SetDepth(depth);
                    entity.Draw();
                }
            }
        }

        public DisposableArray<T> GetSortedEntities<T>(bool ShouldReverse = false) where T : class, IDraw
        {
            DisposableList<T> visibleSortableEntities = GetEntitiesOfType<T>(
                e => e.IsVisible() && e.GetBackingBox() != BackingBox.Dummy && Camera.ContainsBox(e.GetBackingBox().B)
            );
            DisposableList<T> visibleDummyEntities = GetEntitiesOfType<T>(
                e => e.IsVisible() && e.GetBackingBox() == BackingBox.Dummy
            );
            for (int i = 0; i < visibleSortableEntities.Count; i++)
            {
                T entity = visibleSortableEntities[i];
                outboundEdges[entity].Clear();
                inboundEdges[entity].Clear();
            }
            for (int i = 0; i < visibleSortableEntities.Count; i++)
            {
                var ie = visibleSortableEntities[i];
                var ib = ie.GetBackingBox();
                for (int j = i + 1; j < visibleSortableEntities.Count; j++)
                {
                    var je = visibleSortableEntities[j];
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

            int totalEntityCount = visibleDummyEntities.Count + visibleSortableEntities.Count;
            int insertIndex = 0;
            DisposableArray<T> sortedEntities = DisposableArray<T>.Shared.Rent().Initialize(totalEntityCount);
            HashSet<IDraw> sortedLookup = hashSetPool.Rent();
            int visibleLower = 0;
            int visibleUpper = visibleSortableEntities.Count;
            while (insertIndex < visibleSortableEntities.Count)
            {
                bool didLoopInfinitely = true;
                for (int i = visibleLower; i < visibleUpper; i++)
                {
                    var entity = visibleSortableEntities[i];
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
                        sortedEntities[insertIndex++] = entity;
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
                    for (int i = 0; i < visibleSortableEntities.Count; i++)
                    {
                        T outerEntity = visibleSortableEntities[i];
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
                for (int i = 0; i < Math.Ceiling(totalEntityCount / 2f); i++)
                {
                    var swap = sortedEntities[totalEntityCount - 1 - i];
                    sortedEntities[totalEntityCount - 1 - i] = sortedEntities[i];
                    sortedEntities[i] = swap;
                }
                for (int i = 0; i < visibleDummyEntities.Count; i++)
                {
                    sortedEntities[i] = visibleDummyEntities[i];
                }
            }
            else
            {
                for (int i = 0; i < visibleDummyEntities.Count; i++)
                {
                    sortedEntities[visibleSortableEntities.Count + i] = visibleDummyEntities[i];
                }
            }
            hashSetPool.Return(sortedLookup);
            visibleSortableEntities.Dispose();
            visibleDummyEntities.Dispose();
            return sortedEntities;
        }

        public void PostDraw()
        {
            UpdateActiveEntities();
            var postDrawableEntities = GetEntitiesOfType<IPostDraw>();
            for (int i = 0; i < postDrawableEntities.Count; i++)
            {
                postDrawableEntities[i].PostDraw();
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

        public DisposableList<TActor> GetEntitiesOfType<TActor>()
        {
            cachedTypeToEntities.TryGetValue(typeof(TActor), out object maybeList);
            if (maybeList is DisposableList<TActor> cachedList && !cachedList.IsDisposed)
            {
                return cachedList;
            }

            DisposableList<TActor> filteredEntities = DisposableList<TActor>.Shared.Rent().Initialize(entities.Count);
            foreach (var entity in entities)
            {
                if (entity is TActor actor)
                {
                    filteredEntities.Add(actor);
                }
            }
            cachedTypeToEntities[typeof(TActor)] = filteredEntities;
            return filteredEntities;
        }

        public DisposableList<TActor> GetEntitiesOfType<TActor>(Func<TActor, bool> Predicate)
        {
            DisposableList<TActor> filteredEntities = DisposableList<TActor>.Shared.Rent().Initialize(entities.Count);
            foreach (var entity in entities)
            {
                if (entity is TActor actor && Predicate(actor))
                {
                    filteredEntities.Add(actor);
                }
            }
            return filteredEntities;
        }

        public List<TActor> GetEntitiesOfTypeSLOW<TActor>()
        {
            return entities.OfType<TActor>().ToList();
        }

        public TActor GetFirstEntityOfType<TActor>()
        {
            foreach (var entity in entities)
            {
                if (entity is TActor actor)
                {
                    return actor;
                }
            }
            return default;
        }

        public abstract void OnSceneStart();
        public abstract void OnSceneEnd();

        private void UpdateActiveEntities()
        {
            foreach (ISceneEntity entity in removedEntities)
            {
                invalidCacheKeys.Add(entity.GetType());
                entities.Remove(entity);
                if (entity is IDraw drawable)
                {
                    if (outboundEdges.ContainsKey(drawable))
                    {
                        hashSetPool.Return(outboundEdges[drawable]);
                        outboundEdges.Remove(drawable);
                    }
                    if (inboundEdges.ContainsKey(drawable))
                    {
                        hashSetPool.Return(inboundEdges[drawable]);
                        inboundEdges.Remove(drawable);
                    }
                }
            }
            removedEntities.Clear();
            foreach (ISceneEntity entity in pendingEntities)
            {
                if (entities.Contains(entity))
                {
                    continue;
                }
                invalidCacheKeys.Add(entity.GetType());
                entities.Add(entity);
                if (entity is IDraw drawable)
                {
                    outboundEdges[drawable] = hashSetPool.Rent();
                    inboundEdges[drawable] = hashSetPool.Rent();
                }
            }
            pendingEntities.Clear();
            foreach (Type invalidKey in invalidCacheKeys)
            {
                foreach (var Entry in cachedTypeToEntities)
                {
                    if (Entry.Key.IsAssignableFrom(invalidKey))
                    {
                        ((IDisposable)Entry.Value).Dispose();
                    }
                }
            }
            invalidCacheKeys.Clear();
        }

        public void FlushAllPending()
        {
            UpdateActiveEntities();
        }

        public virtual bool SceneContainsDrawableEntity<T>(T Entity) where T : ISceneEntity, IDraw
        {
            return entities.Contains(Entity) && Camera.ContainsBox(Entity.GetBackingBox().B);
        }

        public virtual Effect GetCompositeEffect()
        {
            return null;
        }
    }
}
