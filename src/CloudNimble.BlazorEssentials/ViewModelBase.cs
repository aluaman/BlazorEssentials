﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace CloudNimble.BlazorEssentials
{

    /// <summary>
    /// A base class for your Blazor MVVM implementation that gives you access to all the useful stuff Blazor and BlazorEssentials inject into the app.
    /// </summary>
    /// <typeparam name="TAppState"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    public class ViewModelBase<TConfig, TAppState> : BlazorObservable
        where TConfig : ConfigurationBase
        where TAppState : AppStateBase
    {

        #region Properties

        /// <summary>
        /// A boolean signifying whether or not Anonymous users can access the current Page. Defaults to true.
        /// </summary>
        public bool AllowAnonymous { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        public List<string> AllowedRoles { get; set; }

        /// <summary>
        /// The injected <see cref="AppStateBase"/> instance for the ViewModel.
        /// </summary>
        public TAppState AppState { get; internal set; }

        /// <summary>
        /// The injected <see cref="ConfigurationBase"/> instance for the ViewModel. 
        /// </summary>
        public TConfig Configuration { get; internal set; }

        /// <summary>
        /// Allows you to set any additional filtering criteria for this ViewModels' HTTP requests from inside the Page itself.
        /// </summary>
        public string FilterCriteria { get; set; }

        /// <summary>
        /// The injected <see cref="HttpClient"/> instance for the ViewModel.
        /// </summary>
        public HttpClient HttpClient { get; internal set; }

        /// <summary>
        /// A boolean signifying whether or not the <see cref="AppState.CurrentUser"/> is authorized to see the Page, given the <see cref="AllowAnonymous"/> and <see cref="AllowedRoles"/> criteria.
        /// </summary>
        public bool IsAuthorized
        {
            //RWM: If AllowAnonymous, authorized. Otherwise, if you're signed in but no roles are specified, authorized. Otherwise, ff you're signed in and you have any of the roles, authorized.
            get => AllowAnonymous ? true : AppState.IsSignedIn && AllowedRoles.Count == 0 ? true : AppState.IsSignedIn && AllowedRoles.Select(c => AppState.CurrentUser.IsInRole(c)).Any(c => c == true);
        }

        /// <summary>
        /// A boolean signifying whether or not the ViewModel has loaded the required data.
        /// </summary>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// A boolean signifying whether or not the ViewModel is presently in the middle of loading the required data.
        /// </summary>
        public bool IsLoading { get; set; }

        /// <summary>
        /// The injected <see cref="NavigationManager"/> instance for the ViewModel.
        /// </summary>
        public NavigationManager NavigationManager { get; internal set; }

        /// <summary>
        /// Allows the Blazor page to pass the StateHasChanged function back to the ViewModels so ViewModel operations can trigger state chamges.
        /// </summary>
        public Action StateHasChangedAction { get; set; } = () => { };

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ViewModelBase"/>.
        /// </summary>
        /// <param name="navigationManager">The <see cref="IUriHelper"/> instance injected from the DI container.</param>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance injected from the DI container.</param>
        /// <param name="configuration">The <typeparamref name="TConfig"/> instance injected from the DI container.</param>
        /// <param name="appState">The <typeparamref name="TAppState"/> instance injected from the DI container.</param>
        public ViewModelBase(NavigationManager navigationManager, HttpClient httpClient, TConfig configuration = null, TAppState appState = null)
        {
            NavigationManager = navigationManager;
            HttpClient = httpClient;
            Configuration = configuration;
            AppState = appState ?? (TAppState)new AppStateBase(navigationManager);
            AllowedRoles = new List<string>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the specified roles to the <see cref="AllowedRoles"/> list if they are not already present, and disabled Anonymous access.
        /// </summary>
        /// <param name="roles">A comma-separated list of roles to add to the <see cref="AllowedRoles"/>.</param>
        /// <remarks>
        /// If you want to leave Anonymous access on and just light up new features, then add to the <see cref="AllowedRoles"/> list manually.
        /// </remarks>
        public void AddRoles(params string[] roles)
        {
            // RWM: Add any roles that aren't already in the list.
            AllowedRoles.AddRange(roles.Where(c => !AllowedRoles.Contains(c)));
            AllowAnonymous = false;
        }

        /// <summary>
        /// Checks the <see cref="AppState.CurrentUser"/> authorization and boots the user to the Unauthorized page if required.
        /// </summary>
        public void Authorize()
        {
            if (Configuration == null)
            {
                throw new ArgumentNullException("configuration", "You must add a Configuration section to the DI container if you want to use Authorization.");
            }

            if (!IsAuthorized)
            {
                //RWM: If we're not signed in at all, go to the sign in prompt.
                if (!AppState.IsSignedIn) AppState.SignIn();
                //RWM: Otherwise, you're signed in but not allowed to see it. Redirect.
                NavigationManager.NavigateTo(Configuration.UnauthorizedRedirectUrl);
            }
        }

        #endregion

    }

}