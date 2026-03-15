using KataevLIB;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KataevWPF
{
    public partial class KataevPartnerEditWindow : Window
    {
        private readonly IMainApp _appService;
        private readonly KataevPartner _editingPartner; 
        private bool _isEditMode;

        public KataevPartnerEditWindow(IMainApp appService, KataevPartner partner)
        {
            InitializeComponent();
            _appService = appService;
            _editingPartner = partner;
            _isEditMode = (partner != null);

            this.Title = _isEditMode
                ? $"Редактирование партнера: {partner.Name} — Катаев"
                : "Добавление нового партнера — Катаев";

            if (_isEditMode)
            {
                LoadPartnerData();
            }
        }
        private void LoadPartnerData()
        {
            if (_editingPartner == null) return;

            KataevNameBox.Text = _editingPartner.Name;

            // Выбор типа из ComboBox
            string typeToSelect = _editingPartner.PartnerType;
            foreach (ComboBoxItem item in KataevTypeBox.Items)
            {
                if (item.Content.ToString() == typeToSelect)
                {
                    item.IsSelected = true;
                    break;
                }
            }

            KataevRatingBox.Text = _editingPartner.Rating.ToString();
            KataevInnBox.Text = _editingPartner.INN;
            KataevAddressBox.Text = _editingPartner.Address;
            KataevDirLastBox.Text = _editingPartner.DirectorLastName;
            KataevDirFirstBox.Text = _editingPartner.DirectorFirstName;
            KataevDirMidBox.Text = _editingPartner.DirectorMiddleName;
            KataevPhoneBox.Text = _editingPartner.Phone;
            KataevEmailBox.Text = _editingPartner.Email;
        }
        private void KataevRatingBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void KataevSaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(KataevNameBox.Text))
                {
                    MessageBox.Show("Поле «Наименование» является обязательным.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    KataevNameBox.Focus();
                    return;
                }

                if (!int.TryParse(KataevRatingBox.Text, out int rating) || rating < 0)
                {
                    MessageBox.Show("Рейтинг должен быть целым неотрицательным числом.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    KataevRatingBox.Focus();
                    return;
                }

                if (_isEditMode)
                {
                    _editingPartner.Name = KataevNameBox.Text;
                    _editingPartner.PartnerType = ((ComboBoxItem)KataevTypeBox.SelectedItem).Content.ToString();
                    _editingPartner.Rating = rating;
                    _editingPartner.INN = KataevInnBox.Text;
                    _editingPartner.Address = KataevAddressBox.Text;
                    _editingPartner.DirectorLastName = KataevDirLastBox.Text;
                    _editingPartner.DirectorFirstName = KataevDirFirstBox.Text;
                    _editingPartner.DirectorMiddleName = KataevDirMidBox.Text;
                    _editingPartner.Phone = KataevPhoneBox.Text;
                    _editingPartner.Email = KataevEmailBox.Text;

                    _appService.UpdatePartner(_editingPartner);
                }
                else
                {
                    var newPartner = new KataevPartner
                    {
                        Name = KataevNameBox.Text,
                        PartnerType = ((ComboBoxItem)KataevTypeBox.SelectedItem).Content.ToString(),
                        Rating = rating,
                        INN = KataevInnBox.Text,
                        Address = KataevAddressBox.Text,
                        DirectorLastName = KataevDirLastBox.Text,
                        DirectorFirstName = KataevDirFirstBox.Text,
                        DirectorMiddleName = KataevDirMidBox.Text,
                        Phone = KataevPhoneBox.Text,
                        Email = KataevEmailBox.Text
                    };

                    _appService.AddPartner(newPartner);
                }

                _appService.SaveChanges();

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Произошла ошибка при сохранении данных:\n{ex.Message}\n\nПроверьте корректность введенных данных и подключение к БД.",
                    "Ошибка сохранения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void KataevCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}