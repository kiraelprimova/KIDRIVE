<template>
  <div class="catalog-page">
    <h1 class="page-title">Каталог автомобилей</h1>

    <!-- Блок фильтрации и поиска -->
    <div class="filter-card">
      <div class="search-bar">
        <span class="search-icon">🔍</span>
        <input
          type="text"
          v-model="searchQuery"
          placeholder="Поиск по марке и модели..."
          class="search-input"
        />
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
          <label class="filter-label">Сортировка</label>
          <select v-model="selectedSort" class="filter-select">
            <option value="all">Все</option>
            <option value="asc">А-Я</option>
            <option value="desc">Я-А</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Загрузка -->
    <div v-if="loading" class="loading-state">
      Загрузка автомобилей...
    </div>

    <!-- Список карточек -->
    <div v-else-if="filteredCars.length > 0" class="cars-grid">
      <CarCard
        v-for="car in filteredCars"
        :key="car.idCar"
        :car="car"
        :category-name="getCategoryName(car.idCategory)"
        @details="openDetails"
        @book="openBooking"
      />
    </div>

    <!-- Пустое состояние -->
    <div v-else class="empty-state">
      Здесь пока ничего нет
    </div>

    <!-- Модальное окно Подробнее -->
    <CarDetailsModal
      v-if="activeDetailsCar"
      :car="activeDetailsCar"
      :category-name="getCategoryName(activeDetailsCar.idCategory)"
      @close="activeDetailsCar = null"
      @book="openBookingFromDetails"
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
import CarCard from '../components/CarCard.vue';
import CarDetailsModal from '../components/CarDetailsModal.vue';
import BookingModal from '../components/BookingModal.vue';

const cars = ref([]);
const categories = ref([]);
const loading = ref(true);

const searchQuery = ref('');
const selectedBrand = ref('');
const selectedYear = ref('');
const selectedSort = ref('all');

const activeDetailsCar = ref(null);
const activeBookingCar = ref(null);

onMounted(async () => {
  await loadData();
});

async function loadData() {
  loading.value = true;
  try {
    const [catsRes, carsRes] = await Promise.all([
      apiClient.getCategories(),
      apiClient.getCars(),
    ]);
    categories.value = Array.isArray(catsRes) ? catsRes : [];
    cars.value = Array.isArray(carsRes) ? carsRes : [];
  } catch (err) {
    console.error('Ошибка загрузки данных:', err);
  } finally {
    loading.value = false;
  }
}

function getCategoryName(catId) {
  const c = categories.value.find(item => item.idCategory === catId);
  return c ? c.nameCategory : '';
}

const availableYears = computed(() => {
  const years = cars.value
    .map(c => c.yearCar)
    .filter(y => y != null);
  return [...new Set(years)].sort((a, b) => b - a);
});

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

  // Сортировка по марке
  if (selectedSort.value === 'asc') {
    list.sort((a, b) => {
      const nameA = getCategoryName(a.idCategory);
      const nameB = getCategoryName(b.idCategory);
      return nameA.localeCompare(nameB);
    });
  } else if (selectedSort.value === 'desc') {
    list.sort((a, b) => {
      const nameA = getCategoryName(a.idCategory);
      const nameB = getCategoryName(b.idCategory);
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
}
</script>

<style scoped>
.catalog-page {
  padding: 20px;
}

.page-title {
  font-size: 30px;
  font-weight: bold;
  color: #111827;
  margin-bottom: 20px;
}

.filter-card {
  background-color: #ffffff;
  border: 1px solid #E5E7EB;
  border-radius: 10px;
  padding: 18px;
  margin-bottom: 18px;
}

.search-bar {
  background-color: #F9FAFB;
  border: 1.5px solid #1E3A8A;
  border-radius: 12px;
  height: 46px;
  display: flex;
  align-items: center;
  padding: 0 15px;
}

.search-icon {
  font-size: 14px;
  color: #1E3A8A;
  margin-right: 10px;
}

.search-input {
  border: none;
  background: transparent;
  font-size: 14px;
  color: #111827;
  width: 100%;
}

.filters-row {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 20px;
  margin-top: 15px;
}

.filter-col {
  display: flex;
  flex-direction: column;
}

.filter-label {
  font-size: 12px;
  color: #111827;
  margin-bottom: 6px;
  margin-left: 2px;
}

.filter-select {
  height: 27px;
  background-color: #F3F4F6;
  border: 1px solid #E5E7EB;
  border-radius: 4px;
  font-size: 12px;
  padding: 0 6px;
  color: #111827;
}

.cars-grid {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 10px;
  margin-top: 10px;
}

.empty-state {
  font-size: 24px;
  font-weight: 600;
  color: #9CA3AF;
  text-align: center;
  margin-top: 60px;
}

.loading-state {
  font-size: 16px;
  color: #6B7280;
  text-align: center;
  margin-top: 40px;
}

@media (max-width: 768px) {
  .filters-row {
    grid-template-columns: 1fr;
    gap: 12px;
  }
}
</style>
