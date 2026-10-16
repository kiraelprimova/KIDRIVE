<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="details-window">
      <div class="top-row">
        <button class="back-btn" @click="$emit('close')">
          <span class="arrow-back">←</span>
          <span>Назад</span>
        </button>
        <h2 class="window-title">Информация об автомобиле</h2>
      </div>

      <div class="content-grid">
        <!-- Левая колонка: картинка -->
        <div class="image-box">
          <img
            :src="carImageSrc"
            :alt="`${categoryName} ${car.model}`"
            class="car-detail-img"
            @error="onImgError"
          />
        </div>

        <!-- Правая колонка: детали -->
        <div class="info-column">
          <span class="sub-label">Автомобиль</span>
          <h1 class="car-headline">{{ categoryName }} {{ car.model }}</h1>

          <div class="detail-card">
            <span class="field-title">Год выпуска</span>
            <div class="field-value-year">{{ car.yearCar }}</div>
          </div>

          <div class="detail-card desc-card">
            <span class="field-title">Описание</span>
            <p class="field-value-desc">{{ car.description }}</p>
          </div>

          <div class="booking-row" v-if="car.price">
            <div class="price-box">
              <span class="price-val">{{ car.price }} ₽</span>
              <span class="price-unit">за час</span>
            </div>
            <button class="book-now-btn" @click="$emit('book', car)">
              Забронировать
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  car: {
    type: Object,
    required: true,
  },
  categoryName: {
    type: String,
    default: '',
  },
});

defineEmits(['close', 'book']);

const carImageSrc = computed(() => {
  if (!props.car.imagePath) return '/Resources/заглушка.png';
  return `/Resources/${props.car.imagePath}`;
});

function onImgError(e) {
  e.target.src = '/Resources/заглушка.png';
}
</script>

<style scoped>
.details-window {
  width: 1030px;
  max-width: 95vw;
  max-height: 90vh;
  overflow-y: auto;
  background-color: #ffffff;
  border-radius: 20px;
  padding: 28px;
  box-shadow: 0 12px 28px rgba(0, 0, 0, 0.12);
  display: flex;
  flex-direction: column;
}

.top-row {
  display: flex;
  align-items: center;
  margin-bottom: 25px;
}

.back-btn {
  width: 110px;
  height: 42px;
  background-color: #1E3A8A;
  color: #ffffff;
  font-size: 14px;
  border-radius: 10px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.arrow-back {
  font-size: 16px;
}

.window-title {
  font-size: 24px;
  font-weight: bold;
  color: #111827;
  margin-left: 20px;
}

.content-grid {
  display: grid;
  grid-template-columns: 1.2fr 1fr;
  gap: 35px;
  align-items: start;
}

.image-box {
  border-radius: 18px;
  background-color: #ffffff;
  border: 1px solid #E5E7EB;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 25px;
  min-height: 380px;
}

.car-detail-img {
  max-width: 100%;
  max-height: 400px;
  object-fit: contain;
}

.info-column {
  display: flex;
  flex-direction: column;
}

.sub-label {
  font-size: 13px;
  color: #6B7280;
}

.car-headline {
  font-size: 34px;
  font-weight: bold;
  color: #111827;
  margin-top: 5px;
  margin-bottom: 20px;
  line-height: 1.2;
}

.detail-card {
  background-color: #F9FAFB;
  border: 1px solid #E5E7EB;
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 18px;
  display: flex;
  flex-direction: column;
}

.field-title {
  font-size: 12px;
  color: #6B7280;
  font-weight: 600;
}

.field-value-year {
  font-size: 22px;
  font-weight: 600;
  color: #1E3A8A;
  margin-top: 4px;
}

.desc-card {
  padding: 18px;
}

.field-value-desc {
  font-size: 15px;
  color: #374151;
  margin-top: 10px;
  line-height: 26px;
}

.booking-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 10px;
  padding: 15px 18px;
  background: #EFF6FF;
  border-radius: 12px;
}

.price-box {
  display: flex;
  align-items: baseline;
  gap: 6px;
}

.price-val {
  font-size: 24px;
  font-weight: bold;
  color: #1E3A8A;
}

.price-unit {
  font-size: 13px;
  color: #6B7280;
}

.book-now-btn {
  height: 42px;
  padding: 0 24px;
  background-color: #1E3A8A;
  color: #ffffff;
  font-size: 14px;
  font-weight: 600;
  border-radius: 8px;
  border: none;
  cursor: pointer;
}

@media (max-width: 850px) {
  .content-grid {
    grid-template-columns: 1fr;
  }
}
</style>
