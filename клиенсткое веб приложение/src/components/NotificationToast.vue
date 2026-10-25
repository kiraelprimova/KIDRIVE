<template>
  <div class="notifications-container">
    <!-- Список всплывающих тостов -->
    <transition-group name="toast-fade" tag="div" class="toast-list">
      <div
        v-for="toast in toasts"
        :key="toast.id"
        class="toast-item"
        :class="`toast-${toast.type}`"
        @click="removeToast(toast.id)"
      >
        <span class="toast-icon">
          <svg v-if="toast.type === 'success'" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="20 6 9 17 4 12"/>
          </svg>
          <svg v-else-if="toast.type === 'error'" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18"/>
            <line x1="6" y1="6" x2="18" y2="18"/>
          </svg>
          <svg v-else-if="toast.type === 'warning'" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"/>
            <line x1="12" y1="8" x2="12" y2="12"/>
            <line x1="12" y1="16" x2="12.01" y2="16"/>
          </svg>
          <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"/>
            <line x1="12" y1="16" x2="12" y2="12"/>
            <line x1="12" y1="8" x2="12.01" y2="8"/>
          </svg>
        </span>
        <span class="toast-text">{{ toast.text }}</span>
        <button class="toast-close" @click.stop="removeToast(toast.id)">×</button>
      </div>
    </transition-group>

    <!-- Модальное окно подтверждения (вместо browser confirm) -->
    <div v-if="confirmDialog.isOpen" class="confirm-overlay" @click.self="handleConfirm(false)">
      <div class="confirm-modal">
        <h3 class="confirm-title">{{ confirmDialog.title }}</h3>
        <p class="confirm-message">{{ confirmDialog.message }}</p>
        <div class="confirm-actions">
          <button class="btn-confirm-cancel" @click="handleConfirm(false)">
            Отмена
          </button>
          <button class="btn-confirm-ok" @click="handleConfirm(true)">
            Подтвердить
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { toasts, removeToast, confirmDialog, handleConfirm } from '../notifications';
</script>

<style scoped>
.notifications-container {
  position: fixed;
  z-index: 99999;
  pointer-events: none;
  inset: 0;
}

.toast-list {
  position: fixed;
  top: 24px;
  right: 24px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  pointer-events: auto;
  max-width: 380px;
  width: calc(100% - 48px);
}

.toast-item {
  display: flex;
  align-items: center;
  padding: 12px 16px;
  background-color: #ffffff;
  border-radius: 10px;
  box-shadow: 0 4px 18px rgba(0, 0, 0, 0.15);
  font-size: 14px;
  color: #1f2937;
  cursor: pointer;
  border-left: 5px solid #1E3A8A;
  transition: all 0.2s ease;
  word-break: break-word;
}

.toast-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.2);
}

.toast-success {
  border-left-color: #10B981;
}

.toast-error {
  border-left-color: #EF4444;
}

.toast-warning {
  border-left-color: #F59E0B;
}

.toast-icon {
  font-weight: bold;
  font-size: 16px;
  margin-right: 12px;
  flex-shrink: 0;
}

.toast-success .toast-icon {
  color: #10B981;
}

.toast-error .toast-icon {
  color: #EF4444;
}

.toast-warning .toast-icon {
  color: #F59E0B;
}

.toast-info .toast-icon {
  color: #1E3A8A;
}

.toast-text {
  flex: 1;
  line-height: 1.4;
  white-space: pre-line;
}

.toast-close {
  background: none;
  border: none;
  font-size: 18px;
  color: #9CA3AF;
  cursor: pointer;
  margin-left: 10px;
  padding: 0 4px;
  line-height: 1;
}

.toast-close:hover {
  color: #111827;
}

/* Анимация тостов */
.toast-fade-enter-active,
.toast-fade-leave-active {
  transition: all 0.25s ease;
}

.toast-fade-enter-from {
  opacity: 0;
  transform: translateX(30px);
}

.toast-fade-leave-to {
  opacity: 0;
  transform: translateY(-20px);
}

/* Окно подтверждения */
.confirm-overlay {
  position: fixed;
  inset: 0;
  background-color: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: auto;
  z-index: 100000;
  padding: 20px;
}

.confirm-modal {
  background-color: #ffffff;
  border-radius: 14px;
  padding: 24px;
  width: 100%;
  max-width: 420px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
  display: flex;
  flex-direction: column;
}

.confirm-title {
  margin: 0 0 10px 0;
  font-size: 18px;
  font-weight: bold;
  color: #111827;
}

.confirm-message {
  margin: 0 0 20px 0;
  font-size: 14px;
  color: #4B5563;
  line-height: 1.5;
  white-space: pre-line;
}

.confirm-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.btn-confirm-cancel {
  padding: 8px 16px;
  border-radius: 8px;
  border: 1px solid #D1D5DB;
  background-color: #ffffff;
  color: #374151;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.15s ease;
}

.btn-confirm-cancel:hover {
  background-color: #F3F4F6;
}

.btn-confirm-ok {
  padding: 8px 18px;
  border-radius: 8px;
  border: none;
  background-color: #1E3A8A;
  color: #ffffff;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.15s ease;
}

.btn-confirm-ok:hover {
  background-color: #172554;
}
</style>
