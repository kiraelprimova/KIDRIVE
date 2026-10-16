<template>
  <div class="welcome-container">
    <div class="welcome-buttons">
      <button class="btn-login" @click="goToLogin">
        Войти
      </button>
      <button class="btn-register" @click="goToRegister">
        Регистрация
      </button>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router';

const router = useRouter();

function goToLogin() {
  router.push('/login');
}

function goToRegister() {
  router.push('/register');
}
</script>

<style scoped>
.welcome-container {
  width: 100vw;
  height: 100vh;
  position: relative;
  background-image: url('/Resources/welcome.png');
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  display: flex;
  align-items: flex-end;
  justify-content: flex-start;
}

.welcome-buttons {
  display: flex;
  flex-direction: row;
  align-items: center;
  margin-left: 75px;
  margin-bottom: 95px;
}

.btn-login {
  width: 180px;
  height: 52px;
  background-color: #1E3A8A;
  color: #ffffff;
  font-size: 16px;
  font-weight: 600;
  border-radius: 12px;
  border: none;
  cursor: pointer;
  margin-right: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.2s ease, transform 0.1s ease;
}

.btn-login:hover {
  opacity: 0.92;
}

.btn-login:active {
  transform: translateY(1px);
}

.btn-register {
  width: 220px;
  height: 52px;
  background-color: #ffffff;
  color: #1E3A8A;
  font-size: 16px;
  font-weight: 600;
  border: 2px solid #1E3A8A;
  border-radius: 12px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.2s ease, transform 0.1s ease;
}

.btn-register:hover {
  background-color: #f8fafc;
}

.btn-register:active {
  transform: translateY(1px);
}

@media (max-width: 640px) {
  .welcome-buttons {
    margin-left: 20px;
    margin-bottom: 40px;
    flex-direction: column;
    gap: 14px;
  }
  .btn-login, .btn-register {
    width: 220px;
    margin-right: 0;
  }
}
</style>
