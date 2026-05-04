using ActionCalculator.Abstractions;
using ActionCalculator.Models;

namespace ActionCalculator
{
    public class CalculationBuilder(IPlayerActionsBuilder playerActionsBuilder, ICalculationContext context) : ICalculationBuilder
    {
        public Calculation Build(string calculationString, int rerolls)
        {
            var season = Season.Season3;
            var input = calculationString;

            if (input.EndsWith("~S2"))
            {
                input = input[..^3];
                season = Season.Season2;
            }

            context.Season = season;
            return new Calculation(playerActionsBuilder.Build(input), rerolls, season);
        }
    }
}
