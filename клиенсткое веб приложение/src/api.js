import axios from 'axios';

// Базовый клиент: используем относительный путь '/api' благодаря Vite proxy,
// а при необходимости можно использовать полный URL http://localhost:5128/api
export const API_BASE = '/api';

const api = axios.create({
  baseURL: API_BASE,
});

export const apiClient = {
  // Авторизация
  async login(phone, password) {
    const response = await api.post(`/User/Authorization?phone=${encodeURIComponent(phone)}&password=${encodeURIComponent(password)}`);
    return response.data;
  },

  // Регистрация
  async register(data) {
    const response = await api.post('/User/Registration', data);
    return response.data;
  },

  // Получить всех пользователей (для проверки дубликатов телефона и почты)
  async getAllClients() {
    const response = await api.get('/User/GetAllClients');
    return response.data;
  },

  // Получить данные пользователя по ID
  async getUser(id) {
    const response = await api.get(`/User/GetClient/${id}`);
    return response.data;
  },

  // Обновить профиль
  async updateUser(id, data) {
    const response = await api.put(`/User/UpdateClient/${id}`, data);
    return response.data;
  },

  // Удалить аккаунт (пометить как удаленный)
  async deleteUser(id) {
    const response = await api.put(`/User/DeleteUser/${id}`);
    return response.data;
  },

  // Смена пароля
  async changePassword(phone, newPassword) {
    const response = await api.put(`/User/ChangePassword?phone=${encodeURIComponent(phone)}&newPassword=${encodeURIComponent(newPassword)}`);
    return response.data;
  },

  // Категории (марки)
  async getCategories() {
    const response = await api.get('/Category/GetAllCategories');
    return response.data;
  },

  async getCategory(id) {
    const response = await api.get(`/Category/GetCategory?id=${id}`);
    return response.data;
  },

  // Автомобили
  async getCars() {
    const response = await api.get('/Car/GetAllCars');
    return response.data;
  },

  async getCar(id) {
    const response = await api.get(`/Car/GetCar?id=${id}`);
    return response.data;
  },

  // Занятое время для авто на дату
  async getBusyTimes(carId, dateStr) {
    const response = await api.get(`/Car/GetBusyTimes?carId=${carId}&date=${dateStr}`);
    return response.data;
  },

  // Бронирования
  async getBookings() {
    const response = await api.get('/Booking/GetAllBookings');
    return response.data;
  },

  async createBooking(bookingData) {
    const response = await api.post('/Booking/CreateBooking', bookingData);
    return response.data;
  },

  async cancelBooking(bookingId) {
    const response = await api.put(`/Booking/UpdateStatus?id=${bookingId}&statusId=3`);
    return response.data;
  },

  // Справочники: Типы топлива и коробки передач
  async getFuelTypes() {
    const response = await api.get('/FuelType/GetAllFuelTypes');
    return response.data;
  },

  async getTransmissions() {
    const response = await api.get('/Transmission/GetAllTransmissions');
    return response.data;
  },

  async getStatuses() {
    const response = await api.get('/BookingStatus/GetAllStatuses');
    return response.data;
  },

  // Избранное
  async getFavorites(userId) {
    const response = await api.get(`/Favorite/GetFavorites?userId=${userId}`);
    return response.data;
  },

  async addFavorite(userId, carId) {
    const response = await api.post(`/Favorite/AddFavorite?idUser=${userId}&idCar=${carId}`);
    return response.data;
  },

  async removeFavorite(userId, carId) {
    const response = await api.delete(`/Favorite/RemoveFavorite?userId=${userId}&carId=${carId}`);
    return response.data;
  },

  async checkFavorite(userId, carId) {
    const response = await api.get(`/Favorite/CheckFavorite?userId=${userId}&carId=${carId}`);
    return response.data;
  },

  // ИИ-чат для подбора авто
  async sendAiMessage(userId, message) {
    const response = await api.post(`/AiChat/SendMessage?idUser=${userId}&message=${encodeURIComponent(message)}`);
    return response.data;
  },

  async getAiHistory(userId) {
    const response = await api.get(`/AiChat/GetHistory?userId=${userId}`);
    return response.data;
  },

  async clearAiHistory(userId) {
    const response = await api.delete(`/AiChat/ClearHistory?userId=${userId}`);
    return response.data;
  }
};

export default apiClient;
