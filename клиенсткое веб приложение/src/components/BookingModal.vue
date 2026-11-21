
<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="booking-window">
      <div class="top-row">
        <button class="back-btn" @click="$emit('close')">
          <span class="arrow-back">←</span>
          <span>Назад</span>
        </button>
        <h2 class="window-title">Бронирование тест-драйва</h2>
      </div>

      <div class="content-grid">
        <!-- Левая часть: картинка -->
        <div class="image-box">
          <img
            :src="carImageSrc"
            :alt="`${categoryName} ${car.model}`"
            class="car-booking-img"
            @error="onImgError"
          />
        </div>

        <!-- Правая часть: форма бронирования -->
        <div class="form-column">
          <h1 class="car-title">{{ categoryName }} {{ car.model }}</h1>
          <div class="car-price">{{ car.price }} ₽</div>

          <div class="form-group">
            <label class="field-label">Дата</label>
            <input
              type="date"
              v-model="bookingDate"
              :min="minDateStr"
              :max="maxDateStr"
              class="form-input"
              @change="onDateChange"
            />
          </div>

          <div class="form-group">
            <label class="field-label">Время</label>

            <select
              v-model="selectedTime"
              class="form-input"
              :disabled="
                !bookingDate ||
                loadingTimes ||
                timeLoadError ||
                freeSlots.length === 0
              "
            >
              <option value="" disabled>{{ timePlaceholder }}</option>

              <!-- Показываем только свободное время -->
              <option
                v-for="t in freeSlots"
                :key="t.timeValue"
                :value="t.timeValue"
              >
                {{ t.timeString }}
              </option>
            </select>

            <!-- Ошибка загрузки -->
            <div
              v-if="timeLoadError"
              class="no-times-hint"
            >
              Не удалось загрузить свободное время. Попробуйте выбрать дату ещё раз.
            </div>

            <!-- Нет свободного времени -->
            <div
              v-if="
                bookingDate &&
                !loadingTimes &&
                !timeLoadError &&
                freeSlots.length === 0
              "
              class="no-times-hint"
            >
              На выбранную дату нет доступного времени
            </div>

            <!-- Информация о моих бронях -->
            <div v-if="myBookings.length" class="my-booking-warning">
              <div v-for="b in myBookings" :key="b.time + b.car">
                На этот день у вас уже есть бронь на {{ b.time
                }}<span v-if="b.car"> ({{ b.car }})</span>
              </div>

              <div v-if="myConflict">
                Выбранное время пересекается с вашей бронью.
                Между бронями должно пройти не менее 4 часов.
              </div>
            </div>
          </div>

          <div class="form-group">
            <label class="field-label">Способ оплаты</label>

            <div class="radio-options">
              <label class="radio-label">
                <input
                  type="radio"
                  name="paymentMethod"
                  value="1"
                  v-model="paymentMethod"
                />
                <span>Наличный расчет</span>
              </label>

              <label class="radio-label">
                <input
                  type="radio"
                  name="paymentMethod"
                  value="2"
                  v-model="paymentMethod"
                />
                <span>Оплата картой</span>
              </label>
            </div>
          </div>

          <div class="agreement-group">
            <label class="checkbox-label">
              <input
                type="checkbox"
                v-model="agreed"
              />
              <span>
                Я соглашаюсь с условиями обработки персональных данных
              </span>
            </label>
          </div>

          <button
            class="btn-book"
            @click="submitBooking"
            :disabled="submitting || !!myConflict"
          >
            {{ submitting ? 'Оформление...' : 'Забронировать' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { apiClient } from '../api';
import { authState } from '../auth';
import { showToast } from '../notifications';

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

const emit = defineEmits(['close', 'success']);

const bookingDate = ref('');
const selectedTime = ref('');
const paymentMethod = ref('');
const agreed = ref(false);
const loadingTimes = ref(false);
const submitting = ref(false);
const timeLoadError = ref(false);

const allStandardTimes = [
  { timeString: '9:00', timeValue: '09:00:00' },
  { timeString: '10:00', timeValue: '10:00:00' },
  { timeString: '11:00', timeValue: '11:00:00' },
  { timeString: '12:00', timeValue: '12:00:00' },
  { timeString: '13:00', timeValue: '13:00:00' },
  { timeString: '14:00', timeValue: '14:00:00' },
  { timeString: '15:00', timeValue: '15:00:00' },
  { timeString: '16:00', timeValue: '16:00:00' },
  { timeString: '17:00', timeValue: '17:00:00' },
];

const timeSlots = ref([]);

// Мои брони на выбранную дату
const myBookings = ref([]);

let loadCounter = 0;

// Только свободное время
const freeSlots = computed(() =>
  timeSlots.value.filter(t => t.isAvailable)
);

const timePlaceholder = computed(() => {
  if (loadingTimes.value) return 'Загрузка времени...';
  if (timeLoadError.value) return 'Ошибка загрузки времени';
  if (bookingDate.value && freeSlots.value.length === 0) {
    return 'Нет свободного времени';
  }
  return 'Выберите время';
});

// Переводим время в минуты
function toMinutes(timeStr) {
  const [h, m] = String(timeStr).split(':');
  return Number(h) * 60 + Number(m || 0);
}

// Проверка конфликта с моей бронью
const myConflict = computed(() => {
  if (!selectedTime.value) return null;

  return (
    myBookings.value.find(
      b =>
        Math.abs(
          toMinutes(b.time) - toMinutes(selectedTime.value)
        ) < 240
    ) || null
  );
});

const today = new Date();

const minDateStr = computed(() => {
  return today.toISOString().split('T')[0];
});

const maxDateStr = computed(() => {
  const d = new Date(today);
  d.setMonth(d.getMonth() + 1);
  return d.toISOString().split('T')[0];
});

const carImageSrc = computed(() => {
  const path = props.car?.imagePath || props.car?.ImagePath;

  if (!path) return '/Resources/заглушка.png';
  if (path.startsWith('http') || path.startsWith('/')) return path;

  return `/Resources/${path}`;
});

function onImgError(e) {
  e.target.src = '/Resources/заглушка.png';
}

// Загрузка свободного времени
async function onDateChange() {
  if (!bookingDate.value) return;

  const requestId = ++loadCounter;

  loadingTimes.value = true;
  timeLoadError.value = false;
  selectedTime.value = '';
  timeSlots.value = [];
  myBookings.value = [];

  try {
    const [busyList, allBookings] = await Promise.all([
      apiClient.getBusyTimes(props.car.idCar, bookingDate.value),
      apiClient.getBookings(),
    ]);

    // Проверяем, что дата не была изменена во время загрузки
    if (requestId !== loadCounter) return;

    // Мои брони на выбранную дату
    myBookings.value = (Array.isArray(allBookings) ? allBookings : [])
      .filter(b =>
        Number(b.idUser) === Number(authState.userId) &&
        Number(b.idStatus) !== 3 &&
        String(b.bookingDate).substring(0, 10) === bookingDate.value
      )
      .map(b => ({
        time: String(b.bookingTime).substring(0, 5),
        car: b.idCarNavigation?.model || '',
      }))
      .sort((a, b) => a.time.localeCompare(b.time));

    // Время, занятое выбранной машиной
    const busyTimes = new Set(
      Array.isArray(busyList)
        ? busyList.map(item => String(item).substring(0, 8))
        : []
    );

    // Проверяем все стандартные часы
    timeSlots.value = allStandardTimes.map(t => {
      // Занято ли время выбранной машины
      const carBusy = busyTimes.has(t.timeValue);

      // Есть ли у пользователя другая бронь
      // ближе чем через 4 часа
      const userBusy = myBookings.value.some(b =>
        Math.abs(
          toMinutes(b.time) - toMinutes(t.timeValue)
        ) < 240
      );

      return {
        ...t,
        isAvailable: !carBusy && !userBusy,
      };
    });

  } catch (err) {
    console.error('Ошибка получения занятого времени:', err);

    if (requestId === loadCounter) {
      timeSlots.value = [];
      myBookings.value = [];
      timeLoadError.value = true;

      showToast('Не удалось загрузить свободное время', 'error');
    }
  } finally {
    if (requestId === loadCounter) {
      loadingTimes.value = false;
    }
  }
}

// Создание бронирования
async function submitBooking() {
  if (loadingTimes.value || timeLoadError.value) {
    showToast('Сначала загрузите свободное время', 'warning');
    return;
  }

  if (!bookingDate.value || !selectedTime.value) {
    showToast('Заполните дату и время', 'warning');
    return;
  }

  const selectedSlot = timeSlots.value.find(
    t => t.timeValue === selectedTime.value
  );

  if (!selectedSlot || !selectedSlot.isAvailable) {
    showToast('Это время недоступно для бронирования', 'warning');
    return;
  }

  if (myConflict.value) {
    showToast(
      'У вас уже есть бронь в этот промежуток времени',
      'warning'
    );
    return;
  }

  const selDate = new Date(bookingDate.value);
  selDate.setHours(0, 0, 0, 0);

  const now = new Date();
  now.setHours(0, 0, 0, 0);

  if (selDate < now) {
    showToast('Нельзя выбрать прошедшую дату', 'warning');
    return;
  }

  const monthLater = new Date(now);
  monthLater.setMonth(monthLater.getMonth() + 1);

  if (selDate > monthLater) {
    showToast(
      'Бронирование доступно только в пределах месяца',
      'warning'
    );
    return;
  }

  if (!agreed.value) {
    showToast(
      'Для оформления бронирования необходимо согласиться с условиями',
      'warning'
    );
    return;
  }

  if (!paymentMethod.value) {
    showToast('Выберите способ оплаты', 'warning');
    return;
  }

  const bookingDto = {
    idUser: authState.userId,
    idCar: props.car.idCar,
    bookingDate: bookingDate.value,
    bookingTime: selectedTime.value,
    idStatus: 1,
    idPayment: parseInt(paymentMethod.value, 10),
  };

  submitting.value = true;

  try {
    const result = await apiClient.createBooking(bookingDto);

    showToast(
      typeof result === 'string'
        ? result
        : 'Бронирование успешно создано!',
      'success'
    );

    emit('success');
    emit('close');

  } catch (err) {
    const errorText =
      err.response?.data ||
      err.message ||
      'Ошибка оформления бронирования';

    showToast(
      typeof errorText === 'string'
        ? errorText
        : JSON.stringify(errorText),
      'error'
    );

  } finally {
    submitting.value = false;
  }
}
</script>

<style scoped>
.booking-window {
  width: 1030px;
  max-width: 95vw;
  max-height: 90vh;
  overflow-y: auto;
  background-color: #ffffff;
  border-radius: 20px;
  padding: 25px;
  box-shadow: 0 12px 28px rgba(0, 0, 0, 0.12);
  display: flex;
  flex-direction: column;
}

.top-row {
  display: flex;
  align-items: center;
  margin-bottom: 20px;
}

.back-btn {
  width: 110px;
  height: 40px;
  background-color: #1E3A8A;
  color: #ffffff;
  font-size: 14px;
  border-radius: 10px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
}

.arrow-back {
  font-size: 14px;
}

.window-title {
  font-size: 24px;
  font-weight: bold;
  color: #111827;
  margin-left: 20px;
}

.content-grid {
  display: grid;
  grid-template-columns: 1.1fr 1fr;
  gap: 30px;
  align-items: start;
}

.image-box {
  border-radius: 16px;
  background-color: #F3F4F6;
  border: 1px solid #E5E7EB;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  height: 380px;
  min-height: 380px;
  padding: 0;
}

.car-booking-img {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.form-column {
  display: flex;
  flex-direction: column;
}

.car-title {
  font-size: 28px;
  font-weight: bold;
  color: #111827;
  margin-bottom: 15px;
}

.car-price {
  font-size: 20px;
  font-weight: 600;
  color: #1E3A8A;
  margin-bottom: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  margin-bottom: 15px;
}

.field-label {
  font-size: 13px;
  color: #6B7280;
  margin-bottom: 5px;
}

.form-input {
  height: 40px;
  padding: 0 12px;
  border: 1px solid #D1D5DB;
  border-radius: 6px;
  background: #ffffff;
  font-size: 14px;
  color: #111827;
}

.no-times-hint {
  margin-top: 6px;
  font-size: 13px;
  color: #6B7280;
}

.my-booking-warning {
  margin-top: 8px;
  font-size: 13px;
  font-weight: 500;
  line-height: 1.4;
  color: #DC2626;
}

.radio-options {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-top: 5px;
}

.radio-label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #374151;
  cursor: pointer;
}

.agreement-group {
  margin-top: 10px;
  margin-bottom: 20px;
}

.checkbox-label {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  font-size: 12px;
  color: #374151;
  cursor: pointer;
  line-height: 1.4;
}

.checkbox-label input {
  margin-top: 2px;
}

.btn-book {
  height: 48px;
  background-color: #1E3A8A;
  color: #ffffff;
  font-size: 15px;
  font-weight: 600;
  border-radius: 12px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-book:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

@media (max-width: 850px) {
  .content-grid {
    grid-template-columns: 1fr;
  }
}
</style>
