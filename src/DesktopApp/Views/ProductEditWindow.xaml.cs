using DataLayer.DTOs;
using DataLayer.Models;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace DesktopApp.Views
{
    public partial class ProductEditWindow : Window
    {
        private Product _product;
        private string _imagePath;

        public ProductEditWindow(ProductWPFDto productDto = null)
        {
            InitializeComponent();

            _product = productDto != null ? new Product
            {
                ProductId = productDto.ProductId,
                Article = productDto.Article,
                Name = productDto.Name,
                Price = productDto.Price,
                Discount = productDto.Discount ?? 0,
                StockQuantity = productDto.StockQuantity ?? 0,
                Unit = productDto.Unit,
                Description = productDto.Description,
                Photo = productDto.Photo,
                CategoryId = productDto.CategoryId,
                ManufacturerId = productDto.ManufacturerId,
                SupplierId = productDto.SupplierId
            } : new Product();

            Loaded += ProductEditWindow_Loaded;
        }

        private async void ProductEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadComboBoxes();

            if (_product.ProductId > 0)
            {
                // Режим редактирования
                Title = "Редактирование товара";
                txtArticle.Text = _product.Article;
                txtName.Text = _product.Name;
                txtPrice.Text = _product.Price.ToString("N2");
                txtDiscount.Text = _product.Discount.ToString();
                txtDescription.Text = _product.Description;

                // Загрузка изображения
                if (!string.IsNullOrEmpty(_product.Photo))
                {
                    // Проверяем существование файла
                    string photoPath = Path.Combine(App.ImagesFolder, _product.Photo);
                    if (File.Exists(photoPath))
                    {
                        _imagePath = photoPath;
                        imgPreview.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(photoPath));
                    }
                    else
                    {
                        // Если файл не найден, показываем заглушку
                        string defaultImagePath = Path.Combine(App.ImagesFolder, "picture.png");
                        if (File.Exists(defaultImagePath))
                        {
                            imgPreview.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(defaultImagePath));
                        }
                    }
                }
                else
                {
                    // Если фото нет в базе, показываем заглушку
                    string defaultImagePath = Path.Combine(App.ImagesFolder, "picture.png");
                    if (File.Exists(defaultImagePath))
                    {
                        imgPreview.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(defaultImagePath));
                    }
                }
            }
            else
            {
                Title = "Добавление товара";

                // Для нового товара показываем заглушку
                string defaultImagePath = Path.Combine(App.ImagesFolder, "picture.png");
                if (File.Exists(defaultImagePath))
                {
                    imgPreview.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(defaultImagePath));
                }
            }

            txtArticle.Focus();
        }

        private async Task LoadComboBoxes()
        {
            try
            {
                var categories = await App.ProductService.GetCategoriesAsync();
                cbCategory.ItemsSource = categories;
                cbCategory.DisplayMemberPath = "Name";
                cbCategory.SelectedValuePath = "CategoryId";

                var manufacturers = await App.ProductService.GetManufacturersAsync();
                cbManufacturer.ItemsSource = manufacturers;
                cbManufacturer.DisplayMemberPath = "Name";
                cbManufacturer.SelectedValuePath = "ManufacturerId";

                if (_product.ProductId > 0)
                {
                    cbCategory.SelectedValue = _product.CategoryId;
                    cbManufacturer.SelectedValue = _product.ManufacturerId;
                }
                else
                {
                    // Для нового товара выбираем первый элемент
                    if (categories.Any()) cbCategory.SelectedIndex = 0;
                    if (manufacturers.Any()) cbManufacturer.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Все файлы|*.*",
                Title = "Выберите изображение товара"
            };

            if (dialog.ShowDialog() == true)
            {
                _imagePath = dialog.FileName;
                imgPreview.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(_imagePath));
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lblError.Text = "";

                // Валидация
                if (string.IsNullOrWhiteSpace(txtArticle.Text))
                {
                    lblError.Text = "Введите артикул";
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    lblError.Text = "Введите название";
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
                {
                    lblError.Text = "Введите корректную цену";
                    return;
                }

                if (!byte.TryParse(txtDiscount.Text, out byte discount) || discount > 100)
                {
                    lblError.Text = "Скидка должна быть от 0 до 100%";
                    return;
                }

                // Сохранение изображения
                if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
                {
                    // Сохраняем изображение с именем на основе артикула
                    _product.Photo = await App.ProductService.SaveProductImageAsync(_imagePath, txtArticle.Text);
                }
                // Если редактируется существующий товар и изображение не выбрано, 
                // но в базе есть фото, оставляем старое фото
                else if (_product.ProductId > 0 && string.IsNullOrEmpty(_imagePath) && !string.IsNullOrEmpty(_product.Photo))
                {
                    // Старое фото сохраняется
                }
                // Если это новый товар и фото не выбрано
                else if (_product.ProductId == 0 && string.IsNullOrEmpty(_imagePath))
                {
                    _product.Photo = null; // Устанавливаем null, чтобы не пытаться загрузить несуществующее фото
                }

                // Заполнение данных
                _product.Article = txtArticle.Text.Trim();
                _product.Name = txtName.Text.Trim();
                _product.Price = price;
                _product.Discount = discount;
                _product.Description = txtDescription.Text.Trim();
                _product.Unit = "шт.";

                // Устанавливаем поставщика по умолчанию (можно изменить)
                if (_product.ProductId == 0) // Только для новых товаров
                {
                    _product.SupplierId = 1; // Поставщик по умолчанию
                }

                if (cbCategory.SelectedValue is int categoryId)
                    _product.CategoryId = categoryId;

                if (cbManufacturer.SelectedValue is int manufacturerId)
                    _product.ManufacturerId = manufacturerId;

                // Сохранение товара в базе
                await App.ProductService.SaveProductAsync(_product);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                lblError.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}