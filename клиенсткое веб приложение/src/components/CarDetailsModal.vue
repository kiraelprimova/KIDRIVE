<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="details-window">
      <div class="top-row">
        <button class="back-btn" @click="$emit('close')">
          <span class="arrow-back">←</span>
          <span>Назад</span>
        </button>
        <h2 class="window-title">Информация об автомобиле</h2>

        <button
          class="btn-fav-modal"
          :class="{ active: isFavorite }"
          @click="handleToggleFavorite"
        >
          <svg width="15" height="15" viewBox="0 0 24 24" :fill="isFavorite ? '#EF4444' : 'none'" :stroke="isFavorite ? '#EF4444' : 'currentColor'" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/>
          </svg>
          <span>{{ isFavorite ? 'В избранном' : 'В избранное' }}</span>
        </button>
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

          <div class="specs-row">
            <div class="detail-card">
              <span class="field-title">Год выпуска</span>
              <div class="field-value-spec">{{ car.yearCar }}</div>
            </div>

            <div class="detail-card" v-if="car.powerHp">
              <span class="field-title">Мощность</span>
              <div class="field-value-spec">{{ car.powerHp }} л.с.</div>
            </div>

            <div class="detail-card" v-if="transmissionName">
              <span class="field-title">КПП</span>
              <div class="field-value-spec">{{ transmissionName }}</div>
            </div>

            <div class="detail-card" v-if="fuelTypeName">
              <span class="field-title">Топливо</span>
              <div class="field-value-spec">{{ fuelTypeName }}</div>
            </div>
          </div>

          <div class="detail-card desc-card">
            <span class="field-title">Описание</span>
            <p class="field-value-desc">{{ car.description || 'Описание отсутствует' }}</p>
          </div>

          <div class="booking-row" v-if="car.price">
            <div class="price-box">
              <span class="price-val">{{ car.price }} ₽</span>
              <span class="price-unit">за час</span>
            </div>
            <button class="book-now-btn" @click="handleBook">
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
import { useRouter } from 'vue-router';
import { authState } from '../auth';
import { showConfirm } from '../notifications';

const props = defineProps({
  car: {
    type: Object,
    required: true,
  },
  categoryName: {
    type: String,
    default: '',
  },
  transmissionName: {
    type: String,
    default: '',
  },
  fuelTypeName: {
    type: String,
    default: '',
  },
  isFavorite: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['close', 'book', 'toggle-favorite']);
const router = useRouter();

const carImageSrc = computed(() => {
  if (!props.car.imagePath) return '/Resources/заглушка.png';
  return `/Resources/${props.car.imagePath}`;
});

function onImgError(e) {
  e.target.src = '/Resources/заглушка.png';
}

async function handleToggleFavorite() {
  if (authState.isGuest) {
    const wantLogin = await showConfirm(
      'Чтобы добавить автомобиль в избранное, вам нужно авторизоваться.\n\nХотите войти в систему?',
      'Требуется авторизация'
    );
    if (wantLogin) {
      router.push('/login');
    }
    return;
  }
  emit('toggle-favorite', props.car);
}

async function handleBook() {
  if (authState.isGuest) {
    const wantLogin = await showConfirm(
      'Чтобы забронировать автомобиль вам нужно авторизоваться.\n\nХотите войти в систему?',
      'Требуется авторизация'
    );
    if (wantLogin) {
      router.push('/login');
    }
    return;
  }
  emit('book', props.car);
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
  margin-right: auto;
}

.btn-fav-modal {
  padding: 8px 16px;
  border: 1px solid #D1D5DB;
  border-radius: 8px;
  background: #ffffff;
  color: #4B5563;
  font-size: 13px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
}

.btn-fav-modal.active {
  color: #EF4444;
  border-color: #EF4444;
  background: #FEF2F2;
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
  font-size: 32px;
  font-weight: bold;
  color: #111827;
  margin-top: 5px;
  margin-bottom: 18px;
  line-height: 1.2;
}

.specs-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(100px, 1fr));
  gap: 10px;
  margin-bottom: 16px;
}

.detail-card {
  background-color: #F9FAFB;
  border: 1px solid #E5E7EB;
  border-radius: 10px;
  padding: 12px;
  display: flex;
  flex-direction: column;
}

.field-title {
  font-size: 11px;
  color: #6B7280;
  font-weight: 600;
}

.field-value-spec {
  font-size: 17px;
  font-weight: 600;
  color: #1E3A8A;
  margin-top: 4px;
}

.desc-card {
  padding: 16px;
  margin-bottom: 18px;
}

.field-value-desc {
  font-size: 14px;
  color: #374151;
  margin-top: 8px;
  line-height: 22px;
}

.booking-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 6px;
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
