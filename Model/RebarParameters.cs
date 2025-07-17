using RevitTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PunchingFoundRebarModule.Model
{
    internal enum RebarLocation
    {
        OnFoundation,
        OnPlate
    }

    internal class RebarParameters
    {
        internal double RebarDiameter { get; set; }
        internal RebarClass RebarClass { get; set; }
        internal double StirrupStep { get; set; }
        internal double FrameStep { get; set; }
        internal RebarLocation RebarLocation { get; set; }
        internal double BackRebarDiameter { get; set; }
        internal bool IsRebarCoverFromModel { get; set; }
        internal double RebarCoverDown {  get; set; }
        internal double RebarCoverUp {  get; set; }

        internal RebarParameters() { }
    }
}
