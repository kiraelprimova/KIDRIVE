<template>
  <div class="auth-page">
    <div class="auth-card">
      <h1 class="auth-title">Авторизация</h1>

      <div class="field-group">
        <label class="field-label">Номер телефона</label>
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
        <label class="field-label">Пароль</label>
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
            title="Показать / скрыть пароль"
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
      </div>

      <div class="forgot-wrapper">
        <button
          type="button"
          class="link-btn"
          @click="showForgotModal = true"
        >
          Забыли пароль?
        </button>
      </div>

      <div class="buttons-row">
        <button
          class="btn-primary btn-login"
          @click="handleLogin"
          :disabled="loading"
        >
          {{ loading ? 'Вход...' : 'Войти' }}
        </button>
        <button
          class="btn-primary btn-guest"
          @click="handleGuest"
        >
          Гость
        </button>
      </div>

      <div class="register-wrapper">
        <button
          type="button"
          class="link-btn register-link"
          @click="goToRegister"
        >
          Нет аккаунта? Регистрация
        </button>
      </div>
    </div>

    <!-- Модальное окно восстановления пароля -->
    <ForgotPasswordModal
      v-if="showForgotModal"
      @close="showForgotModal = false"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { apiClient } from '../api';
import { setUser, setGuest, formatPhoneNumber } from '../auth';
import { showToast } from '../notifications';
import ForgotPasswordModal from '../components/ForgotPasswordModal.vue';

const router = useRouter();

const phone = ref('');
const password = ref('');
const showPassword = ref(false);
const showForgotModal = ref(false);
const loading = ref(false);

function onPhoneInput(e) {
  const formatted = formatPhoneNumber(e.target.value);
  phone.value = formatted;
  e.target.value = formatted;
}

async function handleLogin() {
  if (!phone.value || !password.value || !phone.value.trim() || !password.value.trim()) {
    showToast('Заполните все поля', 'warning');
    return;
  }

  loading.value = true;
  try {
    const user = await apiClient.login(phone.value, password.value);
    if (user.idRole === 1) {
      setUser(user);
      showToast('Добро пожаловать!', 'success');
      router.push('/app/catalog');
    } else if (user.idRole === 2) {
      showToast('Данный аккаунт принадлежит менеджеру. Для работы администратора используйте папку "админское настольное приложение".', 'warning', 6000);
    } else {
      setUser(user);
      showToast('Добро пожаловать!', 'success');
      router.push('/app/catalog');
    }
  } catch (err) {
    const errorText = err.response?.data || err.message || 'Ошибка авторизации';
    showToast(typeof errorText === 'string' ? errorText : JSON.stringify(errorText), 'error');
  } finally {
    loading.value = false;
  }
}

function handleGuest() {
  setGuest();
  router.push('/app/catalog');
}

function goToRegister() {
  router.push('/register');
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #ffffff;
  padding: 20px;
}

.auth-card {
  width: 380px;
  max-width: 100%;
  padding: 35px;
  background: #ffffff;
  display: flex;
  flex-direction: column;
}

.auth-title {
  font-size: 28px;
  font-weight: bold;
  color: #3D3D3D;
  text-align: center;
  margin-bottom: 30px;
}

.field-group {
  display: flex;
  flex-direction: column;
  margin-bottom: 20px;
}

.field-label {
  font-size: 14px;
  color: #666666;
  margin-bottom: 6px;
}

.text-input {
  height: 38px;
  font-size: 14px;
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

.forgot-wrapper {
  display: flex;
  justify-content: flex-end;
  margin-top: -5px;
  margin-bottom: 15px;
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

.buttons-row {
  display: flex;
  flex-direction: row;
  align-items: center;
  margin-bottom: 18px;
}

.btn-primary {
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

.btn-login {
  width: 150px;
}

.btn-guest {
  width: 135px;
  margin-left: 10px;
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.register-wrapper {
  display: flex;
  justify-content: center;
}

.register-link {
  font-size: 12px;
}
</style>
