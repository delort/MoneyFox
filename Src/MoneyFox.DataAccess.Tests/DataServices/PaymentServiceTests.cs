﻿using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using MoneyFox.Business.ViewModels;
using MoneyFox.DataAccess.Entities;
using MoneyFox.DataAccess.Infrastructure;
using MoneyFox.DataAccess.Repositories;
using MoneyFox.Foundation;
using MoneyFox.Foundation.Constants;
using MoneyFox.Service;
using MoneyFox.Service.DataServices;
using MoneyFox.Service.Pocos;
using MvvmCross.Platform.Core;
using Xunit;

namespace MoneyFox.DataAccess.Tests.DataServices
{
    /// <summary>
    ///     This test is in the DataAccess project, because it tests the direct access to the database.
    /// </summary>
    public class PaymentServiceTests : IDisposable
    {
        private readonly DbFactory dbFactory;

        /// <summary>
        ///     Setup Logic who is executed before every test
        /// </summary>
        public PaymentServiceTests()
        {
            dbFactory = new DbFactory();
            ApplicationContext.DbPath = Path.Combine(AppContext.BaseDirectory, DatabaseConstants.DB_NAME);
            using (var db = new ApplicationContext())
            {
                db.Database.Migrate();
            }
        }

        /// <summary>
        ///     Cleanup logic who is executed after executign every test.
        /// </summary>
        public void Dispose()
        {
            dbFactory.DisposeIfDisposable();
            if (File.Exists(ApplicationContext.DbPath))
            {
                File.Delete(ApplicationContext.DbPath);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async void Save_WithRecurringPayment_GetRecurringPaymentFromHelper()
        {
            // Arrange
            var unitOfWork = new UnitOfWork(dbFactory);

            var repository = new PaymentRepository(dbFactory);

            var accountRepository = new AccountRepository(dbFactory);
            var testAccount = new AccountEntity { Name = "testAccount" };
            accountRepository.Add(testAccount);
            await unitOfWork.Commit();

            var testEntry = new PaymentViewModel(new Payment
            {
                Data =
                {
                    ChargedAccount = testAccount,
                    Date = DateTime.Now,
                    IsRecurring = true,
                    Note = "Testtext"
                }
            });
            testEntry.RecurringPayment = new RecurringPaymentViewModel(
                RecurringPaymentHelper.GetRecurringFromPayment(testEntry.Payment,
                                                               true,
                                                               PaymentRecurrence.Bimonthly,
                                                               DateTime.Now));

            var paymentService = new PaymentService(repository, unitOfWork);

            // Act
            await paymentService.SavePayment(testEntry.Payment);
            await unitOfWork.Commit();
            var payment = await paymentService.GetById(testEntry.Payment.Data.Id);

            // Assert
            Assert.NotNull(payment);
        }
    }
}