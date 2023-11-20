namespace nFirewall.Application.DataProcessors.Models;

public class CalculateSlowestUrlData
{
    public long Time { get; private set; } = DateTime.Now.Ticks;
    public long Duration { get; private set; } = 1;

    public CalculateSlowestUrlData Set(long duration)
    {
        if (duration <= Duration)
        {
            return this;
        }
        
        Duration = duration;
        Time = DateTime.Now.Ticks;

        return this;
    }
}