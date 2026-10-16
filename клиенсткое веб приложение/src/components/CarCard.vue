<template>
  <div class="car-card">
    <div class="card-image-box">
      <img
        :src="carImageSrc"
        :alt="`${categoryName} ${car.model}`"
        class="card-img"
        @error="onImageError"
      />
    </div>

    <div class="card-body">
      <h3 class="card-title">{{ categoryName }} {{ car.model }}</h3>
      <div class="card-year">Год выпуска: {{ car.yearCar }}</div>
      <div class="card-price">{{ car.price }} ₽ за час</div>
      <p class="card-desc" :title="car.description">{{ car.description }}</p>

      <div class="card-actions">
        <button class="btn-card" @click="$emit('details', car)">
          Подробнее
        </button>
        <button class="btn-card" @click="handleBook">
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

const emit = defineEmits(['details', 'book']);
const router = useRouter();

const carImageSrc = computed(() => {
  if (!props.car.imagePath) return '/Resources/заглушка.png';
  return `/Resources/${props.car.imagePath}`;
});

function onImageError(e) {
  e.target.src = '/Resources/заглушка.png';
}

function handleBook() {
  if (authState.isGuest) {
    const wantLogin = confirm('Чтобы забронировать автомобиль вам нужно авторизоваться.\n\nХотите войти в систему?');
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
  width: 301px;
  height: 471px;
  background-color: #ffffff;
  border-radius: 15px;
  margin: 10px;
  box-shadow: 0 3px 15px rgba(0, 0, 0, 0.15);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-sizing: border-box;
}

.card-image-box {
  height: 236px;
  width: 100%;
  background-color: #F3F4F6;
  border-radius: 15px 15px 0 0;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  margin-top: 0;
}

.card-img {
  max-width: 90%;
  max-height: 90%;
  object-fit: contain;
}

.card-body {
  padding: 15px;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.card-title {
  font-size: 22px;
  font-weight: bold;
  color: #1F2937;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin-bottom: 0;
}

.card-year {
  font-size: 14px;
  color: #6B7280;
  margin-top: 5px;
}

.card-price {
  font-size: 16px;
  font-weight: bold;
  color: #1E3A8A;
  margin-top: 8px;
}

.card-desc {
  font-size: 13px;
  color: #4B5563;
  margin-top: 10px;
  margin-bottom: 15px;
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
  flex-direction: row;
  align-items: center;
  margin-top: auto;
  gap: 14px;
}

.btn-card {
  width: 116px;
  height: 40px;
  background-color: #1E3A8A;
  color: #ffffff;
  font-size: 14px;
  border-radius: 8px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
