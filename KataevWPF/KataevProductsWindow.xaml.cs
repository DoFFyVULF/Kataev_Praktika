using KataevLIB;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KataevWPF
{
    public partial class KataevProductsWindow : Window
    {
        private readonly IMainApp _app;

        public KataevProductsWindow(IMainApp app)
        {
            InitializeComponent();
            _app = app ?? throw new ArgumentNullException(nameof(app));
            LoadProducts();
            ProductsGrid.SelectionChanged += ProductsGrid_SelectionChanged;
        }

        private void LoadProducts()
        {
            try
            {
                var products = _app.GetProducts().ToList();
                ProductsGrid.ItemsSource = products;

                BtnEdit.IsEnabled = false;
                BtnDelete.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продукции:\n{ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = ProductsGrid.SelectedItem != null;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var editWin = new KataevProductEditWindow(_app, null);
            if (editWin.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is KataevProduct product)
            {
                var editWin = new KataevProductEditWindow(_app, product);
                if (editWin.ShowDialog() == true)
                {
                    LoadProducts();
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is KataevProduct product)
            {
                string message = $"Удалить продукт \"{product.Name}\" (артикул {product.Article})?\n" +
                                 "Все связанные записи о продажах также будут удалены.";

                if (MessageBox.Show(message, "Подтверждение удаления",
                                   MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _app.RemoveProduct(product);
                        _app.SaveChanges();
                        LoadProducts();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления:\n{ex.Message}",
                                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}