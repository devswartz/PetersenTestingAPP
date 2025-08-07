using Microsoft.AspNetCore.Components;
using PetersenTestingAppLibrary.Classes;

namespace PetersenTestingApp.Components.Monitoring;

public partial class LiveSensorCard : ComponentBase
{
    [Parameter]
    public SensorReading Reading { get; set; } = default!;
}
