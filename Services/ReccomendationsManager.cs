using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services {
    public class RecommendationsManager {
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
                return null;
            }

            var questTypeFrequency = completedQuests
                .GroupBy(q => q.QuestType)
                .ToDictionary(g => g.Key, g => g.Count());

            var minFrequency = questTypeFrequency.Values.Min();
            var leastFrequentQuestTypes = questTypeFrequency
                .Where(kvp => kvp.Value == minFrequency)
                .Select(kvp => kvp.Key)
                .ToList();

            var questsByLeastFrequentType = new List<Quest>();
            foreach (var quest in allQuests) {
                if (leastFrequentQuestTypes.Contains(quest.QuestType) && !completedQuestIds.Contains(quest.QuestID)) {
                    questsByLeastFrequentType.Add(quest);
                }
            }

            var commonWords = GetCommonWords(completedQuests.Select(q => q.QuestDescription).ToList());

            var similarQuests = new List<Quest>();
            foreach (var quest in questsByLeastFrequentType) {
                if (ContainsCommonWords(quest.QuestDescription, commonWords)) {
                    similarQuests.Add(quest);
                }
            }

            var similarQuestIds = new HashSet<string>(similarQuests.Select(q => q.QuestID));
            var recommendedQuests = new List<Quest>();

            foreach (var quest in questsByLeastFrequentType) {
                if (similarQuestIds.Contains(quest.QuestID) || recommendedQuests.Count < numberOfQuests) {
                    recommendedQuests.Add(quest);
                }
                if (recommendedQuests.Count >= numberOfQuests) break;
            }

            var completedQuestTypes = completedQuests.Select(q => q.QuestType).ToList();
            var numberOfRecyclingQuests = completedQuestTypes.Count(t => t == "Recycling");
            var numberOfEnergyQuests = completedQuestTypes.Count(t => t == "Energy");
            var numberOfEnvironmentQuests = completedQuestTypes.Count(t => t == "Environment");

            var completedQuestStatistics = new {
                Recycling = numberOfRecyclingQuests,
                Energy = numberOfEnergyQuests,
                Environment = numberOfEnvironmentQuests
            };

            return new { completedQuestStatistics, result = recommendedQuests };
        }

        private static Dictionary<string, int> GetCommonWords(IEnumerable<string> descriptions) {
            var wordCounts = new Dictionary<string, int>();
            foreach (var desc in descriptions) {
                var words = desc.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words) {
                    if (!wordCounts.ContainsKey(word)) wordCounts[word] = 0;
                    wordCounts[word]++;
                }
            }
            return wordCounts.Where(kvp => kvp.Value > 1).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private static bool ContainsCommonWords(string description, Dictionary<string, int> commonWords) {
            var words = description.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return words.Any(word => commonWords.ContainsKey(word));
        }
    }
}