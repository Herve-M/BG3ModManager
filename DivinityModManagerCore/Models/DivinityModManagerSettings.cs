﻿using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Windows.Input;
using DivinityModManager.Util;
using System.Reactive.Disposables;
using System.Reflection;
using Alphaleonis.Win32.Filesystem;
using DivinityModManager.Models.App;
using System.Reactive;
using ReactiveUI.Fody.Helpers;

namespace DivinityModManager.Models
{
	[DataContract]
	public class DivinityModManagerSettings : ReactiveObject, IDisposable
	{
		[SettingsEntry("Game Data Path", "The path to the Data folder, for loading editor mods.\nExample: Baldur's Gate 3/Data")]

		[DataMember][Reactive]
		public string GameDataPath { get; set; } = "";

		[SettingsEntry("Game Executable Path", "The path to bg3.exe.")]
		[DataMember][Reactive]
		public string GameExecutablePath { get; set; } = "";

		[SettingsEntry("Enable Story Log", "When launching the game, enable the Osiris story log (osiris.log).")]
		[DataMember][Reactive] public bool GameStoryLogEnabled { get; set; } = false;

		[SettingsEntry("Always Disable Telemetry", "If enabled, Larian's telemetry (data gathering for early access) for BG3 will always be disabled, regardless of active mods. Telemetry is always disabled if mods are active.")]
		[DataMember][Reactive] public bool TelemetryDisabled { get; set; } = false;

		[SettingsEntry("Enable DirectX 11 Mode", "If enabled, when launching the game, bg3_dx11.exe is used instead.")]
		[DataMember][Reactive] public bool LaunchDX11 { get; set; } = false;

		[SettingsEntry("Workshop Path", "The workshop folder.&#x0a;Used for detecting mod updates and new mods to be copied into the local mods folder.")]
		[DataMember][Reactive] public string WorkshopPath { get; set; } = "";

		[SettingsEntry("Saved Load Orders Path", "The folder containing mod load orders.")]
		[DataMember][Reactive] public string LoadOrderPath { get; set; } = "Orders";

		[SettingsEntry("Enable Internal Log", "Enable the log for the mod manager.")]
		[DataMember][Reactive] public bool LogEnabled { get; set; } = false;

		[SettingsEntry("Enable Automatic Updates", "Automatically check for updates when the program starts.")]
		[DataMember][Reactive] public bool CheckForUpdates { get; set; } = true;

		[SettingsEntry("Add Dependencies When Exporting", "Automatically add dependency mods above their dependents in the exported load order, if omitted from the active order.")]
		[DataMember][Reactive] public bool AutoAddDependenciesWhenExporting { get; set; } = true;

		[SettingsEntry("Disable Missing Mod Warnings", "If a load order is missing mods, no warnings will be displayed.")]
		[DataMember][Reactive] public bool DisableMissingModWarnings { get; set; } = false;

		[SettingsEntry("Shift Focus on Swap", "When moving selected mods to the opposite list with Enter, move focus to that list as well.")]
		[DataMember][Reactive] public bool ShiftListFocusOnSwap { get; set; } = false;

		//[SettingsEntry("Disable Checking for Steam Workshop Tags", "The mod manager will try and find mod tags from the workshop by default.")]
		[DataMember][Reactive] public bool DisableWorkshopTagCheck { get; set; } = false;

		//[SettingsEntry("Automatically Load GM Campaign Mods", "When a GM campaign is selected, its dependency mods will automatically be loaded without needing to manually import them.")]
		[Reactive] public bool AutomaticallyLoadGMCampaignMods { get; set; } = false;

		[DataMember][Reactive] public long LastUpdateCheck { get; set; } = -1;
		private string lastOrder = "";

		[DataMember]
		public string LastOrder
		{
			get => lastOrder;
			set { this.RaiseAndSetIfChanged(ref lastOrder, value); }
		}

		private string lastLoadedOrderFilePath = "";

		[DataMember]
		public string LastLoadedOrderFilePath
		{
			get => lastLoadedOrderFilePath;
			set { this.RaiseAndSetIfChanged(ref lastLoadedOrderFilePath, value); }
		}

		private string lastExtractOutputPath = "";

		[DataMember]
		public string LastExtractOutputPath
		{
			get => lastExtractOutputPath;
			set { this.RaiseAndSetIfChanged(ref lastExtractOutputPath, value); }
		}

		private bool darkThemeEnabled = true;

		[DataMember]
		public bool DarkThemeEnabled
		{
			get => darkThemeEnabled;
			set { this.RaiseAndSetIfChanged(ref darkThemeEnabled, value); }
		}

		private ScriptExtenderSettings extenderSettings;

		[DataMember]
		public ScriptExtenderSettings ExtenderSettings
		{
			get => extenderSettings;
			set { this.RaiseAndSetIfChanged(ref extenderSettings, value); }
		}

		public string ExtenderLogDirectory
		{
			get
			{
				if (ExtenderSettings == null || String.IsNullOrWhiteSpace(ExtenderSettings.LogDirectory))
				{

					return Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OsirisLogs");
				}
				return ExtenderSettings.LogDirectory;
			}
		}


		private DivinityGameLaunchWindowAction actionOnGameLaunch = DivinityGameLaunchWindowAction.None;

		[DataMember]
		public DivinityGameLaunchWindowAction ActionOnGameLaunch
		{
			get => actionOnGameLaunch;
			set { this.RaiseAndSetIfChanged(ref actionOnGameLaunch, value); }
		}

		[DataMember][Reactive] public bool ExportDefaultExtenderSettings { get; set; } = false;

		//Not saved for now

		private bool displayFileNames = false;

		public bool DisplayFileNames
		{
			get => displayFileNames;
			set { this.RaiseAndSetIfChanged(ref displayFileNames, value); }
		}

		private bool debugModeEnabled = false;

		[SettingsEntry("Enable Developer Mode", "This enables features for mod developers, such as being able to copy a mod's UUID in context menus, and additional Extender options.")]
		[DataMember]
		public bool DebugModeEnabled
		{
			get => debugModeEnabled;
			set
			{
				this.RaiseAndSetIfChanged(ref debugModeEnabled, value);
				DivinityApp.DeveloperModeEnabled = value;
			}
		}

		[Reactive][DataMember] public string GameLaunchParams { get; set; }

		[Reactive] public bool GameMasterModeEnabled { get; set; } = false;
		[Reactive] public bool ExtenderTabIsVisible { get; set; } = false;

		[Reactive] public bool KeybindingsTabIsVisible { get; set; } = false;

		private Hotkey selectedHotkey;

		public Hotkey SelectedHotkey
		{
			get => selectedHotkey;
			set { this.RaiseAndSetIfChanged(ref selectedHotkey, value); }
		}

		[Reactive] public int SelectedTabIndex { get; set; } = 0;

		public ICommand SaveSettingsCommand { get; set; }
		public ICommand OpenSettingsFolderCommand { get; set; }
		public ICommand ExportExtenderSettingsCommand { get; set; }
		public ICommand ResetExtenderSettingsToDefaultCommand { get; set; }
		public ICommand ResetKeybindingsCommand { get; set; }
		public ICommand ClearWorkshopCacheCommand { get; set; }
		public ICommand AddLaunchParamCommand { get; set; }
		public ICommand ClearLaunchParamsCommand { get; set; }

		public CompositeDisposable Disposables { get; internal set; }

		private bool canSaveSettings = false;

		public bool CanSaveSettings
		{
			get => canSaveSettings;
			set { this.RaiseAndSetIfChanged(ref canSaveSettings, value); }
		}

		public bool SettingsWindowIsOpen { get; set; } = false;

		public void Dispose()
		{
			Disposables?.Dispose();
			Disposables = null;
		}

		public DivinityModManagerSettings()
		{
			Disposables = new CompositeDisposable();
			ExtenderSettings = new ScriptExtenderSettings();

			var properties = typeof(DivinityModManagerSettings)
			.GetRuntimeProperties()
			.Where(prop => Attribute.IsDefined(prop, typeof(DataMemberAttribute)))
			.Select(prop => prop.Name)
			.ToArray();

			this.WhenAnyPropertyChanged(properties).Subscribe((c) =>
			{
				if (SettingsWindowIsOpen) CanSaveSettings = true;
			}).DisposeWith(Disposables);

			var extender_properties = typeof(ScriptExtenderSettings)
			.GetRuntimeProperties()
			.Where(prop => Attribute.IsDefined(prop, typeof(DataMemberAttribute)))
			.Select(prop => prop.Name)
			.ToArray();

			ExtenderSettings.WhenAnyPropertyChanged(extender_properties).Subscribe((c) =>
			{
				if (SettingsWindowIsOpen) CanSaveSettings = true;
				this.RaisePropertyChanged("ExtenderLogDirectory");
			}).DisposeWith(Disposables);

			this.WhenAnyValue(x => x.SelectedTabIndex, (index) => index == 1).BindTo(this, x => x.ExtenderTabIsVisible);
			this.WhenAnyValue(x => x.SelectedTabIndex, (index) => index == 2).BindTo(this, x => x.KeybindingsTabIsVisible);
		}
	}
}
