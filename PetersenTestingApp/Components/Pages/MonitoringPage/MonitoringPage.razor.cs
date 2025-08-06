using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Blazorise.Charts;
using PetersenTestingAppLibrary.Classes;
using PetersenTestingAppLibrary.Services;

namespace PetersenTestingApp.Components.Pages.MonitoringPage;

public partial class MonitoringPage : ComponentBase
{
    private List<SensorReading> liveReadings = new();
    private List<SensorReading> plotData = new();
    private LineChart<double> chart;

    private LineChartOptions chartOptions = new()
    {
        Responsive = true,
        Plugins = new()
        {
            Title = new()
            {
                Display = true,
                Text = "Pressure Over Time"
            }
        }
    };

    private PlotQuery query = new()
    {
        StartDate = DateTime.UtcNow.AddHours(-12),
        EndDate = DateTime.UtcNow
    };

    protected override async Task OnInitializedAsync()
    {
        var readings = await DashboardBackendService.GetLatestSensorReadingsAsync();

        var cutoff = DateTime.UtcNow.AddHours(-12);

        var recent = readings
            .Where(r => r.TimeStamp >= cutoff)
            .OrderByDescending(r => r.TimeStamp)
            .ToList();

        if (recent.Count < 5)
        {
            liveReadings = readings
                .OrderByDescending(r => r.TimeStamp)
                .Take(5)
                .ToList();
        }
        else
        {
            liveReadings = recent;
        }
    }

    private async Task OnQuerySubmit()
    {
        plotData = await DashboardBackendService
            .GetSensorReadingsForDateRangeAsync(query.SensorId, query.StartDate, query.EndDate);

        StateHasChanged();
        await Task.Delay(50);

        if (chart != null)
        {
            await chart.Clear();

            var labels = plotData.Select(p => p.TimeStamp.ToString("s")).ToList();
            var values = plotData.Select(p => (double)p.PressurePSI).ToList();

            await chart.AddLabelsDatasetsAndUpdate(
                labels,
                new LineChartDataset<double>
                {
                    Label = "Pressure (PSI)",
                    Data = values,
                    Fill = false,
                    BackgroundColor = "#007bff",
                    BorderWidth = 2
                });
        }
    }

    public class PlotQuery
    {
        public string SensorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}