
namespace RacingGame.Utils
{
	public static class StringUtils
	{
		//  https://stackoverflow.com/questions/20156/is-there-an-easy-way-to-create-ordinals-in-c
		public static string AddOrdinal( int number )
		{
			switch ( number % 100 )
			{
				case 11:
				case 12:
				case 13:
					return number + "th";
			}

			switch ( number % 10 )
			{
				case 1:
					return number + "st";
				case 2:
					return number + "nd";
				case 3:
					return number + "rd";
				default:
					return number + "th";
			}
		}
	}
}
