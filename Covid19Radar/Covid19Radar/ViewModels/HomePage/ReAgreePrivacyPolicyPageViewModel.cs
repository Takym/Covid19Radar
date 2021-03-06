﻿using System;
using System.Threading.Tasks;
using Covid19Radar.Model;
using Covid19Radar.Resources;
using Covid19Radar.Services;
using Covid19Radar.Services.Logs;
using Covid19Radar.Views;
using Prism.Navigation;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Covid19Radar.ViewModels
{
	public class ReAgreePrivacyPolicyPageViewModel : ViewModelBase
	{
		private readonly ILoggerService      _logger;
		private readonly INavigationService  _ns;
		private readonly ITermsUpdateService _terms_update;
		private          DateTime            _update_dt;
		private          string?             _update_text;

		public string? UpdateText
		{
			get => _update_text;
			set => this.SetProperty(ref _update_text, value);
		}

		public Func<string, BrowserLaunchMode, Task> OpenBrowserAsync { get; set; }

		public Command OpenWebView => new(async () => {
			_logger.StartMethod();
			await this.OpenBrowserAsync(AppResources.UrlPrivacyPolicy, BrowserLaunchMode.SystemPreferred);
			_logger.EndMethod();
		});

		public Command OnClickReAgreeCommand => new(async () => {
			_logger.StartMethod();
			await _terms_update.SaveLastUpdateDateAsync(TermsType.PrivacyPolicy, _update_dt);
			await _ns.NavigateAsync("/" + nameof(MenuPage) + "/" + nameof(NavigationPage) + "/" + nameof(HomePage));
			_logger.EndMethod();
		});

		public ReAgreePrivacyPolicyPageViewModel(
			ILoggerService      logger,
			INavigationService  navigationService,
			ITermsUpdateService termsUpdate)
		{
			_logger               = logger            ?? throw new ArgumentNullException(nameof(logger));
			_ns                   = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
			_terms_update         = termsUpdate       ?? throw new ArgumentNullException(nameof(termsUpdate));
			this.OpenBrowserAsync = Browser.OpenAsync;
		}

		public override void Initialize(INavigationParameters parameters)
		{
			_logger.StartMethod();
			base.Initialize(parameters);
			var updateInfo  = ((TermsUpdateInfoModel.Detail)(parameters["updatePrivacyPolicyInfo"]));
			_update_dt      = updateInfo.UpdateDateTime;
			this.UpdateText = updateInfo.Text;
			_logger.EndMethod();
		}
	}
}
