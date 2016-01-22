﻿using MoneyManager.Core.ViewModels;
using MoneyManager.Foundation.Interfaces;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;

namespace MoneyManager.Core
{
    public class App : MvxApplication
    {
        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        public override void Initialize()
        {
            // Start the app with the Main View Model.
            RegisterAppStart<MainViewModel>();

            Mvx.Resolve<IRecurringTransactionManager>().CheckRecurringPayments();
            Mvx.Resolve<IPaymentManager>().ClearPayments();
        }
    }
}