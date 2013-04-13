using System;

namespace ImageShrinker2.Framework
{
    interface ICloseInteraction
    {
        bool OnClose();
        event EventHandler RequestClose;
    }
}
