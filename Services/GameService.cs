﻿using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WarnerEngine.Services
{
    public class GameService
    {
        public static class ServiceEvents
        {
            public const string PreDrawStart = "gse_pre_draw_start";
            public const string PreDrawEnd = "gse_pre_draw_end";

            public const string DrawStart = "gse_draw_start";
            public const string DrawEnd = "gse_draw_end";

            public const string PostDrawStart = "gse_post_draw_start";
            public const string PostDrawEnd = "gse_post_draw_end";
        }

        private static Dictionary<Type, IService> services;
        private static List<IService> orderedServices;

        static GameService()
        {
            services = new Dictionary<Type, IService>();
        }

        public static void Initialize()
        {
            RegisterService(new Implementations.EventService());
            RegisterService(new Implementations.InputService());
            RegisterService(new Implementations.ECSService());
            RegisterService(new Implementations.TerminalService());
            RegisterService(new Implementations.SceneService());
            RegisterService(new Implementations.NewSceneService());
            RegisterService(new Implementations.InteractionService());
            RegisterService(new Implementations.GraphicsService());
            RegisterService(new Implementations.RenderService());
            RegisterService(new Implementations.AudioService());
            RegisterService(new Implementations.ContentService());
            RegisterService(new Implementations.StateService());
            RegisterService(new Implementations.BindingService());
            orderedServices = DetermineTopologicalOrdering();
            InitializeRegisteredServices();
        }

        public static void InitializeTest()
        {
            RegisterService(new Implementations.EventService());
            RegisterService(new Implementations.Test.TestInputService());
            RegisterService(new Implementations.TerminalService());
            RegisterService(new Implementations.SceneService());
            RegisterService(new Implementations.InteractionService());
            RegisterService(new Implementations.GraphicsService());
            RegisterService(new Implementations.RenderService());
            RegisterService(new Implementations.AudioService());
            RegisterService(new Implementations.Test.TestContentService());
            RegisterService(new Implementations.StateService());
            RegisterService(new Implementations.BindingService());
            orderedServices = DetermineTopologicalOrdering();
            InitializeRegisteredServices();
        }

        private static List<IService> DetermineTopologicalOrdering()
        {
            // Build the dependency graph from the registered services
            Dictionary<Type, HashSet<Type>> upstream = new Dictionary<Type, HashSet<Type>>();
            Dictionary<Type, HashSet<Type>> downstream = new Dictionary<Type, HashSet<Type>>();
            foreach (KeyValuePair<Type, IService> registeredService in services)
            {
                Type serviceKey = registeredService.Key;
                IService service = registeredService.Value;

                if (!upstream.ContainsKey(serviceKey))
                {
                    upstream[registeredService.Key] = new HashSet<Type>() { };
                }

                foreach (Type dependency in service.GetDependencies())
                {
                    upstream[serviceKey].Add(dependency);
                    if (!downstream.ContainsKey(dependency))
                    {
                        downstream[dependency] = new HashSet<Type>() { };
                    }
                    downstream[dependency].Add(serviceKey);
                }
            }

            // Topologically sort to find the order in which to initialize dependencies
            List<IService> orderedServices = new List<IService>();
            while (upstream.Count > 0)
            {
                foreach (Type serviceKey in services.Keys)
                {
                    if (!upstream.ContainsKey(serviceKey))
                    {
                        continue;
                    }
                    var upstreamDependencies = upstream[serviceKey];
                    if (upstreamDependencies.Count > 0)
                    {
                        continue;
                    }
                    if (downstream.ContainsKey(serviceKey))
                    {
                        foreach (var downstreamDependency in downstream[serviceKey])
                        {
                            upstream[downstreamDependency].Remove(serviceKey);
                        }
                    }
                    orderedServices.Add(services[serviceKey]);
                    upstream.Remove(serviceKey);
                }
            }
            return orderedServices;
        }

        private static void InitializeRegisteredServices()
        {
            foreach (IService service in orderedServices)
            {
                service.Initialize();
            }
        }

        public static void RegisterService<T>(T Service) where T : IService
        {
            services[Service.GetBackingInterfaceType()] = Service;
        }

        public static T GetService<T>() where T : IService
        {
            return (T)services[typeof(T)];
        }

        public static void PreDraw(float DT)
        {
            GetService<IEventService>().Notify(ServiceEvents.PreDrawStart);
            foreach (IService service in orderedServices)
            {
                service.PreDraw(DT);
            }
            GetService<IEventService>().Notify(ServiceEvents.PreDrawEnd);
        }

        public static void Draw()
        {
            GetService<IEventService>().Notify(ServiceEvents.DrawStart);
            IRenderService rs = GetService<IRenderService>();
            List<ServiceCompositionMetadata> outputs = new List<ServiceCompositionMetadata>();
            foreach (IService service in orderedServices)
            {
                ServiceCompositionMetadata output = service.Draw();
                if (rs.IsDrawing)
                {
                    throw new Exception("Service finished drawing without calling End()");
                }
                if (output != ServiceCompositionMetadata.Empty)
                {
                    outputs.Add(output);
                }
            }
            var sortedOutputs = outputs.OrderBy(output => output.Priority);

            rs
                .SetRenderTarget(Implementations.RenderService.FINAL_TARGET_KEY, Color.Black)
                .SetDepthStencilState(DepthStencilState.None)
                .Start()
                .Render(() => {
                     foreach (ServiceCompositionMetadata output in sortedOutputs)
                     {
                        if (output.CompositeEffect != rs.GetEffect())
                        {
                            rs
                                .End()
                                .SetEffect(output.CompositeEffect)
                                .Start();
                        }
                        rs.DrawTargetAtPosition(output.RenderTargetKey, output.Position, Tint: output.Tint);
                     }
                 })
                .End()
                .SetEffect(null)
                .Start()
                .StretchCurrentTargetToBackBuffer(ShouldLetterbox: true)
                .End();
            GetService<IEventService>().Notify(ServiceEvents.DrawEnd);
        }

        public static void PostDraw()
        {
            GetService<IEventService>().Notify(ServiceEvents.PostDrawStart);
            foreach (IService service in orderedServices)
            {
                service.PostDraw();
            }
            GetService<IEventService>().Notify(ServiceEvents.PostDrawEnd);
        }
    }
}
