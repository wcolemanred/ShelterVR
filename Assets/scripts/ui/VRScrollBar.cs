using UnityEngine;
using UnityEngine.UI;

namespace SFDC.wef.ui
{
	[RequireComponent(typeof(Scrollbar))]
	public class VRScrollBar : MonoBehaviour
	{
		private Scrollbar scrollbar;

		void Start()
		{
			scrollbar = GetComponent<Scrollbar>();
		}

		public void scrollUp()
		{
			if (scrollbar.value < 1)
				scrollbar.value += 0.2f;
		}

		public void scrollDown()
		{
			if (scrollbar.value > 0)
				scrollbar.value -= 0.2f;
		}
	}
}
