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
        public string fileName;
        public int tileSize;
        public int tileBorderWidth;
        [MarshalAs(UnmanagedType.U1)]
        public bool isHeightmap;
        [MarshalAs(UnmanagedType.U1)]
        public bool createNormalmap;
        public float minHeight;
        public float maxHeight;
        public int leafQuadTreeNodeSize;
        public int lodLevelCount;
    }
}
