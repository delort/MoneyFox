﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using MoneyManager.Core.Helper;
using MoneyManager.Core.Repositories;
using MoneyManager.Core.Tests.Mocks;
using MoneyManager.Foundation;
using MoneyManager.Foundation.Model;
using MoneyManager.Foundation.OperationContracts;
using Moq;
using Xunit;

namespace MoneyManager.Core.Tests.Repositories
{
    public class TransactionRepositoryTests
    {
        [Fact]
        public void SaveWithouthAccount_NoAccount_InvalidDataException()
        {
            var repository = new TransactionRepository(new TransactionDataAccessMock(), new RecurringTransactionDataAccessMock());

            var transaction = new FinancialTransaction
            {
                Amount = 20
            };

            Assert.Throws<InvalidDataException>(() => repository.Save(transaction));
        }

        [Theory]
        [InlineData(TransactionType.Income)]
        public void Save_DifferentTransactionTypes_CorrectlySaved(TransactionType type)
        {
            var transactionDataAccessMock = new TransactionDataAccessMock();
            var repository = new TransactionRepository(transactionDataAccessMock, new RecurringTransactionDataAccessMock());

            var account = new Account
            {
                Name = "TestAccount"
            };

            var transaction = new FinancialTransaction
            {
                ChargedAccount = account,
                TargetAccount = null,
                Amount = 20,
                Type = (int)type
            };

            repository.Save(transaction);

            transactionDataAccessMock.FinancialTransactionTestList[0].ShouldBeSameAs(transaction);
            transactionDataAccessMock.FinancialTransactionTestList[0].ChargedAccount.ShouldBeSameAs(account);
            transactionDataAccessMock.FinancialTransactionTestList[0].TargetAccount.ShouldBeNull();
            transactionDataAccessMock.FinancialTransactionTestList[0].Type.ShouldBe((int)type);
        }

        [Fact]
        public void Save_TransferTransaction_CorrectlySaved()
        {
            var transactionDataAccessMock = new TransactionDataAccessMock();
            var repository = new TransactionRepository(transactionDataAccessMock, new RecurringTransactionDataAccessMock());

            var account = new Account
            {
                Name = "TestAccount"
            };

            var targetAccount = new Account
            {
                Name = "targetAccount"
            };

            var transaction = new FinancialTransaction
            {
                ChargedAccount = account,
                TargetAccount = targetAccount,
                Amount = 20,
                Type = (int) TransactionType.Transfer
            };

            repository.Save(transaction);

            transactionDataAccessMock.FinancialTransactionTestList[0].ShouldBeSameAs(transaction);
            transactionDataAccessMock.FinancialTransactionTestList[0].ChargedAccount.ShouldBeSameAs(account);
            transactionDataAccessMock.FinancialTransactionTestList[0].TargetAccount.ShouldBeSameAs(targetAccount);
            transactionDataAccessMock.FinancialTransactionTestList[0].Type.ShouldBe((int)TransactionType.Transfer);
        }

        [Fact]
        public void TransactionRepository_Delete()
        {
            var transactionDataAccessMock = new TransactionDataAccessMock();
            var repository = new TransactionRepository(transactionDataAccessMock, new RecurringTransactionDataAccessMock());

            var account = new Account
            {
                Name = "TestAccount"
            };

            var transaction = new FinancialTransaction
            {
                ChargedAccount = account,
                Amount = 20,
            };

            repository.Save(transaction);
            transactionDataAccessMock.FinancialTransactionTestList[0].ShouldBeSameAs(transaction);

            repository.Delete(transaction);

            transactionDataAccessMock.FinancialTransactionTestList.Any().ShouldBeFalse();
            repository.Data.Any().ShouldBeFalse();
        }

        [Fact]
        public void TransactionRepository_AccessCache()
        {
            new TransactionRepository(new TransactionDataAccessMock(), new RecurringTransactionDataAccessMock()).Data.ShouldNotBeNull();
        }

        [Fact]
        public void TransactionRepository_AddMultipleToCache()
        {
            var repository = new TransactionRepository(new TransactionDataAccessMock(), new RecurringTransactionDataAccessMock());

            var account = new Account
            {
                Name = "TestAccount"
            };

            var transaction = new FinancialTransaction
            {
                ChargedAccount = account,
                Amount = 20,
            };

            var secondTransaction = new FinancialTransaction
            {
                ChargedAccount = account,
                Amount = 60,
            };

            repository.Save(transaction);
            repository.Save(secondTransaction);

            repository.Data.Count.ShouldBe(2);
            repository.Data[0].ShouldBeSameAs(transaction);
            repository.Data[1].ShouldBeSameAs(secondTransaction);
        }

        [Fact]
        public void AddItemToDataList_SaveAccount_IsAddedToData()
        {
            var repository = new TransactionRepository(new TransactionDataAccessMock(), new RecurringTransactionDataAccessMock());

            var account = new Account
            {
                Name = "TestAccount"
            };

            var transaction = new FinancialTransaction
            {
                ChargedAccount = account,
                Amount = 20,
                Type = (int) TransactionType.Transfer
            };

            repository.Save(transaction);
            repository.Data.Contains(transaction).ShouldBeTrue();
        }

        [Fact]
        public void GetUnclearedTransactions_PastDate_PastTransactions()
        {
            var repository = new TransactionRepository(new TransactionDataAccessMock(), new RecurringTransactionDataAccessMock());

            var account = new Account
            {
                Name = "TestAccount"
            };

            repository.Save(new FinancialTransaction
            {
                ChargedAccount = account,
                Amount = 55,
                Date = DateTime.Today.AddDays(-1),
                Note = "this is a note!!!",
                Cleared = false
            }
                );

            var transactions = repository.GetUnclearedTransactions();

            transactions.Count().ShouldBe(1);
        }

        /// <summary>
        ///     This Test may fail if the date overlaps with the month transition.
        /// </summary>
        [Fact]
        public void GetUnclearedTransactions_FutureDate_PastTransactions()
        {
            var repository = new TransactionRepository(new TransactionDataAccessMock(), new RecurringTransactionDataAccessMock());

            var account = new Account
            {
                Name = "TestAccount"
            };

            repository.Save(new FinancialTransaction
            {
                ChargedAccount = account,
                Amount = 55,
                Date = Utilities.GetEndOfMonth().AddDays(-1),
                Note = "this is a note!!!",
                Cleared = false
            }
                );

            var transactions = repository.GetUnclearedTransactions();
            transactions.Count().ShouldBe(0);

            transactions = repository.GetUnclearedTransactions(Utilities.GetEndOfMonth());
            transactions.Count().ShouldBe(1);
        }

        [Fact]
        public void GetUnclearedTransactions_AccountNull()
        {
            var repository = new TransactionRepository(new TransactionDataAccessMock(), new RecurringTransactionDataAccessMock());

            repository.Data.Add(new FinancialTransaction
            {
                Amount = 55,
                Date = DateTime.Today.AddDays(-1),
                Note = "this is a note!!!",
                Cleared = false
            }
                );

            var transactions = repository.GetUnclearedTransactions();
            transactions.Count().ShouldBe(1);
        }

        //TODO: Move to platformspecific project
        //[Fact]
        //public void LoadRecurringList_ListWithRecurringTransaction()
        //{
        //    var transacitonList = new List<FinancialTransaction>
        //    {
        //        new FinancialTransaction {Id = 3, Amount = 999, IsRecurring = false},
        //        new FinancialTransaction {Id = 4, Amount = 123, IsRecurring = true, RecurringTransaction = new RecurringTransaction {Id = 12}},
        //    };

        //    var transDataAccessSetup = new Mock<IDataAccess<FinancialTransaction>>();
        //    transDataAccessSetup.Setup(x => x.LoadList(It.IsAny<Expression<Func<FinancialTransaction, bool>>>()))
        //        .Returns<Expression<Func<FinancialTransaction, bool>>>(action => transacitonList.Where(x => action.Compile().Invoke(x)).ToList());

        //    var repository = new TransactionRepository(transDataAccessSetup.Object, new Mock<IDataAccess<RecurringTransaction>>().Object);

        //    var result = repository.LoadRecurringList().ToList();

        //    result.Count.ShouldBe(1);

        //    result.First().Id.ShouldBe(4);
        //    result.First().RecurringTransaction.Id.ShouldBe(12);
        //}
    }
}