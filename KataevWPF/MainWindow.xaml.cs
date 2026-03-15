using KataevLIB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KataevWPF
{
    public partial class MainWindow : Window
    {
        private readonly MainApp app;
        private List<KataevSalesHistory> _currentOrdersCache;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                app = new MainApp();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске:\n{ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void KataevWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPartners();
        }

        private void LoadPartners()
        {
            try
            {
                var partners = app.GetPartners().ToList();
                KataevPartnersList.ItemsSource = partners;
                UpdatePartnersCount(partners.Count);

                DetailsPanel.Visibility = Visibility.Collapsed;
                PlaceholderText.Visibility = Visibility.Visible;
                BtnEditPartner.IsEnabled = false;
                BtnDeletePartner.IsEnabled = false;
                KataevPartnersList.SelectedItem = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить партнеров:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void KataevPartnersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPartner = KataevPartnersList.SelectedItem as KataevPartner;

            if (selectedPartner != null)
            {
                BtnEditPartner.IsEnabled = true;
                BtnDeletePartner.IsEnabled = true;
                PlaceholderText.Visibility = Visibility.Collapsed;
                DetailsPanel.Visibility = Visibility.Visible;

                LoadPartnerDetails(selectedPartner);
                LoadOrders(selectedPartner);

                Keyboard.Focus(KataevPartnersList);
            }
            else
            {
                BtnEditPartner.IsEnabled = false;
                BtnDeletePartner.IsEnabled = false;
                DetailsPanel.Visibility = Visibility.Collapsed;
                PlaceholderText.Visibility = Visibility.Visible;
            }

            BtnEditOrder.IsEnabled = false;
            BtnDeleteOrder.IsEnabled = false;
        }

        private void LoadPartnerDetails(KataevPartner partner)
        {
            LblPartnerName.Text = partner.Name;
            LblPartnerType.Text = partner.PartnerType;
            LblPhone.Text = partner.Phone;
            LblEmail.Text = partner.Email;
            LblAddress.Text = partner.Address;

            int totalVolume = app.GetSalesHistory(partner.Id).Sum(s => s.Quantity);
            LblTotalVolume.Text = string.Format("{0:N0} ед.", totalVolume);

            int discount = app.CalculatePartnerDiscount(partner.Id);
            LblDiscount.Text = discount + "%";

            Color color;
            if (discount == 0)
                color = Color.FromRgb(128, 128, 128);
            else if (discount == 5)
                color = Color.FromRgb(33, 150, 243);
            else if (discount == 10)
                color = Color.FromRgb(76, 175, 80);
            else if (discount == 15)
                color = Color.FromRgb(255, 193, 7);
            else
                color = Colors.Gray;

            BadgeDiscount.Background = new SolidColorBrush(color);
        }

        private void LoadOrders(KataevPartner partner)
        {
            try
            {
                _currentOrdersCache = app.GetSalesHistory(partner.Id).ToList();
                OrdersGrid.ItemsSource = null;
                OrdersGrid.ItemsSource = _currentOrdersCache;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории продаж:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Партнеры CRUD

        private void KataevAddButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new KataevPartnerEditWindow(app, null);
            if (win.ShowDialog() == true)
            {
                app.SaveChanges();
                LoadPartners();
            }
        }

        private void KataevEditButton_Click(object sender, RoutedEventArgs e)
        {
            var p = KataevPartnersList.SelectedItem as KataevPartner;
            if (p == null) return;

            var win = new KataevPartnerEditWindow(app, p);
            if (win.ShowDialog() == true)
            {
                app.SaveChanges();
                var currentId = p.Id;
                LoadPartners();
                var refreshed = app.GetPartners().FirstOrDefault(x => x.Id == currentId);
                if (refreshed != null)
                {
                    KataevPartnersList.SelectedItem = refreshed;
                    KataevPartnersList.ScrollIntoView(refreshed);
                }
            }
        }

        private void KataevDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var p = KataevPartnersList.SelectedItem as KataevPartner;
            if (p == null) return;

            var result = MessageBox.Show(
                "Удалить партнера \"" + p.Name + "\" и всю историю продаж?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                app.RemovePartner(p);
                app.SaveChanges();
                LoadPartners();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Заказы CRUD

        private void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = OrdersGrid.SelectedItem != null;
            BtnEditOrder.IsEnabled = hasSelection;
            BtnDeleteOrder.IsEnabled = hasSelection;
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            var partner = KataevPartnersList.SelectedItem as KataevPartner;
            if (partner == null) return;

            var win = new KataevOrderEditWindow(app, partner, null);
            if (win.ShowDialog() == true)
            {
                app.SaveChanges();
                LoadOrders(partner);
                LoadPartnerDetails(partner);
            }
        }

        private void BtnEditOrder_Click(object sender, RoutedEventArgs e)
        {
            var partner = KataevPartnersList.SelectedItem as KataevPartner;
            var order = OrdersGrid.SelectedItem as KataevSalesHistory;
            if (partner == null || order == null) return;

            var win = new KataevOrderEditWindow(app, partner, order);
            if (win.ShowDialog() == true)
            {
                app.SaveChanges();
                LoadOrders(partner);
                LoadPartnerDetails(partner);
            }
        }

        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            var partner = KataevPartnersList.SelectedItem as KataevPartner;
            var order = OrdersGrid.SelectedItem as KataevSalesHistory;
            if (partner == null || order == null) return;

            var result = MessageBox.Show("Удалить эту запись о продаже?",
                                         "Подтверждение",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                app.RemoveSalesHistory(order);
                app.SaveChanges();
                LoadOrders(partner);
                LoadPartnerDetails(partner);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private void PartnerCardAddOrder_Click(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var partner = element.DataContext as KataevPartner;
            if (partner == null) return;

            KataevPartnersList.SelectedItem = partner;

            var win = new KataevOrderEditWindow(app, partner, null);
            if (win.ShowDialog() == true)
            {
                app.SaveChanges();
                LoadOrders(partner);
                LoadPartnerDetails(partner);
            }
        }

        private void BtnManageProducts_Click(object sender, RoutedEventArgs e)
        {
            var window = new KataevProductsWindow(app);
            window.Owner = this;
            window.ShowDialog();
        }

        private void UpdatePartnersCount(int count)
        {
            KataevPartnersCountText.Text = "Партнеров: " + count;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (app != null) app.Dispose();
        }
    }
}