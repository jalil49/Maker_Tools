using System;
using System.Collections.Generic;
using System.Text;

namespace Extensions.GUI_Classes
{
    internal interface IDraw<T>
    {
        void Draw(Action<T> action);
    }
}
