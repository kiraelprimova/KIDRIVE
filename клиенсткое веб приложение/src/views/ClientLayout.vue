<template>
  <div class="client-layout">
    <!-- Левый сайдбар -->
    <aside class="sidebar">
      <div class="sidebar-header">
        <h2 class="app-brand">Автосалон</h2>
        <span class="app-sub">Тест-драйв</span>
      </div>

      <nav class="sidebar-nav">
        <button
          class="nav-item"
          :class="{ active: currentTab === 'catalog' }"
          @click="navigate('catalog')"
        >
          <span class="nav-icon">🚘</span>
          <span class="nav-text">Каталог автомобилей</span>
        </button>

        <button
          class="nav-item"
          :class="{ active: currentTab === 'bookings' }"
          @click="navigate('bookings')"
        >
          <span class="nav-icon">📅</span>
          <span class="nav-text">Мои бронирования</span>
        </button>

        <button
          class="nav-item"
          :class="{ active: currentTab === 'profile' }"
          @click="navigate('profile')"
        >
          <span class="nav-icon">👤</span>
          <span class="nav-text">Профиль</span>
        </button>
      </nav>
    </aside>

    <!-- Основная рабочая область -->
    <div class="main-wrapper">
      <!-- Верхняя панель -->
      <header class="topbar">
        <div class="user-info">
          <div class="user-name">{{ displayName }}</div>
          <div class="user-email">{{ displayEmail }}</div>
        </div>

        <button class="exit-btn" @click="handleExit">
          <span class="exit-icon">↪</span>
          <span>Выйти</span>
        </button>
      </header>

      <!-- Контент выбранной страницы -->
      <main class="content-area">
        <router-view />
      </main>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { authState, logout } from '../auth';

const route = useRoute();
const router = useRouter();

const currentTab = computed(() => {
  if (route.path.includes('/bookings')) return 'bookings';
  if (route.path.includes('/profile')) return 'profile';
  return 'catalog';
});

const displayName = computed(() => {
  if (authState.isGuest) return 'Гость';
  const u = authState.user;
  return `${u.lastName || ''} ${u.firstName || ''} ${u.patronymic || ''}`.trim() || 'Пользователь';
});

const displayEmail = computed(() => {
  if (authState.isGuest) return 'Не авторизован';
  return authState.user?.email || '';
});

function checkGuest(message) {
  if (!authState.isGuest) return true;
  const wantLogin = confirm(`${message}\n\nХотите войти в систему?`);
  if (wantLogin) {
    router.push('/login');
  }
  return false;
}

function navigate(tab) {
  if (tab === 'catalog') {
    router.push('/app/catalog');
  } else if (tab === 'bookings') {
    if (checkGuest('Чтобы просмотреть бронирования вам нужно авторизоваться.')) {
      router.push('/app/bookings');
    }
  } else if (tab === 'profile') {
    if (checkGuest('Чтобы просмотреть профиль вам нужно авторизоваться.')) {
      router.push('/app/profile');
    }
  }
}

function handleExit() {
  logout();
  router.push('/login');
}
</script>

<style scoped>
.client-layout {
  display: flex;
  width: 100vw;
  height: 100vh;
  overflow: hidden;
  background-color: #F5F5F5;
}

/* Сайдбар */
.sidebar {
  width: 200px;
  min-width: 200px;
  background-color: #ffffff;
  border-right: 1px solid #E5E7EB;
  display: flex;
  flex-direction: column;
}

.sidebar-header {
  padding: 20px 10px 0 10px;
}

.app-brand {
  font-size: 22px;
  font-weight: bold;
  color: #1E3A8A;
  margin: 0;
}

.app-sub {
  font-size: 12px;
  color: #9CA3AF;
  display: block;
  margin-top: 5px;
}

.sidebar-nav {
  margin-top: 25px;
  padding: 0 8px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.nav-item {
  height: 34px;
  width: 100%;
  background: transparent;
  border: none;
  border-radius: 8px;
  display: flex;
  align-items: center;
  padding: 0 10px;
  cursor: pointer;
  text-align: left;
  transition: background-color 0.15s ease;
}

.nav-item .nav-icon {
  font-size: 14px;
  margin-right: 10px;
}

.nav-item .nav-text {
  font-size: 13px;
  color: #111827;
  white-space: nowrap;
}

.nav-item.active {
  background-color: #1E3A8A;
}

.nav-item.active .nav-text {
  color: #ffffff;
}

/* Основная часть */
.main-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
  height: 100vh;
}

.topbar {
  height: 55px;
  min-height: 55px;
  background-color: #ffffff;
  border-bottom: 1px solid #E5E7EB;
  padding: 0 15px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.user-info {
  display: flex;
  flex-direction: column;
}

.user-name {
  font-size: 15px;
  font-weight: 600;
  color: #111827;
}

.user-email {
  font-size: 12px;
  color: #9CA3AF;
  margin-top: 2px;
}

.exit-btn {
  width: 90px;
  height: 30px;
  background: transparent;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  font-size: 13px;
  color: #6B7280;
}

.exit-btn:hover {
  color: #111827;
}

.exit-icon {
  font-size: 13px;
}

.content-area {
  flex: 1;
  overflow-y: auto;
  background-color: #F5F5F5;
}
</style>
