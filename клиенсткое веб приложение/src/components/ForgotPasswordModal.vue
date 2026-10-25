<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="forgot-window">
      <div class="header-row">
        <h2 class="title">Восстановление пароля</h2>
        <button class="close-x" @click="$emit('close')">✕</button>
      </div>

      <div class="field-group">
        <label class="label">Номер телефона</label>
        <input
          type="text"
          :value="phone"
          @input="onPhoneInput"
          placeholder="+7 (___) ___-__-__"
          maxlength="20"
          class="input-box"
          :disabled="codeSent"
        />
      </div>

      <button
        v-if="!codeSent"
        class="action-btn"
        @click="sendCode"
        :disabled="loading"
      >
        Отправить код
      </button>

      <!-- Панель ввода кода -->
      <div v-if="codeSent && !codeConfirmed" class="code-panel">
        <div class="field-group">
          <label class="label">Введите код</label>
          <input
            type="text"
            v-model="enteredCode"
            placeholder="6-значный код"
            class="input-box"
            maxlength="6"
          />
        </div>

        <button class="action-btn" @click="confirmCode">
          Подтвердить
        </button>
      </div>

      <!-- Панель нового пароля -->
      <div v-if="codeConfirmed" class="password-panel">
        <div class="field-group">
          <label class="label">Новый пароль</label>
          <div class="password-wrapper">
            <input
              :type="showPassword ? 'text' : 'password'"
              v-model="newPassword"
              class="input-box password-input"
              maxlength="20"
            />
            <button
              type="button"
              class="eye-btn"
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
          <label class="label">Подтверждение пароля</label>
          <div class="password-wrapper">
            <input
              :type="showConfirmPassword ? 'text' : 'password'"
              v-model="confirmPassword"
              class="input-box password-input"
              maxlength="20"
            />
            <button
              type="button"
              class="eye-btn"
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

        <button
          class="action-btn"
          @click="changePassword"
          :disabled="loading"
        >
          Изменить пароль
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { apiClient } from '../api';
import { formatPhoneNumber, validatePassword } from '../auth';
import { showToast } from '../notifications';

const emit = defineEmits(['close', 'success']);

const phone = ref('');
const enteredCode = ref('');
const generatedCode = ref('');
const codeSent = ref(false);
const codeConfirmed = ref(false);

const newPassword = ref('');
const confirmPassword = ref('');
const showPassword = ref(false);
const showConfirmPassword = ref(false);
const loading = ref(false);

function onPhoneInput(e) {
  const formatted = formatPhoneNumber(e.target.value);
  phone.value = formatted;
  e.target.value = formatted;
}

function sendCode() {
  if (!phone.value || phone.value.trim() === '') {
    showToast('Введите номер телефона', 'warning');
    return;
  }
  const digits = phone.value.replace(/\D/g, '');
  if (digits.length !== 11) {
    showToast('Введите полный номер телефона', 'warning');
    return;
  }

  // Генерация 6-значного кода
  generatedCode.value = Math.floor(100000 + Math.random() * 900000).toString();
  console.log(`Код восстановления: ${generatedCode.value}`);
  showToast(`Код отправлен! Код подтверждения: ${generatedCode.value}`, 'info', 6000);
  codeSent.value = true;
}

function confirmCode() {
  if (!enteredCode.value || enteredCode.value.trim() === '') {
    showToast('Введите код', 'warning');
    return;
  }
  if (enteredCode.value.trim() !== generatedCode.value) {
    showToast('Неверный код подтверждения', 'error');
    return;
  }
  codeConfirmed.value = true;
  showToast('Код успешно подтвержден!', 'success');
}

async function changePassword() {
  const pwd = newPassword.value;
  const cpwd = confirmPassword.value;

  if (!pwd) {
    showToast('Введите пароль', 'warning');
    return;
  }
  if (pwd.length < 8) {
    showToast('Пароль должен содержать минимум 8 символов', 'warning');
    return;
  }
  if (!validatePassword(pwd)) {
    showToast(
      'Пароль должен содержать: минимум 8 символов, цифру, строчную, заглавную букву и спецсимвол',
      'warning',
      5000
    );
    return;
  }
  if (pwd !== cpwd) {
    showToast('Пароли не совпадают', 'error');
    return;
  }

  loading.value = true;
  try {
    const result = await apiClient.changePassword(phone.value, pwd);
    showToast('Пароль успешно изменён!', 'success');
    emit('success');
    emit('close');
  } catch (err) {
    const msg = err.response?.data || err.message || 'Ошибка смены пароля';
    showToast(typeof msg === 'string' ? msg : JSON.stringify(msg), 'error');
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.forgot-window {
  width: 420px;
  max-width: 95vw;
  background-color: #ffffff;
  border-radius: 12px;
  padding: 35px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15);
  display: flex;
  flex-direction: column;
}

.header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
  position: relative;
}

.title {
  font-size: 24px;
  font-weight: bold;
  color: #3D3D3D;
  text-align: center;
  flex: 1;
}

.close-x {
  background: transparent;
  font-size: 18px;
  color: #9CA3AF;
  border: none;
  cursor: pointer;
}
.close-x:hover {
  color: #111827;
}

.field-group {
  display: flex;
  flex-direction: column;
  margin-bottom: 18px;
}

.label {
  font-size: 13px;
  color: #666666;
  margin-bottom: 5px;
}

.input-box {
  height: 35px;
  font-size: 13px;
  padding: 0 10px;
  border: 1px solid #D0D0D0;
  border-radius: 4px;
  background-color: #ffffff;
  color: #111827;
}

.password-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.password-input {
  width: 100%;
  padding-right: 40px;
}

.eye-btn {
  position: absolute;
  right: 8px;
  background: transparent;
  border: none;
  cursor: pointer;
  color: #6B7280;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 4px;
}

.eye-btn:hover {
  color: #111827;
}

.hint-text {
  font-size: 11px;
  color: #999999;
  margin-top: 4px;
}

.action-btn {
  height: 42px;
  background-color: #1E3A8A;
  color: #ffffff;
  font-size: 14px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  margin-top: 6px;
  margin-bottom: 15px;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
}

.action-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
