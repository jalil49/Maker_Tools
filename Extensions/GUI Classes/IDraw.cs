using System;

namespace Extensions.GUI_Classes
{
    internal interface IDraw<T>
    {
        void Draw(Action<T> action);
    }
}
