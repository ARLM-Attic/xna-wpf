using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XnaGame;
using XNAPF;

namespace TestSample.Game
{
    public class GameProxy : Game1, IXnapfGame
    {
        public void IntializeData()
        {
            base.Initialize();
        }

        public void Resize(double width, double height)
        {
            base.graphics.PreferredBackBufferWidth = (int)width;
            base.graphics.PreferredBackBufferHeight = (int)height;
            base.graphics.ApplyChanges();
        }
    }
}
