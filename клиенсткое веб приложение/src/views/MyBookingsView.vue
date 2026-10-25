<template>
  <div class="bookings-page">
    <div class="header-section">
      <h1 class="page-title">Мои бронирования</h1>
      <span class="bookings-counter" v-if="!loading">Всего записей: {{ filteredBookings.length }}</span>
    </div>

    <div class="filter-wrapper">
      <div class="filter-tabs">
        <button
          v-for="status in ['Все', 'В рассмотрении', 'Подтверждено', 'Завершено', 'Отменено']"
          :key="status"
          class="status-tab-btn"
          :class="{ active: selectedStatus === status }"
          @click="selectedStatus = status"
        >
          {{ status }}
        </button>
      </div>
    </div>

    <!-- Загрузка -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <div>Загрузка ваших бронирований...</div>
    </div>

    <!-- Список бронирований -->
    <div v-else-if="filteredBookings.length > 0" class="bookings-list">
      <BookingCard
        v-for="b in filteredBookings"
        :key="b.idBooking"
        :booking="b"
        :category-name="getCategoryName(b.idCarNavigation?.idCategory || b.car?.idCategory)"
        @select="openBookingDetails"
        @cancelled="loadBookings"
      />
    </div>

    <!-- Пустое состояние -->
    <div v-else class="empty-state">
      <svg class="empty-icon-svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#9CA3AF" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
        <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
        <line x1="16" y1="2" x2="16" y2="6"/>
        <line x1="8" y1="2" x2="8" y2="6"/>
        <line x1="3" y1="10" x2="21" y2="10"/>
      </svg>
      <div class="empty-title">Бронирований не найдено</div>
      <div class="empty-desc">
        {{ selectedStatus === 'Все'
          ? 'Вы еще не оформляли запись на тест-драйв. Выберите автомобиль в каталоге!'
          : `В статусе "${selectedStatus}" записей нет.`
        }}
      </div>
      <button class="btn-go-catalog" @click="goToCatalog">
        Перейти в каталог
      </button>
    </div>

    <!-- Модалка с деталями бронирования и информацией об авто -->
    <BookingDetailsModal
      v-if="selectedBooking"
      :booking="selectedBooking"
      :category-name="getCategoryName(selectedBooking.idCarNavigation?.idCategory || selectedBooking.car?.idCategory)"
      @close="selectedBooking = null"
      @cancelled="loadBookings"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { apiClient } from '../api';
import { authState } from '../auth';
import { showToast } from '../notifications';
import BookingCard from '../components/BookingCard.vue';
import BookingDetailsModal from '../components/BookingDetailsModal.vue';

const router = useRouter();

const allBookings = ref([]);
const categories = ref([]);
const selectedStatus = ref('Все');
const loading = ref(true);
const selectedBooking = ref(null);

onMounted(async () => {
  await loadBookings();
});

async function loadBookings() {
  loading.value = true;
  try {
    const [bookingsRes, catsRes, carsRes] = await Promise.all([
      apiClient.getBookings().catch(() => []),
      apiClient.getCategories().catch(() => []),
      apiClient.getCars().catch(() => []),
    ]);

    categories.value = Array.isArray(catsRes) ? catsRes : [];
    const carsList = Array.isArray(carsRes) ? carsRes : [];
    const carsMap = new Map();
    for (const car of carsList) {
      if (car.idCar != null) {
        carsMap.set(Number(car.idCar), car);
      }
    }

    const currentId = authState.userId;
    const rawList = Array.isArray(bookingsRes) ? bookingsRes : [];

    // Фильтруем бронирования только текущего пользователя и добавляем полную информацию об авто
    allBookings.value = rawList
      .filter(b => Number(b.idUser) === Number(currentId))
      .map(b => {
        const carId = Number(b.idCar || b.idCarNavigation?.idCar);
        const fullCar = carsMap.get(carId) || {};
        const mergedCar = {
          ...(b.idCarNavigation || {}),
          ...fullCar,
        };
        return {
          ...b,
          car: mergedCar,
          idCarNavigation: mergedCar,
        };
      })
      .sort((a, b) => {
        const dateA = new Date(`${a.bookingDate}T${a.bookingTime || '00:00:00'}`);
        const dateB = new Date(`${b.bookingDate}T${b.bookingTime || '00:00:00'}`);
        return dateB - dateA;
      });
  } catch (err) {
    console.error('Ошибка загрузки бронирований:', err);
    showToast('Не удалось загрузить бронирования', 'error');
  } finally {
    loading.value = false;
  }
}

function getCategoryName(catId) {
  if (!catId) return '';
  const c = categories.value.find(item => item.idCategory === catId);
  return c ? c.nameCategory : '';
}

function openBookingDetails(booking) {
  selectedBooking.value = booking;
}

function goToCatalog() {
  router.push('/app/catalog');
}

const filteredBookings = computed(() => {
  if (selectedStatus.value === 'Все') {
    return allBookings.value;
  }
  if (selectedStatus.value === 'В рассмотрении') {
    return allBookings.value.filter(b => b.idStatus === 1);
  }
  if (selectedStatus.value === 'Подтверждено') {
    return allBookings.value.filter(b => b.idStatus === 2);
  }
  if (selectedStatus.value === 'Отменено') {
    return allBookings.value.filter(b => b.idStatus === 3);
  }
  if (selectedStatus.value === 'Завершено') {
    return allBookings.value.filter(b => b.idStatus === 4);
  }
  return allBookings.value;
});
</script>

<style scoped>
.bookings-page {
  padding: 24px 32px;
  max-width: 1000px;
  margin: 0 auto;
  box-sizing: border-box;
}

.header-section {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  margin-bottom: 20px;
}

.page-title {
  font-size: 28px;
  font-weight: 700;
  color: #111827;
  margin: 0;
}

.bookings-counter {
  font-size: 14px;
  color: #6B7280;
  font-weight: 500;
}

.filter-wrapper {
  margin-bottom: 22px;
}

.filter-tabs {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.status-tab-btn {
  padding: 8px 16px;
  border-radius: 8px;
  border: 1px solid #D1D5DB;
  background-color: #ffffff;
  color: #374151;
  font-size: 13.5px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.status-tab-btn:hover {
  background-color: #F3F4F6;
  border-color: #9CA3AF;
}

.status-tab-btn.active {
  background-color: #1E3A8A;
  color: #ffffff;
  border-color: #1E3A8A;
}

.bookings-list {
  display: flex;
  flex-direction: column;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  background-color: #ffffff;
  border-radius: 12px;
  border: 1px dashed #D1D5DB;
  text-align: center;
}

.empty-icon-svg {
  margin-bottom: 12px;
}

.empty-title {
  font-size: 18px;
  font-weight: 600;
  color: #111827;
  margin-bottom: 6px;
}

.empty-desc {
  font-size: 14px;
  color: #6B7280;
  max-width: 420px;
  margin-bottom: 20px;
  line-height: 1.4;
}

.btn-go-catalog {
  padding: 9px 18px;
  background-color: #1E3A8A;
  color: #ffffff;
  font-size: 14px;
  font-weight: 600;
  border-radius: 8px;
  border: none;
  cursor: pointer;
  transition: background-color 0.15s ease;
}

.btn-go-catalog:hover {
  background-color: #172554;
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  font-size: 15px;
  color: #6B7280;
  gap: 12px;
}

.spinner {
  width: 32px;
  height: 32px;
  border: 3px solid #E5E7EB;
  border-top-color: #1E3A8A;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 768px) {
  .bookings-page {
    padding: 16px;
  }
}
</style>
