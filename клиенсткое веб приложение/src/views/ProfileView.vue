<template>
  <div class="profile-page">
    <div class="profile-card">
      <div class="card-header">
        <h1 class="profile-title">Профиль</h1>

        <div class="actions-group">
          <!-- Кнопка Редактировать / Сохранить -->
          <button
            class="btn-edit"
            @click="handleEditToggle"
            :disabled="saving"
          >
            {{ isEditing ? (saving ? 'Сохранение...' : 'Сохранить') : 'Редактировать' }}
          </button>

          <!-- Кнопка Отменить -->
          <button
            v-if="isEditing"
            class="btn-cancel"
            @click="cancelEdit"
          >
            Отменить
          </button>

          <!-- Кнопка Удалить аккаунт -->
          <button
            class="btn-delete"
            @click="deleteAccount"
            :disabled="deleting"
          >
            {{ deleting ? 'Удаление...' : 'Удалить аккаунт' }}
          </button>
        </div>
      </div>

      <!-- Поля профиля -->
      <div class="profile-fields">
        <div class="field-item">
          <label class="field-label">Имя</label>
          <input
            type="text"
            v-model="firstName"
            @input="onFirstNameInput"
            maxlength="10"
            :disabled="!isEditing"
            class="profile-input"
          />
        </div>

        <div class="field-item">
          <label class="field-label">Фамилия</label>
          <input
            type="text"
            v-model="lastName"
            @input="onLastNameInput"
            maxlength="15"
            :disabled="!isEditing"
            class="profile-input"
          />
        </div>

        <div class="field-item">
          <label class="field-label">Отчество</label>
          <input
            type="text"
            v-model="patronymic"
            @input="onPatronymicInput"
            maxlength="15"
            :disabled="!isEditing"
            class="profile-input"
          />
        </div>

        <div class="field-item">
          <label class="field-label">Телефон</label>
          <input
            type="text"
            :value="phone"
            @input="onPhoneInput"
            maxlength="20"
            placeholder="+7 (___) ___-__-__"
            :disabled="!isEditing"
            class="profile-input"
          />
        </div>

        <div class="field-item">
          <label class="field-label">Почта</label>
          <input
            type="email"
            v-model="email"
            maxlength="30"
            :disabled="!isEditing"
            class="profile-input"
          />
        </div>

        <div class="field-item">
          <label class="field-label">Пароль</label>
          <div class="password-box">
            <input
              :type="showPassword ? 'text' : 'password'"
              v-model="password"
              maxlength="20"
              :disabled="!isEditing"
              class="profile-input password-input"
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
        </div>

        <div class="field-item">
          <label class="field-label">Повторите пароль</label>
          <div class="password-box">
            <input
              :type="showConfirmPassword ? 'text' : 'password'"
              v-model="confirmPassword"
              maxlength="20"
              :disabled="!isEditing"
              class="profile-input password-input"
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

        <div class="field-item">
          <label class="field-label">Дата рождения</label>
          <input
            type="date"
            v-model="birthDate"
            :disabled="!isEditing"
            class="profile-input"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { apiClient } from '../api';
import {
  authState,
  setUser,
  logout,
  formatPhoneNumber,
  capitalizeName,
  isAdult,
  validatePassword
} from '../auth';

const router = useRouter();

const isEditing = ref(false);
const saving = ref(false);
const deleting = ref(false);

const firstName = ref('');
const lastName = ref('');
const patronymic = ref('');
const phone = ref('');
const email = ref('');
const password = ref('');
const confirmPassword = ref('');
const birthDate = ref('');

const showPassword = ref(false);
const showConfirmPassword = ref(false);

// Исходные данные для отката
let originalData = {};

onMounted(async () => {
  await loadUserData();
});

async function loadUserData() {
  const userId = authState.userId;
  if (!userId) return;

  try {
    const user = await apiClient.getUser(userId);
    firstName.value = user.firstName || '';
    lastName.value = user.lastName || '';
    patronymic.value = user.patronymic || '';
    phone.value = user.phone || '';
    email.value = user.email || '';
    password.value = user.password || '';
    confirmPassword.value = user.password || '';

    if (user.birthDate) {
      birthDate.value = String(user.birthDate).split('T')[0];
    }

    saveOriginal();
  } catch (err) {
    console.error('Ошибка загрузки профиля:', err);
  }
}

function saveOriginal() {
  originalData = {
    firstName: firstName.value,
    lastName: lastName.value,
    patronymic: patronymic.value,
    phone: phone.value,
    email: email.value,
    password: password.value,
    confirmPassword: confirmPassword.value,
    birthDate: birthDate.value,
  };
}

function restoreOriginal() {
  firstName.value = originalData.firstName || '';
  lastName.value = originalData.lastName || '';
  patronymic.value = originalData.patronymic || '';
  phone.value = originalData.phone || '';
  email.value = originalData.email || '';
  password.value = originalData.password || '';
  confirmPassword.value = originalData.confirmPassword || '';
  birthDate.value = originalData.birthDate || '';
}

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

function handleEditToggle() {
  if (!isEditing.value) {
    isEditing.value = true;
  } else {
    saveProfile();
  }
}

function cancelEdit() {
  restoreOriginal();
  isEditing.value = false;
}

async function saveProfile() {
  if (!firstName.value || !firstName.value.trim()) {
    alert('Введите имя');
    return;
  }
  if (!lastName.value || !lastName.value.trim()) {
    alert('Введите фамилию');
    return;
  }
  if (!phone.value || !phone.value.trim()) {
    alert('Введите номер телефона');
    return;
  }

  const phoneDigits = phone.value.replace(/\D/g, '');
  if (phoneDigits.length !== 11) {
    alert('Введите полный номер телефона');
    return;
  }
  if (!phoneDigits.startsWith('7') && !phoneDigits.startsWith('8')) {
    alert('Номер должен начинаться с +7 или 8');
    return;
  }

  if (!email.value || !email.value.trim()) {
    alert('Введите электронную почту');
    return;
  }
  if (/[А-Яа-яЁё]/.test(email.value)) {
    alert('Email не должен содержать русские буквы');
    return;
  }
  const emailRegex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
  if (!emailRegex.test(email.value)) {
    alert('Введите корректный email');
    return;
  }

  if (!password.value || !password.value.trim()) {
    alert('Введите пароль');
    return;
  }
  if (password.value.length < 8) {
    alert('Пароль должен содержать минимум 8 символов');
    return;
  }
  if (!validatePassword(password.value)) {
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

  if (!confirmPassword.value || !confirmPassword.value.trim()) {
    alert('Подтвердите пароль');
    return;
  }
  if (password.value !== confirmPassword.value) {
    alert('Пароли не совпадают');
    return;
  }

  if (!birthDate.value) {
    alert('Выберите дату рождения');
    return;
  }
  if (!isAdult(birthDate.value)) {
    alert('Редактирование доступно только с 18 лет');
    return;
  }

  const payload = {
    firstName: firstName.value.trim(),
    lastName: lastName.value.trim(),
    patronymic: (patronymic.value || '').trim(),
    phone: phone.value,
    email: email.value.trim(),
    password: password.value,
    birthDate: birthDate.value,
  };

  saving.value = true;
  try {
    const res = await apiClient.updateUser(authState.userId, payload);
    alert(typeof res === 'string' ? res : 'Профиль успешно обновлен!');
    saveOriginal();
    isEditing.value = false;

    // Обновляем данные пользователя в authState
    setUser({
      ...authState.user,
      ...payload,
    });
  } catch (err) {
    const errorText = err.response?.data || err.message || 'Ошибка обновления профиля';
    alert(typeof errorText === 'string' ? errorText : JSON.stringify(errorText));
  } finally {
    saving.value = false;
  }
}

async function deleteAccount() {
  const confirmed = confirm('Вы уверены, что хотите удалить свой аккаунт?');
  if (!confirmed) return;

  deleting.value = true;
  try {
    await apiClient.deleteUser(authState.userId);
    alert('Аккаунт успешно удален');
    logout();
    router.push('/login');
  } catch (err) {
    const errorText = err.response?.data || err.message || 'Ошибка удаления аккаунта';
    alert(typeof errorText === 'string' ? errorText : JSON.stringify(errorText));
  } finally {
    deleting.value = false;
  }
}
</script>

<style scoped>
.profile-page {
  padding: 40px;
}

.profile-card {
  background-color: #ffffff;
  border-radius: 15px;
  padding: 40px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.profile-title {
  font-size: 30px;
  font-weight: bold;
  color: #111827;
}

.actions-group {
  display: flex;
  align-items: center;
  gap: 15px;
}

.btn-edit {
  width: 160px;
  height: 40px;
  background-color: #1E3A8A;
  color: #ffffff;
  font-size: 13px;
  font-weight: 600;
  border-radius: 10px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-cancel {
  width: 160px;
  height: 40px;
  background-color: #DC2626;
  color: #ffffff;
  font-size: 13px;
  font-weight: 600;
  border-radius: 10px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-delete {
  width: 160px;
  height: 40px;
  background-color: #DC2626;
  color: #ffffff;
  font-size: 13px;
  font-weight: 600;
  border-radius: 10px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-edit:disabled,
.btn-delete:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.profile-fields {
  display: flex;
  flex-direction: column;
}

.field-item {
  display: flex;
  flex-direction: column;
  margin-bottom: 10px;
}

.field-label {
  font-size: 13px;
  color: #111827;
  margin-bottom: 3px;
}

.profile-input {
  height: 36px;
  padding: 0 10px;
  border: 1px solid #D1D5DB;
  border-radius: 4px;
  background-color: #ffffff;
  font-size: 14px;
  color: #111827;
}

.profile-input:disabled {
  background-color: #F9FAFB;
  color: #6B7280;
  cursor: not-allowed;
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

.eye-btn {
  position: absolute;
  right: 10px;
  background: transparent;
  border: none;
  font-size: 14px;
  cursor: pointer;
  color: #666666;
}

@media (max-width: 768px) {
  .profile-page {
    padding: 20px 10px;
  }
  .card-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 15px;
  }
  .actions-group {
    flex-wrap: wrap;
    width: 100%;
  }
  .btn-edit, .btn-cancel, .btn-delete {
    width: 100%;
  }
}
</style>
