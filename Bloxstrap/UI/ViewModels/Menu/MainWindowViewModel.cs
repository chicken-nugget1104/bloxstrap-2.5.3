﻿using System.Windows;
using System.Windows.Input;

using Microsoft.Win32;

using CommunityToolkit.Mvvm.Input;

using Wpf.Ui.Mvvm.Contracts;

using Bloxstrap.UI.Elements.Menu.Pages;

namespace Bloxstrap.UI.ViewModels.Menu
{
    public class MainWindowViewModel : NotifyPropertyChangedViewModel
    {
        private readonly Window _window;

        public ICommand CloseWindowCommand => new RelayCommand(CloseWindow);
        public ICommand ConfirmSettingsCommand => new RelayCommand(ConfirmSettings);

        public Visibility NavigationVisibility { get; set; } = Visibility.Visible;
        public string ConfirmButtonText => App.IsFirstRun ? "Install" : "Save";
        public bool ConfirmButtonEnabled { get; set; } = true;

        public MainWindowViewModel(Window window)
        {
            _window = window;
        }

        private void CloseWindow() => _window.Close();

        private void ConfirmSettings()
        {
            if (!App.IsFirstRun)
            {
                App.ShouldSaveConfigs = true;
                App.FastFlags.Save();
                CloseWindow();

                return;
            }

            if (string.IsNullOrEmpty(App.BaseDirectory))
            {
                Controls.ShowMessageBox("You must set an install location", MessageBoxImage.Error);
                return;
            }

            if (NavigationVisibility == Visibility.Visible)
            {
                try
                {
                    // check if we can write to the directory (a bit hacky but eh)
                    string testFile = Path.Combine(App.BaseDirectory, $"{App.ProjectName}WriteTest.txt");

                    Directory.CreateDirectory(App.BaseDirectory);
                    File.WriteAllText(testFile, "hi");
                    File.Delete(testFile);
                }
                catch (UnauthorizedAccessException)
                {
                    Controls.ShowMessageBox(
                        $"{App.ProjectName} does not have write access to the install location you've selected. Please choose another location.",
                        MessageBoxImage.Error
                    );
                    return;
                }
                catch (Exception ex)
                {
                    Controls.ShowMessageBox(ex.Message, MessageBoxImage.Error);
                    return;
                }

                if (!App.BaseDirectory.EndsWith(App.ProjectName) && Directory.Exists(App.BaseDirectory) && Directory.EnumerateFileSystemEntries(App.BaseDirectory).Any())
                {
                    string suggestedChange = Path.Combine(App.BaseDirectory, App.ProjectName);

                    MessageBoxResult result = Controls.ShowMessageBox(
                        $"The folder you've chosen to install {App.ProjectName} to already exists and is NOT empty. It is strongly recommended for {App.ProjectName} to be installed to its own independent folder.\n\n" +
                        "Changing to the following location is suggested:\n" +
                        $"{suggestedChange}\n\n" +
                        "Would you like to change to the suggested location?\n" +
                        "Selecting 'No' will ignore this warning and continue installation.",
                        MessageBoxImage.Warning,
                        MessageBoxButton.YesNoCancel,
                        MessageBoxResult.Yes
                    );

                    if (result == MessageBoxResult.Yes)
                        App.BaseDirectory = suggestedChange;
                    else if (result == MessageBoxResult.Cancel)
                        return;
                }

                if (
                    App.BaseDirectory.Length <= 3 || // prevent from installing to the root of a drive
                    App.BaseDirectory.StartsWith("\\\\") || // i actually haven't encountered anyone doing this and i dont even know if this is possible but this is just to be safe lmao
                    App.BaseDirectory.ToLowerInvariant().Contains("onedrive") || // prevent from installing to a onedrive folder
                    Directory.GetParent(App.BaseDirectory)!.ToString().ToLowerInvariant() == Paths.UserProfile.ToLowerInvariant() // prevent from installing to an essential user profile folder
                )
                {
                    Controls.ShowMessageBox(
                        $"{App.ProjectName} cannot be installed here. Please choose a different location, or resort to using the default location by clicking the reset button.",
                        MessageBoxImage.Error,
                        MessageBoxButton.OK
                    );

                    return;
                }
            }
            
            if (NavigationVisibility == Visibility.Visible)
            {
                ((INavigationWindow)_window).Navigate(typeof(PreInstallPage));

                NavigationVisibility = Visibility.Collapsed;
                OnPropertyChanged(nameof(NavigationVisibility));
                    
                ConfirmButtonEnabled = false;
                OnPropertyChanged(nameof(ConfirmButtonEnabled));

                Task.Run(async delegate
                {
                    await Task.Delay(3000);
                    
                    ConfirmButtonEnabled = true;
                    OnPropertyChanged(nameof(ConfirmButtonEnabled));
                });
            }
            else
            {
                App.IsSetupComplete = true;
                CloseWindow();
            }
        }
    }
}
