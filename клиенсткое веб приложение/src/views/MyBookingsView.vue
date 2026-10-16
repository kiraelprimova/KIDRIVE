<template>
  <div class="bookings-page">
    <h1 class="page-title">Мои бронирования</h1>

    <div class="filter-wrapper">
      <select v-model="selectedStatus" class="status-select">
        <option value="Все">Все</option>
        <option value="В рассмотрении">В рассмотрении</option>
        <option value="Подтверждено">Подтверждено</option>
        <option value="Отменено">Отменено</option>
      </select>
    </div>

    <!-- Загрузка -->
    <div v-if="loading" class="loading-state">
      Загрузка бронирований...
    </div>

    <!-- Список бронирований -->
    <div v-else-if="filteredBookings.length > 0" class="bookings-list">
      <BookingCard
        v-for="b in filteredBookings"
        :key="b.idBooking"
        :booking="b"
        :category-name="getCategoryName(b.idCarNavigation?.idCategory)"
        @cancelled="loadBookings"
      />
    </div>

    <!-- Пустое состояние -->
    <div v-else class="empty-state">
      Здесь пока пусто, перейдите в каталог, чтобы забронировать автомобиль
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { apiClient } from '../api';
import { authState } from '../auth';
import BookingCard from '../components/BookingCard.vue';

const allBookings = ref([]);
const categories = ref([]);
const selectedStatus = ref('Все');
const loading = ref(true);

onMounted(async () => {
  await loadBookings();
});

async function loadBookings() {
  loading.value = true;
  try {
    const [bookingsRes, catsRes] = await Promise.all([
      apiClient.getBookings(),
      apiClient.getCategories(),
    ]);

    categories.value = Array.isArray(catsRes) ? catsRes : [];

    const currentId = authState.userId;
    const rawList = Array.isArray(bookingsRes) ? bookingsRes : [];

    // Фильтруем бронирования только текущего пользователя и сортируем по убыванию даты и времени
    allBookings.value = rawList
      .filter(b => Number(b.idUser) === Number(currentId))
      .sort((a, b) => {
        const dateA = new Date(`${a.bookingDate}T${a.bookingTime || '00:00:00'}`);
        const dateB = new Date(`${b.bookingDate}T${b.bookingTime || '00:00:00'}`);
        return dateB - dateA;
      });
  } catch (err) {
    console.error('Ошибка загрузки бронирований:', err);
  } finally {
    loading.value = false;
  }
}

function getCategoryName(catId) {
  if (!catId) return '';
  const c = categories.value.find(item => item.idCategory === catId);
  return c ? c.nameCategory : '';
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
  return allBookings.value;
});
</script>

<style scoped>
.bookings-page {
  padding: 20px;
}

.page-title {
  font-size: 30px;
  font-weight: bold;
  color: #111827;
  margin-bottom: 20px;
}

.filter-wrapper {
  margin-bottom: 20px;
}

.status-select {
  width: 220px;
  height: 38px;
  padding: 0 10px;
  border: 1px solid #D1D5DB;
  border-radius: 6px;
  background-color: #ffffff;
  font-size: 14px;
  color: #111827;
}

.bookings-list {
  display: flex;
  flex-direction: column;
}

.empty-state {
  font-size: 22px;
  font-weight: 600;
  color: #9CA3AF;
  text-align: center;
  max-width: 500px;
  margin: 80px auto 0 auto;
  line-height: 1.4;
}

.loading-state {
  font-size: 16px;
  color: #6B7280;
  text-align: center;
  margin-top: 40px;
}
</style>
