using DataLayer.Models;
using System.Windows;
using System.Windows.Input;

namespace DesktopApp.Views
{
    public partial class LoginWindow : Window
    {
        public User CurrentUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            txtLogin.Focus();

            // Обработчик нажатия кнопки Войти по Enter
            txtPassword.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    LoginButton_Click(s, e);
                }
            };
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Введите логин и пароль";
                return;
            }

            try
            {
                lblError.Text = "Проверка данных...";

                var user = await App.ProductService.AuthenticateUserAsync(login, password);

                if (user != null)
                {
                    CurrentUser = user;
                    // Устанавливаем DialogResult в true - окно закроется
                    DialogResult = true;
                }
                else
                {
                    lblError.Text = "Неверный логин или пароль";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Устанавливаем DialogResult в false - приложение закроется
            DialogResult = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, e);
            }
        }
    }
}