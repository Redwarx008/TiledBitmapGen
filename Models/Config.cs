using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TiledBitmapGen.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Config
    {
        public string FileName;
        public int TileSize;
        public bool IsHeightmap;
        public float MinHeight;
        public float MaxHeight;
    }
}
