using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using PetersenTestingAppLibrary.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace PetersenTestingAppLibrary.Services
{
    public class UserServiceMiddleware
    {
        private readonly RequestDelegate next;

        public UserServiceMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context, UserService service, BackendService backendService)
        {
            //service.SetUser(context.User);

            //backendService.Initialize(context.User.Claims.Where(e => e.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").First().Value);

            await next(context);
        }
    }


    public class UserService
    {
        public Utils utils {  get; set; }
        public UserService(Utils utils) { 
            this.utils = utils;
        }
        private ClaimsPrincipal currentUser = new(new ClaimsIdentity());
        public string Email { get; set; }
        public string Name { get; set; }
        public string azureId { get; set; }

        public ClaimsPrincipal GetUser()
        {
            return currentUser;
        }

        internal void SetUser(ClaimsPrincipal user) {
            if (currentUser != user)
            {
                currentUser = user;
                Email = currentUser.Identity.Name;
                Name = currentUser.GetDisplayName();
                azureId = user.Claims.Where(e => e.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").First().Value;

            }

        }

        public string GetUser_AzureId(string auth0_user_id)
        {

            string query = @"SELECT user_id FROM EMPLOYEE.dbo.EMPLOYEES WHERE auth0_user_id=@auth0_user_id";
            using (SqlConnection conn = new SqlConnection(utils.sqlServerConnection))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@auth0_user_id", auth0_user_id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader["user_id"].ToString();
                        }
                    }
                }
            }
            return auth0_user_id;
        }

    }

    public sealed class UserCircuitHandler : CircuitHandler, IDisposable
    {
        private readonly AuthenticationStateProvider authenticationStateProvider;
        private readonly UserService userService;
        private readonly BackendService backendService;

        public UserCircuitHandler(
            AuthenticationStateProvider authenticationStateProvider,
            UserService userService,
            BackendService backendService)
        {
            this.authenticationStateProvider = authenticationStateProvider;
            this.userService = userService;
            this.backendService = backendService;
        }

        public override Task OnCircuitOpenedAsync(Circuit circuit,
            CancellationToken cancellationToken)
        {
            authenticationStateProvider.AuthenticationStateChanged +=
                AuthenticationChanged;

            return base.OnCircuitOpenedAsync(circuit, cancellationToken);
        }

        private void AuthenticationChanged(Task<AuthenticationState> task)
        {
            _ = UpdateAuthentication(task);

            async Task UpdateAuthentication(Task<AuthenticationState> task)
            {
                try
                {
                    var state = await task;
                    userService.SetUser(state.User);

                    await backendService.Initialize(userService.azureId);
                }
                catch
                {
                }
            }
        }

        public override async Task OnConnectionUpAsync(Circuit circuit,
            CancellationToken cancellationToken)
        {
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            userService.SetUser(state.User);

            await backendService.Initialize(userService.azureId);
        }

        public void Dispose()
        {
            authenticationStateProvider.AuthenticationStateChanged -=
                AuthenticationChanged;
        }





    }


}
