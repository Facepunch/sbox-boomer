
namespace Boomer.UI;

internal class SliderStepAttribute : Attribute
{

	public readonly float Step;

	public SliderStepAttribute( float step ) => Step = step;

}
