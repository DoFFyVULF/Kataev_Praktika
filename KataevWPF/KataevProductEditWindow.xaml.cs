using KataevLIB;
using System;
using System.Windows;

namespace KataevWPF
{
    public partial class KataevProductEditWindow : Window
    {
        private readonly IMainApp _app;
        private readonly KataevProduct _editingProduct;
        private readonly bool _isEdit;

        public KataevProductEditWindow(IMainApp app, KataevProduct productToEdit)
        {
            InitializeComponent();
            _app = app;
            _editingProduct = productToEdit;
            _isEdit = productToEdit != null;

            Title = _isEdit ? "Редактирование продукта" : "Новый продукт";

            if (_isEdit && _editingProduct != null)
            {
                TxtName.Text = _editingProduct.Name;
                TxtArticle.Text = _editingProduct.Article;
                TxtType.Text = _editingProduct.Type;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtName.Text?.Trim();
            string article = TxtArticle.Text?.Trim();
            string type = TxtType.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(article))
            {
                MessageBox.Show("Наименование и артикул обязательны.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEdit)
                {
                    _editingProduct.Name = name;
                    _editingProduct.Article = article;
                    _editingProduct.Type = type;
                    _app.UpdateProduct(_editingProduct);
                }
                else
                {
                    var newProduct = new KataevProduct
                    {
                        Name = name,
                        Article = article,
                        Type = type
                    };
                    _app.AddProduct(newProduct);
                }

                _app.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}