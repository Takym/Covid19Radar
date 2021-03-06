﻿using System;
using System.Threading.Tasks;
using Covid19Radar.Common;
using Covid19Radar.Model;
using Covid19Radar.Services.Logs;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Covid19Radar.Services
{
	public interface IUserDataService
	{
		public event EventHandler<UserDataModel>? UserDataChanged;
		public       ValueTask<bool>              RegisterUserAsync(UserDataModel userData);
		public       UserDataModel                Get();
		public       ValueTask                    SetAsync(UserDataModel userData);
		public       ValueTask                    ResetAllDataAsync();
	}

	/// <summary>
	///  This service registers, retrieves, stores, and automatically updates user data.
	/// </summary>
	public class UserDataService : IUserDataService
	{
		private readonly ILoggerService               _logger;
		private readonly IHttpDataService             _http_data;
		private          UserDataModel?               _current;
		public  event    EventHandler<UserDataModel>? UserDataChanged;

		public UserDataService(IHttpDataService httpDataService, ILoggerService logger)
		{
			_logger    = logger;
			_http_data = httpDataService;
		}

		public async ValueTask<bool> RegisterUserAsync(UserDataModel userData)
		{
			_logger.StartMethod();
			if (userData is null) {
				throw new ArgumentNullException(nameof(userData));
			}
			_logger.Info("The user data is not null.");
			if (await _http_data.PostRegisterUserAsync(userData)) {
				userData.StartDateTime                 = DateTime.UtcNow;
				userData.IsExposureNotificationEnabled = false;
				userData.IsNotificationEnabled         = false;
				userData.IsOptined                     = false;
				userData.IsPolicyAccepted              = false;
				userData.IsPositived                   = false;
				await this.SetAsync(userData);
				_logger.EndMethod();
				return true;
			} else {
				_logger.Warning("Failed to register an user.");
				_logger.EndMethod();
				return false;
			}
		}

		public UserDataModel Get()
		{
			_logger.StartMethod();
			if (_current is null) {
				if (Application.Current.Properties.TryGetValue(AppConstants.StorageKey.UserData, out object config)) {
					_logger.Info("The user data exists.");
					_current = JsonConvert.DeserializeObject<UserDataModel>(config.ToString());
				} else {
					_logger.Warning("The user data does not exists.");
					_logger.Info("Creating a new user data model instance...");
					_current = new UserDataModel();
				}
			}
			_logger.EndMethod();
			return _current;
		}

		public async ValueTask SetAsync(UserDataModel newdata)
		{
			_logger.StartMethod();
			if (newdata is null) {
				throw new ArgumentNullException(nameof(newdata));
			}
			string json = JsonConvert.SerializeObject(newdata);
			if (Application.Current.Properties.TryGetValue(AppConstants.StorageKey.UserData, out object config) &&
				json == config.ToString()) {
				_logger.Info("The new data is equal to the current data.");
				_logger.EndMethod();
				return;
			} else {
				_logger.Info("Saving the new data...");
				Application.Current.Properties[AppConstants.StorageKey.UserData] = json;
				await Application.Current.SavePropertiesAsync();
				_current = newdata;
				this.UserDataChanged?.Invoke(this, newdata);
				_logger.EndMethod();
			}
		}

		public async ValueTask ResetAllDataAsync()
		{
			_logger.StartMethod();
			Application.Current.Properties.Remove(AppConstants.StorageKey.UserData);
			await Application.Current.SavePropertiesAsync();
			_logger.EndMethod();
		}
	}
}
