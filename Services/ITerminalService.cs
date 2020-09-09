using System;
using System.Collections.Generic;
using System.Text;

namespace WarnerEngine.Services
{
    public interface ITerminalService : IService
    {
        bool IsActive { get; }

        string GetCurrentCommandForDraw();
    }
}
