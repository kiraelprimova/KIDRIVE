<template>
  <div class="client-layout">
    <!-- Левый сайдбар -->
    <aside class="sidebar">
      <div class="sidebar-header">
        <h2 class="app-brand">Автосалон KIDRIVE</h2>
      </div>

      <nav class="sidebar-nav">
        <button
          class="nav-item"
          :class="{ active: currentTab === 'catalog' }"
          @click="navigate('catalog')"
        >
          <span class="nav-icon">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M19 17h2c.6 0 1-.4 1-1v-3c0-.9-.7-1.7-1.5-1.9C18.7 10.6 16 10 16 10s-1.3-1.4-2.2-2.3c-.5-.4-1.1-.7-1.8-.7H5c-.6 0-1.1.4-1.4.9l-1.4 2.9C2.1 11.1 2 11.5 2 12v4c0 .6.4 1 1 1h2"/>
              <circle cx="7" cy="17" r="2"/>
              <circle cx="17" cy="17" r="2"/>
            </svg>
          </span>
          <span class="nav-text">Каталог автомобилей</span>
        </button>

        <button
          class="nav-item"
          :class="{ active: currentTab === 'favorites' }"
          @click="navigate('favorites')"
        >
          <span class="nav-icon">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/>
            </svg>
          </span>
          <span class="nav-text">Избранное</span>
        </button>

        <button
          class="nav-item"
          :class="{ active: currentTab === 'ai-chat' }"
          @click="navigate('ai-chat')"
        >
          <span class="nav-icon">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
            </svg>
          </span>
          <span class="nav-text">Подбор авто</span>
        </button>

        <button
          class="nav-item"
          :class="{ active: currentTab === 'bookings' }"
          @click="navigate('bookings')"
        >
          <span class="nav-icon">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
              <line x1="16" y1="2" x2="16" y2="6"/>
              <line x1="8" y1="2" x2="8" y2="6"/>
              <line x1="3" y1="10" x2="21" y2="10"/>
            </svg>
          </span>
          <span class="nav-text">Мои бронирования</span>
        </button>

        <button
          class="nav-item"
          :class="{ active: currentTab === 'profile' }"
          @click="navigate('profile')"
        >
          <span class="nav-icon">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
              <circle cx="12" cy="7" r="4"/>
            </svg>
          </span>
          <span class="nav-text">Профиль</span>
        </button>
      </nav>
    </aside>

    <!-- Основная рабочая область -->
    <div class="main-wrapper">
      <!-- Верхняя панель (увеличенная и гармоничная) -->
      <header class="topbar">
        <div class="user-profile-badge">
          <div class="user-avatar-circle">
            {{ userInitial }}
          </div>
          <div class="user-info">
            <div class="user-name">{{ displayName }}</div>
            <div class="user-email">{{ displayEmail }}</div>
          </div>
        </div>

        <button class="exit-btn" @click="handleExit">
          <span class="exit-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
              <polyline points="16 17 21 12 16 7"/>
              <line x1="21" y1="12" x2="9" y2="12"/>
            </svg>
          </span>
          <span>{{ authState.isGuest ? 'Войти' : 'Выйти' }}</span>
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
import { showConfirm } from '../notifications';

const route = useRoute();
const router = useRouter();

const currentTab = computed(() => {
  if (route.path.includes('/favorites')) return 'favorites';
  if (route.path.includes('/bookings')) return 'bookings';
  if (route.path.includes('/profile') || route.path.includes('/change-password')) return 'profile';
  if (route.path.includes('/ai-chat')) return 'ai-chat';
  return 'catalog';
});

const displayName = computed(() => {
  if (authState.isGuest) return 'Гость';
  const u = authState.user;
  return `${u?.lastName || ''} ${u?.firstName || ''} ${u?.patronymic || ''}`.trim() || 'Пользователь';
});

const userInitial = computed(() => {
  if (authState.isGuest) return 'Г';
  const name = authState.user?.firstName || authState.user?.lastName;
  return name ? name.charAt(0).toUpperCase() : 'У';
});

const displayEmail = computed(() => {
  if (authState.isGuest) return 'Не авторизован';
  return authState.user?.email || authState.user?.phone || '';
});

async function checkGuest(message) {
  if (!authState.isGuest) return true;
  const wantLogin = await showConfirm(`${message}\n\nХотите войти в систему?`, 'Требуется авторизация');
  if (wantLogin) {
    router.push('/login');
  }
  return false;
}

async function navigate(tab) {
  if (tab === 'catalog') {
    router.push('/app/catalog');
  } else if (tab === 'favorites') {
    if (await checkGuest('Чтобы просмотреть избранное, вам нужно авторизоваться.')) {
      router.push('/app/favorites');
    }
  } else if (tab === 'ai-chat') {
    if (await checkGuest('Чтобы использовать подбор авто, вам нужно авторизоваться.')) {
      router.push('/app/ai-chat');
    }
  } else if (tab === 'bookings') {
    if (await checkGuest('Чтобы просмотреть бронирования, вам нужно авторизоваться.')) {
      router.push('/app/bookings');
    }
  } else if (tab === 'profile') {
    if (await checkGuest('Чтобы просмотреть профиль, вам нужно авторизоваться.')) {
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
  width: 220px;
  min-width: 220px;
  background-color: #ffffff;
  border-right: 1px solid #E5E7EB;
  display: flex;
  flex-direction: column;
}

.sidebar-header {
  padding: 24px 18px 8px 18px;
}

.app-brand {
  font-size: 20px;
  font-weight: 700;
  color: #1E3A8A;
  margin: 0;
  letter-spacing: -0.3px;
  line-height: 1.25;
}

.sidebar-nav {
  margin-top: 25px;
  padding: 0 10px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.nav-item {
  height: 42px;
  width: 100%;
  background: transparent;
  border: none;
  border-radius: 8px;
  display: flex;
  align-items: center;
  padding: 0 12px;
  cursor: pointer;
  text-align: left;
  transition: background-color 0.15s ease;
}

.nav-item .nav-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 10px;
  color: #4B5563;
}

.nav-item .nav-text {
  font-size: 13.5px;
  color: #111827;
  white-space: nowrap;
  font-weight: 500;
}

.nav-item:hover {
  background-color: #F3F4F6;
}

.nav-item.active {
  background-color: #1E3A8A;
}

.nav-item.active .nav-text,
.nav-item.active .nav-icon {
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

/* Увеличенная гармоничная шапка */
.topbar {
  height: 68px;
  min-height: 68px;
  background-color: #ffffff;
  border-bottom: 1px solid #E5E7EB;
  padding: 0 28px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.03);
}

.user-profile-badge {
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-avatar-circle {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  background: #EEF2FF;
  color: #1E3A8A;
  font-weight: 700;
  font-size: 15px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid #DBEAFE;
}

.user-info {
  display: flex;
  flex-direction: column;
}

.user-name {
  font-size: 15px;
  font-weight: 600;
  color: #111827;
  line-height: 1.2;
}

.user-email {
  font-size: 12.5px;
  color: #6B7280;
  margin-top: 3px;
}

.exit-btn {
  height: 38px;
  padding: 0 16px;
  background: #ffffff;
  border: 1px solid #D1D5DB;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  font-size: 13.5px;
  font-weight: 500;
  color: #374151;
  transition: all 0.15s ease;
}

.exit-btn:hover {
  background-color: #F9FAFB;
  color: #111827;
  border-color: #9CA3AF;
}

.exit-icon {
  font-size: 15px;
}

.content-area {
  flex: 1;
  overflow-y: auto;
  background-color: #F5F5F5;
}
</style>
