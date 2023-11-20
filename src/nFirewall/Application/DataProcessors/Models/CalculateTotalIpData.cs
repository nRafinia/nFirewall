namespace nFirewall.Application.DataProcessors.Models;

public class CalculateTotalIpData
{
    public long Start { get; } = DateTime.Now.Ticks;
    public long End { get; set; } = DateTime.Now.Ticks;
    public long Total { get; set; } = 1;
    
    public CalculateTotalIpData Increase()
    {
        Total++;
        End = DateTime.Now.Ticks;
        return this;
    }
}