using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectWarnerShared.Services
{
    public class Events
    {
        // Rendering events
        public const string GRAPHICS_DEVICE_INITIALIZED = "rs_gdi";
        public const string INTERNAL_RESOLUTION_CHANGED = "rs_irc";
        public const string AREA_TRAVERSAL_PORTAL_ENTERED = "scene_t_ap_enter";
        public const string AREA_TRAVERSAL_BEGIN = "scene_at_begin";
        public const string AREA_TRAVERSAL_SWAP = "scene_at_swap";
        public const string AREA_TRAVERSAL_END = "scene_at_complete";
    }
}
