using ActionCalculator.Models;
using FluentValidation;

namespace ActionCalculator
{
    public class CalculationValidator : AbstractValidator<Calculation>
    {
        public CalculationValidator()
        {
            RuleFor(x => x.Rerolls).GreaterThanOrEqualTo(0).LessThanOrEqualTo(8);
            RuleFor(x => x.PlayerActions).NotEmpty();
            RuleFor(x => x.PlayerActions)
                .Must(x => x.Select(y => y.Action.ActionType).Count(y => y == ActionType.Block) > 1)
                .When(x => x.PlayerActions.Select(y => y.Action.ActionType).Contains(ActionType.Dauntless));
            RuleFor(x => x.PlayerActions)
                .Must(x => x.All(pa => pa.Player.ShortName != null || (pa.Player.MightyBlowValue <= 1 && pa.Player.DirtyPlayerValue <= 1)))
                .When(x => x.Season == Season.Season3)
                .WithMessage("In Season 3, Mighty Blow and Dirty Player can only have a +1 modifier");
        }
    }
}
