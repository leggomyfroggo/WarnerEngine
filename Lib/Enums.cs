using System;
using System.Collections.Generic;

namespace WarnerEngine.Lib
{
    public class Enums
    {
        private static string[] directionNames = new string[] {"right", "down", "left", "up"};
        public enum Direction { right, down, left, up };
        public static string DirectionNameFromEnum(Direction Dir)
        {
            return directionNames[(int)Dir];
        }

        public enum LocaleCode { en_US, ja_JP };

        [Flags]
        public enum ShadowCasterClass : short 
        { 
            None = 0,
            Static = 1, 
            DynamicLarge = 2, 
            DynamicSmall = 4
        }
    }
}
