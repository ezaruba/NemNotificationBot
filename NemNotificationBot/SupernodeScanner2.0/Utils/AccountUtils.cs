﻿using CSharp2nem;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTelegramBotApi;
using NetTelegramBotApi.Requests;
using SupernodeScanner2._0.DBControllers;
using SuperNodeScanner;

namespace SupernodeScanner2._0.DataContextModel
{
    internal class AccountUtils
    {
        
        internal static void AddAccount(long chatId, List<string> accounts)
        {
            var context = new AccountDataContext();

            foreach (var acc in accounts)
            {
                switch (acc)
                {
                    case "NBZMQO7ZPBYNBDUR7F75MAKA2S3DHDCIFG775N3D":
                    case "NCPAYOUTH2BGEGT3Q7K75PV27QKMVNN2IZRVZWMD":
                    case null:
                        continue;
                }

                if (GetAccount(acc, chatId)?.EncodedAddress != null) return;

                var a = new AccountFactory().FromEncodedAddress(acc);
                
                var blocks = a.GetHarvestingInfoAsync().Result.data; // check hash

                var transactions = a.GetAllTransactionsAsync().Result.data;
               
                var account = new Account()
                {
                    
                    OwnedByUser = chatId,
                    EncodedAddress = acc,
                    LastTransactionHash = transactions?[0]?.meta?.hash?.data ?? "none",
                    LastBlockHarvestedHeight =  blocks.Count > 0 ? blocks[0]?.height : 0,
                    CheckBlocks = true,
                    CheckTxs = true

                };
                
                context.Accounts.InsertOnSubmit(account);

                try
                {
                    context.SubmitChanges();
                }
                catch (Exception e)
                {

                }
            }        
        }

        internal static void DeleteAccount(long chatId, List<string> accounts)
        {
            var context = new AccountDataContext();

            foreach (var acc in accounts)
            {
                Account account;
                try
                {
                    account = context.Accounts.Where(e=>e.OwnedByUser == chatId).Single(e => e.EncodedAddress == acc);
                }
                catch (Exception)
                {
                    continue;
                }

                context.Accounts.DeleteOnSubmit(account);

                context.SubmitChanges();
            }   
        }


        internal static List<Account> GetAccountByUser(long chatId)
        {

            if (UserUtils.GetUser(chatId)?.UserName == null) return null;

            var context = new AccountDataContext();

            return context.Accounts.Where(node => node.OwnedByUser == chatId).ToList();
        }

        internal static void UpdateAccount(Account a)
        {
            var context = new AccountDataContext();

            var acc = context.Accounts.Where(e => e.EncodedAddress == a.EncodedAddress).Single(i => i.OwnedByUser == a.OwnedByUser);

            acc.LastTransactionHash = a.LastTransactionHash;
            acc.LastBlockHarvestedHeight = a.LastBlockHarvestedHeight;
            acc.CheckBlocks = a.CheckBlocks;
            acc.CheckTxs = a.CheckTxs;
            context.SubmitChanges();
        }
        internal static void UpdateAccount(List<Account> accs, long user)
        {
            var context = new AccountDataContext();
            
            foreach(var acc in accs)
            {
                var a = context.Accounts.Where(e => e.EncodedAddress == acc.EncodedAddress).Single( i => i.OwnedByUser == user);


                a.LastTransactionHash = acc.LastTransactionHash;
                a.LastBlockHarvestedHeight = acc.LastBlockHarvestedHeight;
                a.CheckBlocks = acc.CheckBlocks;
                a.CheckTxs = acc.CheckTxs;

                context.SubmitChanges();
            }    
        }

        internal static List<Account> GetAccounts()
        {
            var context = new AccountDataContext();

            try
            {
                var acc = context.Accounts.ToList();
                return acc;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

        }
        internal static Account GetAccount(string add, long user)
        {
            var context = new AccountDataContext();

            try
            {
              var acc = context.Accounts.Where(e => e.OwnedByUser == user).Single(e => e.EncodedAddress == add);
             
              return acc;
            }
            catch (Exception)
            {
                return null;
            }
            
        }
    }
}
