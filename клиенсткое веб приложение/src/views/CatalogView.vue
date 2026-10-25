<template>
  <div class="catalog-page">
    <div class="header-section">
      <h1 class="page-title">Каталог автомобилей</h1>
      <span class="cars-counter" v-if="!loading">Найдено: {{ filteredCars.length }}</span>
    </div>

    <!-- Блок фильтрации и поиска -->
    <div class="filter-card">
      <div class="search-bar">
        <svg class="search-icon" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="11" cy="11" r="8"/>
          <line x1="21" y1="21" x2="16.65" y2="16.65"/>
        </svg>
        <input
          type="text"
          v-model="searchQuery"
          placeholder="Поиск по марке и модели..."
          class="search-input"
        />
        <button v-if="searchQuery" class="clear-search-btn" @click="searchQuery = ''">✕</button>
      </div>

      <div class="filters-row">
        <div class="filter-col">
          <label class="filter-label">Марка</label>
          <select v-model="selectedBrand" class="filter-select">
            <option value="">Все марки</option>
            <option
              v-for="cat in categories"
              :key="cat.idCategory"
              :value="cat.nameCategory"
            >
              {{ cat.nameCategory }}
            </option>
          </select>
        </div>

        <div class="filter-col">
          <label class="filter-label">Год выпуска</label>
          <select v-model="selectedYear" class="filter-select">
            <option value="">Все года</option>
            <option v-for="y in availableYears" :key="y" :value="y">
              {{ y }}
            </option>
          </select>
        </div>

        <div class="filter-col">
          <label class="filter-label">Коробка передач</label>
          <select v-model="selectedTransmission" class="filter-select">
            <option value="">Все коробки</option>
            <option
              v-for="t in transmissions"
              :key="getTransId(t)"
              :value="String(getTransId(t))"
            >
              {{ t.name }}
            </option>
          </select>
        </div>

        <div class="filter-col">
          <label class="filter-label">Тип топлива</label>
          <select v-model="selectedFuelType" class="filter-select">
            <option value="">Все виды</option>
            <option
              v-for="f in fuelTypes"
              :key="getFuelId(f)"
              :value="String(getFuelId(f))"
            >
              {{ f.name }}
            </option>
          </select>
        </div>

        <div class="filter-col">
          <label class="filter-label">Сортировка</label>
          <select v-model="selectedSort" class="filter-select">
            <option value="all">По умолчанию</option>
            <option value="asc">По марке (А-Я)</option>
            <option value="desc">По марке (Я-А)</option>
            <option value="price_asc">По цене (по возрастанию)</option>
            <option value="price_desc">По цене (по убыванию)</option>
          </select>
        </div>
      </div>

      <!-- Кнопка сброса если применены фильтры -->
      <div v-if="hasActiveFilters" class="reset-row">
        <button class="btn-reset-filters" @click="resetFilters">
          Сбросить все фильтры
        </button>
      </div>
    </div>

    <!-- Загрузка -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <div>Загрузка автомобилей...</div>
    </div>

    <!-- Список карточек -->
    <div v-else-if="filteredCars.length > 0" class="cars-grid">
      <CarCard
        v-for="car in filteredCars"
        :key="car.idCar"
        :car="car"
        :category-name="getCategoryName(car.idCategory)"
        :transmission-name="getTransmissionName(getCarTransId(car))"
        :fuel-type-name="getFuelTypeName(getCarFuelId(car))"
        :is-favorite="favoriteCarIds.has(car.idCar)"
        @details="openDetails"
        @book="openBooking"
        @toggle-favorite="toggleFavorite"
      />
    </div>

    <!-- Пустое состояние -->
    <div v-else class="empty-state">
      <svg class="empty-icon-svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#9CA3AF" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
        <path d="M19 17h2c.6 0 1-.4 1-1v-3c0-.9-.7-1.7-1.5-1.9C18.7 10.6 16 10 16 10s-1.3-1.4-2.2-2.3c-.5-.4-1.1-.7-1.8-.7H5c-.6 0-1.1.4-1.4.9l-1.4 2.9C2.1 11.1 2 11.5 2 12v4c0 .6.4 1 1 1h2"/>
        <circle cx="7" cy="17" r="2"/>
        <circle cx="17" cy="17" r="2"/>
      </svg>
      <div class="empty-text">По вашему запросу ничего не найдено</div>
      <button class="btn-reset-filters" @click="resetFilters">Сбросить фильтры</button>
    </div>

    <!-- Модальное окно Подробнее -->
    <CarDetailsModal
      v-if="activeDetailsCar"
      :car="activeDetailsCar"
      :category-name="getCategoryName(activeDetailsCar.idCategory)"
      :transmission-name="getTransmissionName(getCarTransId(activeDetailsCar))"
      :fuel-type-name="getFuelTypeName(getCarFuelId(activeDetailsCar))"
      :is-favorite="favoriteCarIds.has(activeDetailsCar.idCar)"
      @close="activeDetailsCar = null"
      @book="openBookingFromDetails"
      @toggle-favorite="toggleFavorite"
    />

    <!-- Модальное окно Бронирования -->
    <BookingModal
      v-if="activeBookingCar"
      :car="activeBookingCar"
      :category-name="getCategoryName(activeBookingCar.idCategory)"
      @close="activeBookingCar = null"
      @success="onBookingSuccess"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { apiClient } from '../api';
import { authState } from '../auth';
import { showToast } from '../notifications';
import CarCard from '../components/CarCard.vue';
import CarDetailsModal from '../components/CarDetailsModal.vue';
import BookingModal from '../components/BookingModal.vue';

const cars = ref([]);
const categories = ref([]);
const transmissions = ref([]);
const fuelTypes = ref([]);
const favoriteCarIds = ref(new Set());
const loading = ref(true);

const searchQuery = ref('');
const selectedBrand = ref('');
const selectedYear = ref('');
const selectedTransmission = ref('');
const selectedFuelType = ref('');
const selectedSort = ref('all');

const activeDetailsCar = ref(null);
const activeBookingCar = ref(null);

onMounted(async () => {
  await loadData();
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

async function loadData() {
  loading.value = true;
  try {
    const promises = [
      apiClient.getCategories().catch(() => []),
      apiClient.getCars().catch(() => []),
      apiClient.getTransmissions().catch(() => []),
      apiClient.getFuelTypes().catch(() => []),
    ];

    if (!authState.isGuest && authState.userId) {
      promises.push(apiClient.getFavorites(authState.userId).catch(() => []));
    }

    const [catsRes, carsRes, transRes, fuelRes, favsRes] = await Promise.all(promises);

    categories.value = Array.isArray(catsRes) ? catsRes : [];
    cars.value = Array.isArray(carsRes) ? carsRes : [];
    transmissions.value = Array.isArray(transRes) ? transRes : [];
    fuelTypes.value = Array.isArray(fuelRes) ? fuelRes : [];

    if (favsRes && Array.isArray(favsRes)) {
      favoriteCarIds.value = new Set(favsRes.map(f => f.idCar || f.car?.idCar));
    }
  } catch (err) {
    console.error('Ошибка загрузки данных каталога:', err);
    showToast('Не удалось загрузить автомобили', 'error');
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

async function toggleFavorite(car) {
  if (authState.isGuest || !authState.userId) return;

  const carId = car.idCar;
  const isCurrentlyFav = favoriteCarIds.value.has(carId);

  try {
    if (isCurrentlyFav) {
      await apiClient.removeFavorite(authState.userId, carId);
      favoriteCarIds.value.delete(carId);
      favoriteCarIds.value = new Set(favoriteCarIds.value);
      showToast('Автомобиль удален из избранного', 'info');
    } else {
      await apiClient.addFavorite(authState.userId, carId);
      favoriteCarIds.value.add(carId);
      favoriteCarIds.value = new Set(favoriteCarIds.value);
      showToast('Автомобиль добавлен в избранное', 'success');
    }
  } catch (err) {
    console.error('Ошибка обновления избранного:', err);
    showToast('Ошибка при обновлении избранного', 'error');
  }
}

const availableYears = computed(() => {
  const years = cars.value
    .map(c => c.yearCar)
    .filter(y => y != null);
  return [...new Set(years)].sort((a, b) => b - a);
});

const hasActiveFilters = computed(() => {
  return (
    searchQuery.value !== '' ||
    selectedBrand.value !== '' ||
    selectedYear.value !== '' ||
    selectedTransmission.value !== '' ||
    selectedFuelType.value !== '' ||
    selectedSort.value !== 'all'
  );
});

function resetFilters() {
  searchQuery.value = '';
  selectedBrand.value = '';
  selectedYear.value = '';
  selectedTransmission.value = '';
  selectedFuelType.value = '';
  selectedSort.value = 'all';
}

const filteredCars = computed(() => {
  let list = [...cars.value];

  // Поиск по названию марки и модели
  if (searchQuery.value && searchQuery.value.trim() !== '') {
    const q = searchQuery.value.toLowerCase().trim();
    list = list.filter(c => {
      const brand = getCategoryName(c.idCategory).toLowerCase();
      const model = String(c.model || '').toLowerCase();
      const full = `${brand} ${model}`;
      return full.includes(q);
    });
  }

  // Фильтр по марке
  if (selectedBrand.value) {
    list = list.filter(c => getCategoryName(c.idCategory) === selectedBrand.value);
  }

  // Фильтр по году
  if (selectedYear.value) {
    list = list.filter(c => Number(c.yearCar) === Number(selectedYear.value));
  }

  // Фильтр по КПП (работает вместе со всеми фильтрами)
  if (selectedTransmission.value !== '') {
    list = list.filter(c => {
      const carTrans = getCarTransId(c);
      return String(carTrans) === String(selectedTransmission.value);
    });
  }

  // Фильтр по типу топлива (работает вместе со всеми фильтрами)
  if (selectedFuelType.value !== '') {
    list = list.filter(c => {
      const carFuel = getCarFuelId(c);
      return String(carFuel) === String(selectedFuelType.value);
    });
  }

  // Сортировка
  if (selectedSort.value === 'price_asc') {
    list.sort((a, b) => Number(a.price || 0) - Number(b.price || 0));
  } else if (selectedSort.value === 'price_desc') {
    list.sort((a, b) => Number(b.price || 0) - Number(a.price || 0));
  } else if (selectedSort.value === 'asc') {
    list.sort((a, b) => {
      const nameA = `${getCategoryName(a.idCategory)} ${a.model || ''}`;
      const nameB = `${getCategoryName(b.idCategory)} ${b.model || ''}`;
      return nameA.localeCompare(nameB);
    });
  } else if (selectedSort.value === 'desc') {
    list.sort((a, b) => {
      const nameA = `${getCategoryName(a.idCategory)} ${a.model || ''}`;
      const nameB = `${getCategoryName(b.idCategory)} ${b.model || ''}`;
      return nameB.localeCompare(nameA);
    });
  }

  return list;
});

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
  showToast('Заявка на бронирование успешно отправлена!', 'success');
}
</script>

<style scoped>
.catalog-page {
  padding: 24px 32px;
  max-width: 1400px;
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

.cars-counter {
  font-size: 14px;
  color: #6B7280;
  font-weight: 500;
}

.filter-card {
  background-color: #ffffff;
  border: 1px solid #E5E7EB;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 24px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
}

.search-bar {
  background-color: #F9FAFB;
  border: 1.5px solid #1E3A8A;
  border-radius: 10px;
  height: 44px;
  display: flex;
  align-items: center;
  padding: 0 14px;
}

.search-icon {
  font-size: 15px;
  color: #1E3A8A;
  margin-right: 10px;
}

.search-input {
  border: none;
  background: transparent;
  font-size: 14px;
  color: #111827;
  width: 100%;
  outline: none;
}

.clear-search-btn {
  background: none;
  border: none;
  color: #9CA3AF;
  cursor: pointer;
  font-size: 14px;
  padding: 4px;
}

.clear-search-btn:hover {
  color: #111827;
}

.filters-row {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 16px;
  margin-top: 16px;
}

.filter-col {
  display: flex;
  flex-direction: column;
}

.filter-label {
  font-size: 12.5px;
  font-weight: 500;
  color: #374151;
  margin-bottom: 6px;
}

.filter-select {
  height: 36px;
  background-color: #F9FAFB;
  border: 1px solid #D1D5DB;
  border-radius: 8px;
  font-size: 13.5px;
  padding: 0 10px;
  color: #111827;
  outline: none;
  transition: border-color 0.15s ease;
}

.filter-select:focus {
  border-color: #1E3A8A;
}

.reset-row {
  margin-top: 14px;
  display: flex;
  justify-content: flex-end;
}

.btn-reset-filters {
  background: none;
  border: none;
  color: #1E3A8A;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 6px;
  transition: background-color 0.15s ease;
}

.btn-reset-filters:hover {
  background-color: #EFF6FF;
  text-decoration: underline;
}

/* Красивая сетка без пустого пространства справа */
.cars-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(290px, 1fr));
  gap: 24px;
  width: 100%;
  margin-top: 10px;
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
}

.empty-icon-svg {
  margin-bottom: 12px;
}

.empty-text {
  font-size: 17px;
  font-weight: 600;
  color: #6B7280;
  margin-bottom: 16px;
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

@media (max-width: 1100px) {
  .filters-row {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (max-width: 768px) {
  .catalog-page {
    padding: 16px;
  }
  .filters-row {
    grid-template-columns: 1fr;
  }
}
</style>
