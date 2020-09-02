namespace ProjectWarnerShared.Lib.Helpers
{
    public class DataStructureHelper
    {
        public static T[] CreateFilledArray<T>(int Size, T InitialValue)
        {
            T[] array = new T[Size];
            for (int i = 0; i < Size; i++)
            {
                array[i] = InitialValue;
            }
            return array;
        }
    }
}
