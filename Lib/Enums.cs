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
    }
}
