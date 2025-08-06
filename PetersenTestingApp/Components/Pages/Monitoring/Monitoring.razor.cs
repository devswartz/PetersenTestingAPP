using Microsoft.AspNetCore.Components;
using PetersenTestingAppLibrary.Classes;
using PetersenTestingAppLibrary.Services;

namespace PetersenTestingApp.Pages.Monitoring;


public partial class Monitoring : ComponentBase
{
    [Inject]
    public DashboardBackendService DashboardBackendService { get; set; }

    private List<SensorReading> liveReadings = new();

    protected override async Task OnInitializedAsync()
    {
        liveReadings = await DashboardBackendService.GetLatestSensorReadingsAsync();
    }

}

