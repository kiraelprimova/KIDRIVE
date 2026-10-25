<template>
  <div class="profile-page">
    <div class="profile-card">
      <div class="card-header">
        <div>
          <h1 class="profile-title">Профиль</h1>
          <p class="profile-sub">Управляйте личными данными вашего аккаунта</p>
        </div>

        <div class="actions-group">
          <!-- Кнопка Изменить пароль -->
          <button
            class="btn-change-pwd"
            @click="goToChangePassword"
          >
            Изменить пароль
          </button>

          <!-- Кнопка Сохранить изменения -->
          <button
            class="btn-save"
            @click="saveProfile"
            :disabled="saving"
          >
            {{ saving ? 'Сохранение...' : 'Сохранить изменения' }}
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

      <!-- Поля профиля: всегда открыты для ввода -->
      <div class="profile-fields">
        <div class="field-item">
          <label class="field-label">Имя *</label>
          <input
            type="text"
            v-model="firstName"
            @input="onFirstNameInput"
            maxlength="20"
            placeholder="Ваше имя"
            class="profile-input"
          />
        </div>

        <div class="field-item">
          <label class="field-label">Фамилия *</label>
          <input
            type="text"
            v-model="lastName"
            @input="onLastNameInput"
            maxlength="25"
            placeholder="Ваша фамилия"
            class="profile-input"
          />
        </div>

        <div class="field-item">
          <label class="field-label">Отчество</label>
          <input
            type="text"
            v-model="patronymic"
            @input="onPatronymicInput"
            maxlength="25"
            placeholder="Ваше отчество (если есть)"
            class="profile-input"
          />
        </div>

        <div class="field-item">
          <label class="field-label">Номер телефона *</label>
          <input
            type="text"
            :value="phone"
            @input="onPhoneInput"
            maxlength="20"
            placeholder="+7 (___) ___-__-__"
            class="profile-input"
          />
          <span class="field-hint">Проверяется на уникальность среди аккаунтов</span>
        </div>

        <div class="field-item">
          <label class="field-label">Электронная почта *</label>
          <input
            type="email"
            v-model="email"
            maxlength="40"
            placeholder="example@mail.ru"
            class="profile-input"
          />
          <span class="field-hint">Проверяется на уникальность среди аккаунтов</span>
        </div>

        <div class="field-item">
          <label class="field-label">Дата рождения *</label>
          <input
            type="date"
            v-model="birthDate"
            class="profile-input"
          />
          <span class="field-hint">Доступно только лицам старше 18 лет</span>
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
  isAdult
} from '../auth';
import { showToast, showConfirm } from '../notifications';

const router = useRouter();

const saving = ref(false);
const deleting = ref(false);

const firstName = ref('');
const lastName = ref('');
const patronymic = ref('');
const phone = ref('');
const email = ref('');
const birthDate = ref('');

// Храним пароль пользователя, чтобы не затереть его при обычном обновлении профиля
let existingPassword = '';

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
    existingPassword = user.password || '';

    if (user.birthDate) {
      birthDate.value = String(user.birthDate).split('T')[0];
    }
  } catch (err) {
    console.error('Ошибка загрузки профиля:', err);
    showToast('Не удалось загрузить данные профиля', 'error');
  }
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

function goToChangePassword() {
  router.push('/app/change-password');
}

function cleanDigits(val) {
  return String(val || '').replace(/\D/g, '');
}

async function checkPhoneAndEmailUnique(newPhone, newEmail, currentUserId) {
  try {
    const clients = await apiClient.getAllClients();
    if (!Array.isArray(clients)) return { ok: true };

    const cleanNewPhone = cleanDigits(newPhone);
    const normalizedNewPhone = cleanNewPhone.startsWith('8') ? '7' + cleanNewPhone.substring(1) : cleanNewPhone;
    const cleanNewEmail = String(newEmail || '').trim().toLowerCase();

    for (const client of clients) {
      const clientId = client.idUser || client.IdUser;
      if (Number(clientId) === Number(currentUserId)) {
        continue; // Свой аккаунт пропускаем
      }

      // Проверка телефона
      const clientPhoneDigits = cleanDigits(client.phone);
      const normalizedClientPhone = clientPhoneDigits.startsWith('8') ? '7' + clientPhoneDigits.substring(1) : clientPhoneDigits;
      if (normalizedNewPhone && normalizedClientPhone && normalizedNewPhone === normalizedClientPhone) {
        return {
          ok: false,
          error: 'Пользователь с таким номером телефона уже существует!'
        };
      }

      // Проверка почты
      const clientEmail = String(client.email || '').trim().toLowerCase();
      if (cleanNewEmail && clientEmail && cleanNewEmail === clientEmail) {
        return {
          ok: false,
          error: 'Пользователь с таким адресом электронной почты уже существует!'
        };
      }
    }

    return { ok: true };
  } catch (err) {
    console.warn('Не удалось проверить уникальность через GetAllClients:', err);
    return { ok: true };
  }
}

async function saveProfile() {
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

  if (!email.value || !email.value.trim()) {
    showToast('Введите электронную почту', 'warning');
    return;
  }
  if (/[А-Яа-яЁё]/.test(email.value)) {
    showToast('Email не должен содержать русские буквы', 'warning');
    return;
  }
  const emailRegex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
  if (!emailRegex.test(email.value)) {
    showToast('Введите корректный email (например, user@mail.ru)', 'warning');
    return;
  }

  if (!birthDate.value) {
    showToast('Выберите дату рождения', 'warning');
    return;
  }
  if (!isAdult(birthDate.value)) {
    showToast('Сервис доступен только лицам с 18 лет', 'warning');
    return;
  }

  saving.value = true;

  // Проверка уникальности телефона и почты
  const uniqueCheck = await checkPhoneAndEmailUnique(phone.value, email.value, authState.userId);
  if (!uniqueCheck.ok) {
    showToast(uniqueCheck.error, 'error', 4500);
    saving.value = false;
    return;
  }

  const payload = {
    firstName: firstName.value.trim(),
    lastName: lastName.value.trim(),
    patronymic: (patronymic.value || '').trim(),
    phone: phone.value,
    email: email.value.trim(),
    password: existingPassword, // сохраняем существующий пароль
    birthDate: birthDate.value,
  };

  try {
    const res = await apiClient.updateUser(authState.userId, payload);
    showToast(typeof res === 'string' ? res : 'Профиль успешно обновлен!', 'success');

    // Обновляем данные пользователя в authState
    setUser({
      ...authState.user,
      ...payload,
    });
  } catch (err) {
    const errorText = err.response?.data || err.message || 'Ошибка обновления профиля';
    showToast(typeof errorText === 'string' ? errorText : JSON.stringify(errorText), 'error');
  } finally {
    saving.value = false;
  }
}

async function deleteAccount() {
  const confirmed = await showConfirm(
    'Вы уверены, что хотите удалить свой аккаунт? Это действие нельзя будет отменить.',
    'Удаление аккаунта'
  );
  if (!confirmed) return;

  deleting.value = true;
  try {
    await apiClient.deleteUser(authState.userId);
    showToast('Аккаунт успешно удален', 'info');
    logout();
    router.push('/login');
  } catch (err) {
    const errorText = err.response?.data || err.message || 'Ошибка удаления аккаунта';
    showToast(typeof errorText === 'string' ? errorText : JSON.stringify(errorText), 'error');
  } finally {
    deleting.value = false;
  }
}
</script>

<style scoped>
.profile-page {
  padding: 32px;
  max-width: 900px;
  margin: 0 auto;
}

.profile-card {
  background-color: #ffffff;
  border-radius: 14px;
  padding: 32px 36px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.05);
  border: 1px solid #E5E7EB;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  flex-wrap: wrap;
  gap: 16px;
  border-bottom: 1px solid #F3F4F6;
  padding-bottom: 20px;
  margin-bottom: 28px;
}

.profile-title {
  font-size: 26px;
  font-weight: 700;
  color: #111827;
  margin: 0;
}

.profile-sub {
  font-size: 13.5px;
  color: #6B7280;
  margin: 4px 0 0 0;
}

.actions-group {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.btn-change-pwd {
  height: 38px;
  padding: 0 16px;
  background-color: #EFF6FF;
  color: #1E3A8A;
  border: 1px solid #BFDBFE;
  border-radius: 8px;
  font-size: 13.5px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
  display: flex;
  align-items: center;
  gap: 6px;
}

.btn-change-pwd:hover {
  background-color: #DBEAFE;
}

.btn-save {
  height: 38px;
  padding: 0 18px;
  background-color: #1E3A8A;
  color: #ffffff;
  border: none;
  border-radius: 8px;
  font-size: 13.5px;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.15s ease;
}

.btn-save:hover {
  background-color: #172554;
}

.btn-delete {
  height: 38px;
  padding: 0 14px;
  background-color: transparent;
  color: #DC2626;
  border: 1px solid #FECACA;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-delete:hover {
  background-color: #FEF2F2;
  border-color: #F87171;
}

/* Поля профиля */
.profile-fields {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 22px;
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

.profile-input {
  height: 42px;
  padding: 0 14px;
  background-color: #F9FAFB;
  border: 1px solid #D1D5DB;
  border-radius: 8px;
  font-size: 14px;
  color: #111827;
  outline: none;
  transition: border-color 0.15s ease, background-color 0.15s ease;
}

.profile-input:focus {
  border-color: #1E3A8A;
  background-color: #ffffff;
}

.field-hint {
  font-size: 12px;
  color: #9CA3AF;
  margin-top: 5px;
}

@media (max-width: 768px) {
  .profile-page {
    padding: 16px;
  }
  .profile-fields {
    grid-template-columns: 1fr;
  }
  .card-header {
    flex-direction: column;
  }
  .actions-group {
    width: 100%;
  }
  .btn-save, .btn-change-pwd, .btn-delete {
    width: 100%;
    justify-content: center;
  }
}
</style>
