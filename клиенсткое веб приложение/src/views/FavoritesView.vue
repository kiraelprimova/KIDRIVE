<template>
  <div class="favorites-page">
    <div class="header-row">
      <h1 class="page-title">Избранные автомобили</h1>
      <span class="cars-counter" v-if="!loading && favorites.length > 0">В избранном: {{ favorites.length }}</span>
    </div>

    <!-- Загрузка -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <div>Загрузка избранного...</div>
    </div>

    <!-- Список избранных карточек -->
    <div v-else-if="favorites.length > 0" class="cars-grid">
      <CarCard
        v-for="fav in favorites"
        :key="fav.idFavorite || getCar(fav).idCar"
        :car="getCar(fav)"
        :category-name="getCar(fav).categoryName || getCategoryName(getCar(fav).idCategory)"
        :transmission-name="getCar(fav).transmissionName || getTransmissionName(getCarTransId(getCar(fav)))"
        :fuel-type-name="getCar(fav).fuelTypeName || getFuelTypeName(getCarFuelId(getCar(fav)))"
        :is-favorite="true"
        @details="openDetails"
        @book="openBooking"
        @toggle-favorite="removeFavorite"
      />
    </div>

    <!-- Пустое состояние -->
    <div v-else class="empty-state">
      <svg class="empty-icon-svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#9CA3AF" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
        <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/>
      </svg>
      <div class="empty-title">В избранном пока ничего нет</div>
      <div class="empty-desc">Вы можете добавлять понравившиеся автомобили из каталога.</div>
      <button class="btn-go-catalog" @click="goToCatalog">
        Перейти в каталог
      </button>
    </div>

    <!-- Модальное окно Подробнее -->
    <CarDetailsModal
      v-if="activeDetailsCar"
      :car="activeDetailsCar"
      :category-name="activeDetailsCar.categoryName || getCategoryName(activeDetailsCar.idCategory)"
      :transmission-name="activeDetailsCar.transmissionName || getTransmissionName(getCarTransId(activeDetailsCar))"
      :fuel-type-name="activeDetailsCar.fuelTypeName || getFuelTypeName(getCarFuelId(activeDetailsCar))"
      :is-favorite="true"
      @close="activeDetailsCar = null"
      @book="openBookingFromDetails"
      @toggle-favorite="removeFavorite"
    />

    <!-- Модальное окно Бронирования -->
    <BookingModal
      v-if="activeBookingCar"
      :car="activeBookingCar"
      :category-name="activeBookingCar.categoryName || getCategoryName(activeBookingCar.idCategory)"
      @close="activeBookingCar = null"
      @success="onBookingSuccess"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { apiClient } from '../api';
import { authState } from '../auth';
import { showToast } from '../notifications';
import CarCard from '../components/CarCard.vue';
import CarDetailsModal from '../components/CarDetailsModal.vue';
import BookingModal from '../components/BookingModal.vue';

const router = useRouter();
const favorites = ref([]);
const categories = ref([]);
const transmissions = ref([]);
const fuelTypes = ref([]);
const loading = ref(true);

const activeDetailsCar = ref(null);
const activeBookingCar = ref(null);

onMounted(async () => {
  await loadFavorites();
});

function getTransId(t) {
  if (!t) return '';
  return t.id ?? t.Id ?? t.idTreansmission ?? t.idTransmission ?? '';
}

function getFuelId(f) {
  if (!f) return '';
  return f.id ?? f.Id ?? f.idFuelType ?? f.idFuel ?? '';
}

function getCarTransId(c) {
  if (!c) return '';
  return c.idTreansmission ?? c.idTransmission ?? c.IdTreansmission ?? c.IdTransmission ?? '';
}

function getCarFuelId(c) {
  if (!c) return '';
  return c.idFuelType ?? c.IdFuelType ?? c.idFuel ?? c.IdFuel ?? '';
}

async function loadFavorites() {
  loading.value = true;
  try {
    const [catsRes, transRes, fuelRes, favsRes] = await Promise.all([
      apiClient.getCategories().catch(() => []),
      apiClient.getTransmissions().catch(() => []),
      apiClient.getFuelTypes().catch(() => []),
      apiClient.getFavorites(authState.userId).catch(() => []),
    ]);

    categories.value = Array.isArray(catsRes) ? catsRes : [];
    transmissions.value = Array.isArray(transRes) ? transRes : [];
    fuelTypes.value = Array.isArray(fuelRes) ? fuelRes : [];
    favorites.value = Array.isArray(favsRes) ? favsRes : [];
  } catch (err) {
    console.error('Ошибка загрузки избранного:', err);
    showToast('Не удалось загрузить избранное', 'error');
  } finally {
    loading.value = false;
  }
}

function getCategoryName(catId) {
  const c = categories.value.find(item => item.idCategory === catId);
  return c ? c.nameCategory : '';
}

function getTransmissionName(transId) {
  if (!transId) return '';
  const t = transmissions.value.find(item => String(getTransId(item)) === String(transId));
  return t ? t.name : '';
}

function getFuelTypeName(fuelId) {
  if (!fuelId) return '';
  const f = fuelTypes.value.find(item => String(getFuelId(item)) === String(fuelId));
  return f ? f.name : '';
}

function getCar(fav) {
  if (!fav) return {};
  return fav.car || fav.Car || fav;
}

async function removeFavorite(car) {
  const carId = car.idCar || car.IdCar;
  try {
    await apiClient.removeFavorite(authState.userId, carId);
    favorites.value = favorites.value.filter(f => {
      const c = getCar(f);
      return (c.idCar || c.IdCar || f.idCar || f.IdCar) !== carId;
    });
    if (activeDetailsCar.value && (activeDetailsCar.value.idCar === carId || activeDetailsCar.value.IdCar === carId)) {
      activeDetailsCar.value = null;
    }
    showToast('Автомобиль удален из избранного', 'info');
  } catch (err) {
    console.error('Ошибка при удалении из избранного:', err);
    showToast('Ошибка при удалении из избранного', 'error');
  }
}

function openDetails(car) {
  activeDetailsCar.value = car;
}

function openBooking(car) {
  activeBookingCar.value = car;
}

function openBookingFromDetails(car) {
  activeDetailsCar.value = null;
  activeBookingCar.value = car;
}

function onBookingSuccess() {
  activeBookingCar.value = null;
  showToast('Заявка на бронирование успешно оформлена!', 'success');
}

function goToCatalog() {
  router.push('/app/catalog');
}
</script>

<style scoped>
.favorites-page {
  padding: 24px 32px;
  max-width: 1400px;
  margin: 0 auto;
  box-sizing: border-box;
}

.header-row {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  margin-bottom: 24px;
}

.page-title {
  font-size: 28px;
  font-weight: 700;
  color: #111827;
  margin: 0;
}

.cars-counter {
  font-size: 14px;
  color: #6B7280;
  font-weight: 500;
}

.cars-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(290px, 1fr));
  gap: 24px;
  width: 100%;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 20px;
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
  margin-bottom: 8px;
}

.empty-desc {
  font-size: 14px;
  color: #6B7280;
  max-width: 400px;
  margin-bottom: 24px;
  line-height: 1.4;
}

.btn-go-catalog {
  padding: 10px 20px;
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
  .favorites-page {
    padding: 16px;
  }
}
</style>
