using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TiledBitmapGen.Models;

namespace TiledBitmapGen.Service
{
    internal partial class NativeUtility
    {
        [DllImport("TildBitMapLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Create(Config config);

        [LibraryImport("TildBitMapLib.dll", StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller))]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetImgInfo(string fileName, ref int nChannel, ref int bitDepth);
    }
}
