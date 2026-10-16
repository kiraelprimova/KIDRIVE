<template>
  <div class="booking-card">
    <div class="booking-info">
      <h3 class="car-title">{{ categoryName }} {{ car?.model || 'Автомобиль' }}</h3>
      <div class="booking-datetime">{{ formattedDateTime }}</div>
    </div>

    <div class="status-col">
      <div class="status-badge" :style="{ backgroundColor: statusColor }">
        {{ statusName }}
      </div>

      <button
        v-if="isPending"
        class="btn-cancel"
        @click="cancelBooking"
        :disabled="cancelling"
      >
        {{ cancelling ? 'Отмена...' : 'Отменить бронирование' }}
      </button>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue';
import { apiClient } from '../api';

const props = defineProps({
  booking: {
    type: Object,
    required: true,
  },
  categoryName: {
    type: String,
    default: '',
  },
});

const emit = defineEmits(['cancelled']);
const cancelling = ref(false);

const car = computed(() => {
  return props.booking.idCarNavigation || {};
});

const statusName = computed(() => {
  return props.booking.idStatusNavigation?.statusName || 'Неизвестно';
});

const isPending = computed(() => {
  return statusName.value.toLowerCase() === 'в рассмотрении';
});

const statusColor = computed(() => {
  switch (statusName.value.toLowerCase()) {
    case 'в рассмотрении':
      return '#F59E0B';
    case 'подтверждено':
      return '#10B981';
    case 'отменено':
      return '#EF4444';
    default:
      return '#6B7280';
  }
});

const formattedDateTime = computed(() => {
  if (!props.booking.bookingDate) return '';
  const d = new Date(props.booking.bookingDate);
  const day = String(d.getDate()).padStart(2, '0');
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const year = d.getFullYear();
  const time = props.booking.bookingTime || '';
  return `${day}.${month}.${year} ${time}`;
});

async function cancelBooking() {
  const confirmed = confirm('Вы уверены, что хотите отменить бронирование?');
  if (!confirmed) return;

  cancelling.value = true;
  try {
    await apiClient.cancelBooking(props.booking.idBooking);
    alert('Бронирование отменено');
    emit('cancelled');
  } catch (err) {
    const errorText = err.response?.data || err.message || 'Ошибка отмены бронирования';
    alert(typeof errorText === 'string' ? errorText : JSON.stringify(errorText));
  } finally {
    cancelling.value = false;
  }
}
</script>

<style scoped>
.booking-card {
  background-color: #ffffff;
  border-radius: 15px;
  padding: 20px;
  margin-bottom: 15px;
  box-shadow: 0 3px 15px rgba(0, 0, 0, 0.12);
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-sizing: border-box;
}

.booking-info {
  display: flex;
  flex-direction: column;
}

.car-title {
  font-size: 24px;
  font-weight: bold;
  color: #111827;
  margin-bottom: 0;
}

.booking-datetime {
  font-size: 16px;
  color: #6B7280;
  margin-top: 15px;
}

.status-col {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

.status-badge {
  height: 35px;
  padding: 0 15px;
  border-radius: 10px;
  color: #ffffff;
  font-size: 14px;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
  white-space: nowrap;
}

.btn-cancel {
  width: 170px;
  height: 34px;
  margin-top: 10px;
  background-color: #EF4444;
  color: #ffffff;
  font-size: 13px;
  border-radius: 8px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-cancel:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

@media (max-width: 640px) {
  .booking-card {
    flex-direction: column;
    align-items: flex-start;
    gap: 15px;
  }
  .status-col {
    align-items: flex-start;
    width: 100%;
  }
  .btn-cancel {
    width: 100%;
  }
}
</style>
