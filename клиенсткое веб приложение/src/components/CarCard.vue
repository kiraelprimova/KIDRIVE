<template>
  <div class="car-card">
    <div class="card-image-box">
      <img
        :src="carImageSrc"
        :alt="`${categoryName} ${car.model}`"
        class="card-img"
        @error="onImageError"
      />
      <button
        class="btn-fav"
        :class="{ active: isFavorite }"
        :title="isFavorite ? 'Удалить из избранного' : 'Добавить в избранное'"
        @click.stop="handleToggleFavorite"
      >
        <svg width="17" height="17" viewBox="0 0 24 24" :fill="isFavorite ? '#EF4444' : 'none'" :stroke="isFavorite ? '#EF4444' : '#6B7280'" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/>
        </svg>
      </button>
    </div>

    <div class="card-body">
      <h3 class="card-title">{{ categoryName }} {{ car.model }}</h3>
      <div class="card-specs">
        <span>{{ car.yearCar }} г.</span>
        <span v-if="car.powerHp">• {{ car.powerHp }} л.с.</span>
        <span v-if="transmissionName">• {{ transmissionName }}</span>
        <span v-if="fuelTypeName">• {{ fuelTypeName }}</span>
      </div>
      <div class="card-price">{{ Number(car.price || 0).toLocaleString('ru-RU') }} ₽ / час</div>
      <p class="card-desc" :title="car.description">{{ car.description || 'Описание отсутствует' }}</p>

      <div class="card-actions">
        <button class="btn-card btn-details" @click="$emit('details', car)">
          Подробнее
        </button>
        <button class="btn-card btn-book" @click="handleBook">
          Бронь
        </button>
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

const emit = defineEmits(['details', 'book', 'toggle-favorite']);
const router = useRouter();

const carImageSrc = computed(() => {
  if (!props.car.imagePath) return '/Resources/заглушка.png';
  return `/Resources/${props.car.imagePath}`;
});

function onImageError(e) {
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
.car-card {
  width: 100%;
  background-color: #ffffff;
  border-radius: 14px;
  box-shadow: 0 4px 14px rgba(0, 0, 0, 0.08);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-sizing: border-box;
  position: relative;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  border: 1px solid #E5E7EB;
}

.car-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
}

.card-image-box {
  height: 210px;
  width: 100%;
  background-color: #F3F4F6;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  position: relative;
}

.card-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.car-card:hover .card-img {
  transform: scale(1.04);
}

.btn-fav {
  position: absolute;
  top: 12px;
  right: 12px;
  width: 36px;
  height: 36px;
  background-color: rgba(255, 255, 255, 0.92);
  border: 1px solid #E5E7EB;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.12);
  transition: transform 0.15s ease, background-color 0.15s ease;
  z-index: 2;
}

.btn-fav:hover {
  transform: scale(1.12);
  background-color: #ffffff;
}

.fav-icon {
  font-size: 18px;
  color: #9CA3AF;
  line-height: 1;
}

.btn-fav.active .fav-icon {
  color: #EF4444;
}

.card-body {
  padding: 16px;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.card-title {
  font-size: 19px;
  font-weight: 700;
  color: #111827;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin: 0;
}

.card-specs {
  font-size: 13px;
  color: #6B7280;
  margin-top: 6px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.card-price {
  font-size: 17px;
  font-weight: 700;
  color: #1E3A8A;
  margin-top: 10px;
}

.card-desc {
  font-size: 13px;
  color: #4B5563;
  margin-top: 8px;
  margin-bottom: 14px;
  height: 38px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  line-height: 19px;
}

.card-actions {
  display: flex;
  align-items: center;
  margin-top: auto;
  gap: 10px;
}

.btn-card {
  flex: 1;
  height: 38px;
  font-size: 14px;
  font-weight: 600;
  border-radius: 8px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s ease;
}

.btn-details {
  background-color: #EFF6FF;
  color: #1E3A8A;
  border: 1px solid #BFDBFE;
}

.btn-details:hover {
  background-color: #DBEAFE;
}

.btn-book {
  background-color: #1E3A8A;
  color: #ffffff;
}

.btn-book:hover {
  background-color: #172554;
}
</style>
