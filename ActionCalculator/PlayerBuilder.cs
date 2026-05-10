using ActionCalculator.Abstractions;
using ActionCalculator.Models;
using ActionCalculator.Utilities;

namespace ActionCalculator
{
    public class PlayerBuilder(ICalculationContext context) : IPlayerBuilder
    {
        public Player Build(string playerInput)
        {
            string? starPlayerShortName = null;
            var input = playerInput;

            if (StarPlayerRules.ByShortName(context.Season)
                .TryGetValue(playerInput.Replace("*", "").Replace("^", ""), out var rule))
            {
                starPlayerShortName = playerInput;
                var starPlayerSkills = rule.SkillsInput.Split(',').ToList();

                bool hasHatred = playerInput.EndsWith('*');
                if (!hasHatred)
                {
                    starPlayerSkills = RemoveSkill(starPlayerSkills, "H");
                }

                bool illCarryYou = playerInput.EndsWith('^');
                if (!illCarryYou && rule.StarPlayer == StarPlayer.Grak)
                {
                    starPlayerSkills = RemoveSkill(RemoveSkill(starPlayerSkills, "BT3"), "D");
                }

                starPlayerSkills = ReplaceDwarfenScourgeWithMightyBlow(starPlayerSkills, hasHatred);
                input = string.Join(',', starPlayerSkills);
            }

            var skillsWithValues = GetSkillsWithValues(input);

            return new Player(Guid.NewGuid(),
                skillsWithValues.Aggregate(CalculatorSkills.None, (current, skill) => current | skill.Item1),
                GetSkillValue(skillsWithValues, CalculatorSkills.Loner),
                GetSkillValue(skillsWithValues, CalculatorSkills.BreakTackle),
                GetSkillValue(skillsWithValues, CalculatorSkills.MightyBlow),
                GetSkillValue(skillsWithValues, CalculatorSkills.DirtyPlayer),
                starPlayerShortName);
        }

        private List<string> RemoveDodgeAndBreakTackle(List<string> skills)
        {
            skills.Remove("D");
            skills.Remove("BT");
            return skills;
        }

        private static List<string> RemoveSkill(List<string> skills, string skillToRemove)
        {
            skills.Remove(skillToRemove);
            return skills;
        }

        private static List<string> ReplaceDwarfenScourgeWithMightyBlow(List<string> skills, bool vsDwarves)
        {
            if (!skills.Contains("DS"))
                return skills;

            skills.Add(vsDwarves ? "MB2" : "MB1");
            return skills;
        }

        private static List<Tuple<CalculatorSkills, int>> GetSkillsWithValues(string playerInput) =>
            playerInput.Split(',')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(GetSkillAndRoll)
                .Select(x =>
                    new Tuple<CalculatorSkills, int>(x.Item1.GetValueFromDescription<CalculatorSkills>(), x.Item2))
                .ToList();

        private static Tuple<string, int> GetSkillAndRoll(string input)
        {
            var last = input.Last();

            return last is '1' or '2' or '3' or '4' or '5' or '6'
                ? new Tuple<string, int>(input.Remove(input.Length - 1), int.Parse(last.ToString()))
                : input is "L"
                    ? new Tuple<string, int>(input, 4)
                    : input is "P"
                        ? new Tuple<string, int>(input, 3)
                        : new Tuple<string, int>(input, 0);
        }

        private static int GetSkillValue(IEnumerable<Tuple<CalculatorSkills, int>> skills, CalculatorSkills skill) =>
            skills.SingleOrDefault(x => x.Item1 == skill && x.Item2 > 1)?.Item2 ?? 1;
    }
}
