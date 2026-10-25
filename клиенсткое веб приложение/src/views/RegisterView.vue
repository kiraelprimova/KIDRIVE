<template>
  <div class="register-page">
    <div class="register-card">
      <h1 class="register-title">Регистрация</h1>

      <div class="field-group">
        <label class="field-label">Имя *</label>
        <input
          type="text"
          v-model="firstName"
          @input="onFirstNameInput"
          maxlength="10"
          class="text-input"
        />
      </div>

      <div class="field-group">
        <label class="field-label">Фамилия *</label>
        <input
          type="text"
          v-model="lastName"
          @input="onLastNameInput"
          maxlength="15"
          class="text-input"
        />
      </div>

      <div class="field-group">
        <label class="field-label">Отчество</label>
        <input
          type="text"
          v-model="patronymic"
          @input="onPatronymicInput"
          maxlength="15"
          class="text-input"
        />
      </div>

      <div class="field-group">
        <label class="field-label">Номер телефона *</label>
        <input
          type="text"
          :value="phone"
          @input="onPhoneInput"
          placeholder="+7 (___) ___-__-__"
          maxlength="20"
          class="text-input"
        />
      </div>

      <div class="field-group">
        <label class="field-label">Пароль *</label>
        <div class="password-box">
          <input
            :type="showPassword ? 'text' : 'password'"
            v-model="password"
            maxlength="20"
            class="text-input password-input"
          />
          <button
            type="button"
            class="toggle-eye"
            @click="showPassword = !showPassword"
            tabindex="-1"
          >
            <svg v-if="!showPassword" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
              <circle cx="12" cy="12" r="3"/>
            </svg>
            <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/>
              <line x1="1" y1="1" x2="23" y2="23"/>
            </svg>
          </button>
        </div>
        <span class="hint-text">Минимум 8 символов</span>
      </div>

      <div class="field-group">
        <label class="field-label">Подтверждение пароля *</label>
        <div class="password-box">
          <input
            :type="showConfirmPassword ? 'text' : 'password'"
            v-model="confirmPassword"
            maxlength="20"
            class="text-input password-input"
          />
          <button
            type="button"
            class="toggle-eye"
            @click="showConfirmPassword = !showConfirmPassword"
            tabindex="-1"
          >
            <svg v-if="!showConfirmPassword" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
              <circle cx="12" cy="12" r="3"/>
            </svg>
            <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/>
              <line x1="1" y1="1" x2="23" y2="23"/>
            </svg>
          </button>
        </div>
      </div>

      <div class="field-group">
        <label class="field-label">Дата рождения *</label>
        <input
          type="date"
          v-model="birthDate"
          class="text-input"
        />
      </div>

      <div class="button-wrapper">
        <button
          class="btn-register"
          @click="handleRegister"
          :disabled="loading"
        >
          {{ loading ? 'Регистрация...' : 'Зарегистрироваться' }}
        </button>
      </div>

      <div class="login-wrapper">
        <button
          type="button"
          class="link-btn login-link"
          @click="goToLogin"
        >
          Уже есть аккаунт? Войти
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { apiClient } from '../api';
import { formatPhoneNumber, capitalizeName, isAdult, validatePassword } from '../auth';
import { showToast } from '../notifications';

const router = useRouter();

const firstName = ref('');
const lastName = ref('');
const patronymic = ref('');
const phone = ref('');
const password = ref('');
const confirmPassword = ref('');
const birthDate = ref('');
const showPassword = ref(false);
const showConfirmPassword = ref(false);
const loading = ref(false);

function onFirstNameInput(e) {
  firstName.value = capitalizeName(e.target.value);
}

function onLastNameInput(e) {
  lastName.value = capitalizeName(e.target.value);
}

function onPatronymicInput(e) {
  patronymic.value = capitalizeName(e.target.value);
}

function onPhoneInput(e) {
  const formatted = formatPhoneNumber(e.target.value);
  phone.value = formatted;
  e.target.value = formatted;
}

async function handleRegister() {
  if (!firstName.value || !firstName.value.trim()) {
    showToast('Введите имя', 'warning');
    return;
  }
  if (!lastName.value || !lastName.value.trim()) {
    showToast('Введите фамилию', 'warning');
    return;
  }
  if (!phone.value || !phone.value.trim()) {
    showToast('Введите номер телефона', 'warning');
    return;
  }

  const phoneDigits = phone.value.replace(/\D/g, '');
  if (phoneDigits.length !== 11) {
    showToast('Введите полный номер телефона (11 цифр)', 'warning');
    return;
  }
  if (!phoneDigits.startsWith('7') && !phoneDigits.startsWith('8')) {
    showToast('Номер должен начинаться с +7 или 8', 'warning');
    return;
  }

  if (!password.value || !password.value.trim()) {
    showToast('Введите пароль', 'warning');
    return;
  }
  if (password.value.length < 8) {
    showToast('Пароль должен содержать минимум 8 символов', 'warning');
    return;
  }
  if (!validatePassword(password.value)) {
    showToast(
      'Пароль должен содержать: минимум 8 символов, цифру, строчную, заглавную букву и спецсимвол',
      'warning',
      5000
    );
    return;
  }
  if (!confirmPassword.value || !confirmPassword.value.trim()) {
    showToast('Подтвердите пароль', 'warning');
    return;
  }
  if (password.value !== confirmPassword.value) {
    showToast('Пароли не совпадают', 'error');
    return;
  }
  if (!birthDate.value) {
    showToast('Выберите дату рождения', 'warning');
    return;
  }
  if (!isAdult(birthDate.value)) {
    showToast('Регистрация доступна только с 18 лет', 'warning');
    return;
  }

  const dto = {
    firstName: firstName.value.trim(),
    lastName: lastName.value.trim(),
    patronymic: (patronymic.value || '').trim(),
    phone: phone.value,
    email: 'fatkhlislamovad@mail.ru',
    password: password.value,
    birthDate: birthDate.value,
  };

  loading.value = true;
  try {
    const res = await apiClient.register(dto);
    showToast(typeof res === 'string' ? res : 'Регистрация прошла успешно!', 'success');
    router.push('/login');
  } catch (err) {
    const errorText = err.response?.data || err.message || 'Ошибка регистрации';
    showToast(typeof errorText === 'string' ? errorText : JSON.stringify(errorText), 'error');
  } finally {
    loading.value = false;
  }
}

function goToLogin() {
  router.push('/login');
}
</script>

<style scoped>
.register-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #ffffff;
  padding: 30px 15px;
}

.register-card {
  width: 450px;
  max-width: 100%;
  padding: 35px;
  background: #ffffff;
  display: flex;
  flex-direction: column;
}

.register-title {
  font-size: 28px;
  font-weight: bold;
  color: #3D3D3D;
  text-align: center;
  margin-bottom: 25px;
}

.field-group {
  display: flex;
  flex-direction: column;
  margin-bottom: 12px;
}

.field-label {
  font-size: 13px;
  color: #666666;
  margin-bottom: 5px;
}

.text-input {
  height: 35px;
  font-size: 13px;
  padding: 0 10px;
  border: 1px solid #D0D0D0;
  border-radius: 4px;
  background-color: #ffffff;
  color: #111827;
}

.password-box {
  position: relative;
  display: flex;
  align-items: center;
}

.password-input {
  width: 100%;
  padding-right: 40px;
}

.toggle-eye {
  position: absolute;
  right: 10px;
  background: transparent;
  border: none;
  cursor: pointer;
  color: #6B7280;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 4px;
}

.toggle-eye:hover {
  color: #111827;
}

.hint-text {
  font-size: 11px;
  color: #999999;
  margin-top: 3px;
}

.button-wrapper {
  display: flex;
  justify-content: center;
  margin-top: 15px;
  margin-bottom: 15px;
}

.btn-register {
  width: 180px;
  height: 42px;
  background-color: #1E3A8A;
  color: #ffffff;
  font-size: 14px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-register:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.login-wrapper {
  display: flex;
  justify-content: center;
}

.link-btn {
  background: transparent;
  border: none;
  color: #028AB4;
  font-size: 12px;
  text-decoration: underline;
  cursor: pointer;
  padding: 0;
  transition: color 0.15s ease;
}

.link-btn:hover {
  color: #4F2826;
}
</style>
