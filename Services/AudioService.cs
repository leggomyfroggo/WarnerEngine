using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

using WarnerEngine.Lib.Components;

namespace WarnerEngine.Services
{
    public class AudioService : Service
    {
        public const float DISTANCE_LIMIT = 300f;
        private List<(SoundEffectInstance, Vector3)> queue;

        public BackingBox TrackedObject { get; private set; }

        public AudioService()
        {
            SoundEffect.DistanceScale = 100;
            queue = new List<(SoundEffectInstance, Vector3)>();
            TrackedObject = BackingBox.Dummy;
        }

        public override void PreDraw(float DT)
        {
            if (TrackedObject == null)
            {
                return;
            }
            Vector3 trackedObjectPos = TrackedObject.GetCenterPoint();
            for (int i = 0; i < queue.Count; )
            {
                if (queue[i].Item1.State != SoundState.Playing)
                {
                    queue.RemoveAt(i);
                    continue;
                }
                queue[i].Item1.Volume = MathHelper.Clamp((DISTANCE_LIMIT - Vector3.Distance(trackedObjectPos, queue[i].Item2)) / DISTANCE_LIMIT, 0, 1);
                i++;
            }
        }

        public AudioService SetTrackedObject(BackingBox TrackedObject)
        {
            this.TrackedObject = TrackedObject;
            return this;
        }

        public AudioService PlaySoundEffect(string Key, Vector3? SourcePosition = null)
        {
            Random rand = GameService.GetService<StateService>().GetGlobalRandom();
            SoundEffectInstance instance = GameService.GetService<IContentService>().GetSoundEffect(Key)?.CreateInstance();
            // TODO: Make this error out, but provide a test implementation of AudioService
            if (instance != null)
            {
                Vector3 sourcePosition = SourcePosition.HasValue ? SourcePosition.Value : TrackedObject.GetCenterPoint();
                //instance.Pitch = ((float)rand.NextDouble() - 0.5f) * 2f * 0.05f;
                instance.Play();
                queue.Add((instance, sourcePosition));
            }
            return this;
        }

        public override Type GetBackingInterfaceType()
        {
            return typeof(AudioService);
        }
    }
}
