<template>
  <div class="change-password-page">
    <div class="password-card">
      <div class="card-header">
        <button class="back-link" @click="goBack">
          ← Назад в профиль
        </button>
        <h1 class="page-title">Изменение пароля</h1>
        <p class="page-subtitle">Придумайте новый надежный пароль для вашего аккаунта</p>
      </div>

      <form @submit.prevent="handleChangePassword" class="password-form">
        <!-- Текущий пароль -->
        <div class="field-item">
          <label class="field-label">Текущий пароль</label>
          <div class="password-box">
            <input
              :type="showCurrent ? 'text' : 'password'"
              v-model="currentPassword"
              placeholder="Введите старый пароль"
              maxlength="30"
              class="form-input"
            />
            <button
              type="button"
              class="eye-btn"
              @click="showCurrent = !showCurrent"
              tabindex="-1"
            >
              <svg v-if="!showCurrent" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
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

        <!-- Новый пароль -->
        <div class="field-item">
          <label class="field-label">Новый пароль</label>
          <div class="password-box">
            <input
              :type="showNew ? 'text' : 'password'"
              v-model="newPassword"
              placeholder="Минимум 8 символов"
              maxlength="30"
              class="form-input"
            />
            <button
              type="button"
              class="eye-btn"
              @click="showNew = !showNew"
              tabindex="-1"
            >
              <svg v-if="!showNew" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
                <circle cx="12" cy="12" r="3"/>
              </svg>
              <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/>
                <line x1="1" y1="1" x2="23" y2="23"/>
              </svg>
            </button>
          </div>
          <span class="password-rules">
            Должен содержать цифру, заглавную и строчную буквы, спецсимвол
          </span>
        </div>

        <!-- Повторите новый пароль -->
        <div class="field-item">
          <label class="field-label">Повторите новый пароль</label>
          <div class="password-box">
            <input
              :type="showConfirm ? 'text' : 'password'"
              v-model="confirmPassword"
              placeholder="Повторите новый пароль"
              maxlength="30"
              class="form-input"
            />
            <button
              type="button"
              class="eye-btn"
              @click="showConfirm = !showConfirm"
              tabindex="-1"
            >
              <svg v-if="!showConfirm" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
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
            class="forgot-link"
            @click="showForgotModal = true"
          >
            Забыли пароль?
          </button>
        </div>

        <div class="form-actions">
          <button
            type="submit"
            class="btn-save"
            :disabled="loading"
          >
            {{ loading ? 'Сохранение...' : 'Сохранить пароль' }}
          </button>
          <button
            type="button"
            class="btn-cancel"
            @click="goBack"
          >
            Отмена
          </button>
        </div>
      </form>
    </div>

    <!-- Модалка восстановления пароля (как в авторизации) -->
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
import { authState, validatePassword } from '../auth';
import { showToast } from '../notifications';
import ForgotPasswordModal from '../components/ForgotPasswordModal.vue';

const router = useRouter();

const currentPassword = ref('');
const newPassword = ref('');
const confirmPassword = ref('');

const showCurrent = ref(false);
const showNew = ref(false);
const showConfirm = ref(false);

const loading = ref(false);
const showForgotModal = ref(false);

function goBack() {
  router.push('/app/profile');
}

async function handleChangePassword() {
  if (!currentPassword.value) {
    showToast('Введите текущий пароль', 'warning');
    return;
  }

  if (!newPassword.value) {
    showToast('Введите новый пароль', 'warning');
    return;
  }

  if (newPassword.value.length < 8) {
    showToast('Новый пароль должен содержать минимум 8 символов', 'warning');
    return;
  }

  if (!validatePassword(newPassword.value)) {
    showToast(
      'Пароль должен содержать: минимум 8 знаков, цифру, строчную, заглавную букву и спецсимвол (!@#$%^&*)',
      'warning',
      5000
    );
    return;
  }

  if (newPassword.value !== confirmPassword.value) {
    showToast('Пароли не совпадают', 'error');
    return;
  }

  const userPhone = authState.user?.phone;
  if (!userPhone) {
    showToast('Не удалось определить телефон пользователя', 'error');
    return;
  }

  loading.value = true;
  try {
    // Пробуем сменить через changePassword
    await apiClient.changePassword(userPhone, newPassword.value);
    showToast('Пароль успешно изменен!', 'success');
    router.push('/app/profile');
  } catch (err) {
    // Если на бэкенде changePassword не сработал или нет эндпоинта, пробуем обновить через UpdateClient
    try {
      const u = await apiClient.getUser(authState.userId);
      await apiClient.updateUser(authState.userId, {
        ...u,
        password: newPassword.value,
      });
      showToast('Пароль успешно изменен!', 'success');
      router.push('/app/profile');
    } catch (updateErr) {
      const msg = updateErr.response?.data || updateErr.message || 'Ошибка изменения пароля';
      showToast(typeof msg === 'string' ? msg : 'Ошибка изменения пароля', 'error');
    }
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.change-password-page {
  padding: 32px;
  max-width: 600px;
  margin: 0 auto;
}

.password-card {
  background-color: #ffffff;
  border-radius: 14px;
  padding: 32px 36px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.06);
  border: 1px solid #E5E7EB;
}

.back-link {
  background: none;
  border: none;
  color: #1E3A8A;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  padding: 0;
  margin-bottom: 16px;
  display: inline-block;
}

.back-link:hover {
  text-decoration: underline;
}

.page-title {
  font-size: 24px;
  font-weight: 700;
  color: #111827;
  margin: 0 0 6px 0;
}

.page-subtitle {
  font-size: 14px;
  color: #6B7280;
  margin: 0 0 24px 0;
}

.password-form {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.field-item {
  display: flex;
  flex-direction: column;
}

.field-label {
  font-size: 13.5px;
  font-weight: 600;
  color: #374151;
  margin-bottom: 6px;
}

.password-box {
  position: relative;
  display: flex;
  align-items: center;
}

.form-input {
  width: 100%;
  height: 42px;
  padding: 0 40px 0 14px;
  background-color: #F9FAFB;
  border: 1px solid #D1D5DB;
  border-radius: 8px;
  font-size: 14px;
  color: #111827;
  outline: none;
  box-sizing: border-box;
  transition: border-color 0.15s ease;
}

.form-input:focus {
  border-color: #1E3A8A;
  background-color: #ffffff;
}

.eye-btn {
  position: absolute;
  right: 10px;
  background: none;
  border: none;
  cursor: pointer;
  font-size: 16px;
  color: #6B7280;
  padding: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.eye-btn:hover {
  color: #111827;
}

.password-rules {
  font-size: 12px;
  color: #6B7280;
  margin-top: 5px;
  line-height: 1.3;
}

.forgot-wrapper {
  display: flex;
  justify-content: flex-end;
  margin-top: -6px;
}

.forgot-link {
  background: none;
  border: none;
  color: #1E3A8A;
  font-size: 13.5px;
  cursor: pointer;
  padding: 0;
  font-weight: 500;
}

.forgot-link:hover {
  text-decoration: underline;
}

.form-actions {
  display: flex;
  gap: 12px;
  margin-top: 10px;
}

.btn-save {
  flex: 1;
  height: 44px;
  background-color: #1E3A8A;
  color: #ffffff;
  border: none;
  border-radius: 8px;
  font-size: 14.5px;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.15s ease;
}

.btn-save:hover {
  background-color: #172554;
}

.btn-cancel {
  height: 44px;
  padding: 0 20px;
  background-color: #ffffff;
  color: #4B5563;
  border: 1px solid #D1D5DB;
  border-radius: 8px;
  font-size: 14.5px;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.15s ease;
}

.btn-cancel:hover {
  background-color: #F3F4F6;
}
</style>
