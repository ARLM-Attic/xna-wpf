using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace XNAPF
{
    public static class RenderTargetHelper
    {
        #region Fields

        private static readonly MethodInfo m_GetRenderTargetSurface;
        private static readonly FieldInfo m_helper;

        #endregion

        static RenderTargetHelper()
        {
            m_helper = typeof(RenderTarget2D).GetField("helper", BindingFlags.Instance | BindingFlags.NonPublic);
            m_GetRenderTargetSurface = m_helper.FieldType.GetMethod("GetRenderTargetSurface", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static IntPtr GetPtr(this RenderTarget2D t)
        {
            Object ptr = m_GetRenderTargetSurface.Invoke(m_helper.GetValue(t), new object[] { CubeMapFace.PositiveY });
            unsafe
            {
                return new IntPtr(Pointer.Unbox(ptr));
            }
        }
    }
}
