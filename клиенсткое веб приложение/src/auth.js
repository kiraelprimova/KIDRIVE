import { reactive } from 'vue';

const USER_STORAGE_KEY = 'kidrive_user';

const storedUser = localStorage.getItem(USER_STORAGE_KEY);
let initialUser = null;
try {
  initialUser = storedUser ? JSON.parse(storedUser) : null;
} catch (e) {
  initialUser = null;
}

export const authState = reactive({
  user: initialUser,
  get userId() {
    if (!this.user) return 0;
    return this.user.idUser || this.user.IdUser || 0;
  },
  get isGuest() {
    if (!this.user) return true;
    const id = this.user.idUser || this.user.IdUser;
    return !id || id === 0;
  }
});

export function setUser(user) {
  if (user && !user.idUser && user.IdUser) {
    user.idUser = user.IdUser;
  }
  authState.user = user;
  if (user) {
    localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(user));
  } else {
    localStorage.removeItem(USER_STORAGE_KEY);
  }
}

export function setGuest() {
  const guestUser = { idUser: 0, firstName: 'Гость', lastName: '', patronymic: '', email: 'Не авторизован' };
  authState.user = guestUser;
  localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(guestUser));
}

export function logout() {
  authState.user = null;
  localStorage.removeItem(USER_STORAGE_KEY);
}

// Надежное форматирование номера телефона без залипаний и смещений
export function formatPhoneNumber(input) {
  if (!input) return '';
  const raw = String(input);
  const digits = raw.replace(/\D/g, '');
  if (!digits) return '';

  // Определяем префикс: 8 или +7
  const startsWith8 = raw.trim().startsWith('8') || (digits.startsWith('8') && !raw.trim().startsWith('+7'));
  const prefix = startsWith8 ? '8' : '+7';

  // Извлекаем 10 цифр самого номера
  let body = digits;
  if (digits.startsWith('7') || digits.startsWith('8')) {
    body = digits.substring(1);
  }
  body = body.substring(0, 10);

  if (body.length === 0) {
    return prefix;
  }

  let formatted = prefix + ' (' + body.substring(0, Math.min(3, body.length));

  if (body.length >= 3) {
    formatted += ') ';
  }
  if (body.length > 3) {
    formatted += body.substring(3, Math.min(6, body.length));
  }
  if (body.length > 6) {
    formatted += '-' + body.substring(6, Math.min(8, body.length));
  }
  if (body.length > 8) {
    formatted += '-' + body.substring(8, Math.min(10, body.length));
  }

  return formatted;
}

// Капитализация имен (только русские буквы и дефис)
export function capitalizeName(input) {
  if (!input) return '';
  let text = String(input).replace(/[^А-Яа-яЁё\-]/g, '');
  text = text.replace(/-{2,}/g, '-');
  text = text.replace(/^-+/, '');
  
  if (!text) return '';
  
  const parts = text.split('-');
  for (let i = 0; i < parts.length; i++) {
    if (parts[i].length > 0) {
      parts[i] = parts[i][0].toUpperCase() + parts[i].substring(1).toLowerCase();
    }
  }
  return parts.join('-');
}

// Проверка совершеннолетия (18+)
export function isAdult(dateStr) {
  if (!dateStr) return false;
  const birth = new Date(dateStr);
  if (isNaN(birth.getTime())) return false;
  
  const today = new Date();
  let age = today.getFullYear() - birth.getFullYear();
  const m = today.getMonth() - birth.getMonth();
  if (m < 0 || (m === 0 && today.getDate() < birth.getDate())) {
    age--;
  }
  return age >= 18;
}

// Проверка сложности пароля
export function validatePassword(password) {
  if (!password || password.length < 8) return false;
  const hasDigit = /\d/.test(password);
  const hasUpper = /[A-Z]/.test(password);
  const hasLower = /[a-z]/.test(password);
  const hasSpecial = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password);
  return hasDigit && hasUpper && hasLower && hasSpecial;
}
