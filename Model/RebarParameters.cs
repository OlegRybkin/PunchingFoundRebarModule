namespace PunchingFoundRebarModule.Model
{
    internal class RebarParameters
    {
        internal string FamilyName { get; set; }
        internal string FamilyType { get; set; }

        internal double RebarDiameter { get; set; }
        internal int RebarClass { get; set; }
        internal double StirrupStep { get; set; }
        internal double FrameWidth { get; set; }

        internal double BackRebarDiameter { get; set; }
        internal bool IsRebarCoverFromModel { get; set; }
        internal double RebarCoverDown {  get; set; }
        internal double RebarCoverUp {  get; set; }

        internal RebarParameters() { }
    }
}
