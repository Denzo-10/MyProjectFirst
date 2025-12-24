using DataLayer.DTOs;
using DataLayer.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;

namespace DesktopApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<ProductWPFDto> _products = new();
        private List<ProductWPFDto> _allProducts = new();
        private User _currentUser;
        private ICollectionView _productsView;
        private string _currentSortProperty = "";
        private ListSortDirection _currentSortDirection = ListSortDirection.Ascending;
        private ProductWPFDto _selectedProduct;
        private Border _selectedBorder;

        public bool IsManagerOrAdmin { get; private set; }
        public ProductWPFDto SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                UpdateButtonsState();
            }
        }

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            DataContext = this;
            UpdateUserInfo();

            // Инициализируем видимость элементов
            txtNoProducts.Visibility = Visibility.Collapsed;
            itemsProducts.Visibility = Visibility.Visible;

            // Инициализируем состояние кнопок
            UpdateButtonsState();

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void UpdateUserInfo()
        {
            IsManagerOrAdmin = _currentUser?.Role?.Name == "Менеджер" ||
                              _currentUser?.Role?.Name == "Администратор";

            DataContext = new
            {
                UserInfo = $"{_currentUser?.FullName} ({_currentUser?.Role?.Name})",
                IsManagerOrAdmin = IsManagerOrAdmin
            };
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Загрузка производителей
                var manufacturers = await App.ProductService.GetManufacturersAsync();
                cbManufacturer.Items.Clear();
                cbManufacturer.Items.Add(new Manufacturer { ManufacturerId = 0, Name = "Все производители" });
                foreach (var manufacturer in manufacturers)
                {
                    cbManufacturer.Items.Add(manufacturer);
                }
                cbManufacturer.DisplayMemberPath = "Name";
                cbManufacturer.SelectedIndex = 0;

                // Загрузка товаров
                _allProducts = await App.ProductService.GetProductsAsync();

                // Устанавливаем путь к папке с изображениями для каждого товара
                foreach (var product in _allProducts)
                {
                    product.SetImagesFolderPath(App.ImagesFolder);
                }

                ApplyFilters();

                // Обновляем максимальную цену
                UpdateMaxPriceLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateMaxPriceLabel()
        {
            if (_allProducts.Any())
            {
                var maxPrice = _allProducts.Max(p => p.Price);
                lblMaxPrice.Text = maxPrice.ToString("N2");
            }
            else
            {
                lblMaxPrice.Text = "0";
            }
        }

        private void ApplyFilters()
        {
            // Сброс выделения при фильтрации
            SelectedProduct = null;
            ClearSelection();

            var filtered = _allProducts.AsEnumerable();

            // Поиск по описанию
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string search = txtSearch.Text.ToLower();
                filtered = filtered.Where(p =>
                    (p.Description ?? "").ToLower().Contains(search) ||
                    (p.Name ?? "").ToLower().Contains(search));
            }

            // Фильтр по производителю
            if (cbManufacturer.SelectedItem is Manufacturer selected && selected.ManufacturerId > 0)
            {
                filtered = filtered.Where(p => p.ManufacturerId == selected.ManufacturerId);
            }

            // Фильтр по максимальной цене
            if (decimal.TryParse(txtMaxPrice.Text, out decimal maxPrice) && maxPrice > 0)
            {
                filtered = filtered.Where(p => p.Price <= maxPrice);
            }

            // Только со скидкой
            if (cbDiscounted.IsChecked == true)
            {
                filtered = filtered.Where(p => p.HasDiscount);
            }

            // Только в наличии
            if (cbInStock.IsChecked == true)
            {
                filtered = filtered.Where(p => p.InStock);
            }

            // Обновляем коллекцию
            _products.Clear();
            foreach (var product in filtered)
            {
                _products.Add(product);
            }

            // Создаем или обновляем представление для сортировки
            _productsView = CollectionViewSource.GetDefaultView(_products);

            // Применяем текущую сортировку
            ApplyCurrentSort();

            // Показываем/скрываем сообщение об отсутствии товаров
            txtNoProducts.Visibility = _products.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            itemsProducts.Visibility = _products.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            itemsProducts.ItemsSource = _productsView;
        }

        private void ApplyCurrentSort()
        {
            if (_productsView != null && !string.IsNullOrEmpty(_currentSortProperty))
            {
                _productsView.SortDescriptions.Clear();
                _productsView.SortDescriptions.Add(
                    new SortDescription(_currentSortProperty, _currentSortDirection));
            }
        }

        // Обработчики фильтров
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void FilterChanged(object sender, RoutedEventArgs e) => ApplyFilters();
        private void ApplyFilters_Click(object sender, RoutedEventArgs e) => ApplyFilters();

        // Методы сортировки
        private void SortByNameAsc_Click(object sender, RoutedEventArgs e)
        {
            _currentSortProperty = "Name";
            _currentSortDirection = ListSortDirection.Ascending;
            ApplyCurrentSort();
        }

        private void SortByNameDesc_Click(object sender, RoutedEventArgs e)
        {
            _currentSortProperty = "Name";
            _currentSortDirection = ListSortDirection.Descending;
            ApplyCurrentSort();
        }

        private void SortBySupplierAsc_Click(object sender, RoutedEventArgs e)
        {
            _currentSortProperty = "SupplierName";
            _currentSortDirection = ListSortDirection.Ascending;
            ApplyCurrentSort();
        }

        private void SortBySupplierDesc_Click(object sender, RoutedEventArgs e)
        {
            _currentSortProperty = "SupplierName";
            _currentSortDirection = ListSortDirection.Descending;
            ApplyCurrentSort();
        }

        private void SortByPriceAsc_Click(object sender, RoutedEventArgs e)
        {
            _currentSortProperty = "Price";
            _currentSortDirection = ListSortDirection.Ascending;
            ApplyCurrentSort();
        }

        private void SortByPriceDesc_Click(object sender, RoutedEventArgs e)
        {
            _currentSortProperty = "Price";
            _currentSortDirection = ListSortDirection.Descending;
            ApplyCurrentSort();
        }

        // Обработчик клика по товару
        private void ProductItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ProductWPFDto product)
            {
                // Снимаем выделение с предыдущего элемента
                ClearSelection();

                // Выделяем текущий элемент
                SelectedProduct = product;
                _selectedBorder = border;
                border.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 33, 150, 243));
                border.BorderBrush = System.Windows.Media.Brushes.DodgerBlue;
                border.BorderThickness = new Thickness(2);
            }
        }

        private void ClearSelection()
        {
            if (_selectedBorder != null)
            {
                _selectedBorder.Background = System.Windows.Media.Brushes.White;
                _selectedBorder.BorderBrush = System.Windows.Media.Brushes.LightGray;
                _selectedBorder.BorderThickness = new Thickness(1);
                _selectedBorder = null;
            }
        }

        // Метод обновления состояния кнопок
        private void UpdateButtonsState()
        {
            bool canEditDelete = IsManagerOrAdmin && SelectedProduct != null;
            btnEdit.IsEnabled = canEditDelete;
            btnDelete.IsEnabled = canEditDelete;
        }

        // Кнопка выхода
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Выйти из системы?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Скрываем текущее окно
                this.Hide();

                // Создаем новое окно входа
                var loginWindow = new LoginWindow();

                // Показываем окно входа как диалоговое окно
                bool? loginResult = loginWindow.ShowDialog();

                if (loginResult == true && loginWindow.CurrentUser != null)
                {
                    // Успешный вход с другим пользователем
                    _currentUser = loginWindow.CurrentUser;
                    UpdateUserInfo();
                    LoadDataAsync();

                    // Показываем окно снова
                    this.Show();
                }
                else
                {
                    // Вход отменен или неудачен - закрываем приложение
                    Application.Current.Shutdown();
                }
            }
        }

        // Кнопки управления товарами
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new ProductEditWindow();
            editWindow.Owner = this;

            if (editWindow.ShowDialog() == true)
            {
                LoadDataAsync();
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Для редактирования выберите товар из списка", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var editWindow = new ProductEditWindow(SelectedProduct);
            editWindow.Owner = this;

            if (editWindow.ShowDialog() == true)
            {
                LoadDataAsync();
            }
        }

        private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Для удаления выберите товар из списка", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Вы действительно хотите удалить товар \"{SelectedProduct.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await App.ProductService.DeleteProductAsync(SelectedProduct.ProductId);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обработчик закрытия окна
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            var result = MessageBox.Show("Закрыть приложение?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}