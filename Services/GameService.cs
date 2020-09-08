using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WarnerEngine.Services
{
    public class GameService
    {
        private static Dictionary<Type, IService> services;
        private static List<IService> orderedServices;

        static GameService()
        {
            services = new Dictionary<Type, IService>();
        }

        public static void Initialize()
        {
            RegisterService(new EventService());
            RegisterService(new InputService());
            RegisterService(new TerminalService());
            RegisterService(new InventoryService());
            RegisterService(new SceneService());
            RegisterService(new ActionService());
            RegisterService(new DialogService());
            RegisterService(new RenderService());
            RegisterService(new AudioService());
            RegisterService(new ContentService());
            RegisterService(new LootService());
            RegisterService(new StateService());
            orderedServices = DetermineTopologicalOrdering();
            InitializeRegisteredServices();
        }

        public static void InitializeTest()
        {
            RegisterService(new EventService());
            RegisterService(new TestInputService());
            RegisterService(new TerminalService());
            RegisterService(new InventoryService());
            RegisterService(new SceneService());
            RegisterService(new ActionService());
            RegisterService(new DialogService());
            RegisterService(new RenderService());
            RegisterService(new AudioService());
            RegisterService(new TestContentService());
            RegisterService(new LootService());
            RegisterService(new StateService());
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

        private static void RegisterService<T>(T Service) where T : IService
        {
            services[Service.GetBackingInterfaceType()] = Service;
        }

        public static T GetService<T>() where T : IService
        {
            return (T)services[typeof(T)];
        }

        public static void PreDraw(float DT)
        {
            foreach (IService service in orderedServices)
            {
                service.PreDraw(DT);
            }
        }

        public static void Draw()
        {
            RenderService rs = GetService<RenderService>();
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
                .SetRenderTarget(RenderService.FINAL_TARGET_KEY, Color.Black)
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
        }

        public static void PostDraw()
        {
            foreach (IService service in orderedServices)
            {
                service.PostDraw();
            }
        }
    }
}
