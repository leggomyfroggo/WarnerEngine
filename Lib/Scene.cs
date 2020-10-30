using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Resolvers;
using Microsoft.Xna.Framework.Graphics;

using WarnerEngine.Lib.Components;
using WarnerEngine.Lib.Entities;
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
                using (var preDrawableEntities = GetEntitiesOfType<IPreDraw>())
                {
                    for (int i = 0; i < preDrawableEntities.Count; i++)
                    {
                        preDrawableEntities[i].PreDraw(DT);
                    }
                }
            }
        }

        public virtual void Draw()
        {
            IDraw[] sortedEntities = GetSortedEntities<IDraw>(true);
            int currentIndex = 0;
            float totalEntities = sortedEntities.Length;
            foreach (IDraw entity in sortedEntities)
            {
                if (entity == null)
                {
                    break;
                }
                float depth = (currentIndex++) / totalEntities;
                GameService.GetService<IRenderService>().SetDepth(depth);
                entity.Draw();
            }
            ArrayPool<IDraw>.Shared.Return(sortedEntities);
        }

        public T[] GetSortedEntities<T>(bool ShouldReverse = false) where T : class, IDraw
        {
            DisposableArray<T> visibleSortableEntities = GetEntitiesOfType<T>(
                e => e.IsVisible() && e.GetBackingBox() != BackingBox.Dummy && Camera.ContainsBox(e.GetBackingBox().B)
            );
            DisposableArray<T> visibleDummyEntities = GetEntitiesOfType<T>(
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
            T[] sortedEntities = ArrayPool<T>.Shared.Rent(totalEntityCount);
            HashSet<T> sortedLookup = new HashSet<T>();
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
                for (int i = 0; i < Math.Floor(totalEntityCount / 2f); i++)
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
            visibleSortableEntities.Dispose();
            visibleDummyEntities.Dispose();
            return sortedEntities;
        }

        public void PostDraw()
        {
            UpdateActiveEntities();
            using (var postDrawableEntities = GetEntitiesOfType<IPostDraw>())
            {
                for (int i = 0; i < postDrawableEntities.Count; i++)
                {
                    postDrawableEntities[i].PostDraw();
                }
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

        public DisposableArray<TActor> GetEntitiesOfType<TActor>()
        {
            DisposableArray<TActor> filteredEntities = DisposableArray<TActor>.Shared.Rent().Initialize(entities.Count);
            foreach (var entity in entities)
            {
                if (entity is TActor actor)
                {
                    filteredEntities.Add(actor);
                }
            }
            return filteredEntities;
        }

        public DisposableArray<TActor> GetEntitiesOfType<TActor>(Func<TActor, bool> Predicate)
        {
            DisposableArray<TActor> filteredEntities = DisposableArray<TActor>.Shared.Rent().Initialize(entities.Count);
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
                entities.Remove(entity);
                if (entity is IDraw drawable)
                {
                    outboundEdges.Remove(drawable);
                    inboundEdges.Remove(drawable);
                }
            }
            removedEntities.Clear();
            foreach (ISceneEntity entity in pendingEntities)
            {
                if (entities.Contains(entity))
                {
                    continue;
                }
                entities.Add(entity);
                if (entity is IDraw drawable)
                {
                    outboundEdges[drawable] = new HashSet<IDraw>();
                    inboundEdges[drawable] = new HashSet<IDraw>();
                }
            }
            pendingEntities.Clear();
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
