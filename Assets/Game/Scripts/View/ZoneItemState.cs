namespace CriticalSpin.View
{
    /// <summary>
    /// An enum I made to easily track the 9 possible color states of a zone box.
    /// </summary>
    public enum ZoneItemState
    {
        NormalPassed,
        NormalCurrent,
        NormalUpcoming,
        SafePassed,
        SafeCurrent,
        SafeUpcoming,
        SuperPassed,
        SuperCurrent,
        SuperUpcoming
    }
}
