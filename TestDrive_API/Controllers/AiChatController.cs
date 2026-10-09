using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDrive_API.Classes;
using TestDrive_API.Models;

namespace TestDrive_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AiChatController(AppDbContext context)
        {
            _context = context;
        }

        // Отправить сообщение в ИИ-чат для подбора автомобиля (К8)
        [HttpPost("Message")]
        public async Task<IActionResult> SendMessage([FromBody] AiChatMessageDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Сообщение не может быть пустым");

            var userExists = await _context.Users.AnyAsync(u => u.IdUser == dto.IdUser && u.IsDelete == 0);
            if (!userExists)
                return NotFound("Пользователь не найден");

            // 1. Сохраняем сообщение пользователя в историю (1 = User)
            var userMsg = new AiChatHistory
            {
                IdUser = dto.IdUser,
                IdSender = 1,
                MessageText = dto.Message.Trim(),
                CreatedAt = DateTime.Now
            };
            _context.AiChatHistories.Add(userMsg);
            await _context.SaveChangesAsync();

            // 2. Алгоритм анализа запроса и подбора автомобиля
            string query = dto.Message.ToLowerInvariant();

            // Загружаем все доступные авто с их связями
            var allCars = await _context.Cars
                .Where(c => c.IsDeleted == 0)
                .Include(c => c.IdCategoryNavigation)
                .Include(c => c.IdFuelTypeNavigation)
                .Include(c => c.IdTreansmissionNavigation)
                .ToListAsync();

            var filteredCars = allCars.AsEnumerable();
            bool hasSpecificCriteria = false;

            // Словарь синонимов для популярных брендов на русском
            var brandSynonyms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "мерседес", "Mercedes" },
                { "мерс", "Mercedes" },
                { "бмв", "BMW" },
                { "тойота", "Toyota" },
                { "хонда", "Honda" },
                { "фольксваген", "Volkswagen" },
                { "пассат", "Passat" },
                { "лада", "Lada" },
                { "киа", "Kia" },
                { "ауди", "Audi" },
                { "лексус", "Lexus" },
                { "тесла", "Tesla" },
                { "чери", "Chery" },
                { "камри", "Camry" }
            };

            // Проверка марки/модели/категории
            var matchedBrands = allCars
                .Where(c =>
                {
                    string modelLower = c.Model.ToLower();
                    string catLower = c.IdCategoryNavigation?.NameCategory?.ToLower() ?? "";

                    if (query.Contains(modelLower) || (!string.IsNullOrEmpty(catLower) && query.Contains(catLower)))
                        return true;

                    foreach (var syn in brandSynonyms)
                    {
                        if (query.Contains(syn.Key))
                        {
                            if (modelLower.Contains(syn.Value.ToLower()) || catLower.Contains(syn.Value.ToLower()))
                                return true;
                        }
                    }

                    return false;
                })
                .ToList();

            if (matchedBrands.Any())
            {
                filteredCars = matchedBrands;
                hasSpecificCriteria = true;
            }

            // Поиск бюджета / цены (например: "до 2000", "до 5000", "до 3000000", "бюджет 4000")
            var priceMatch = Regex.Match(query, @"(?:до|бюджет|цена|стоимость)\s*(\d+(?:[\s.,]\d+)?)", RegexOptions.IgnoreCase);
            if (priceMatch.Success)
            {
                string rawPrice = priceMatch.Groups[1].Value.Replace(" ", "").Replace(",", ".");
                if (decimal.TryParse(rawPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal targetPrice))
                {
                    // Если пользователь указал миллионы (например 3000000) или часовую цену (например 3000)
                    filteredCars = filteredCars.Where(c => c.Price <= targetPrice);
                    hasSpecificCriteria = true;
                }
            }

            // Проверка типа коробки передач
            if (query.Contains("автомат") || query.Contains("акпп"))
            {
                filteredCars = filteredCars.Where(c => c.IdTreansmissionNavigation != null &&
                    c.IdTreansmissionNavigation.Name.ToLower().Contains("автомат"));
                hasSpecificCriteria = true;
            }
            else if (query.Contains("механик") || query.Contains("мкпп"))
            {
                filteredCars = filteredCars.Where(c => c.IdTreansmissionNavigation != null &&
                    c.IdTreansmissionNavigation.Name.ToLower().Contains("механик"));
                hasSpecificCriteria = true;
            }
            else if (query.Contains("робот"))
            {
                filteredCars = filteredCars.Where(c => c.IdTreansmissionNavigation != null &&
                    c.IdTreansmissionNavigation.Name.ToLower().Contains("робот"));
                hasSpecificCriteria = true;
            }

            // Проверка типа топлива
            if (query.Contains("бензин"))
            {
                filteredCars = filteredCars.Where(c => c.IdFuelTypeNavigation != null &&
                    c.IdFuelTypeNavigation.Name.ToLower().Contains("бензин"));
                hasSpecificCriteria = true;
            }
            else if (query.Contains("дизель"))
            {
                filteredCars = filteredCars.Where(c => c.IdFuelTypeNavigation != null &&
                    c.IdFuelTypeNavigation.Name.ToLower().Contains("дизель"));
                hasSpecificCriteria = true;
            }
            else if (query.Contains("электро") || query.Contains("электричес"))
            {
                filteredCars = filteredCars.Where(c => c.IdFuelTypeNavigation != null &&
                    c.IdFuelTypeNavigation.Name.ToLower().Contains("электро"));
                hasSpecificCriteria = true;
            }

            // Проверка мощности: "мощный", "быстрый", "от 200 л.с."
            var powerMatch = Regex.Match(query, @"(?:от|более|больше)\s*(\d+)\s*(?:л\.?с|сил|лс)?", RegexOptions.IgnoreCase);
            if (powerMatch.Success && int.TryParse(powerMatch.Groups[1].Value, out int minPower))
            {
                filteredCars = filteredCars.Where(c => c.PowerHp.HasValue && c.PowerHp.Value >= minPower);
                hasSpecificCriteria = true;
            }
            else if (query.Contains("мощн") || query.Contains("быстр") || query.Contains("спорт"))
            {
                filteredCars = filteredCars.OrderByDescending(c => c.PowerHp ?? 0);
                hasSpecificCriteria = true;
            }

            var recommendedList = filteredCars.Take(3).ToList();

            // 3. Формирование ответа бота
            string botReply;

            // Если это просто приветствие или вопрос (без конкретных критериев подбора)
            if (!hasSpecificCriteria && (query.Contains("привет") || query.Contains("здравствуй") || query.Contains("добрый день") || query.Contains("помоги") || string.IsNullOrWhiteSpace(query)))
            {
                botReply = "Здравствуйте! Я умный ассистент автосалона KIDRIVE. Я помогу подобрать идеальный автомобиль для тест-драйва! " +
                           "Напишите мне свои пожелания — например: «Нужен мощный седан до 4000 руб/час», «Ищу Mercedes на автомате» или «Покажи авто на бензине».";
                recommendedList = allCars.Take(2).ToList();
            }
            else if (!recommendedList.Any())
            {
                botReply = "К сожалению, по точным критериям ничего не нашлось. Но в нашем автопарке есть отличные популярные модели, " +
                           "которые вам наверняка понравятся! Рекомендую обратить внимание на:";
                recommendedList = allCars.Take(3).ToList();
                for (int i = 0; i < recommendedList.Count; i++)
                {
                    var c = recommendedList[i];
                    botReply += $"\n{i + 1}. {c.Model} ({c.YearCar} г.) — {c.Price:N0} ₽/час, {c.PowerHp} л.с., КПП: {c.IdTreansmissionNavigation?.Name ?? "Не указано"}.";
                }
                botReply += "\nВы можете оформить заявку на тест-драйв прямо сейчас!";
            }
            else
            {
                botReply = "По вашему запросу я подобрал отличные автомобили для тест-драйва:\n";
                for (int i = 0; i < recommendedList.Count; i++)
                {
                    var c = recommendedList[i];
                    botReply += $"\n{i + 1}. {c.Model} ({c.YearCar} г.) — {c.Price:N0} ₽/час, мощность {c.PowerHp} л.с., топливо: {c.IdFuelTypeNavigation?.Name ?? "Бензин"}, КПП: {c.IdTreansmissionNavigation?.Name ?? "Автомат"}.";
                }
                botReply += "\n\nКакой из них хотите опробовать в деле? Выберите дату и время для бронирования!";
            }

            // 4. Сохраняем ответ бота в историю (2 = Bot)
            var botMsg = new AiChatHistory
            {
                IdUser = dto.IdUser,
                IdSender = 2,
                MessageText = botReply,
                CreatedAt = DateTime.Now
            };
            _context.AiChatHistories.Add(botMsg);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                reply = botReply,
                recommendedCars = recommendedList.Select(c => new
                {
                    idCar = c.IdCar,
                    model = c.Model,
                    price = c.Price,
                    yearCar = c.YearCar,
                    powerHp = c.PowerHp,
                    imagePath = c.ImagePath,
                    category = c.IdCategoryNavigation?.NameCategory,
                    transmission = c.IdTreansmissionNavigation?.Name,
                    fuelType = c.IdFuelTypeNavigation?.Name
                })
            });
        }

        // Получить историю сообщений пользователя
        [HttpGet("History/{userId}")]
        public async Task<IActionResult> GetChatHistory(int userId)
        {
            var history = await _context.AiChatHistories
                .Where(h => h.IdUser == userId)
                .OrderBy(h => h.CreatedAt)
                .Select(h => new
                {
                    idMessage = h.IdMessage,
                    idUser = h.IdUser,
                    idSender = h.IdSender,
                    senderName = h.IdSenderNavigation.SenderName,
                    messageText = h.MessageText,
                    createdAt = h.CreatedAt
                })
                .ToListAsync();

            return Ok(history);
        }

        // Очистить историю сообщений пользователя
        [HttpDelete("History/{userId}")]
        public async Task<IActionResult> ClearHistory(int userId)
        {
            var messages = await _context.AiChatHistories
                .Where(h => h.IdUser == userId)
                .ToListAsync();

            _context.AiChatHistories.RemoveRange(messages);
            await _context.SaveChangesAsync();

            // Если в таблице больше нет сообщений, сбрасываем AUTO_INCREMENT на 1
            if (!await _context.AiChatHistories.AnyAsync())
            {
                try
                {
                    await _context.Database.ExecuteSqlRawAsync("ALTER TABLE ai_chat_history AUTO_INCREMENT = 1;");
                }
                catch { }
            }

            return Ok("История переписки очищена.");
        }
    }
}
