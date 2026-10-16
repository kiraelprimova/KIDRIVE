using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpPost("SendMessage")]
        public async Task<ActionResult> SendMessage(int idUser, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return BadRequest("Сообщение не может быть пустым");

            var userExists = await _context.Users.AnyAsync(u => u.IdUser == idUser && u.IsDelete == 0);
            if (!userExists)
                return NotFound("Пользователь не найден");

            var userMsg = new AiChatHistory
            {
                IdUser = idUser,
                IdSender = 1,
                MessageText = message.Trim(),
                CreatedAt = DateTime.Now
            };
            _context.AiChatHistories.Add(userMsg);
            await _context.SaveChangesAsync();

            string query = message.ToLowerInvariant();

            var allCars = await _context.Cars
                .Where(c => c.IsDeleted == 0)
                .Include(c => c.IdCategoryNavigation)
                .Include(c => c.IdFuelTypeNavigation)
                .Include(c => c.IdTreansmissionNavigation)
                .ToListAsync();

            var filteredCars = allCars.AsEnumerable();
            bool hasSpecificCriteria = false;

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

            var priceMatch = Regex.Match(query, @"(?:до|бюджет|цена|стоимость)\s*(\d+(?:[\s.,]\d+)?)", RegexOptions.IgnoreCase);
            if (priceMatch.Success)
            {
                string rawPrice = priceMatch.Groups[1].Value.Replace(" ", "").Replace(",", ".");
                if (decimal.TryParse(rawPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal targetPrice))
                {
                    filteredCars = filteredCars.Where(c => c.Price <= targetPrice);
                    hasSpecificCriteria = true;
                }
            }

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

            string botReply;

            if (!hasSpecificCriteria && (query.Contains("привет") || query.Contains("здравствуй") || query.Contains("добрый день") || query.Contains("помоги") || string.IsNullOrWhiteSpace(query)))
            {
                botReply = "Привет! Напиши, какую машину ищешь: марку, бюджет, коробку или топливо.";
                recommendedList = allCars.Take(2).ToList();
            }
            else if (!recommendedList.Any())
            {
                botReply = "По запросу ничего не нашлось. Вот популярные авто:";
                recommendedList = allCars.Take(3).ToList();
                for (int i = 0; i < recommendedList.Count; i++)
                {
                    var c = recommendedList[i];
                    botReply += $"\n{i + 1}. {c.Model} ({c.YearCar} г.) — {c.Price:N0} ₽/час, {c.PowerHp} л.с., КПП: {c.IdTreansmissionNavigation?.Name ?? "Не указано"}.";
                }
            }
            else
            {
                botReply = "Подходящие варианты:\n";
                for (int i = 0; i < recommendedList.Count; i++)
                {
                    var c = recommendedList[i];
                    botReply += $"\n{i + 1}. {c.Model} ({c.YearCar} г.) — {c.Price:N0} ₽/час, мощность {c.PowerHp} л.с., топливо: {c.IdFuelTypeNavigation?.Name ?? "Бензин"}, КПП: {c.IdTreansmissionNavigation?.Name ?? "Автомат"}.";
                }
            }

            var botMsg = new AiChatHistory
            {
                IdUser = idUser,
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

        [HttpGet("GetHistory")]
        public async Task<ActionResult> GetChatHistory(int userId)
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

        [HttpDelete("ClearHistory")]
        public async Task<ActionResult> ClearHistory(int userId)
        {
            var messages = await _context.AiChatHistories
                .Where(h => h.IdUser == userId)
                .ToListAsync();

            _context.AiChatHistories.RemoveRange(messages);
            await _context.SaveChangesAsync();

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
