using UnityEngine;
using UnityEngine.UI;

namespace SFDC.wef.ui
{
	class UiHelper
	{
		private static GameObject buttonPrefab;

		public static GameObject addButton(Transform parent, string label, string name, Vector3 position)
		{
			GameObject button = instantiateButtonPrefab();
			button.name = name;
			button.transform.position = position;
			button.transform.SetParent(parent, false);
			button.GetComponentInChildren<Text>().text = label;
			return button;
		}

		private static GameObject instantiateButtonPrefab()
		{
			if (buttonPrefab == null)
				buttonPrefab = Resources.Load<GameObject>("Prefabs/VRButton");
			return GameObject.Instantiate(buttonPrefab);
		}

		public static void setAllButtonEnabled(RectTransform screen, bool isEnabled)
		{
			Button[] buttons = screen.GetComponentsInChildren<Button>();
			foreach (Button button in buttons)
				button.interactable = isEnabled;
		}
	}
}
