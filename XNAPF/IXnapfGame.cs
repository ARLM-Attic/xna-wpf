using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNAPF
{
    public interface IXnapfGame
    {
        void IntializeData();
        void Resize(double width, double height);
    }
}
