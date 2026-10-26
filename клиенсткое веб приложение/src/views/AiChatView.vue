<template>
  <div class="ai-chat-page">
    <div class="chat-container">
      <!-- Шапка чата -->
      <div class="chat-header">
        <div class="chat-header-info">
          <h2 class="chat-title">Чат подбора автомобиля</h2>
          <span class="chat-subtitle">Напишите ваши критерии (марка, бюджет, коробка передач, топливо)</span>
        </div>

        <button
          v-if="messages.length > 0"
          class="btn-clear-chat"
          @click="confirmClearHistory"
          title="Начать диалог заново"
        >
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="3 6 5 6 21 6"/>
            <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
          </svg>
          <span>Очистить</span>
        </button>
      </div>

      <!-- Сообщения чата -->
      <div class="chat-messages" ref="messagesContainer">
        <!-- Приветственное состояние если диалог пуст -->
        <div v-if="messages.length === 0" class="chat-welcome">
          <div class="welcome-icon">
            <svg width="44" height="44" viewBox="0 0 24 24" fill="none" stroke="#1E3A8A" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
            </svg>
          </div>
          <h3 class="welcome-title">Автоматический подбор автомобиля</h3>
          <p class="welcome-subtitle">
            Укажите желаемую марку, коробку передач, тип топлива или ограничение по цене, и система подберет доступные варианты.
          </p>

          <div class="prompt-chips">
            <button
              v-for="prompt in suggestedPrompts"
              :key="prompt"
              class="chip"
              @click="sendPrompt(prompt)"
            >
              {{ prompt }}
            </button>
          </div>
        </div>

        <!-- Список сообщений -->
        <div
          v-for="msg in messages"
          :key="msg.idMessage || msg.tempId"
          class="message-row"
          :class="{ 'user-row': isUser(msg), 'bot-row': !isUser(msg) }"
        >
          <div class="message-bubble" :class="{ 'user-bubble': isUser(msg), 'bot-bubble': !isUser(msg) }">
            <div class="message-text">{{ msg.messageText }}</div>

            <!-- Рекомендованные автомобили от бота -->
            <div v-if="msg.recommendedCars && msg.recommendedCars.length > 0" class="recommended-grid">
              <div
                v-for="car in msg.recommendedCars"
                :key="car.idCar"
                class="ai-car-card"
              >
                <div class="ai-car-img-box">
                  <img
                    :src="getCarImg(car.imagePath, car)"
                    :alt="car.model"
                    class="ai-car-img"
                    @error="onImgError"
                  />
                </div>
                <div class="ai-car-details">
                  <div class="ai-car-title">{{ car.category || '' }} {{ car.model }}</div>
                  <div class="ai-car-specs">
                    <span v-if="car.yearCar">{{ car.yearCar }} г.</span>
                    <span v-if="car.powerHp">• {{ car.powerHp }} л.с.</span>
                    <span v-if="car.transmission">• {{ car.transmission }}</span>
                    <span v-if="car.fuelType">• {{ car.fuelType }}</span>
                  </div>
                  <div class="ai-car-price">{{ Number(car.price || 0).toLocaleString('ru-RU') }} ₽ / час</div>
                  <button class="ai-book-btn" @click="openBooking(car)">
                    Забронировать
                  </button>
                </div>
              </div>
            </div>

            <div class="message-time">{{ formatTime(msg.createdAt) }}</div>
          </div>
        </div>

        <!-- Индикатор набора сообщения ботом -->
        <div v-if="sending" class="message-row bot-row">
          <div class="message-bubble bot-bubble typing-bubble">
            <span class="typing-dot"></span>
            <span class="typing-dot"></span>
            <span class="typing-dot"></span>
            <span class="typing-text">Подбираю подходящие варианты...</span>
          </div>
        </div>
      </div>

      <!-- Быстрые подсказки над инпутом -->
      <div class="quick-chips" v-if="messages.length > 0">
        <button
          v-for="prompt in suggestedPrompts"
          :key="prompt"
          class="quick-chip"
          @click="sendPrompt(prompt)"
        >
          {{ prompt }}
        </button>
      </div>

      <!-- Поле ввода сообщения -->
      <form class="chat-input-bar" @submit.prevent="handleSendMessage">
        <input
          type="text"
          v-model="inputQuery"
          placeholder="Например: Автомобиль на автомате до 5000 рублей..."
          class="chat-input"
          :disabled="sending"
        />
        <button type="submit" class="btn-send" :disabled="!inputQuery.trim() || sending">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="22" y1="2" x2="11" y2="13"/>
            <polygon points="22 2 15 22 11 13 2 9 22 2"/>
          </svg>
          <span>Отправить</span>
        </button>
      </form>
    </div>

    <!-- Модальное окно Бронирования -->
    <BookingModal
      v-if="activeBookingCar"
      :car="activeBookingCar"
      :category-name="activeBookingCar.category || ''"
      @close="activeBookingCar = null"
      @success="onBookingSuccess"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, nextTick } from 'vue';
import { useRouter } from 'vue-router';
import { apiClient } from '../api';
import { authState } from '../auth';
import { showToast, showConfirm } from '../notifications';
import BookingModal from '../components/BookingModal.vue';

const router = useRouter();
const messages = ref([]);
const inputQuery = ref('');
const sending = ref(false);
const messagesContainer = ref(null);
const activeBookingCar = ref(null);

const suggestedPrompts = [
  'Автомобили до 4 000 рублей',
  'Автомобили с коробкой автомат',
  'Бензиновые автомобили',
  'Мощный автомобиль от 200 л.с.',
  'Показать BMW и Mercedes'
];

const allCars = ref([]);

onMounted(async () => {
  if (authState.isGuest) {
    const wantLogin = await showConfirm(
      'Подбор авто доступен только для авторизованных клиентов.\n\nХотите войти в систему?',
      'Требуется авторизация'
    );
    if (wantLogin) {
      router.push('/login');
    } else {
      router.push('/app/catalog');
    }
    return;
  }

  // Загружаем список машин для гарантированного получения фото
  try {
    const carsRes = await apiClient.getCars();
    allCars.value = Array.isArray(carsRes) ? carsRes : [];
  } catch (err) {
    console.error('Ошибка загрузки машин для чата:', err);
  }

  // Загружаем сохраненную историю диалога с сервера
  await loadChatHistory();
});

async function loadChatHistory() {
  const userId = authState.userId;
  if (!userId) return;

  try {
    const history = await apiClient.getAiHistory(userId);
    if (Array.isArray(history) && history.length > 0) {
      messages.value = history.map(item => {
        let cars = [];
        // Для сообщений бота ищем упомянутые автомобили
        if (Number(item.idSender) === 2 && allCars.value.length > 0) {
          cars = extractCarsFromText(item.messageText);
        }
        return {
          idMessage: item.idMessage,
          idSender: item.idSender,
          messageText: item.messageText,
          createdAt: item.createdAt,
          recommendedCars: cars,
        };
      });
      await scrollToBottom();
    } else {
      messages.value = [];
    }
  } catch (err) {
    console.error('Ошибка загрузки истории чата:', err);
  }
}

function extractCarsFromText(text) {
  if (!text || !allCars.value.length) return [];
  const lower = text.toLowerCase();
  const matched = [];
  const addedIds = new Set();

  for (const car of allCars.value) {
    const model = (car.model || '').toLowerCase();
    if (model && lower.includes(model) && !addedIds.has(car.idCar)) {
      matched.push({
        idCar: car.idCar,
        model: car.model,
        price: car.price,
        yearCar: car.yearCar,
        powerHp: car.powerHp,
        imagePath: car.imagePath,
        category: car.categoryName || car.category || '',
      });
      addedIds.add(car.idCar);
    }
  }
  return matched;
}

function isUser(msg) {
  return Number(msg.idSender) === 1;
}

function findCar(car) {
  if (!car) return null;
  return allCars.value.find(c => {
    if (car.idCar && c.idCar === car.idCar) return true;
    if (car.model && c.model && c.model.toLowerCase() === car.model.toLowerCase()) return true;
    return false;
  });
}

function getCarImg(imagePath, car = null) {
  let path = imagePath;
  if (!path && car) {
    const found = findCar(car);
    if (found) path = found.imagePath;
  }
  if (!path) return '/Resources/заглушка.png';
  if (path.startsWith('http') || path.startsWith('/')) return path;
  return `/Resources/${path}`;
}

function onImgError(e) {
  e.target.src = '/Resources/заглушка.png';
}

function formatTime(dateStr) {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  if (isNaN(d)) return '';
  return d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
}

async function scrollToBottom() {
  await nextTick();
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight;
  }
}

function sendPrompt(text) {
  inputQuery.value = text;
  handleSendMessage();
}

async function handleSendMessage() {
  const query = inputQuery.value.trim();
  if (!query || sending.value) return;

  const userMsg = {
    tempId: Date.now(),
    idSender: 1,
    messageText: query,
    createdAt: new Date().toISOString()
  };
  messages.value.push(userMsg);
  inputQuery.value = '';
  sending.value = true;
  await scrollToBottom();

  try {
    const response = await apiClient.sendAiMessage(authState.userId, query);

    const botMsg = {
      tempId: Date.now() + 1,
      idSender: 2,
      messageText: response.reply,
      recommendedCars: response.recommendedCars || [],
      createdAt: new Date().toISOString()
    };
    messages.value.push(botMsg);
  } catch (err) {
    console.error('Ошибка отправки сообщения:', err);
    messages.value.push({
      tempId: Date.now() + 1,
      idSender: 2,
      messageText: 'Произошла ошибка при обработке запроса. Пожалуйста, повторите попытку.',
      createdAt: new Date().toISOString()
    });
  } finally {
    sending.value = false;
    await scrollToBottom();
  }
}

async function confirmClearHistory() {
  const confirmed = await showConfirm(
    'Вы уверены, что хотите очистить историю чата?',
    'Очистка истории диалога'
  );
  if (!confirmed) return;

  try {
    await apiClient.clearAiHistory(authState.userId);
    messages.value = [];
    showToast('История переписки очищена', 'info');
  } catch (e) {
    console.error('Ошибка очистки:', e);
    showToast('Не удалось очистить историю', 'error');
  }
}

function openBooking(car) {
  const found = findCar(car);
  activeBookingCar.value = {
    ...(found || {}),
    ...car,
    idCar: car.idCar || found?.idCar,
    model: car.model || found?.model,
    price: car.price || found?.price,
    yearCar: car.yearCar || found?.yearCar,
    imagePath: car.imagePath || found?.imagePath,
    category: car.category || found?.categoryName || '',
  };
}

function onBookingSuccess() {
  activeBookingCar.value = null;
  showToast('Заявка на бронирование успешно оформлена!', 'success');
}
</script>

<style scoped>
.ai-chat-page {
  padding: 24px 32px;
  height: calc(100vh - 68px);
  box-sizing: border-box;
  display: flex;
  justify-content: center;
}

.chat-container {
  width: 100%;
  max-width: 960px;
  background-color: #ffffff;
  border-radius: 16px;
  border: 1px solid #E5E7EB;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.chat-header {
  padding: 16px 24px;
  border-bottom: 1px solid #E5E7EB;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: #FAFAFA;
}

.chat-title {
  font-size: 19px;
  font-weight: 700;
  color: #111827;
  margin: 0;
}

.chat-subtitle {
  font-size: 13px;
  color: #6B7280;
  margin-top: 3px;
  display: block;
}

.btn-clear-chat {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 7px 14px;
  background-color: #ffffff;
  border: 1px solid #D1D5DB;
  border-radius: 8px;
  font-size: 13px;
  color: #4B5563;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-clear-chat:hover {
  background-color: #FEE2E2;
  color: #DC2626;
  border-color: #FCA5A5;
}

.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  background-color: #F9FAFB;
}

.chat-welcome {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 40px 20px;
  margin: auto 0;
}

.welcome-icon {
  width: 68px;
  height: 68px;
  border-radius: 50%;
  background-color: #EFF6FF;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 16px;
}

.welcome-title {
  font-size: 20px;
  font-weight: 700;
  color: #111827;
  margin: 0 0 8px 0;
}

.welcome-subtitle {
  font-size: 14px;
  color: #6B7280;
  max-width: 520px;
  line-height: 1.5;
  margin: 0 0 24px 0;
}

.prompt-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  justify-content: center;
  max-width: 600px;
}

.chip {
  padding: 8px 16px;
  background-color: #ffffff;
  border: 1px solid #D1D5DB;
  border-radius: 20px;
  font-size: 13.5px;
  color: #1E3A8A;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.chip:hover {
  background-color: #EFF6FF;
  border-color: #93C5FD;
  transform: translateY(-1px);
}

.message-row {
  display: flex;
  width: 100%;
}

.user-row {
  justify-content: flex-end;
}

.bot-row {
  justify-content: flex-start;
}

.message-bubble {
  max-width: 78%;
  padding: 14px 18px;
  border-radius: 14px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  display: flex;
  flex-direction: column;
}

.user-bubble {
  background-color: #1E3A8A;
  color: #ffffff;
  border-bottom-right-radius: 3px;
}

.bot-bubble {
  background-color: #ffffff;
  color: #1F2937;
  border: 1px solid #E5E7EB;
  border-bottom-left-radius: 3px;
}

.message-text {
  font-size: 14.5px;
  line-height: 1.55;
  white-space: pre-line;
  word-break: break-word;
}

.message-time {
  font-size: 11px;
  opacity: 0.7;
  margin-top: 6px;
  align-self: flex-end;
}

.recommended-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 12px;
  margin-top: 14px;
}

.ai-car-card {
  background-color: #ffffff;
  border: 1px solid #E5E7EB;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  display: flex;
  flex-direction: column;
}

.ai-car-img-box {
  width: 100%;
  height: 140px;
  background-color: #F3F4F6;
  overflow: hidden;
}

.ai-car-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.ai-car-details {
  padding: 12px;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.ai-car-title {
  font-size: 15px;
  font-weight: 700;
  color: #111827;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.ai-car-specs {
  font-size: 12px;
  color: #6B7280;
  margin-top: 3px;
}

.ai-car-price {
  font-size: 14.5px;
  font-weight: 700;
  color: #1E3A8A;
  margin-top: 6px;
  margin-bottom: 10px;
}

.ai-book-btn {
  margin-top: auto;
  padding: 7px 12px;
  background-color: #1E3A8A;
  color: #ffffff;
  border: none;
  border-radius: 7px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.15s ease;
}

.ai-book-btn:hover {
  background-color: #172554;
}

.typing-bubble {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #6B7280;
  font-size: 13px;
}

.typing-dot {
  width: 6px;
  height: 6px;
  background-color: #9CA3AF;
  border-radius: 50%;
  animation: typing 1.2s infinite ease-in-out;
}

.typing-dot:nth-child(2) { animation-delay: 0.2s; }
.typing-dot:nth-child(3) { animation-delay: 0.4s; }

@keyframes typing {
  0%, 80%, 100% { transform: scale(0.6); opacity: 0.4; }
  40% { transform: scale(1.1); opacity: 1; }
}

.quick-chips {
  display: flex;
  gap: 8px;
  padding: 10px 20px;
  overflow-x: auto;
  background-color: #ffffff;
  border-top: 1px solid #F3F4F6;
}

.quick-chip {
  padding: 5px 12px;
  background-color: #F3F4F6;
  border: 1px solid #E5E7EB;
  border-radius: 14px;
  font-size: 12px;
  color: #374151;
  white-space: nowrap;
  cursor: pointer;
  transition: all 0.15s ease;
}

.quick-chip:hover {
  background-color: #EFF6FF;
  color: #1E3A8A;
  border-color: #BFDBFE;
}

.chat-input-bar {
  display: flex;
  align-items: center;
  padding: 14px 20px;
  gap: 12px;
  background-color: #ffffff;
  border-top: 1px solid #E5E7EB;
}

.chat-input {
  flex: 1;
  height: 44px;
  padding: 0 16px;
  background-color: #F9FAFB;
  border: 1px solid #D1D5DB;
  border-radius: 10px;
  font-size: 14px;
  color: #111827;
  outline: none;
  transition: border-color 0.15s ease;
}

.chat-input:focus {
  border-color: #1E3A8A;
  background-color: #ffffff;
}

.btn-send {
  height: 44px;
  padding: 0 20px;
  background-color: #1E3A8A;
  color: #ffffff;
  border: none;
  border-radius: 10px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: background-color 0.15s ease;
}

.btn-send:hover:not(:disabled) {
  background-color: #172554;
}

.btn-send:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

@media (max-width: 768px) {
  .ai-chat-page {
    padding: 12px;
  }
  .message-bubble {
    max-width: 90%;
  }
}
</style>
