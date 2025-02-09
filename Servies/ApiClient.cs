using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json.Serialization;
using Admin.Models;



public class ApiClient
{
    static private readonly HttpClient _httpClient;
    static private string? _token;
    static public bool Auth { get; private set; } = false;
    static public string baseUrl = "https://31a1479d-f9d7-4c23-ad50-3d5349292c4c.tunnel4.com";

    static ApiClient()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    #region User
    public async Task<User?> GetCurrentUser()
    {
        return await GetAuthorizedAsync<User>("/api/user");
    }

    public async Task<string> GetRawResponse(string endpoint)
    {
        var response = await _httpClient.GetStringAsync(endpoint);
        return response;
    }

    public async Task<bool> Logout()
    {
        try
        {
            // Выполняем запрос на выход
            var response = await PostAuthorizedAsync<object, LogoutResponse>("/api/logout", null);

            // Если сообщение от сервера получено успешно
            if (response != null && response.Message == "Выход выполнен успешно.")
            {
                _token = null;
                Auth = false;
                return true;
            }

            // Если сообщение не соответствует ожидаемому
            Console.WriteLine("Не удалось выполнить выход пользователя. Ответ от сервера: " + (response?.Message ?? "Ответ пустой"));
            return false;
        }
        catch (Exception ex)
        {
            // Логируем ошибку
            Console.WriteLine("Произошла ошибка при выходе пользователя:");
            Console.WriteLine($"Ошибка: {ex.Message}");
            return false;
        }
    }



    public async Task<bool> RegisterUser(User user)
    {
        try
        {
            Console.WriteLine("Начинаем запрос для регистрации пользователя...");

            // Формируем URL для запроса регистрации
            var requestUrl = "/api/register";
            Console.WriteLine($"Формируем URL: {requestUrl}");

            // Создаем MultipartFormDataContent для отправки формы с файлом и текстовыми данными
            var formData = new MultipartFormDataContent();
            Console.WriteLine("Создаем MultipartFormDataContent для отправки данных...");

            // Добавляем текстовые поля
            formData.Add(new StringContent(user.Username), "username");
            formData.Add(new StringContent(user.Email), "email");
            Console.WriteLine($"Добавлены текстовые поля: Username = {user.Username}, Email = {user.Email}");

            // Если есть пароль, добавляем его
            if (!string.IsNullOrEmpty(user.Password))
            {
                formData.Add(new StringContent(user.Password), "password");
                Console.WriteLine("Добавлен пароль");
            }

            // Добавляем поле для подтверждения пароля
            if (!string.IsNullOrEmpty(user.Password_confirmation))
            {
                formData.Add(new StringContent(user.Password_confirmation), "password_confirmation");
                Console.WriteLine("Добавлено подтверждение пароля");
            }

            // Если есть пол, добавляем его
            if (!string.IsNullOrEmpty(user.Gender))
            {
                formData.Add(new StringContent(user.Gender), "gender");
                Console.WriteLine($"Добавлен пол: {user.Gender}");
            }

            // Проверяем, есть ли изображение (например, аватар), и добавляем его
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                var avatarFilePath = user.Avatar; // путь к изображению
                Console.WriteLine($"Добавляем аватар с пути: {avatarFilePath}");

                var fileContent = new ByteArrayContent(File.ReadAllBytes(avatarFilePath));
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");  // или другой формат изображения
                formData.Add(fileContent, "avatar", Path.GetFileName(avatarFilePath));
                Console.WriteLine("Аватар добавлен в форму");
            }
            else
            {
                Console.WriteLine("Аватар не был передан.");
            }

            // Выполняем POST-запрос на регистрацию
            Console.WriteLine("Отправка POST-запроса на регистрацию...");
            var response = await _httpClient.PostAsync(requestUrl, formData);

            // Проверяем, был ли запрос успешным
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Регистрация прошла успешно.");
                return true;
            }
            else
            {
                // Логируем подробности ответа
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ошибка при регистрации. Статус: {response.StatusCode}, Ответ: {responseBody}");
                return false;
            }
        }
        catch (Exception ex)
        {
            // Обработка ошибок
            Console.WriteLine($"Ошибка при регистрации пользователя: {ex.Message}");
            Console.WriteLine($"Стек вызовов: {ex.StackTrace}");

            // Дополнительные логи
            if (ex is FileNotFoundException)
            {
                Console.WriteLine("Ошибка: Не найден файл аватара.");
            }

            return false;
        }
    }



    #endregion

    #region Authentication
    public void SetToken(string token)
    {
        _token = token;
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        Auth = true;
    }

    public async Task Login(AuthRequest request)
    {
        var token = await PostAsync<AuthRequest, AuthResponse>("/login", request);
        if (token?.Token != null)
        {
            SetToken(token.Token);
            Console.WriteLine("User login successful.");
        }
        else
        {
            Console.WriteLine("User login failed.");
        }
    }

    public async Task Login()
    {
        ApiRequestAdmin request = new ApiRequestAdmin();

        request.Email = "AdminApp@example.com";
        request.Password = "AdminApp";

        var token = await PostAsync<ApiRequestAdmin, AuthResponse>("/api/login", request);
        if (token?.Token != null)
        {
            SetToken(token.Token);
            Console.WriteLine("Admin login successful.");
        }
        else
        {
            Console.WriteLine("Admin login failed.");
        }
    }

    public async Task<bool> Register(RegisterRequest request)
    {
        var response = await PostAsync<RegisterRequest, object>("/register", request);
        if (response != null)
        {
            Console.WriteLine("Registration successful, logging in...");
            //await Login(new AuthRequest { Username = request.Username, Password = request.Password }); У админа убрано
            return true;
        }
        Console.WriteLine("Registration failed.");
        return false;
    }



    #endregion

    #region Movies
    public async Task<List<Movie>?> GetMovies()
    {
        return await GetAuthorizedAsync<List<Movie>>("/api/movies");
    }

    public async Task<Movie?> GetMovieById(int movieId)
    {
        return await GetAuthorizedAsync<Movie>($"/api/movies/{movieId}");
    }

    public async Task<Movie?> UpdateMovie(Movie movie)
    {
        if (!Auth)
        {
            Console.WriteLine("[Ошибка] Пользователь не аутентифицирован.");
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        string filePath = movie.Photo;

        using var formData = new MultipartFormDataContent();
        FileStream fileStream = null;

        try
        {
            Console.WriteLine("[Отладка] Добавляем текстовые данные фильма...");
            formData.Add(new StringContent(movie.Title), "title");
            formData.Add(new StringContent(movie.Release_Year.ToString()), "release_year");
            formData.Add(new StringContent(movie.Duration.ToString()), "duration");
            formData.Add(new StringContent(movie.Description), "description");
            formData.Add(new StringContent(movie.Studio.Id.ToString()), "studio_id");
            formData.Add(new StringContent(movie.age_rating.Id.ToString()), "age_rating_id");
            Console.WriteLine("[Отладка] Текстовые данные успешно добавлены.");

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                Console.WriteLine($"[Отладка] Загружаем файл: {filePath}");
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var streamContent = new StreamContent(fileStream);
                formData.Add(streamContent, "photo", Path.GetFileName(filePath));
                Console.WriteLine("[Отладка] Файл добавлен в форму данных.");
            }
            else
            {
                Console.WriteLine("[Предупреждение] Файл не найден или путь пуст. Фильм будет обновлён без изменения изображения.");
            }

            Console.WriteLine("[Отладка] Отправляем HTTP-запрос...");
            var response = await _httpClient.PostAsync($"/api/movies/update/{movie.Id}", formData);

            if (fileStream != null)
            {
                Console.WriteLine("[Отладка] Закрываем поток файла...");
                fileStream.Close();
            }

            Console.WriteLine($"[Отладка] Ответ от сервера: {(int)response.StatusCode} {response.StatusCode}");
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("[Отладка] Тело ответа:");
            Console.WriteLine(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("[Ошибка] Не удалось обновить фильм.");
                throw new Exception($"Request failed with status code {response.StatusCode}. Response: {responseContent}");
            }

            Console.WriteLine("[Отладка] Десериализуем ответ...");
            var result = JsonSerializer.Deserialize<Movie>(responseContent);
            Console.WriteLine($"[Отладка] Фильм успешно обновлён: {result?.Title}");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Ошибка] Произошла ошибка при обновлении фильма:");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            throw;
        }
        finally
        {
            fileStream?.Dispose();
        }
    }

    public async Task<bool> DeleteMovie(int movieId)
    {
        return await DeleteAuthorizedAsync($"/api/movies/{movieId}");
    }

    public async Task<Movie?> AddMovie(Movie movie, string filePath)
    {
        if (!Auth)
        {
            Console.WriteLine("[Ошибка] Пользователь не аутентифицирован.");
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        using var formData = new MultipartFormDataContent();
        FileStream fileStream = null;

        try
        {
            Console.WriteLine("[Отладка] Добавляем текстовые данные фильма...");
            formData.Add(new StringContent(movie.Title), "title");
            formData.Add(new StringContent(movie.Release_Year.ToString()), "release_year");
            formData.Add(new StringContent(movie.Duration.ToString()), "duration");
            formData.Add(new StringContent(movie.Description), "description");
            formData.Add(new StringContent(movie.Studio.Id.ToString()), "studio_id");
            formData.Add(new StringContent(movie.age_rating.Id.ToString()), "age_rating_id");
            Console.WriteLine("[Отладка] Текстовые данные успешно добавлены.");

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                Console.WriteLine($"[Отладка] Загружаем файл: {filePath}");
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var streamContent = new StreamContent(fileStream);
                formData.Add(streamContent, "photo", Path.GetFileName(filePath));
                Console.WriteLine("[Отладка] Файл добавлен в форму данных.");
            }
            else
            {
                Console.WriteLine("[Предупреждение] Файл не найден или путь пуст. Фильм будет добавлен без изображения.");
            }

            Console.WriteLine("[Отладка] Отправляем HTTP-запрос...");
            var response = await _httpClient.PostAsync("/api/movies/create", formData);

            // Закрытие потока файла теперь гарантировано
            if (fileStream != null)
            {
                Console.WriteLine("[Отладка] Закрываем поток файла...");
                fileStream.Close();
            }

            Console.WriteLine($"[Отладка] Ответ от сервера: {(int)response.StatusCode} {response.StatusCode}");
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("[Отладка] Тело ответа:");
            Console.WriteLine(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("[Ошибка] Не удалось добавить фильм.");
                throw new Exception($"Request failed with status code {response.StatusCode}. Response: {responseContent}");
            }

            Console.WriteLine("[Отладка] Десериализуем ответ...");
            var result = JsonSerializer.Deserialize<Movie>(responseContent);
            Console.WriteLine($"[Отладка] Фильм успешно добавлен: {result?.Title}");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Ошибка] Произошла ошибка при добавлении фильма:");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            throw;
        }
        finally
        {
            fileStream?.Dispose();
        }
    }
    #endregion

    #region Actors
    public async Task<List<Actor>?> GetActors()
    {
        return await GetAuthorizedAsync<List<Actor>>("/api/actors");
    }

    public async Task<Actor?> GetActorById(int actorId)
    {
        return await GetAuthorizedAsync<Actor>($"/api/actors/{actorId}");
    }

    public async Task<Actor?> AddActor(Actor actor, string filePath)
    {
        if (!Auth)
        {
            Console.WriteLine("[Ошибка] Пользователь не аутентифицирован.");
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        using var formData = new MultipartFormDataContent();
        FileStream fileStream = null;

        try
        {
            Console.WriteLine("[Отладка] Добавляем текстовые данные актёра...");
            formData.Add(new StringContent(actor.first_name), "first_name");
            formData.Add(new StringContent(actor.last_name), "last_name");
            formData.Add(new StringContent(actor.birth_date.ToString("yyyy-MM-dd")), "birth_date");
            formData.Add(new StringContent(actor.biography ?? string.Empty), "biography");
            Console.WriteLine("[Отладка] Текстовые данные успешно добавлены.");

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                Console.WriteLine($"[Отладка] Загружаем файл: {filePath}");
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var streamContent = new StreamContent(fileStream);
                formData.Add(streamContent, "photo", Path.GetFileName(filePath));
                Console.WriteLine("[Отладка] Файл добавлен в форму данных.");
            }
            else
            {
                Console.WriteLine("[Предупреждение] Файл не найден или путь пуст. Актёр будет добавлен без изображения.");
            }

            Console.WriteLine("[Отладка] Отправляем HTTP-запрос...");
            var response = await _httpClient.PostAsync("/api/actors", formData);

            // Закрытие потока файла теперь гарантировано
            if (fileStream != null)
            {
                Console.WriteLine("[Отладка] Закрываем поток файла...");
                fileStream.Close();
            }

            Console.WriteLine($"[Отладка] Ответ от сервера: {(int)response.StatusCode} {response.StatusCode}");
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("[Отладка] Тело ответа:");
            Console.WriteLine(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("[Ошибка] Не удалось добавить актёра.");
                throw new Exception($"Request failed with status code {response.StatusCode}. Response: {responseContent}");
            }

            Console.WriteLine("[Отладка] Десериализуем ответ...");
            var result = JsonSerializer.Deserialize<Actor>(responseContent);
            Console.WriteLine($"[Отладка] Актёр успешно добавлен: {result?.first_name} {result?.last_name}");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Ошибка] Произошла ошибка при добавлении актёра:");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            throw;
        }
        finally
        {
            fileStream?.Dispose();
        }
    }

    public async Task<Actor?> UpdateActor(Actor actor)
    {
        if (!Auth)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        string filePath = actor.PhotoFilePath; // Предполагаем, что у актера есть путь к изображению, как у фильма

        using var formData = new MultipartFormDataContent();
        FileStream fileStream = null;

        try
        {
            // Добавляем текстовые данные актера в форму
            Console.WriteLine("[Отладка] Добавляем данные актера...");
            formData.Add(new StringContent(actor.first_name), "first_name");
            formData.Add(new StringContent(actor.last_name), "last_name"); // Например, фамилия актера, если она есть
            formData.Add(new StringContent(actor.birth_date.ToString("yyyy-MM-dd")), "birth_date");
            formData.Add(new StringContent(actor.biography), "biography");
            Console.WriteLine("[Отладка] Данные актера успешно добавлены.");

            // Если есть фото, добавляем его
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                Console.WriteLine($"[Отладка] Загружаем файл: {filePath}");
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var streamContent = new StreamContent(fileStream);
                formData.Add(streamContent, "photo", Path.GetFileName(filePath));
                Console.WriteLine("[Отладка] Фото актера добавлено в форму данных.");
            }
            else
            {
                Console.WriteLine("[Предупреждение] Фото не найдено или путь пуст. Актер будет обновлен без изменения фото.");
            }

            // Отправляем HTTP-запрос на обновление актера
            Console.WriteLine("[Отладка] Отправляем HTTP-запрос для обновления актера...");
            var response = await _httpClient.PostAsync($"/api/actors/update/{actor.Id}", formData);

            Console.WriteLine($"[Отладка] Ответ от сервера: {(int)response.StatusCode} {response.StatusCode}");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Сырой вывод ответа от сервера
            Console.WriteLine("[Отладка] Сырой ответ от сервера:");
            Console.WriteLine(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("[Ошибка] Не удалось обновить актера.");
                throw new Exception($"Request failed with status code {response.StatusCode}. Response: {responseContent}");
            }

            // Десериализуем ответ
            Console.WriteLine("[Отладка] Десериализуем ответ...");
            var result = JsonSerializer.Deserialize<Actor>(responseContent);
            Console.WriteLine($"[Отладка] Актер успешно обновлен: {result?.first_name}");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Ошибка] Произошла ошибка при обновлении актера:");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            throw;
        }
        finally
        {
            fileStream?.Dispose();
        }
    }

    public async Task<bool> DeleteActor(int actorId)
    {
        return await DeleteAuthorizedAsync($"/api/actors/{actorId}");
    }


    #endregion

    #region Reviews

    public async Task<List<MovieRating>?> GetReviews()
    {
        return await GetAuthorizedAsync<List<MovieRating>>("/api/ratings");
    }

    public async Task<MovieRating?> GetReviewById(int reviewId)
    {
        return await GetAuthorizedAsync<MovieRating>($"/api/ratings/{reviewId}");
    }

    public async Task<bool> DeleteReview(int reviewId)
    {
        return await DeleteAuthorizedAsync($"/api/ratings/{reviewId}");
    }

    public async Task<MovieRating?> AddMovieRating(MovieRating movieRating)
    {
        return await PostAuthorizedAsync<MovieRating, MovieRating>("/api/ratings", movieRating);
    }

    public async Task<MovieRating?> UpdateMovieRating(int ratingId, MovieRating movieRating)
    {
        return await PostAuthorizedAsync<MovieRating, MovieRating>($"/api/ratings/{ratingId}", movieRating);
    }

    #endregion

    #region Studios

    public async Task<List<Studio>?> GetStudios()
    {
        return await GetAuthorizedAsync<List<Studio>>("/api/studios");
    }

    public async Task<Studio?> GetStudioById(int studioId)
    {
        return await GetAuthorizedAsync<Studio>($"/api/studios/{studioId}");
    }

    public async Task<bool> DeleteStudio(int studioId)
    {
        return await DeleteAuthorizedAsync($"/api/studios/{studioId}");
    }

    public async Task<Studio?> AddStudio(Studio studio)
    {
        return await PostAuthorizedAsync<Studio, Studio>("/api/studios", studio);
    }

    public async Task<Studio?> UpdateStudio(Studio studio)
    {
        // Формируем URL для PUT-запроса с ID студии
        var url = $"/api/studios/{studio.Id}";

        // Отправляем PUT-запрос с обновлённой студией
        return await PostAuthorizedAsync<Studio, Studio>(url, studio);
    }


    #endregion

    #region Genre
    public async Task<List<Genre>?> GetGenres()
    {
        return await GetAuthorizedAsync<List<Genre>>("/api/genres");
    }

    public async Task<Genre?> GetGenreById(int genreId)
    {
        return await GetAuthorizedAsync<Genre>($"/api/genres/{genreId}");
    }

    public async Task<bool> DeleteGenre(int genreId)
    {
        return await DeleteAuthorizedAsync($"/api/genres/{genreId}");
    }

    public async Task<Genre?> AddGenre(Genre genre)
    {
        return await PostAuthorizedAsync<Genre, Genre>("/api/genres", genre);
    }

    public async Task<Genre?> UpdateGenre(int genreId, Genre genre)
    {
        try
        {
            // Путь для обновления жанра через POST
            var url = $"/api/genres/{genreId}";

            // Отправляем запрос на обновление жанра
            return await PostAuthorizedAsync<Genre, Genre>(url, genre); // Используем POST
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка при обновлении жанра:");
            Console.WriteLine($"Ошибка: {ex.Message}");
            return null;
        }
    }



    #endregion

    #region Helpers

    private async Task<StreamContent> GetPhotoContentAsync(string filePath)
    {
        if (Uri.TryCreate(filePath, UriKind.Absolute, out Uri? uri) && uri.Scheme.StartsWith("http"))
        {
            Console.WriteLine($"[Отладка] Загружаем изображение с URL: {filePath}");
            using var response = await _httpClient.GetAsync(filePath);

            if (!response.IsSuccessStatusCode)
            {
                throw new FileNotFoundException($"Не удалось загрузить изображение: {response.StatusCode}");
            }

            var stream = await response.Content.ReadAsStreamAsync();
            return new StreamContent(stream); // Поток будет освобожден вызывающим кодом
        }
        else if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            Console.WriteLine($"[Отладка] Загружаем локальный файл: {filePath}");
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new StreamContent(fileStream); // Поток будет освобожден вызывающим кодом
        }

        throw new FileNotFoundException("Файл не найден или путь пуст.");
    }

    private async Task<T?> GetAuthorizedAsync<T>(string endpoint)
    {
        if (!Auth) throw new UnauthorizedAccessException("User is not authenticated.");
        var response = await _httpClient.GetAsync(endpoint);
        return await HandleResponse<T>(response);
    }

    private async Task<TResponse?> PostAuthorizedAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        if (!Auth)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        // Преобразуем объект данных в JSON
        var jsonContent = JsonSerializer.Serialize(data);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Выполняем POST-запрос
        var response = await _httpClient.PostAsync(endpoint, content);

        // Логирование ошибки с подробностями
        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ошибка: {response.StatusCode}. Ответ от сервера: {responseContent}");
            throw new Exception($"Request failed with status code {response.StatusCode}. Response: {responseContent}");
        }

        // Преобразуем ответ в объект типа TResponse
        var responseContentSuccess = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TResponse>(responseContentSuccess);

        return result;
    }

    //private async Task<TResponse?> PutAuthorizedAsync<TRequest, TResponse>(string endpoint, TRequest data)
    //{
    //    if (!Auth) throw new UnauthorizedAccessException("User is not authenticated.");
    //    return await PutAsync<TRequest, TResponse>(endpoint, data);
    //}

    private async Task<bool> DeleteAuthorizedAsync(string endpoint)
    {
        if (!Auth) throw new UnauthorizedAccessException("User is not authenticated.");
        var response = await _httpClient.DeleteAsync(endpoint);
        return response.IsSuccessStatusCode;
    }

    private async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);
        return await HandleResponse<TResponse>(response);
    }

    //private async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
    //{
    //    var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
    //    var response = await _httpClient.PutAsync(endpoint, content);
    //    return await HandleResponse<TResponse>(response);
    //}

    private async Task<T?> HandleResponse<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Error ({response.StatusCode}): {error}");
            return default;
        }
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    #endregion

    #region Favorite
    public async Task<List<Favorite>?> GetFavoriteMovies(int userId)
    {
        return await GetAuthorizedAsync<List<Favorite>>($"/api/movies/favorites{userId}");
    }

    private async Task<T?> GetAuthorizedAsync<T>(string endpoint, string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        // Добавляем токен в заголовки запроса
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Выполняем GET-запрос
        var response = await _httpClient.GetAsync(endpoint);

        // Обрабатываем ответ
        return await HandleResponse<T>(response);
    }

    public async Task<List<Favorite>?> GetFavoriteMovies(string token)
    {
        // Формируем URL для получения списка любимых фильмов
        string endpoint = "/api/movies/favorites";

        // Выполняем GET-запрос с авторизацией по токену
        return await GetAuthorizedAsync<List<Favorite>>(endpoint, token);
    }

    public async Task<bool> RemoveMovieFromFavorites(int movieId, string token)
    {

        string endpoint = $"/api/movies/{movieId}/favorite";

        try
        {
            // Выполняем DELETE-запрос с авторизацией по токену
            var response = await _httpClient.DeleteAsync(endpoint);

            // Проверка на успешный ответ от сервера (статус код 200-299)
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Фильм успешно удалён из избранного.");
                return true;
            }
            else
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка при удалении фильма: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка при удалении фильма:");
            Console.WriteLine($"Ошибка: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AddMovieToFavorites(int movieId, string token)
    {
        string endpoint = $"/api/movies/{movieId}/favorite";

        try
        {
            // Добавляем токен в заголовок Authorization
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Выполняем POST-запрос
            var response = await _httpClient.PostAsync(endpoint, null);

            // Проверка на успешный ответ от сервера (статус код 200-299)
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Фильм успешно добавлен в избранное.");
                return true;
            }
            else
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка при добавлении фильма в избранное: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка при добавлении фильма в избранное:");
            Console.WriteLine($"Ошибка: {ex.Message}");
            return false;
        }
    }
    #endregion
}