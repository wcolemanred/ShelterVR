using UnityEngine;
using System;
using System.Collections.Generic;
using SFDC.wef.data;
using UnityEngine.UI;

namespace SFDC.wef.ui
{
	public class ConfigurationSelectionScreen : MonoBehaviour
	{
		public Action<Configuration> onSelectConfiguration;

		private List<Configuration> configurations;

		private Vector2 buttonSize = new Vector2(170, 40);
		private const float BUTTON_SPACING = 10;

		public void init(List<Configuration> configurations)
		{
			this.configurations = configurations;
						
			Transform listContainer = transform.Find("Scroll View/Viewport/List");
			// Set list height
			RectTransform listRect = listContainer.GetComponent<RectTransform>();
			Vector2 listSize = listRect.getSize();
			listSize.y = configurations.Count * (buttonSize.y + BUTTON_SPACING) + 2 * BUTTON_SPACING;
			listRect.setSize(listSize);
			// Add buttons
			float yPosition = BUTTON_SPACING;
			foreach (Configuration config in configurations)
			{
				yPosition -= BUTTON_SPACING + buttonSize.y;
				GameObject button = UiHelper.addButton(listContainer, config.name + " (" + config.briefName + ")", config.id, new Vector3(0, yPosition, 0));
				button.GetComponent<Button>().onClick.AddListener(() => onConfigurationButtonClicked(button));
			}
		}

		private void onConfigurationButtonClicked(GameObject selectedButton)
		{
			Configuration selectedConfiguration = null;
			for (int i = 0; selectedConfiguration == null && i < configurations.Count; i++)
			{
				if (configurations[i].id == selectedButton.name)
					selectedConfiguration = configurations[i];
			}
			Debug.Log("Selected configuration: " + selectedConfiguration.name);
			onSelectConfiguration.Invoke(selectedConfiguration);
		}
	}
}