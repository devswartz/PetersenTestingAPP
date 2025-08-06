using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PetersenTestingAppLibrary.Classes;
using PetersenTestingAppLibrary.Services;
using System.Text.RegularExpressions;
using Timer = System.Timers.Timer;

namespace PetersenTestingApp.Components.Pages.Testing
{
    public partial class Index : IDisposable
    {
        public bool Loaded { get; set; }
        public string errorMessage { get; set; }
        public PetersenUser? currentUser { get; set; }
        public List<TestHeader> TopListHeaders { get; set; }
        public List<TestHeader> AllListHeaders { get; set; }
        public List<TestHeader> FilteredHeaders { get; set; } = new List<TestHeader>();
        public List<TestDetail_InflationPressure> list_InflationPressures { get; set; } = new List<TestDetail_InflationPressure>();
        public bool CanTest { get; set; }
        public int? selectedHeader { get; set;}
        public string MoRefSearch { get; set; }
        public string ItemSearch { get; set; }
        public int StatusSearch { get; set; } = 2;
        public bool isTop { get; set; } = true;
        private const double DebounceIntervalMs = 1000;
        private Timer _debounceTimer { get; set; }
        public TestHeader currentTestObject { get; set; } = new TestHeader();
        public TestDetail_InflationPressure currentInflationPress { get; set; } = new TestDetail_InflationPressure();

        public List<PetersenUser> users = new List<PetersenUser>();

        public List<string> InflationMedium = new List<string>() { "AIR", "WATER", "WATER/N2", "AIR/WATER", "AIR/WATER/N2", "N2" };
        public List<string> TestStatus = new List<string>() { "Testing", "Pretesting", "Pass", "Fail" };
        public List<string> FailureReason = new List<string>() { "N/A", "ASSEMBLY",  "BLADDER", "BOLTS",  "COVER", "FABRIC PULLING", "FITTING", "SEW SEAM", "SLIPPAGE" , "SWAGE", "TESTING", "TEST FIX" };

        protected override async Task OnInitializedAsync()
        {
            Loaded = false;
            currentUser = await TestingService.GetUserInfo(userService.azureId);
            users = await TestingService.GetAllValidUser();
            users = users.OrderBy(x => x.Name).ToList();

            currentInflationPress.AUDUser = currentUser.UserId; //initialize AUDUser
            
            CanTest = currentUser.petersen_testing==1;
            if (!CanTest) {
                errorMessage = "Error in Disply";
                return;
            }
            else
            {
                await showTopData();
                

            }

            _debounceTimer = new Timer(DebounceIntervalMs)
            {
                AutoReset = false
            };
            _debounceTimer.Elapsed += async (_, _) => await DebouncedSearchAsync(); //procedure call makes all input searches

            Loaded = true;

        }


        private void OnSearchMoInput(ChangeEventArgs e)
        {
            MoRefSearch = e.Value?.ToString() ?? "";

            //reset timer
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void OnSearchItemInput(ChangeEventArgs e)
        {
            ItemSearch = e.Value?.ToString() ?? "";

            //reset timer
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void OnSearchStatus(ChangeEventArgs e)
        {
            StatusSearch = Convert.ToInt32(e.Value);

            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        public void SelectedHeader(int headerId) => selectedHeader = headerId;

        public async Task DebouncedSearchAsync()
        {
            isTop = false;
            FilteredHeaders = (await TestingService.GetFilteredHeaders(MoRefSearch, ItemSearch, StatusSearch)).OrderByDescending(h => h.HeaderId).ToList();
            await InvokeAsync(StateHasChanged);
        }

        public async Task showTopData()
        {
            TopListHeaders = await TestingService.GetTopTestHeader();
            MoRefSearch = string.Empty;
            ItemSearch = string.Empty;

            isTop = true;
        }

        public void Dispose()
        {
            _debounceTimer?.Dispose();
        }

        private TestHeader tempHeader;
        public async Task GetItemData(int HeaderId)
        {
            currentTestObject = await TestingService.GetItemdata(HeaderId);
            tempHeader = currentTestObject;
            list_InflationPressures = await TestingService.GetTestDetailInflationPressure(HeaderId);

        }

        public async Task ShowInflationDetail(int detailId)
        {
            currentInflationPress = await TestingService.GetItemInflationPressure(detailId);
        }

        public async Task OpenFlationDetail(TestHeader data)
        {
            currentInflationPress = new TestDetail_InflationPressure();
            if (data!=null)
            {
                currentInflationPress.HeaderID = data.HeaderId;
                currentInflationPress.ReferenceNumber = data.ReferenceNumber;
                currentInflationPress.ORDUNIQ = data.ORDUNIQ;
                currentInflationPress.Item = data.Item;
                currentInflationPress.AreaTempFahrenheit = 65;
                currentInflationPress.InflationMedium = InflationMedium[0];
                currentInflationPress.TestStatus = TestStatus[1];
                currentInflationPress.FailureReason = FailureReason[0];
                currentInflationPress.AUDUser = currentUser.UserId;
                string[] parts = data.RatedMaxInflationPSI.Split(new[] { "psi", "PSI" }, StringSplitOptions.None);
                if (parts.Length > 0) {
                    currentInflationPress.InflatedPSIG = int.Parse(parts[0].Trim());
                }
                else
                    currentInflationPress.InflatedPSIG = 0;
            }
        }


    }
}