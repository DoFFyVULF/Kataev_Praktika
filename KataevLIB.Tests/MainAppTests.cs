using Microsoft.VisualStudio.TestTools.UnitTesting;
using KataevLIB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KataevLIB.Tests
{
    internal class MockMainApp : IMainApp
    {
        private readonly List<KataevPartner> _partners = new List<KataevPartner>();
        private readonly List<KataevProduct> _products = new List<KataevProduct>();
        private readonly List<KataevSalesHistory> _salesHistories = new List<KataevSalesHistory>();

        private int _partnerIdCounter = 1;
        private int _productIdCounter = 1;
        private int _salesHistoryIdCounter = 1;

        public ApplicationContext GetContext() => null;
        public void SaveChanges() { }
        public void SetupAppSchema() { }

        public void AddPartner(KataevPartner partner)
        {
            partner.Id = _partnerIdCounter++;
            _partners.Add(partner);
        }

        public void UpdatePartner(KataevPartner partner)
        {
            var existing = _partners.FirstOrDefault(p => p.Id == partner.Id);
            if (existing != null)
            {
                existing.Name = partner.Name;
                existing.PartnerType = partner.PartnerType;
                existing.Rating = partner.Rating;
                existing.Address = partner.Address;
                existing.INN = partner.INN;
                existing.DirectorLastName = partner.DirectorLastName;
                existing.DirectorFirstName = partner.DirectorFirstName;
                existing.DirectorMiddleName = partner.DirectorMiddleName;
                existing.Phone = partner.Phone;
                existing.Email = partner.Email;
            }
        }

        public void RemovePartner(KataevPartner partner)
        {
            _partners.RemoveAll(p => p.Id == partner.Id);
        }

        public IEnumerable<KataevPartner> GetPartners()
        {
            return _partners.ToList();
        }

        public KataevPartner GetPartner(int partnerId)
        {
            return _partners.FirstOrDefault(p => p.Id == partnerId);
        }

        public IEnumerable<KataevPartner> GetPartners(string searchTerm)
        {
            return _partners.Where(p => p.Name != null && p.Name.Contains(searchTerm)).ToList();
        }

        public void AddProduct(KataevProduct product)
        {
            product.Id = _productIdCounter++;
            _products.Add(product);
        }

        public void UpdateProduct(KataevProduct product)
        {
            var existing = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existing != null)
            {
                existing.Name = product.Name;
                existing.Article = product.Article;
                existing.Type = product.Type;
            }
        }

        public void RemoveProduct(KataevProduct product)
        {
            _products.RemoveAll(p => p.Id == product.Id);
        }

        public IEnumerable<KataevProduct> GetProducts()
        {
            return _products.ToList();
        }

        public KataevProduct GetProduct(int productId)
        {
            return _products.FirstOrDefault(p => p.Id == productId);
        }

        public void AddSalesHistory(KataevSalesHistory salesHistory)
        {
            bool partnerExists = _partners.Any(p => p.Id == salesHistory.PartnerId);
            if (!partnerExists)
                throw new Exception("Партнер с указанным ID не найден. Невозможно добавить историю продаж.");

            salesHistory.Id = _salesHistoryIdCounter++;
            _salesHistories.Add(salesHistory);
        }

        public void UpdateSalesHistory(KataevSalesHistory salesHistory)
        {
            var existing = _salesHistories.FirstOrDefault(s => s.Id == salesHistory.Id);
            if (existing != null)
            {
                existing.Quantity = salesHistory.Quantity;
                existing.SaleDate = salesHistory.SaleDate;
                existing.ProductId = salesHistory.ProductId;
            }
        }

        public void RemoveSalesHistory(KataevSalesHistory salesHistory)
        {
            _salesHistories.RemoveAll(s => s.Id == salesHistory.Id);
        }

        public IEnumerable<KataevSalesHistory> GetSalesHistory(KataevPartner partner)
        {
            return GetSalesHistory(partner.Id);
        }

        public IEnumerable<KataevSalesHistory> GetSalesHistory(int partnerId)
        {
            return _salesHistories.Where(s => s.PartnerId == partnerId).ToList();
        }

        public int CalculatePartnerDiscount(int partnerId)
        {
            int total = _salesHistories
                .Where(s => s.PartnerId == partnerId)
                .Sum(s => s.Quantity);

            if (total < 10000) return 0;
            if (total < 50000) return 5;
            if (total < 300000) return 10;
            return 15;
        }
    }

    [TestClass]
    public class MainAppTests
    {
        private MockMainApp _app;

        [TestInitialize]
        public void Setup()
        {
            _app = new MockMainApp();
        }

        [TestMethod]
        public void AddPartner_ValidPartner_SavedToCollection()
        {
            var partner = new KataevPartner
            {
                Name = "ООО Ромашка",
                DirectorLastName = "Иванов",
                DirectorFirstName = "Иван",
                PartnerType = "ООО",
                Rating = 5
            };

            _app.AddPartner(partner);

            var result = _app.GetPartners().ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("ООО Ромашка", result[0].Name);
            Assert.AreEqual("Иванов", result[0].DirectorLastName);
        }

        [TestMethod]
        public void AddPartner_MultiplePartners_AllSaved()
        {
            _app.AddPartner(new KataevPartner { Name = "Альфа" });
            _app.AddPartner(new KataevPartner { Name = "Бета" });

            Assert.AreEqual(2, _app.GetPartners().Count());
        }

        [TestMethod]
        public void AddPartner_AssignsUniqueIds()
        {
            var p1 = new KataevPartner { Name = "П1" };
            var p2 = new KataevPartner { Name = "П2" };

            _app.AddPartner(p1);
            _app.AddPartner(p2);

            Assert.AreNotEqual(p1.Id, p2.Id);
        }

        [TestMethod]
        public void GetPartners_NoPartners_ReturnsEmpty()
        {
            Assert.AreEqual(0, _app.GetPartners().Count());
        }

        [TestMethod]
        public void GetPartner_ExistingId_ReturnsCorrectPartner()
        {
            var partner = new KataevPartner { Name = "Гамма", INN = "1234567890" };
            _app.AddPartner(partner);

            var result = _app.GetPartner(partner.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Гамма", result.Name);
            Assert.AreEqual("1234567890", result.INN);
        }

        [TestMethod]
        public void GetPartner_NonExistingId_ReturnsNull()
        {
            Assert.IsNull(_app.GetPartner(9999));
        }

        [TestMethod]
        public void GetPartners_SearchByName_ReturnsMatching()
        {
            _app.AddPartner(new KataevPartner { Name = "ООО Ромашка" });
            _app.AddPartner(new KataevPartner { Name = "ООО Лютик" });
            _app.AddPartner(new KataevPartner { Name = "АО Василёк" });

            var result = _app.GetPartners("ООО").ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(p => p.Name.Contains("ООО")));
        }

        [TestMethod]
        public void GetPartners_SearchNoMatch_ReturnsEmpty()
        {
            _app.AddPartner(new KataevPartner { Name = "Альфа" });

            Assert.AreEqual(0, _app.GetPartners("XYZ").Count());
        }

        [TestMethod]
        public void UpdatePartner_ChangeName_NameUpdated()
        {
            var partner = new KataevPartner { Name = "Старое", Rating = 3 };
            _app.AddPartner(partner);

            partner.Name = "Новое";
            partner.Rating = 9;
            _app.UpdatePartner(partner);

            var updated = _app.GetPartner(partner.Id);
            Assert.AreEqual("Новое", updated.Name);
            Assert.AreEqual(9, updated.Rating);
        }

        [TestMethod]
        public void RemovePartner_ExistingPartner_RemovedFromCollection()
        {
            var partner = new KataevPartner { Name = "Удаляемый" };
            _app.AddPartner(partner);

            _app.RemovePartner(partner);

            Assert.AreEqual(0, _app.GetPartners().Count());
        }

        [TestMethod]
        public void RemovePartner_OtherPartnersNotAffected()
        {
            var p1 = new KataevPartner { Name = "П1" };
            var p2 = new KataevPartner { Name = "П2" };
            _app.AddPartner(p1);
            _app.AddPartner(p2);

            _app.RemovePartner(p1);

            var remaining = _app.GetPartners().ToList();
            Assert.AreEqual(1, remaining.Count);
            Assert.AreEqual("П2", remaining[0].Name);
        }


        [TestMethod]
        public void AddProduct_ValidProduct_SavedToCollection()
        {
            var product = new KataevProduct
            {
                Name = "Ламинат",
                Article = "ART-001",
                Type = "Напольное покрытие"
            };

            _app.AddProduct(product);

            var result = _app.GetProducts().ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Ламинат", result[0].Name);
            Assert.AreEqual("ART-001", result[0].Article);
        }

        [TestMethod]
        public void GetProducts_NoProducts_ReturnsEmpty()
        {
            Assert.AreEqual(0, _app.GetProducts().Count());
        }

        [TestMethod]
        public void GetProducts_MultipleProducts_ReturnsAll()
        {
            _app.AddProduct(new KataevProduct { Name = "Продукт А" });
            _app.AddProduct(new KataevProduct { Name = "Продукт Б" });

            Assert.AreEqual(2, _app.GetProducts().Count());
        }

        [TestMethod]
        public void GetProduct_ExistingId_ReturnsCorrectProduct()
        {
            var product = new KataevProduct { Name = "Паркет", Type = "Дерево" };
            _app.AddProduct(product);

            var result = _app.GetProduct(product.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Паркет", result.Name);
            Assert.AreEqual("Дерево", result.Type);
        }

        [TestMethod]
        public void GetProduct_NonExistingId_ReturnsNull()
        {
            Assert.IsNull(_app.GetProduct(9999));
        }

        [TestMethod]
        public void UpdateProduct_ChangeTypeAndArticle_Updated()
        {
            var product = new KataevProduct { Name = "Плитка", Type = "Старый", Article = "OLD" };
            _app.AddProduct(product);

            product.Type = "Керамика";
            product.Article = "NEW-002";
            _app.UpdateProduct(product);

            var updated = _app.GetProduct(product.Id);
            Assert.AreEqual("Керамика", updated.Type);
            Assert.AreEqual("NEW-002", updated.Article);
        }

        [TestMethod]
        public void RemoveProduct_ExistingProduct_RemovedFromCollection()
        {
            var product = new KataevProduct { Name = "Удаляемый" };
            _app.AddProduct(product);

            _app.RemoveProduct(product);

            Assert.AreEqual(0, _app.GetProducts().Count());
        }

        [TestMethod]
        public void AddSalesHistory_ValidPartner_RecordSaved()
        {
            var partner = new KataevPartner { Name = "Партнёр" };
            _app.AddPartner(partner);

            var history = new KataevSalesHistory
            {
                PartnerId = partner.Id,
                Quantity = 100,
                SaleDate = DateTime.Today
            };

            _app.AddSalesHistory(history);

            Assert.AreEqual(1, _app.GetSalesHistory(partner.Id).Count());
        }

        [TestMethod]
        public void AddSalesHistory_InvalidPartnerId_ThrowsException()
        {
            var history = new KataevSalesHistory { PartnerId = 9999, Quantity = 50 };

            bool exceptionThrown = false;
            try
            {
                _app.AddSalesHistory(history);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public void GetSalesHistory_ByPartnerId_ReturnsOnlyThatPartnersRecords()
        {
            var p1 = new KataevPartner { Name = "П1" };
            var p2 = new KataevPartner { Name = "П2" };
            _app.AddPartner(p1);
            _app.AddPartner(p2);

            _app.AddSalesHistory(new KataevSalesHistory { PartnerId = p1.Id, Quantity = 10 });
            _app.AddSalesHistory(new KataevSalesHistory { PartnerId = p1.Id, Quantity = 20 });
            _app.AddSalesHistory(new KataevSalesHistory { PartnerId = p2.Id, Quantity = 30 });

            var result = _app.GetSalesHistory(p1.Id).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(sh => sh.PartnerId == p1.Id));
        }

        [TestMethod]
        public void GetSalesHistory_ByPartnerObject_SameResultAsById()
        {
            var partner = new KataevPartner { Name = "Партнёр" };
            _app.AddPartner(partner);
            _app.AddSalesHistory(new KataevSalesHistory { PartnerId = partner.Id, Quantity = 5 });

            var byId = _app.GetSalesHistory(partner.Id).ToList();
            var byObj = _app.GetSalesHistory(partner).ToList();

            Assert.AreEqual(byId.Count, byObj.Count);
        }

        [TestMethod]
        public void GetSalesHistory_NoRecords_ReturnsEmpty()
        {
            var partner = new KataevPartner { Name = "Пустой" };
            _app.AddPartner(partner);

            Assert.AreEqual(0, _app.GetSalesHistory(partner.Id).Count());
        }

        [TestMethod]
        public void UpdateSalesHistory_ChangeQuantity_Updated()
        {
            var partner = new KataevPartner { Name = "П" };
            _app.AddPartner(partner);

            var history = new KataevSalesHistory { PartnerId = partner.Id, Quantity = 10 };
            _app.AddSalesHistory(history);

            history.Quantity = 999;
            _app.UpdateSalesHistory(history);

            Assert.AreEqual(999, _app.GetSalesHistory(partner.Id).First().Quantity);
        }

        [TestMethod]
        public void RemoveSalesHistory_ExistingRecord_RemovedFromCollection()
        {
            var partner = new KataevPartner { Name = "П" };
            _app.AddPartner(partner);

            var history = new KataevSalesHistory { PartnerId = partner.Id, Quantity = 10 };
            _app.AddSalesHistory(history);

            _app.RemoveSalesHistory(history);

            Assert.AreEqual(0, _app.GetSalesHistory(partner.Id).Count());
        }

        private KataevPartner CreatePartnerWithSales(int quantity)
        {
            var partner = new KataevPartner { Name = $"П_{Guid.NewGuid()}" };
            _app.AddPartner(partner);
            _app.AddSalesHistory(new KataevSalesHistory { PartnerId = partner.Id, Quantity = quantity });
            return partner;
        }

        [TestMethod]
        public void CalculateDiscount_QuantityBelow10000_Returns0()
            => Assert.AreEqual(0, _app.CalculatePartnerDiscount(CreatePartnerWithSales(9999).Id));

        [TestMethod]
        public void CalculateDiscount_QuantityExactly10000_Returns5()
            => Assert.AreEqual(5, _app.CalculatePartnerDiscount(CreatePartnerWithSales(10000).Id));

        [TestMethod]
        public void CalculateDiscount_QuantityBetween10000And50000_Returns5()
            => Assert.AreEqual(5, _app.CalculatePartnerDiscount(CreatePartnerWithSales(30000).Id));

        [TestMethod]
        public void CalculateDiscount_QuantityExactly50000_Returns10()
            => Assert.AreEqual(10, _app.CalculatePartnerDiscount(CreatePartnerWithSales(50000).Id));

        [TestMethod]
        public void CalculateDiscount_QuantityBetween50000And300000_Returns10()
            => Assert.AreEqual(10, _app.CalculatePartnerDiscount(CreatePartnerWithSales(150000).Id));

        [TestMethod]
        public void CalculateDiscount_QuantityExactly300000_Returns15()
            => Assert.AreEqual(15, _app.CalculatePartnerDiscount(CreatePartnerWithSales(300000).Id));

        [TestMethod]
        public void CalculateDiscount_QuantityAbove300000_Returns15()
            => Assert.AreEqual(15, _app.CalculatePartnerDiscount(CreatePartnerWithSales(500000).Id));

        [TestMethod]
        public void CalculateDiscount_MultipleRecordsSummed_CorrectDiscount()
        {
            var partner = new KataevPartner { Name = "Мульти" };
            _app.AddPartner(partner);
            _app.AddSalesHistory(new KataevSalesHistory { PartnerId = partner.Id, Quantity = 20000 });
            _app.AddSalesHistory(new KataevSalesHistory { PartnerId = partner.Id, Quantity = 20000 });
            _app.AddSalesHistory(new KataevSalesHistory { PartnerId = partner.Id, Quantity = 20000 });

            Assert.AreEqual(10, _app.CalculatePartnerDiscount(partner.Id));
        }

        [TestMethod]
        public void CalculateDiscount_NoSalesHistory_Returns0()
        {
            var partner = new KataevPartner { Name = "Без продаж" };
            _app.AddPartner(partner);

            Assert.AreEqual(0, _app.CalculatePartnerDiscount(partner.Id));
        }
    }
}