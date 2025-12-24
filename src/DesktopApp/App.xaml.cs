using DesktopApp.Views;
using DataLayer.Services;
using System;
using System.IO;
using System.Windows;

namespace DesktopApp
{
    public partial class App : Application
    {
        public static ProductServiceWPF ProductService { get; private set; }
        public static string ImagesFolder { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // Создание папки для изображений
                ImagesFolder = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Images");

                if (!Directory.Exists(ImagesFolder))
                {
                    Directory.CreateDirectory(ImagesFolder);
                }

                // Копируем стандартное изображение если его нет
                string defaultImage = Path.Combine(ImagesFolder, "picture.png");
                if (!File.Exists(defaultImage))
                {
                    // Создаем пустой файл или копируем из ресурсов
                    // Можно скопировать из исходных файлов, если они есть
                    try
                    {
                        // Попробуем найти в папке проекта
                        string sourceImage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Images", "picture.png");
                        if (File.Exists(sourceImage))
                        {
                            File.Copy(sourceImage, defaultImage);
                        }
                        else
                        {
                            // Создаем простой PNG файл программно
                            using (var fileStream = new FileStream(defaultImage, FileMode.Create))
                            {
                                
                            }
                        }
                    }
                    catch
                    {
                        // Если не удалось создать, оставляем как есть
                    }
                }

                // Инициализация сервиса
                ProductService = new ProductServiceWPF(ImagesFolder);

                // Показываем окно входа
                ShowLoginWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска приложения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void ShowLoginWindow()
        {
            var loginWindow = new LoginWindow();
            bool? result = loginWindow.ShowDialog();

            if (result == true && loginWindow.CurrentUser != null)
            {
                // Успешный вход - создаем главное окно
                var mainWindow = new MainWindow(loginWindow.CurrentUser);
                mainWindow.Show();
            }
            else
            {
                // Вход отменен или неудачен - закрываем приложение
                Shutdown();
            }
        }
    }
}