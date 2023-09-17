using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TiledBitmapGen.Models;

namespace TiledBitmapGen.Service
{
    internal class NativeUtility
    {
        [DllImport("TildBitMapLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Create(Config config);

        [DllImport("TildBitMapLib.dll", CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        public static extern bool GetImgInfo(string fileName, ref int nChannel, ref int bitDepth);
    }
}
