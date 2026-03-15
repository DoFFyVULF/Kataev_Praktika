using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KataevLIB
{
    public class MainApp : IDisposable, IMainApp
    {
        public ApplicationContext Context { get; private set; } = new ApplicationContext();
        private bool disposedValue;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Context.Dispose();
                }

                disposedValue = true;
            }
        }

        public MainApp()
        {
            SetupAppSchema();
        }

        public ApplicationContext GetContext()
        {
            return this.Context;
        }

        public void SaveChanges()
        {
            this.Context.SaveChanges();
        }

        public void SetupAppSchema()
        {
            string createSchemaSql = "CREATE SCHEMA IF NOT EXISTS app;";

            string setSearchPathSql = "SET search_path TO app, public;";

            string grantPrivilegesSql = "GRANT USAGE ON SCHEMA app TO app; GRANT SELECT ON ALL TABLES IN SCHEMA app TO app;";

            this.Context.Database.ExecuteSqlRaw(createSchemaSql);
            this.Context.Database.ExecuteSqlRaw(setSearchPathSql);
            this.Context.Database.ExecuteSqlRaw(grantPrivilegesSql);
        }


        public void AddPartner(KataevPartner partner)
        {
            this.Context.KataevPartners.Add(partner);
        }

        public void UpdatePartner(KataevPartner partner)
        {
            this.Context.KataevPartners.Update(partner);
        }

        public void RemovePartner(KataevPartner partner)
        {
            this.Context.KataevPartners.Remove(partner);
        }

        public IEnumerable<KataevPartner> GetPartners()
        {
            return this.Context.KataevPartners.Include(p => p.KataevSalesHistories).ToList();
        }

        public KataevPartner GetPartner(int partnerId)
        {
            return this.Context.KataevPartners
                .Include(p => p.KataevSalesHistories)
                .FirstOrDefault(p => p.Id == partnerId);
        }

        public IEnumerable<KataevPartner> GetPartners(string searchTerm)
        {
            return this.GetPartners()
                .Where(p => p.Name.Contains(searchTerm))
                .ToList();
        }

        public void AddProduct(KataevProduct product)
        {
            this.Context.KataevProducts.Add(product);
        }

        public void UpdateProduct(KataevProduct product)
        {
            this.Context.KataevProducts.Update(product);
        }

        public void RemoveProduct(KataevProduct product)
        {
            this.Context.KataevProducts.Remove(product);
        }

        public IEnumerable<KataevProduct> GetProducts()
        {
            return this.Context.KataevProducts.ToList();
        }

        public KataevProduct GetProduct(int productId)
        {
            return this.Context.KataevProducts
                .FirstOrDefault(p => p.Id == productId);
        }

        public void AddSalesHistory(KataevSalesHistory salesHistory)
        {
            var partnerExists = Context.KataevPartners.Any(p => p.Id == salesHistory.PartnerId);
            if (partnerExists)
            {
                Context.KataevSalesHistories.Add(salesHistory);
            }
            else
            {
                throw new Exception("Партнер с указанным ID не найден. Невозможно добавить историю продаж.");
            }
        }

        public void UpdateSalesHistory(KataevSalesHistory salesHistory)
        {
            this.Context.KataevSalesHistories.Update(salesHistory);
        }

        public void RemoveSalesHistory(KataevSalesHistory salesHistory)
        {
            this.Context.KataevSalesHistories.Remove(salesHistory);
        }

        public IEnumerable<KataevSalesHistory> GetSalesHistory(KataevPartner partner)
        {
            return this.GetSalesHistory(partner.Id);
        }

        public IEnumerable<KataevSalesHistory> GetSalesHistory(int partnerId)
        {
            return this.Context.KataevSalesHistories
                .Where(sh => sh.PartnerId == partnerId)
                .Include(sh => sh.KataevProduct)
                .ToList();
        }
        public int CalculatePartnerDiscount(int partnerId)
        {
            int totalQuantity = this.Context.KataevSalesHistories
                .Where(sh => sh.PartnerId == partnerId)
                .Sum(sh => sh.Quantity);

            if (totalQuantity < 10000)
            {
                return 0;  
            }
            else if (totalQuantity >= 10000 && totalQuantity < 50000)
            {
                return 5;   
            }
            else if (totalQuantity >= 50000 && totalQuantity < 300000)
            {
                return 10;  
            }
            else
            {
                return 15;  
            }
        }
        ~MainApp()
        {
            Dispose(false);
        }
    }
}