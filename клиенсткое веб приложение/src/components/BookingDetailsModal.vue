<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-window">
      <div class="modal-header">
        <h2 class="modal-title">Детали бронирования #{{ booking.idBooking }}</h2>
        <button class="close-btn" @click="$emit('close')">✕</button>
      </div>

      <div class="modal-content">
        <!-- Блок автомобиля -->
        <div class="car-section">
          <div class="image-wrapper">
            <img
              :src="carImageSrc"
              :alt="carTitle"
              class="car-image"
              @error="onImageError"
            />
            <div class="status-overlay-badge" :style="{ backgroundColor: statusColor }">
              {{ statusName }}
            </div>
          </div>

          <div class="car-info">
            <h3 class="car-name">{{ carTitle }}</h3>
            <div class="car-specs">
              <span v-if="car.yearCar">{{ car.yearCar }} г.</span>
              <span v-if="car.powerHp">• {{ car.powerHp }} л.с.</span>
              <span v-if="car.price">• {{ Number(car.price).toLocaleString('ru-RU') }} ₽ / час</span>
            </div>
          </div>
        </div>

        <!-- Блок информации о бронировании -->
        <div class="booking-section">
          <h4 class="section-title">Информация о записи</h4>
          <div class="info-grid">
            <div class="info-card">
              <div class="info-label">Дата тест-драйва</div>
              <div class="info-value">{{ formattedDate }}</div>
            </div>

            <div class="info-card">
              <div class="info-label">Время записи</div>
              <div class="info-value">{{ formattedTime }}</div>
            </div>

            <div class="info-card">
              <div class="info-label">Текущий статус</div>
              <div class="info-value status-text" :style="{ color: statusColor }">
                ● {{ statusName }}
              </div>
            </div>

            <div class="info-card">
              <div class="info-label">Стоимость за час</div>
              <div class="info-value price-text">
                {{ Number(car.price || 0).toLocaleString('ru-RU') }} ₽
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="modal-footer">
        <button
          v-if="canCancel"
          class="btn-cancel-booking"
          @click="handleCancel"
          :disabled="cancelling"
        >
          {{ cancelling ? 'Отмена...' : 'Отменить бронирование' }}
        </button>
        <button class="btn-close-modal" @click="$emit('close')">
          Закрыть
        </button>
      </div>
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

const emit = defineEmits(['close', 'cancelled']);
const cancelling = ref(false);

const car = computed(() => {
  return props.booking.car || props.booking.idCarNavigation || {};
});

const carTitle = computed(() => {
  const brand = props.categoryName || car.value.categoryName || '';
  const model = car.value.model || 'Автомобиль';
  return `${brand} ${model}`.trim();
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

const canCancel = computed(() => {
  const s = statusName.value.toLowerCase();
  return s === 'в рассмотрении' || s === 'на рассмотрении' || s === 'подтверждено';
});

const statusColor = computed(() => {
  switch (statusName.value.toLowerCase()) {
    case 'в рассмотрении':
    case 'на рассмотрении':
      return '#F59E0B';
    case 'подтверждено':
      return '#10B981';
    case 'отменено':
      return '#EF4444';
    case 'завершено':
      return '#3182CE';
    default:
      return '#6B7280';
  }
});

const formattedDate = computed(() => {
  if (!props.booking.bookingDate) return 'Не указана';
  const d = new Date(props.booking.bookingDate);
  const day = String(d.getDate()).padStart(2, '0');
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const year = d.getFullYear();
  return `${day}.${month}.${year}`;
});

const formattedTime = computed(() => {
  return props.booking.bookingTime || 'Не указано';
});

async function handleCancel() {
  const confirmed = await showConfirm(
    'Вы уверены, что хотите отменить эту запись на тест-драйв?',
    'Отмена бронирования'
  );
  if (!confirmed) return;

  cancelling.value = true;
  try {
    await apiClient.cancelBooking(props.booking.idBooking);
    showToast('Бронирование успешно отменено', 'info');
    emit('cancelled');
    emit('close');
  } catch (err) {
    const errorText = err.response?.data || err.message || 'Ошибка отмены бронирования';
    showToast(typeof errorText === 'string' ? errorText : JSON.stringify(errorText), 'error');
  } finally {
    cancelling.value = false;
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background-color: rgba(0, 0, 0, 0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
}

.modal-window {
  background-color: #ffffff;
  border-radius: 16px;
  width: 100%;
  max-width: 580px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.25);
  display: flex;
  flex-direction: column;
}

.modal-header {
  padding: 20px 24px;
  border-bottom: 1px solid #E5E7EB;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-title {
  font-size: 20px;
  font-weight: 700;
  color: #111827;
  margin: 0;
}

.close-btn {
  background: none;
  border: none;
  font-size: 20px;
  color: #9CA3AF;
  cursor: pointer;
  padding: 4px;
}

.close-btn:hover {
  color: #111827;
}

.modal-content {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.car-section {
  background-color: #F9FAFB;
  border: 1px solid #E5E7EB;
  border-radius: 12px;
  overflow: hidden;
}

.image-wrapper {
  position: relative;
  height: 200px;
  width: 100%;
  background-color: #F3F4F6;
}

.car-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.status-overlay-badge {
  position: absolute;
  top: 12px;
  right: 12px;
  color: #ffffff;
  font-size: 13px;
  font-weight: 600;
  padding: 5px 12px;
  border-radius: 20px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
}

.car-info {
  padding: 16px;
}

.car-name {
  font-size: 18px;
  font-weight: 700;
  color: #111827;
  margin: 0;
}

.car-specs {
  font-size: 13px;
  color: #6B7280;
  margin-top: 4px;
}

.section-title {
  font-size: 15px;
  font-weight: 600;
  color: #374151;
  margin: 0 0 12px 0;
}

.info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.info-card {
  background-color: #F9FAFB;
  border: 1px solid #E5E7EB;
  border-radius: 10px;
  padding: 12px 14px;
}

.info-label {
  font-size: 12px;
  color: #6B7280;
  margin-bottom: 4px;
}

.info-value {
  font-size: 14px;
  font-weight: 600;
  color: #111827;
}

.status-text {
  font-weight: 700;
}

.price-text {
  color: #1E3A8A;
}

.modal-footer {
  padding: 16px 24px;
  border-top: 1px solid #E5E7EB;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.btn-cancel-booking {
  padding: 9px 18px;
  background-color: #FEF2F2;
  color: #DC2626;
  border: 1px solid #FECACA;
  border-radius: 8px;
  font-size: 13.5px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-cancel-booking:hover {
  background-color: #FEE2E2;
}

.btn-close-modal {
  padding: 9px 20px;
  background-color: #1E3A8A;
  color: #ffffff;
  border: none;
  border-radius: 8px;
  font-size: 13.5px;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.15s ease;
}

.btn-close-modal:hover {
  background-color: #172554;
}
</style>
