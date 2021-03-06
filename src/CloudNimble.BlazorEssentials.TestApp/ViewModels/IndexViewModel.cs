﻿using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Security.Claims;

namespace CloudNimble.BlazorEssentials.TestApp.ViewModels
{
    public class IndexViewModel : ViewModelBase<ConfigurationBase, AppStateBase>
    {

        #region Properties

        public string LoginStatus
        {
            get => $"Signed In: {AppState.IsSignedIn} {(AppState.IsSignedIn ? "| Expires: " + AppState.CurrentUser.FindFirst(ClaimTypes.Expiration).Value : "")}";
        }

        #endregion


        #region Constructors

        public IndexViewModel(ConfigurationBase configuration, AppStateBase appState, NavigationManager navigationManager, HttpClient httpClient) : base(navigationManager, httpClient, configuration, appState)
        {
        }

        #endregion

    }

}