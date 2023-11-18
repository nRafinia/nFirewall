namespace nFirewall.Domain.Models;

public class FirewallSetting
{
    public int ThreadCount { get; set; } = Consts.ThreadCount;
    public List<string> WhiteList { get; set; } = new();
    public List<string> BlackList { get; set; } = new();
    public string ReportPath { get; set; } = Consts.GetReportPath;
}