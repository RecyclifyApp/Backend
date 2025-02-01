using Backend.Models;

namespace Backend.Services {
    public class ReccomendationsManager (MyDbContext context) {
        private readonly MyDbContext _context = context;

        public List<Quest> RecommendQuests() {
            var recommendedQuests = new List<Quest>();

            var completedQuestIds = _context.ClassPoints.Select(cp => cp.QuestID).ToList();
            var completedQuests = _context.Quests.Where(q => completedQuestIds.Contains(q.QuestID)).ToList();

            var questTypeFrequency = completedQuests.GroupBy(q => q.QuestType).ToDictionary(g => g.Key, g => g.Count());
            var leastFrequentQuestType = questTypeFrequency.OrderBy(kvp => kvp.Value).FirstOrDefault().Key;

            var questsByLeastFrequentType = _context.Quests.Where(q => q.QuestType == leastFrequentQuestType && !completedQuestIds.Contains(q.QuestID)).Take(3).ToList();
            recommendedQuests.AddRange(questsByLeastFrequentType);

            var commonWords = GetCommonWords(completedQuests.Select(q => q.QuestDescription).ToList());
            var similarQuests = _context.Quests.Where(q => ContainsCommonWords(q.QuestDescription, commonWords) && !completedQuestIds.Contains(q.QuestID)).Take(3).ToList();
            recommendedQuests.AddRange(similarQuests);

            return recommendedQuests.Distinct().ToList();
        }

        private Dictionary<string, int> GetCommonWords(List<string> descriptions) {
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

        private bool ContainsCommonWords(string description, Dictionary<string, int> commonWords) {
            var words = description.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return words.Any(word => commonWords.ContainsKey(word));
        }
    }
}