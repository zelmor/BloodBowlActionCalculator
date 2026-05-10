using System.Text;
using ActionCalculator.Utilities;

namespace ActionCalculator.Models
{
    public class Player
    {
        public Player()
        {
            Id = Guid.NewGuid();
            Skills = CalculatorSkills.None;
            LonerValue = 4;
            BreakTackleValue = 1;
            MightyBlowValue = 1;
            DirtyPlayerValue = 1;
            ProSuccess = 2m / 3;
        }

        public Player(Guid id, CalculatorSkills skills, int lonerValue, int breakTackleValue, int mightyBlowValue, int dirtyPlayerValue, string? shortName = null)
        {
            Id = id;
            ShortName = shortName;
            Skills = skills;
            LonerValue = lonerValue;
            BreakTackleValue = breakTackleValue;
            MightyBlowValue = mightyBlowValue;
            DirtyPlayerValue = dirtyPlayerValue;
            ProSuccess = HasFreeProSkill(skills) ? 1m : 2m / 3;
        }

        private static bool HasFreeProSkill(CalculatorSkills skills) => 
            skills.Contains(CalculatorSkills.ConsummateProfessional)
                || skills.Contains(CalculatorSkills.HalflingLuck)
                || skills.Contains(CalculatorSkills.ThinkingMansTroll);

        public Guid Id { get; }
        public string? ShortName { get; set; }
        public CalculatorSkills Skills { get; set; }
        public int LonerValue { get; set; }
        public int BreakTackleValue { get; set; }
        public int MightyBlowValue { get; set; }
        public int DirtyPlayerValue { get; set; }
        public decimal ProSuccess { get; }

        public bool CanUseSkill(CalculatorSkills skill, CalculatorSkills usedSkills)
        {
            var underlyingSkill = skill switch
            {
                CalculatorSkills.OldPro => CalculatorSkills.Pro,
                CalculatorSkills.ConsummateProfessional => CalculatorSkills.Pro,
                CalculatorSkills.HalflingLuck => CalculatorSkills.Pro,
                _ => skill
            };

            return Skills.Contains(skill) && !usedSkills.Contains(underlyingSkill);
        }

        public override string ToString() =>
            ShortName != null
                ? ShortName
                : string.Join(',', Skills.ToEnumerable(CalculatorSkills.None)
                    .Select(x => x.GetDescriptionFromValue() + GetSkillRoll(x))
                    .OrderBy(x => x));

        private string GetSkillRoll(CalculatorSkills skill) =>
            skill switch
            {
                CalculatorSkills.Loner => LonerValue.ToString(),
                CalculatorSkills.DirtyPlayer => DirtyPlayerValue > 1 ? DirtyPlayerValue.ToString() : "",
                CalculatorSkills.MightyBlow => MightyBlowValue > 1 ? MightyBlowValue.ToString() : "",
                CalculatorSkills.BreakTackle => BreakTackleValue.ToString(),
                _ => ""
            };

        public bool HasAnySkills() => Skills != CalculatorSkills.None;

        public void Deconstruct(out decimal lonerSuccess, out decimal proSuccess, out Func<CalculatorSkills, CalculatorSkills, bool> canUseSkill)
        {
            lonerSuccess = LonerSuccess();
            proSuccess = ProSuccess;
            canUseSkill = CanUseSkill;
        }

        public decimal LonerSuccess()
        {
            return Skills.Contains(CalculatorSkills.Loner) ? (7m - LonerValue) / 6 : 1;
        }

        public string Description(Season season = Season.Season3)
        {
            if (Skills == CalculatorSkills.None)
            {
                return "None";
            }

            var sb = new StringBuilder();

            foreach (var skill in Skills.ToEnumerable(CalculatorSkills.None))
            {
                sb.Append($"{skill.ToString().PascalCaseToSpaced()}{GetSkillValue(skill, season)}, ");
            }

            return sb.Remove(sb.Length - 2, 2).ToString();
        }

        private string GetSkillValue(CalculatorSkills skill, Season season) => skill switch
        {
            CalculatorSkills.Loner => " " + LonerValue + "+",
            CalculatorSkills.BreakTackle => " +" + BreakTackleValue,
            CalculatorSkills.DirtyPlayer => season == Season.Season3 ? "" : " +" + DirtyPlayerValue,
            CalculatorSkills.MightyBlow => season == Season.Season3 ? "" : " +" + MightyBlowValue,
            _ => ""
        };
    }
}