<template>
  <router-view />
  <NotificationToast />
</template>

<script setup>
import NotificationToast from './components/NotificationToast.vue';
import { showToast } from './notifications';

// Перехват нативного alert, чтобы браузер не показывал окно localhost:5173
if (typeof window !== 'undefined') {
  window.alert = (message) => {
    showToast(message, 'info');
  };
}
</script>

<style>
</style>
