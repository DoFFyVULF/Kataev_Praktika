using KataevLIB;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace KataevWPF
{
    public partial class KataevOrderEditWindow : Window
    {
        private readonly IMainApp _app;
        private readonly KataevPartner _partner;
        private readonly KataevSalesHistory _editingOrder;
        private readonly bool _isEditMode;
        private bool _canProceed = true; 

        public KataevOrderEditWindow(IMainApp app, KataevPartner partner, KataevSalesHistory order)
        {
            InitializeComponent();

            _app = app;
            _partner = partner;
            _editingOrder = order;
            _isEditMode = (order != null);

            this.Title = _isEditMode
                ? $"Редактирование заказа: {_partner.Name}"
                : $"Новый заказ для: {_partner.Name}";

            this.Loaded += KataevOrderEditWindow_Loaded;

            LoadDataIfEdit();
        }

        private void KataevOrderEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!LoadProducts())
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private bool LoadProducts()
        {
            try
            {
                var products = _app.GetProducts().ToList();

                if (products.Count == 0)
                {
                    MessageBox.Show(
                        "Список продукции пуст.\nСначала добавьте товары в базу данных.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return false; 
                }

                CbProducts.ItemsSource = products;
                if (!_isEditMode)
                {
                    CbProducts.SelectedIndex = 0;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продукции: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void LoadDataIfEdit()
        {
            if (_isEditMode && _editingOrder != null)
            {
                CbProducts.SelectedValue = _editingOrder.ProductId;

                TxtQuantity.Text = _editingOrder.Quantity.ToString();
                DpDate.SelectedDate = _editingOrder.SaleDate;
            }
            else
            {
                DpDate.SelectedDate = DateTime.Today;
            }
        }

        private void TxtQuantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CbProducts.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите продукцию из списка.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                CbProducts.Focus();
                return;
            }
            if (!int.TryParse(TxtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Количество должно быть целым положительным числом.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtQuantity.Focus();
                return;
            }
            if (!DpDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату продажи.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                DpDate.Focus();
                return;
            }

            try
            {
                if (_isEditMode)
                {
                    _editingOrder.ProductId = (int)CbProducts.SelectedValue;
                    _editingOrder.Quantity = quantity;
                    _editingOrder.SaleDate = DpDate.SelectedDate.Value;

                    _app.UpdateSalesHistory(_editingOrder);
                }
                else
                {
                    var newOrder = new KataevSalesHistory
                    {
                        PartnerId = _partner.Id,
                        ProductId = (int)CbProducts.SelectedValue,
                        Quantity = quantity,
                        SaleDate = DpDate.SelectedDate.Value
                    };

                    _app.AddSalesHistory(newOrder);
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось сохранить заказ:\n{ex.Message}\n\nПроверьте подключение к БД и целостность данных.",
                    "Ошибка сохранения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}