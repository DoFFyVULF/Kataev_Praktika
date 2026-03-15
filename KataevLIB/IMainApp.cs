using System.Collections.Generic;
using System.Threading.Tasks;

namespace KataevLIB
{
    public interface IMainApp
    {
        ApplicationContext GetContext();
        void SaveChanges();
        void SetupAppSchema();

        void AddPartner(KataevPartner partner);
        void UpdatePartner(KataevPartner partner);
        void RemovePartner(KataevPartner partner);
        IEnumerable<KataevPartner> GetPartners();
        KataevPartner GetPartner(int partnerId);
        IEnumerable<KataevPartner> GetPartners(string searchTerm);

        void AddProduct(KataevProduct product);
        void UpdateProduct(KataevProduct product);
        void RemoveProduct(KataevProduct product);
        IEnumerable<KataevProduct> GetProducts();
        KataevProduct GetProduct(int productId);

        void AddSalesHistory(KataevSalesHistory salesHistory);
        void UpdateSalesHistory(KataevSalesHistory salesHistory);
        void RemoveSalesHistory(KataevSalesHistory salesHistory);
        IEnumerable<KataevSalesHistory> GetSalesHistory(KataevPartner partner);
        IEnumerable<KataevSalesHistory> GetSalesHistory(int partnerId);

        int CalculatePartnerDiscount(int partnerId);
    }
}