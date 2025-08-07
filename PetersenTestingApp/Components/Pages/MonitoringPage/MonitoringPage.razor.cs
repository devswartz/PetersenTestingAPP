using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PetersenTestingAppLibrary.Classes;
using PetersenTestingAppLibrary.Services;
using Microsoft.JSInterop;

namespace PetersenTestingApp.Components.Pages.MonitoringPage;

public partial class MonitoringPage : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; }
    private List<SensorReading> liveReadings = new();
    private List<SensorReading> plotData = new();

    private PlotQuery query = new()
    {
        StartDate = DateTime.UtcNow.AddHours(-12),
        EndDate = DateTime.UtcNow
    };

    protected override async Task OnInitializedAsync()
    {
        liveReadings = await DashboardBackendService.GetLatestSensorReadingsAsync();
    }
    private async Task OnQuerySubmit()
    {
        plotData = await DashboardBackendService
            .GetSensorReadingsForDateRangeAsync(query.SensorId, query.StartDate, query.EndDate);

        if (plotData is null || !plotData.Any())
            return;

        await InvokeAsync(StateHasChanged); // ensures UI re-renders
    }

    public class PlotQuery
    {
        public string SensorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}
