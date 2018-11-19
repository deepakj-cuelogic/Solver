using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    public static class corecrt_wstdio
    {
        // changed from 'FILE stdout' to 'FileStream stdout' FIX_4064d4fc-ad5f-457e-959a-95218bb099e1 19/11/18
        public const FileStream stdout = null;
        public const FILE stdin = null;
        public const FILE stderr = null;
    }
}
