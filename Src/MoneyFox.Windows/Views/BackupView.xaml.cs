﻿using Microsoft.Practices.ServiceLocation;
using MoneyManager.Core.ViewModels;

namespace MoneyManager.Windows.Views
{
    public sealed partial class BackupView
    {
        public BackupView()
        {
            InitializeComponent();
            DataContext = ServiceLocator.Current.GetInstance<BackupViewModel>();
        }
    }
}