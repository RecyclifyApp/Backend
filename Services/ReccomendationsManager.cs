using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services {
    public class ReccommendationsManager {
        public static async Task<dynamic?> RecommendQuestsAsync(MyDbContext context, string classID, int numberOfQuests) {
            var completedQuestIds = await context.ClassPoints
                .Where(cp => cp.ClassID == classID)
                .Select(cp => cp.QuestID)
                .ToListAsync();

            var completedQuests = new List<Quest>();
            var allQuests = await context.Quests.ToListAsync();
            foreach (var quest in allQuests) {
                if (completedQuestIds.Contains(quest.QuestID)) {
                    completedQuests.Add(quest);
                }
            }

            if (!completedQuests.Any()) {
                var randomQuests = allQuests.OrderBy(q => Guid.NewGuid()).Take(numberOfQuests).ToList();
                var emptyStats = new { Recycling = 0, Energy = 0, Environment = 0 };

                return new { completedQuestStatistics = emptyStats, result = randomQuests };
            }

            var allQuestTypes = new List<string> { "Recycling", "Energy", "Environment" };

            var questTypeFrequency = completedQuests
                .GroupBy(q => q.QuestType)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var type in allQuestTypes) {
                if (!questTypeFrequency.ContainsKey(type)) {
                    questTypeFrequency[type] = 0;
                }
            }

            var minFrequency = questTypeFrequency.Values.Min();
            var leastFrequentTypes = questTypeFrequency
                .Where(kvp => kvp.Value == minFrequency)
                .Select(kvp => kvp.Key)
                .ToList();

            var candidateQuests = allQuests
                .Where(q => leastFrequentTypes.Contains(q.QuestType))
                .ToList();

            if (!candidateQuests.Any()) {
                candidateQuests = allQuests.Where(q => !completedQuestIds.Contains(q.QuestID)).ToList();
            }

            candidateQuests = candidateQuests.OrderBy(q => Guid.NewGuid()).ToList();

            var commonWords = GetCommonWords(completedQuests.Select(q => q.QuestDescription));

            var similarQuests = candidateQuests
                .Where(q => ContainsCommonWords(q.QuestDescription, commonWords))
                .ToList();

            var nonSimilarQuests = candidateQuests
                .Except(similarQuests)
                .ToList();

            var finalRecommendations = similarQuests
                .Concat(nonSimilarQuests)
                .Take(numberOfQuests)
                .ToList();

            var typeCounts = completedQuests
                .GroupBy(q => q.QuestType)
                .ToDictionary(g => g.Key, g => g.Count());

            var stats = new {
                Recycling = typeCounts.GetValueOrDefault("Recycling", 0),
                Energy = typeCounts.GetValueOrDefault("Energy", 0),
                Environment = typeCounts.GetValueOrDefault("Environment", 0)
            };

            return new { completedQuestStatistics = stats, result = finalRecommendations };
        }

        private static Dictionary<string, int> GetCommonWords(IEnumerable<string> descriptions) {
            var wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var desc in descriptions) {
                var words = desc.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words) {
                    if (wordCounts.ContainsKey(word)) {
                        wordCounts[word]++;
                    } else {
                        wordCounts[word] = 1;
                    }
                }
            }
            return wordCounts.Where(kvp => kvp.Value > 1).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private static bool ContainsCommonWords(string description, Dictionary<string, int> commonWords) {
            if (commonWords.Count == 0) return false;
            var words = description.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Any(word => commonWords.ContainsKey(word));
        }
    }
}