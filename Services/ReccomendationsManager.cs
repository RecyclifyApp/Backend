using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services {
    public class RecommendationsManager() {

        public static async Task<List<Quest>> RecommendQuestsAsync(MyDbContext context, string classID) {
            var recommendedQuests = new List<Quest>();

            var completedQuestIds = await context.ClassPoints.Where(cp => cp.ClassID == classID).Select(cp => cp.QuestID).ToListAsync();

            var completedQuests = new List<Quest>();
            foreach (var questId in completedQuestIds) {
                var quest = await context.Quests.FindAsync(questId);
                if (quest != null) completedQuests.Add(quest);
            }

            var questTypeFrequency = completedQuests.GroupBy(q => q.QuestType).ToDictionary(g => g.Key, g => g.Count());

            var leastFrequentQuestType = questTypeFrequency.OrderBy(kvp => kvp.Value).FirstOrDefault().Key;

            var questsByLeastFrequentType = new List<Quest>();
            foreach (var quest in await context.Quests.Where(q => q.QuestType == leastFrequentQuestType).ToListAsync()) {
                if (!completedQuestIds.Contains(quest.QuestID)) questsByLeastFrequentType.Add(quest);
            }

            recommendedQuests.AddRange(questsByLeastFrequentType);

            var commonWords = GetCommonWords(completedQuests.Select(q => q.QuestDescription).ToList());

            var similarQuests = new List<Quest>();
            foreach (var quest in await context.Quests.ToListAsync()) {
                if (ContainsCommonWords(quest.QuestDescription, commonWords) && !completedQuestIds.Contains(quest.QuestID)) {
                    similarQuests.Add(quest);
                }
            }

            recommendedQuests.AddRange(similarQuests);

            return recommendedQuests.Distinct().Take(3).ToList();
        }

        private static Dictionary<string, int> GetCommonWords(List<string> descriptions) {
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