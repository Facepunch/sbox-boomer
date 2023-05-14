namespace Facepunch.Boomer;

internal class SliderStepAttribute : Attribute
{
	public readonly float Step;
	public SliderStepAttribute( float step ) => Step = step;
}
