namespace ActionCalculator.Models.Actions
{
	public class Interception(int roll) : Action(ActionType.Interception, roll, false)
	{
        public override bool IsRerollable() => false;
	}
}