﻿using System.Collections.ObjectModel;
using System.Globalization;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using MoneyFox.Core.Constants;
using MoneyFox.Core.Groups;
using MoneyFox.Core.Interfaces;
using MoneyFox.Foundation.Model;
using MoneyFox.Foundation.Resources;
using IDialogService = MoneyFox.Core.Interfaces.IDialogService;

namespace MoneyFox.Core.ViewModels
{
    public class RecurringPaymentListViewModel : ViewModelBase
    {
        private readonly IDialogService dialogService;
        private readonly INavigationService navigationService;
        private readonly IPaymentRepository paymentRepository;

        public RecurringPaymentListViewModel(IPaymentRepository paymentRepository,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            this.paymentRepository = paymentRepository;
            this.dialogService = dialogService;
            this.navigationService = navigationService;

            AllPayments = new ObservableCollection<Payment>();
        }

        public ObservableCollection<Payment> AllPayments { get; }

        /// <summary>
        ///     Returns groupped related payments
        /// </summary>
        public ObservableCollection<AlphaGroupListGroup<Payment>> Source { get; private set; }

        /// <summary>
        ///     Prepares the recurring payment list view
        /// </summary>
        public RelayCommand LoadedCommand => new RelayCommand(Loaded);

        /// <summary>
        ///     Edits the currently selected payment.
        /// </summary>
        public RelayCommand<Payment> EditCommand { get; private set; }

        /// <summary>
        ///     Deletes the selected payment
        /// </summary>
        public RelayCommand<Payment> DeleteCommand => new RelayCommand<Payment>(Delete);

        private void Loaded()
        {
            EditCommand = null;

            AllPayments.Clear();
            foreach (var payment in paymentRepository.LoadRecurringList())
            {
                AllPayments.Add(payment);
            }

            Source = new ObservableCollection<AlphaGroupListGroup<Payment>>(
                AlphaGroupListGroup<Payment>.CreateGroups(AllPayments,
                    CultureInfo.CurrentUICulture,
                    s => s.ChargedAccount.Name));

            //We have to set the command here to ensure that the selection changed event is triggered earlier
            EditCommand = new RelayCommand<Payment>(Edit);
        }

        private void Edit(Payment payment)
        {
            paymentRepository.Selected = payment;

            navigationService.NavigateTo(NavigationConstants.MODIFY_PAYMENT_VIEW, payment);
        }

        private async void Delete(Payment payment)
        {
            if (!await
                dialogService.ShowConfirmMessage(Strings.DeleteTitle, Strings.DeletePaymentConfirmationMessage))
            {
                return;
            }

            paymentRepository.Delete(payment);
            LoadedCommand.Execute(null);
        }
    }
}