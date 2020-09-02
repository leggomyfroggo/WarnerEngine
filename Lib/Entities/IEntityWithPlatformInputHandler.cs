using ProjectWarnerShared.Input;

namespace ProjectWarnerShared.Lib.Entities
{
    public interface IEntityWithPlatformInputHandler
    {
        IPlatformInputHandler GetInputHandler();
    }
}
