import { ref } from 'vue';

export const toasts = ref([]);
export const confirmDialog = ref({
  isOpen: false,
  title: 'Подтверждение',
  message: '',
  resolve: null,
});

let toastId = 0;

export function showToast(message, type = 'info', duration = 3500) {
  const id = ++toastId;
  const text = typeof message === 'string' ? message : JSON.stringify(message);
  toasts.value.push({ id, text, type });

  if (duration > 0) {
    setTimeout(() => {
      removeToast(id);
    }, duration);
  }
}

export function removeToast(id) {
  toasts.value = toasts.value.filter(t => t.id !== id);
}

export function showConfirm(message, title = 'Подтверждение') {
  return new Promise((resolve) => {
    confirmDialog.value = {
      isOpen: true,
      title,
      message,
      resolve,
    };
  });
}

export function handleConfirm(result) {
  if (confirmDialog.value.resolve) {
    confirmDialog.value.resolve(result);
  }
  confirmDialog.value.isOpen = false;
  confirmDialog.value.resolve = null;
}
