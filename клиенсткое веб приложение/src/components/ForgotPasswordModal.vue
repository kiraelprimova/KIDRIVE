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
              👁
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
              👁
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
    alert('Введите номер телефона');
    return;
  }
  const digits = phone.value.replace(/\D/g, '');
  if (digits.length !== 11) {
    alert('Введите полный номер телефона');
    return;
  }

  // Генерация 6-значного кода
  generatedCode.value = Math.floor(100000 + Math.random() * 900000).toString();
  console.log(`Код восстановления: ${generatedCode.value}`);
  alert(`Код отправлен!\nКод подтверждения: ${generatedCode.value}`);
  codeSent.value = true;
}

function confirmCode() {
  if (!enteredCode.value || enteredCode.value.trim() === '') {
    alert('Введите код');
    return;
  }
  if (enteredCode.value.trim() !== generatedCode.value) {
    alert('Неверный код');
    return;
  }
  codeConfirmed.value = true;
}

async function changePassword() {
  const pwd = newPassword.value;
  const cpwd = confirmPassword.value;

  if (!pwd) {
    alert('Введите пароль');
    return;
  }
  if (pwd.length < 8) {
    alert('Пароль должен содержать минимум 8 символов');
    return;
  }
  if (!validatePassword(pwd)) {
    alert(
      'Пароль должен содержать:\n' +
      '- минимум 8 символов\n' +
      '- хотя бы одну цифру\n' +
      '- хотя бы одну заглавную букву\n' +
      '- хотя бы одну строчную букву\n' +
      '- хотя бы один спецсимвол'
    );
    return;
  }
  if (pwd !== cpwd) {
    alert('Пароли не совпадают');
    return;
  }

  loading.value = true;
  try {
    const result = await apiClient.changePassword(phone.value, pwd);
    alert('Пароль успешно изменён');
    emit('success');
    emit('close');
  } catch (err) {
    const msg = err.response?.data || err.message || 'Ошибка смены пароля';
    alert(typeof msg === 'string' ? msg : JSON.stringify(msg));
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
  font-size: 14px;
  cursor: pointer;
  color: #666;
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
