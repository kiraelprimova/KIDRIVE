<template>
  <div class="booking-card" @click="$emit('select', booking)">
    <!-- Миниатюра фото автомобиля -->
    <div class="card-thumb-box">
      <img
        :src="carImageSrc"
        :alt="`${categoryName} ${car?.model || ''}`"
        class="card-thumb-img"
        @error="onImageError"
      />
    </div>

    <!-- Основная информация о машине и дате -->
    <div class="booking-main">
      <div class="header-line">
        <h3 class="car-title">{{ categoryName }} {{ car?.model || 'Автомобиль' }}</h3>
        <span class="booking-id">№ {{ booking.idBooking }}</span>
      </div>

      <div class="booking-datetime">
        <svg class="cal-svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
          <line x1="16" y1="2" x2="16" y2="6"/>
          <line x1="8" y1="2" x2="8" y2="6"/>
          <line x1="3" y1="10" x2="21" y2="10"/>
        </svg>
        <span>{{ formattedDateTime }}</span>
      </div>

      <div class="booking-click-hint">
        Нажмите, чтобы открыть подробности
      </div>
    </div>

    <!-- Статус и действия -->
    <div class="status-col" @click.stop>
      <div class="status-badge" :style="{ backgroundColor: statusColor }">
        {{ statusName }}
      </div>

      <button
        v-if="isPending"
        class="btn-cancel"
        @click="cancelBooking"
        :disabled="cancelling"
      >
        {{ cancelling ? 'Отмена...' : 'Отменить' }}
      </button>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue';
import { apiClient } from '../api';
import { showToast, showConfirm } from '../notifications';

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

const emit = defineEmits(['cancelled', 'select']);
const cancelling = ref(false);

const car = computed(() => {
  return props.booking.car || props.booking.idCarNavigation || {};
});

const carImageSrc = computed(() => {
  const c = car.value;
  const path = c?.imagePath || c?.ImagePath;
  if (!path) return '/Resources/заглушка.png';
  if (path.startsWith('http') || path.startsWith('/')) return path;
  return `/Resources/${path}`;
});

function onImageError(e) {
  e.target.src = '/Resources/заглушка.png';
}

const statusName = computed(() => {
  return props.booking.idStatusNavigation?.statusName || 'Неизвестно';
});

const isPending = computed(() => {
  const s = statusName.value.toLowerCase();
  return s === 'в рассмотрении' || s === 'на рассмотрении';
});

const statusColor = computed(() => {
  switch (statusName.value.toLowerCase()) {
    case 'в рассмотрении':
    case 'на рассмотрении':
      return '#F59E0B'; // оранжевый
    case 'подтверждено':
      return '#10B981'; // зеленый
    case 'отменено':
      return '#EF4444'; // красный
    case 'завершено':
      return '#3182CE'; // синий
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
  return `${day}.${month}.${year} в ${time}`;
});

async function cancelBooking() {
  const confirmed = await showConfirm(
    'Вы уверены, что хотите отменить эту запись на тест-драйв?',
    'Отмена бронирования'
  );
  if (!confirmed) return;

  cancelling.value = true;
  try {
    await apiClient.cancelBooking(props.booking.idBooking);
    showToast('Бронирование отменено', 'info');
    emit('cancelled');
  } catch (err) {
    const errorText = err.response?.data || err.message || 'Ошибка отмены бронирования';
    showToast(typeof errorText === 'string' ? errorText : JSON.stringify(errorText), 'error');
  } finally {
    cancelling.value = false;
  }
}
</script>

<style scoped>
.booking-card {
  background-color: #ffffff;
  border-radius: 14px;
  padding: 16px;
  margin-bottom: 14px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.06);
  border: 1px solid #E5E7EB;
  display: flex;
  align-items: center;
  gap: 18px;
  cursor: pointer;
  transition: transform 0.15s ease, box-shadow 0.15s ease, border-color 0.15s ease;
  position: relative;
}

.booking-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.1);
  border-color: #BFDBFE;
}

.card-thumb-box {
  width: 140px;
  height: 95px;
  min-width: 140px;
  background-color: #F3F4F6;
  border-radius: 10px;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

.card-thumb-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.25s ease;
}

.booking-card:hover .card-thumb-img {
  transform: scale(1.05);
}

.booking-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.header-line {
  display: flex;
  align-items: center;
  gap: 10px;
}

.car-title {
  font-size: 18px;
  font-weight: 700;
  color: #111827;
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.booking-id {
  font-size: 12px;
  background-color: #F3F4F6;
  color: #6B7280;
  padding: 2px 8px;
  border-radius: 6px;
  font-weight: 500;
}

.booking-datetime {
  font-size: 14px;
  color: #4B5563;
  margin-top: 6px;
  display: flex;
  align-items: center;
  gap: 6px;
  font-weight: 500;
}

.cal-svg {
  color: #6B7280;
  flex-shrink: 0;
}

.booking-click-hint {
  font-size: 12px;
  color: #1E3A8A;
  margin-top: 8px;
  font-weight: 500;
}

.status-col {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 8px;
  flex-shrink: 0;
}

.status-badge {
  padding: 6px 14px;
  border-radius: 20px;
  color: #ffffff;
  font-size: 13px;
  font-weight: 600;
  white-space: nowrap;
}

.btn-cancel {
  padding: 6px 12px;
  background-color: #ffffff;
  color: #EF4444;
  border: 1px solid #FECACA;
  border-radius: 6px;
  font-size: 12.5px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-cancel:hover {
  background-color: #FEF2F2;
  border-color: #F87171;
}

@media (max-width: 640px) {
  .booking-card {
    flex-direction: column;
    align-items: flex-start;
  }
  .card-thumb-box {
    width: 100%;
    height: 140px;
  }
  .status-col {
    align-items: flex-start;
    flex-direction: row;
    width: 100%;
    justify-content: space-between;
    margin-top: 10px;
  }
}
</style>
