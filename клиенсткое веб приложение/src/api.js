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

  // Получить данные пользователя по ID
  async getUser(id) {
    const response = await api.get(`/User/${id}`);
    return response.data;
  },

  // Обновить профиль
  async updateUser(id, data) {
    const response = await api.put(`/User/${id}`, data);
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
    const response = await api.get('/Category');
    return response.data;
  },

  async getCategory(id) {
    const response = await api.get(`/Category/${id}`);
    return response.data;
  },

  // Автомобили
  async getCars() {
    const response = await api.get('/Car');
    return response.data;
  },

  async getCar(id) {
    const response = await api.get(`/Car/${id}`);
    return response.data;
  },

  // Занятое время для авто на дату
  async getBusyTimes(carId, dateStr) {
    const response = await api.get(`/Car/GetBusyTimes?carId=${carId}&date=${dateStr}`);
    return response.data;
  },

  // Бронирования
  async getBookings() {
    const response = await api.get('/Booking');
    return response.data;
  },

  async createBooking(bookingData) {
    const response = await api.post('/Booking', bookingData);
    return response.data;
  },

  async cancelBooking(bookingId) {
    const response = await api.put(`/Booking/${bookingId}?statusId=3`);
    return response.data;
  }
};

export default apiClient;
